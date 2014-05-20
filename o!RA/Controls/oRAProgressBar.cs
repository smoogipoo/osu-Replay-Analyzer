using System;
using System.Drawing;
using System.Windows.Forms;

namespace o_RA.Controls
{
    public partial class oRAProgressBar : UserControl
    {
        public oRAProgressBar()
        {
            Minimum = 0;
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private int P_Value;
        private int P_Max = 100;
        public int Value
        {
            get
            {
                return P_Value;   
            }
            set
            {
                if (value < Minimum || value > Maximum)
                    throw new ArgumentOutOfRangeException();
                P_Value = value;
                Invalidate();
            }
        }
        public int Minimum { get; set; }
        public int Maximum { get { return P_Max; } set { P_Max = value; } }

        private void oRAProgressBar_Paint(object sender, PaintEventArgs e)
        {
            float progressWidth = Width * (Value / (Maximum - (float)Minimum));
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), 0, 0, Width, Height);
            if (progressWidth > 1)
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_1), 0, 0, progressWidth, Height);
                e.Graphics.DrawRectangle(new Pen(oRAColours.Colour_Item_BG_0), 0, 0, progressWidth - 1, Height - 1);
            }
        }
    }
}
