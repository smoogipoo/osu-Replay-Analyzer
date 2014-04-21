﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace o_RA
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
            Properties.Settings.Default.ApplicationLocale = v_locale;
            Properties.Settings.Default.Save();
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