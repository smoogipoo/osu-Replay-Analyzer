using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BMAPI.v1;
using o_RA;
using ReplayAPI;

namespace ChartsPlugin
{
    public partial class AimChart : UserControl
    {
        public AimChart()
        {
            InitializeComponent();
        }

        Point LastHitPoint;

        private void TWChart_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    Chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                    Chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
                    break;
                case MouseButtons.Left:
                    {
                        HitTestResult result = Chart.HitTest(e.X, e.Y);
                        if (result.ChartElementType == ChartElementType.DataPoint)
                        {
                            var point = Chart.Series[0].Points.FirstOrDefault(p => p.Color == oRAColours.Colour_Item_BG_0);
                            if (point != null)
                                point.Color = oRAColours.Colour_Item_BG_1;
                            Chart.Series[0].Points[result.PointIndex].Color = oRAColours.Colour_Item_BG_0;
                            oRA.Data.ChangeFrame(result.PointIndex);
                        }
                    }
                    break;
            }
        }
        private void TWChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (new Point(e.X, e.Y) == LastHitPoint)
                return;
            LastHitPoint = new Point(e.X, e.Y);
            HitTestResult result = Chart.HitTest(e.X, e.Y);
            if (result.PointIndex != -1 && result.Series != null && result.PointIndex < Chart.Series[0].Points.Count && Equals(result.Series, Chart.Series[0]))
            {
                    ChartToolTip.Tag = (int)Chart.Series[0].Points[result.PointIndex].XValue;
                    ChartToolTip.SetToolTip(Chart, Chart.Series[0].Points[result.PointIndex].YValues[0].ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                ChartToolTip.Hide(Chart);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Chart.ChartAreas[0].AxisY.Title = oRA.Data.Language["oRA_Displacement"];
            Chart.ChartAreas[0].AxisX.Title = oRA.Data.Language["oRA_CircleNumber"];
            Chart.Series[0].Name = oRA.Data.Language["oRA_AimAccuracy"];
            oRA.Data.ReplayChanged += HandleReplayChanged;
            oRA.Data.FrameChanged += HandleFrameChanged;
        }

        private void HandleReplayChanged(Replay r, Beatmap b)
        {
            Chart.SuspendLayout();
            Chart.Series[0].Points.Clear();
            Chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            Chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            for (int i = 0; i < oRA.Data.ReplayObjects.Count; i++ )
            {
                Chart.Series[0].Points.AddXY(i + 1, Math.Sqrt(Math.Pow(oRA.Data.ReplayObjects[i].Frame.X - oRA.Data.ReplayObjects[i].Object.Location.X, 2) + 
                                                    Math.Pow(oRA.Data.ReplayObjects[i].Frame.Y - oRA.Data.ReplayObjects[i].Object.Location.Y, 2)));
            }
            Chart.ResumeLayout();
        }
        private void HandleFrameChanged(int index)
        {
            if (index > Chart.Series[0].Points.Count - 1)
                return;
            var point = Chart.Series[0].Points.FirstOrDefault(p => p.Color == oRAColours.Colour_Item_BG_0);
            if (point != null)
                point.Color = oRAColours.Colour_Item_BG_1;
            Chart.Series[0].Points[index].Color = oRAColours.Colour_Item_BG_0;
        }
    }
}
