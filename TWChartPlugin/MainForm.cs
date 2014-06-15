using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using o_RA;
using ReplayAPI;
using BMAPI;

namespace TWChartPlugin
{
    public partial class MainForm : UserControl
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Point LastHitPoint;

        private void TWChart_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    TWChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                    TWChart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
                    break;
                case MouseButtons.Left:
                    {
                        HitTestResult result = TWChart.HitTest(e.X, e.Y);
                        if (result.ChartElementType == ChartElementType.DataPoint)
                        {
                            var point = TWChart.Series[3].Points.FirstOrDefault(p => p.Color == oRAColours.Colour_Item_BG_0);
                            if (point != null)
                                point.Color = oRAColours.Colour_BG_P1;
                            TWChart.Series[3].Points[result.PointIndex].Color = oRAColours.Colour_Item_BG_0;
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
            HitTestResult result = TWChart.HitTest(e.X, e.Y);
            if (result.PointIndex != -1 && result.Series != null && result.PointIndex < TWChart.Series[3].Points.Count && Equals(result.Series, TWChart.Series[3]))
            {
                    ChartToolTip.Tag = (int)TWChart.Series[3].Points[result.PointIndex].XValue;
                    ChartToolTip.SetToolTip(TWChart, TWChart.Series[3].Points[result.PointIndex].YValues[0] + "ms");
            }
            else
            {
                ChartToolTip.Hide(TWChart);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            oRA.Data.ReplayChanged += HandleReplayChanged;
            oRA.Data.FrameChanged += HandleFrameChanged;
        }

        private void HandleReplayChanged(Replay r, Beatmap b)
        {
            TWChart.Series[3].Points.Clear();
            TWChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            TWChart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            TWChart.ChartAreas[0].AxisY.Minimum = -oRA.Data.TimingWindows[0];
            TWChart.ChartAreas[0].AxisY.Maximum = oRA.Data.TimingWindows[0];
            for (int i = 0; i < oRA.Data.ReplayObjects.Count; i++)
            {
                TWChart.Series[3].Points.AddXY(i + 1, oRA.Data.ReplayObjects[i].Frame.Time - oRA.Data.ReplayObjects[i].Object.StartTime); 
            }         
            TWChart.Series[0].Points.Clear();
            TWChart.Series[0].Points.AddXY(0, oRA.Data.TimingWindows[2], -oRA.Data.TimingWindows[2]);
            TWChart.Series[0].Points.AddXY(oRA.Data.ReplayObjects.Count, oRA.Data.TimingWindows[2], -oRA.Data.TimingWindows[2]);
            TWChart.Series[1].Points.Clear();
            TWChart.Series[1].Points.AddXY(0, oRA.Data.TimingWindows[1], -oRA.Data.TimingWindows[1]);
            TWChart.Series[1].Points.AddXY(oRA.Data.ReplayObjects.Count, oRA.Data.TimingWindows[1], -oRA.Data.TimingWindows[1]);
            TWChart.Series[2].Points.Clear();
            TWChart.Series[2].Points.AddXY(0, oRA.Data.TimingWindows[0], -oRA.Data.TimingWindows[0]);
            TWChart.Series[2].Points.AddXY(oRA.Data.ReplayObjects.Count, oRA.Data.TimingWindows[0], -oRA.Data.TimingWindows[0]);
        }
        private void HandleFrameChanged(int index)
        {
            if (index > TWChart.Series[3].Points.Count - 1)
                return;
            var point = TWChart.Series[3].Points.FirstOrDefault(p => p.Color == oRAColours.Colour_Item_BG_0);
            if (point != null)
                point.Color = oRAColours.Colour_BG_P1;
            TWChart.Series[3].Points[index].Color = oRAColours.Colour_Item_BG_0;
        }
    }
}
