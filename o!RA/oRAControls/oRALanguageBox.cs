using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using o_RA.oRAForms;

namespace o_RA.oRAControls
{

    public partial class LanguageBox : UserControl
    {
        public LanguageBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
        string v_locale;
        bool mouseEntered;
        bool mouseDown;

        [Description("Set the locale that corresponds to the flag."), Category("Locale")]
        public string Locale
        {
            set { v_locale = value; }
            get { return v_locale; }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (mouseDown)
                e.Graphics.FillRectangle(Brushes.Gray, 0, 0, 155, 99);
            else
                e.Graphics.FillRectangle(mouseEntered ? Brushes.DarkGray : Brushes.White, 0, 0, 155, 99);
            e.Graphics.DrawRectangle(Pens.DarkGray, 0, 0, 154, 99);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
           if (BackgroundImage != null)
           {
               e.Graphics.DrawImage(BackgroundImage, 5, 5, 145, 90);
           }         
        }

        private void LanguageBox_Click(object sender, EventArgs e)
        {
            oRAMainForm.Settings.AddSetting("ApplicationLocale", v_locale);
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
