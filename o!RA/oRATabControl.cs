using System;
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
            l.Location = new Point(1, TotalHeight);
            l.MouseDown += ChangeTab;
            l.Tag = Pages.Count - 1;
            Controls.Add(l);

            if (Pages.Count == 1)
            {
                TabContainer.Controls.Add((Control)Page.Contents);
                    l.BackColor = oRAColours.Colour_Item_Select;
            }
            TotalHeight += 60;
        }
        private void ChangeTab(object sender, EventArgs e)
        {
            if (((Label)sender).BackColor == oRAColours.Colour_Item_Select)
            {
                return;
            }
            TabContainer.Controls.Clear();
            TabContainer.Controls.Add((Control)pages[Convert.ToInt32(((Label)sender).Tag)].Contents);
            foreach (Label l in Controls)
            {
                l.BackColor = oRAColours.Colour_BG_P0;
            }
            ((Label)sender).BackColor = oRAColours.Colour_Item_Select;
        }

        public oRAPage[] GetPages()
        {
            return Pages.ToArray();
        }
    }
}
