using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using o_RA.Forms;

namespace o_RA.Controls
{

    public partial class LanguageBox : UserControl
    {
        public LanguageBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
        bool mouseEntered;
        bool mouseDown;

        [Description("Set the locale that corresponds to the flag."), Category("Locale")]
        public string Locale { get; set; }
        public Bitmap NormalImage { get; set; }
        public Bitmap MouseOverImage { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(mouseDown ? oRAColours.Colour_Item_BG_0 : oRAColours.Colour_BG_P0), e.ClipRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (MouseOverImage == null && NormalImage == null)
                return;
            RectangleF drawRect = new RectangleF(new PointF(e.ClipRectangle.Width / 2 - MouseOverImage.Width / 2, e.ClipRectangle.Height / 2 - MouseOverImage.Height / 2), MouseOverImage.Size);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.DrawImage(mouseEntered ? MouseOverImage : NormalImage, drawRect);
        }

        private void LanguageBox_Click(object sender, EventArgs e)
        {
            oRAMainForm.Settings.AddSetting("ApplicationLocale", Locale);
            oRAMainForm.Settings.Save();
            var findForm = FindForm();
            if (findForm != null)
                findForm.Close();
        }

        private void LanguageBox_MouseEnter(object sender, EventArgs e)
        {
            mouseEntered = true;
            Refresh();
        }

        private void LanguageBox_MouseLeave(object sender, EventArgs e)
        {
            mouseEntered = false;
            Refresh();
        }

        private void LanguageBox_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            Refresh();
        }

        private void LanguageBox_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            Refresh();
        }
    }
}
