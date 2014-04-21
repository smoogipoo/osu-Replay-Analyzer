using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Drawing.Drawing2D;

using System.Windows.Forms;

namespace o_RA
{
    public partial class BarGraph : UserControl
    {
        public BarGraph()
        {
            InitializeComponent();
        }

        public int top = 10, left = 20;
        internal int bottom, right;
        internal float midY, midX, yStep;
        internal List<GraphicsPath> paths = new List<GraphicsPath>();
        internal Graphics barGraphics;

        public GraphStyle graphStyle = GraphStyle.Vertical;
        public FillMode fillMode = FillMode.None;
        public bool showAverageDeviation = true;
        public Font axisFont = new Font("Segoe UI", 8);
        public List<int> xValues = new List<int>();
        public List<int> yValues = new List<int>();
        public Color backColor = Color.White;
        public Color axisColor = Color.Black;
        public Color gridColor = Color.LightGray;
        public Color posColor = Color.Green;
        public Color negColor = Color.Red;
        public int avgLineWidth = 2;

        public void ReDraw()
        {
            Refresh();
        }

        public enum GraphStyle
        {
            Vertical,
            Horizontal
        }
        public enum FillMode
        {
            None,
            Fill
        }

        private void BarGraph_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void BarGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(backColor);
            bottom = Height - top > top ? Height - top : top;
            right = Width - left > left ? Width - left : left;
            barGraphics = e.Graphics;
            if (graphStyle == GraphStyle.Vertical)
            {
                if (xValues.Count != 0 && yValues.Count != 0 && xValues.Count == yValues.Count)
                {
                    float posAvg = 0, negAvg = 0;
                    int posCount = 0, negCount = 0;
                    foreach (int val in yValues)
                    {
                        if (val > 0)
                        {
                            posAvg += val;
                            posCount += 1;
                        }
                        else
                        {
                            negAvg += val;
                            negCount += 1;
                        }
                    }
                    posAvg = posAvg / posCount;
                    negAvg = negAvg / negCount;

                    float yMax = yValues.Max() + (yValues.Max() / 10);
                    float yMin = yValues.Min() - (-1 * yValues.Min() / 10);
                    if (yMax - yMin < 0.00001 && yMax == 0)
                        return;
                    yStep = (bottom - top) / (yMax - yMin);
                    midY = top + (bottom - top + yStep * (Math.Abs(yMax) - Math.Abs(yMin))) / 2;
                    if (midY < top)
                        midY = top;
                    if (midY > bottom)
                        midY = bottom;
                    float xStep = (right - left) / (float)xValues.Count;
                    float maxY = 0, minY = 0;

                    //Draw y-axis labels, points and grid
                    for (int i = 0; i <= (Math.Abs(yMax) > Math.Abs(yMin) ? Math.Abs(yMax) : Math.Abs(yMin)) + 1; i++)
                    {
                        if (midY + yStep * i <= bottom)
                        {
                            e.Graphics.DrawLine(new Pen(new SolidBrush(gridColor)), new Point(left, (int)(midY + yStep * i)), new Point(right, (int)(midY + yStep * i))); //HGrid
                            minY = midY + yStep * i;
                        }
                        if (midY - yStep * i >= top)
                        {
                            e.Graphics.DrawLine(new Pen(new SolidBrush(gridColor)), new Point(left, (int)(midY - yStep * i)), new Point(right, (int)(midY - yStep * i))); //HGrid
                            maxY = midY - yStep * i;
                        }
                    }
                    for (int i = 0; i <= xValues.Count; i++)
                    {
                        e.Graphics.DrawLine(new Pen(new SolidBrush(gridColor)), new Point((int)(left + xStep * (i + 1)), (int)maxY), new Point((int)(left + xStep * (i + 1)), (int)minY)); //VGrid
                    }

                    for (int i = 0; i <= (Math.Abs(yMax) > Math.Abs(yMin) ? Math.Abs(yMax) : Math.Abs(yMin)) + 1; i++)
                    {
                        if (midY + yStep * i <= bottom)
                        {
                            if (i % (Math.Abs(yMax) > Math.Abs(yMin) ? (int)(Math.Abs(yMax) / 10) : (int)(Math.Abs(yMin) / 10)) == 0)
                            {
                                e.Graphics.DrawLine(new Pen(new SolidBrush(axisColor)) { DashStyle = DashStyle.Dash }, new Point(left, (int)(midY + yStep * i)), new Point(right, (int)(midY + yStep * i))); //Dark HGrid
                                e.Graphics.DrawString((i * -1).ToString(CultureInfo.InvariantCulture), new Font("Segoe UI", 8), new SolidBrush(axisColor), new PointF(left - 10 - e.Graphics.MeasureString((i * -1).ToString(CultureInfo.InvariantCulture), new Font("Segoe UI", 8)).Width / 2, midY + yStep * i - e.Graphics.MeasureString((i * -1).ToString(CultureInfo.InvariantCulture), new Font("Segoe UI", 8)).Height / 2));
                            }
                        }
                        if (midY - yStep * i >= top)
                        {
                            if (i % (Math.Abs(yMax) > Math.Abs(yMin) ? (int)(Math.Abs(yMax) / 10) : (int)(Math.Abs(yMin) / 10)) == 0)
                            {
                                e.Graphics.DrawLine(new Pen(new SolidBrush(axisColor)) { DashStyle = DashStyle.Dash }, new Point(left, (int)(midY - yStep * i)), new Point(right, (int)(midY - yStep * i))); //Dark HGrid
                                e.Graphics.DrawString(i.ToString(CultureInfo.InvariantCulture), new Font("Segoe UI", 8), new SolidBrush(axisColor), new PointF(left - 10 - e.Graphics.MeasureString(i.ToString(CultureInfo.InvariantCulture), new Font("Segoe UI", 8)).Width / 2, midY - yStep * i - e.Graphics.MeasureString(i.ToString(CultureInfo.InvariantCulture), new Font("Segoe UI", 8)).Height / 2));
                            }
                        }
                    }

                    //Draw graph
                    for (int i = 0; i <= xValues.Count; i++)
                    {
                        if ((int)(left + (i + 1) * xStep) <= right)
                        {
                            GraphicsPath gp = new GraphicsPath();
                            gp.AddLine(left + i * xStep, midY, left + i * xStep, midY - yValues[i == xValues.Count ? xValues.Count - 1 : i] * yStep); //Up
                            gp.AddLine(left + i * xStep, midY - yValues[i == xValues.Count ? xValues.Count - 1 : i] * yStep, left + (i + 1) * xStep, midY - yValues[i == xValues.Count ? xValues.Count - 1 : i] * yStep); //Right
                            gp.AddLine(left + (i + 1) * xStep, midY - yValues[i == xValues.Count ? xValues.Count - 1 : i] * yStep, left + (i + 1) * xStep, midY); //Down
                            gp.AddLine(left + (i + 1) * xStep, midY, left + i * xStep, midY); //Aaaand come full circle
                            paths.Add(gp);
                            if (fillMode == FillMode.Fill)
                                e.Graphics.FillPath(new SolidBrush(yValues[i == xValues.Count ? xValues.Count - 1 : i] >= 0 ? posColor : negColor), gp);
                            else
                                e.Graphics.DrawPath(new Pen(new SolidBrush(yValues[i == xValues.Count ? xValues.Count - 1 : i] >= 0 ? posColor : negColor)), gp);
                        }
                    }

                    //Draw average bars
                    if (posCount != 0)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(posColor)) { DashStyle = DashStyle.Dash, Width = avgLineWidth }, left, midY - yStep * posAvg, right, midY - yStep * posAvg);
                    if (negCount != 0)
                        e.Graphics.DrawLine(new Pen(new SolidBrush(negColor)) { DashStyle = DashStyle.Dash, Width = avgLineWidth }, left, midY - yStep * negAvg, right, midY - yStep * negAvg);

                    //Draw axese (Do this at the end to get rid of overlaps)
                    e.Graphics.DrawLine(new Pen(new SolidBrush(axisColor)), new Point(left, top), new Point(left, bottom));
                    e.Graphics.DrawLine(new Pen(new SolidBrush(axisColor)), new Point(left, (int)midY), new Point(right, (int)midY));
                }
            }
            else
            {
                //Todo: Horizontal graph style
                //NOPE!
            }
        }

        private void BarGraph_Load(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
    }
}
