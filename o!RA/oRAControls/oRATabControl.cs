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
        private readonly static Bitmap P_Icon = new Bitmap(40, 40);
        private readonly static Graphics P_IconGraphics = Graphics.FromImage(P_Icon);
        public Bitmap Icon {
            get
            {
                return P_Icon;
            }
            set
            {
                if (value != null)
                    P_IconGraphics.DrawImage(value, 0, 0, 50, 50);
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public object Contents { get; set; }
    }

    public class PageCollection : UserControl
    {
        public Panel TabContainer { get; set; }

        private List<oRAPage> pages = new List<oRAPage>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<oRAPage> Pages
        {
            get
            {
                return pages;
            }
            set
            {
                pages = value;
            }
        }

        internal static int TotalHeight = 0;
        public void Add(oRAPage Page)
        {
            Pages.Add(Page);

            Label l = new Label();
            l.AutoSize = false;
            l.Height = 60;
            l.Width = 60;
            l.BackColor = oRAColours.Colour_BG_P0;
            l.Image = Page.Icon;
            l.Location = new Point(0, TotalHeight);
            l.MouseDown += ChangeTab;
            l.Paint += PaintOverride;
            l.Tag = Pages.Count - 1 + "0";
            Controls.Add(l);

            if (Pages.Count == 1)
            {
                TabContainer.Controls.Add((Control)Page.Contents);
                l.Tag = l.Tag.ToString().Substring(0, 1) + "1";
                l.Refresh();
            }
            TotalHeight += 61;
        }
        private void ChangeTab(object sender, EventArgs e)
        {
            if (((Label)sender).Tag.ToString().Substring(1, 1) == "1")
            {
                return;
            }
            TabContainer.Controls.Clear();
            TabContainer.Controls.Add((Control)pages[Convert.ToInt32(((Label)sender).Tag.ToString().Substring(0, 1))].Contents);
            foreach (Label l in Controls)
            {
                l.Tag = l.Tag.ToString().Substring(0, 1) + ((Label)sender == l ? "1" : "0");
                l.Refresh();
            }
        }

        static private void PaintOverride(object sender, PaintEventArgs e)
        {
            if (((Label)sender).Tag.ToString().Substring(1, 1) == "1")
            {
                e.Graphics.FillRectangle(new LinearGradientBrush(new Point(((Label)sender).Width / 2, 0), new Point(((Label)sender).Width / 2, ((Label)sender).Height), oRAColours.Colour_Item_BG_0, oRAColours.Colour_Item_BG_1), 0, 0, ((Label)sender).Width, ((Label)sender).Height);
            }
            e.Graphics.DrawImage(((Label)sender).Image, 5, 5, 50, 50);
        }

        public oRAPage[] GetPages()
        {
            return Pages.ToArray();
        }
    }
}
