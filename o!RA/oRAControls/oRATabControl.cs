using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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

        public PageCollection()
        {
            oRALabel expandLabel = new oRALabel
            {
                Width = 60,
                Height = 60,
                Icon_Hot = Properties.Resources.Menu_H,
                Icon_Normal = Properties.Resources.Menu_N,
                Colour_Hot = oRAColours.Colour_BG_Main,
            };
            expandLabel.Paint += PaintOverride;
            expandLabel.MouseDown += ExpandTC;
            Controls.Add(expandLabel);
            TotalHeight += 61;
        }

        public void Add(oRAPage Page)
        {
            Pages.Add(Page);

            oRALabel l = new oRALabel
            {
                AutoSize = false,
                Width = 60,
                Height = 60,
                Icon_Normal = Page.Icon_Normal,
                Icon_Hot = Page.Icon_Hot,
                Location = new Point(0, TotalHeight),
                Index = Pages.Count - 1,
                Text = Page.Name,
            };
            l.MouseDown += ChangeTab;
            l.Paint += PaintOverride;
            Controls.Add(l);

            TabContainer.Controls.Add((Control)Page.Contents);
            if (Pages.Count == 1)
            {  
                l.Activated = true;
                l.Refresh();
            }
            else
            {
                TabContainer.Controls.Remove((Control)Page.Contents);
            }
            TotalHeight += 61;
        }

        private void ExpandTC(object sender, EventArgs e)
        {
            ((oRALabel)sender).Activated = !((oRALabel)sender).Activated;
            ((oRALabel)sender).Parent.Width = ((oRALabel)sender).Activated ? 200 : 60;
            foreach (oRALabel l in Controls)
            {
                l.Width = ((oRALabel)sender).Activated ? 200 : 60;
                l.Refresh();
            }
        }

        private void ChangeTab(object sender, EventArgs e)
        {
            if (((oRALabel)sender).Activated)
            {
                return;
            }
            TabContainer.Controls.Clear();
            TabContainer.Controls.Add((Control)Pages[((oRALabel)sender).Index].Contents);
            foreach (oRALabel l in Controls)
            {
                if (l.Index != -1)
                    l.Activated = sender == l;
                l.Refresh();
            }
        }

        private void PaintOverride(object sender, PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (((oRALabel)sender).Activated)
            {
                e.Graphics.FillRectangle(new SolidBrush(((oRALabel)sender).Colour_Hot), 0, 0, ((oRALabel)sender).Width, ((oRALabel)sender).Height);
                e.Graphics.DrawImage(((oRALabel)sender).Icon_Hot, 5, 5, 50, 50);
                if (((oRALabel)Controls[0]).Activated)
                    e.Graphics.DrawString(((oRALabel)sender).Text, oRAFonts.Font_Title, new SolidBrush(((oRALabel)sender).Colour_Normal), new RectangleF(60, 30 - e.Graphics.MeasureString(((oRALabel)sender).Text, oRAFonts.Font_Title).Height / 2, 140, e.Graphics.MeasureString(((oRALabel)sender).Text, oRAFonts.Font_Title).Height));                    
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(((oRALabel)sender).Colour_Normal), 0, 0, ((oRALabel)sender).Width, ((oRALabel)sender).Height);
                e.Graphics.DrawImage(((oRALabel)sender).Icon_Normal, 5, 5, 50, 50);
                if (((oRALabel)Controls[0]).Activated)
                    e.Graphics.DrawString(((oRALabel)sender).Text, oRAFonts.Font_Title, new SolidBrush(((oRALabel)sender).Colour_Hot), new RectangleF(60, 30 - e.Graphics.MeasureString(((oRALabel)sender).Text, oRAFonts.Font_Title).Height / 2, 140, e.Graphics.MeasureString(((oRALabel)sender).Text, oRAFonts.Font_Title).Height));                    
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
        public Color Colour_Normal = oRAColours.Colour_BG_P0;
        public Color Colour_Hot = oRAColours.Colour_Item_BG_0;
        public int Index = -1;
        public bool Activated;
    }
}
