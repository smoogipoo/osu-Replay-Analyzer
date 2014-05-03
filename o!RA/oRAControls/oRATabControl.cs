using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace o_RA.oRAControls
{
    public partial class oRATabControl : UserControl
    {
        public oRATabControl()
        {
            InitializeComponent();
            TabPages.TabContainer = TabContainer;
        }

        public void Add(oRAPage page)
        {
            TabPages.Add(page);
        }
    }

    [Serializable]
    public class oRAPage
    {
        private Bitmap P_Icon_N = new Bitmap(50, 50);
        private Bitmap P_Icon_H = new Bitmap(50, 50);
        public Bitmap Icon_Normal
        {
            get
            {
                return P_Icon_N;
            }
            set
            {
                if (value != null)
                    P_Icon_N = new Bitmap(value, 50, 50);
            }
        }
        public Bitmap Icon_Hot
        {
            get
            {
                return P_Icon_H;
            }
            set
            {
                if (value != null)
                    P_Icon_H = new Bitmap(value, 50, 50);
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public object Contents { get; set; }
    }

    public class PageCollection : UserControl
    {
        public Panel TabContainer { get; set; }

        public List<oRAPage> Pages = new List<oRAPage>();

        internal static int TotalHeight = 0;
        public void Add(oRAPage Page)
        {
            Pages.Add(Page);

            oRALabel l = new oRALabel();
            l.AutoSize = false;
            l.Height = 60;
            l.Width = 60;
            l.Icon_Normal = Page.Icon_Normal;
            l.Icon_Hot = Page.Icon_Hot;
            l.Location = new Point(0, TotalHeight);
            l.MouseDown += ChangeTab;
            l.Paint += PaintOverride;
            l.Tag = Pages.Count - 1 + "0";
            Controls.Add(l);

            TabContainer.Controls.Add((Control)Page.Contents);
            if (Pages.Count == 1)
            {    
                l.Tag = l.Tag.ToString().Substring(0, 1) + "1";
                l.Refresh();
            }
            else
            {
                TabContainer.Controls.Remove((Control)Page.Contents);
            }
            TotalHeight += 61;
        }
        private void ChangeTab(object sender, EventArgs e)
        {
            if (((oRALabel)sender).Tag.ToString().Substring(1, 1) == "1")
            {
                return;
            }
            TabContainer.Controls.Clear();
            TabContainer.Controls.Add((Control)Pages[Convert.ToInt32(((oRALabel)sender).Tag.ToString().Substring(0, 1))].Contents);
            foreach (oRALabel l in Controls)
            {
                l.Tag = l.Tag.ToString().Substring(0, 1) + ((oRALabel)sender == l ? "1" : "0");
                l.Refresh();
            }
        }

        static private void PaintOverride(object sender, PaintEventArgs e)
        {
            if (((oRALabel)sender).Tag.ToString().Substring(1, 1) == "1")
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_0), 0, 0, ((oRALabel)sender).Width, ((oRALabel)sender).Height);
                e.Graphics.DrawImage(((oRALabel)sender).Icon_Hot, 5, 5, 50, 50);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), 0, 0, ((oRALabel)sender).Width, ((oRALabel)sender).Height);
                e.Graphics.DrawImage(((oRALabel)sender).Icon_Normal, 5, 5, 50, 50);
            }
        }

        public oRAPage[] GetPages()
        {
            return Pages.ToArray();
        }
    }

    public class oRALabel : Label
    {
        public Bitmap Icon_Normal { get; set; }
        public Bitmap Icon_Hot { get; set; }
    }
}
