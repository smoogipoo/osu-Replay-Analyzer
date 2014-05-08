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
            HitTestResult result = TWChart.HitTest(e.X, e.Y);

            if (result.PointIndex != -1 && result.Series != null && result.PointIndex < TWChart.Series[3].Points.Count)
            {
                if (ChartToolTip.Tag == null || (int)ChartToolTip.Tag != (int)TWChart.Series[3].Points[result.PointIndex].XValue)
                {
                    ChartToolTip.Tag = (int)TWChart.Series[3].Points[result.PointIndex].XValue;
                    ChartToolTip.SetToolTip(TWChart, TWChart.Series[3].Points[result.PointIndex].YValues[0] + "ms");
                }
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
            foreach (int time in oRA.Data.TimingDifference)
            {
                TWChart.Series[3].Points.Add(time);
            }
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
