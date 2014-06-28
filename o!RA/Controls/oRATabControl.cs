using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using o_RAResources;

namespace o_RA.Controls
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
        public ToolTip TextToolTip = new ToolTip();
        public List<oRAPage> Pages = new List<oRAPage>();
        internal static int TotalHeight = 0;

        public PageCollection()
        {
            TextToolTip.AutomaticDelay = 0;
            oRALabel expandLabel = new oRALabel
            {
                Width = 60,
                Height = 60,
                Icon_Hot = new Bitmap(ResourceHelper.GetResourceStream("Menu_H.png")),
                Icon_Normal = new Bitmap(ResourceHelper.GetResourceStream("Menu_N.png")),
                Colour_Hot = oRAColours.Colour_BG_Main,
                Text = @"Expand Tabs",
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
            l.MouseEnter += HandleMouseEnter;
            l.MouseLeave += HandleMouseLeave;
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

        public void Remove(oRAPage Page)
        {
            Pages.Remove(Page);
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(oRALabel) && c.Text == Page.Name)
                    Controls.Remove(c);
            }
            TabContainer.Controls.Remove((Control)Page.Contents);
            TotalHeight -= 61;
        }

        public void RemoveAt(int index)
        {
            Pages.RemoveAt(index);
            Controls.RemoveAt(index);
            TabContainer.Controls.RemoveAt(index);
            TotalHeight -= 61;
        }

        private void ExpandTC(object sender, EventArgs e)
        {
            oRALabel label = (oRALabel)sender;
            label.Activated = !label.Activated;
            label.Parent.Width = label.Activated ? 200 : 60;
            foreach (oRALabel l in Controls)
            {
                l.Width = label.Activated ? 200 : 60;
                l.Refresh();
            }
        }

        private void HandleMouseEnter(object sender, EventArgs e)
        {
            oRALabel label = (oRALabel)sender;
            if (label.HasEllipsis && ((oRALabel)Controls[0]).Activated)
            {
                TextToolTip.Show(label.Text, label, new Point(60, label.Height));
            }
        }
        private void HandleMouseLeave(object sender, EventArgs e)
        {
            if (TextToolTip.Active)
                TextToolTip.Hide((oRALabel)sender);
        }
        private void ChangeTab(object sender, EventArgs e)
        {
            oRALabel label = (oRALabel)sender;
            if (label.Activated)
            {
                return;
            }
            TabContainer.Controls.Clear();
            TabContainer.Controls.Add((Control)Pages[label.Index].Contents);
            foreach (oRALabel l in Controls)
            {
                if (l.Index != -1)
                    l.Activated = sender == l;
                l.Refresh();
            }
        }

        private void PaintOverride(object sender, PaintEventArgs e)
        {
            oRALabel label = (oRALabel)sender;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            StringFormat sF = new StringFormat();
            sF.Trimming = StringTrimming.EllipsisCharacter;
            SizeF textSize = e.Graphics.MeasureString(label.Text, oRAFonts.Font_Title);

            if (label.Activated)
            {
                e.Graphics.FillRectangle(new SolidBrush(label.Colour_Hot), 0, 0, label.Width, label.Height);
                e.Graphics.DrawImage(label.Icon_Hot, 5, 5, 50, 50);
                if (((oRALabel)Controls[0]).Activated)
                {
                    e.Graphics.DrawString(label.Text, oRAFonts.Font_Title, new SolidBrush(label.Colour_Normal), new RectangleF(60, 30 - textSize.Height / 2, 140, textSize.Height), sF);
                    label.HasEllipsis = textSize.Width > 140;
                }
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(label.Colour_Normal), 0, 0, label.Width, label.Height);
                e.Graphics.DrawImage(label.Icon_Normal, 5, 5, 50, 50);
                if (((oRALabel)Controls[0]).Activated)
                {
                    e.Graphics.DrawString(label.Text, oRAFonts.Font_Title, new SolidBrush(label.Colour_Hot), new RectangleF(60, 30 - textSize.Height / 2, 140, textSize.Height), sF);
                    label.HasEllipsis = textSize.Width > 140;
                }
            }
        }

        public oRAPage[] GetPages()
        {
            return Pages.ToArray();
        }
    }
}
