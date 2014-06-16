using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Linq;
using o_RA;

namespace MapInfoPlugin
{
    class CustomListView : ListView
    {
        public override sealed Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        protected override sealed bool DoubleBuffered
        {
            get { return base.DoubleBuffered; }
            set { base.DoubleBuffered = value; }
        }

        public CustomListView()
        {
            DoubleBuffered = true;
            BackColor = oRAColours.Colour_BG_P0;
            OwnerDraw = true;
            UseCompatibleStateImageBehavior = false;
            FullRowSelect = true;
            View = View.Details;
            AllowColumnReorder = false;
            GridLines = false;

            DrawColumnHeader += HandleDrawHeader;
            DrawSubItem += HandleDrawSubItem;
            Resize += HandleResize;
        }

        private static void HandleDrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), e.Bounds);
            e.Graphics.DrawLine(new Pen(oRAColours.Colour_BG_P1), e.Bounds.X + e.Bounds.Width - 1, e.Bounds.Y, e.Bounds.X + e.Bounds.Width - 1, e.Bounds.Height - 1);
            e.Graphics.DrawLine(new Pen(oRAColours.Colour_BG_P1), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Header.Text, oRAFonts.Font_Description, new SolidBrush(oRAColours.Colour_Text_H), e.Bounds.Left + e.Bounds.Width / 2 - e.Graphics.MeasureString(e.Header.Text, oRAFonts.Font_Description).Width / 2, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Header.Text, oRAFonts.Font_Description).Height / 2);
        }
        private void HandleDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ItemIndex == -1)
                return;
            e.Graphics.FillRectangle(new SolidBrush(e.ItemState.HasFlag(ListViewItemStates.Selected) ? oRAColours.Colour_Item_BG_0 : oRAColours.Colour_BG_P0), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            if (e.Item.Text == "")
                e.Graphics.DrawLine(new Pen(oRAColours.Colour_BG_P1), Columns[0].Width - 1, e.Bounds.Y, Columns[0].Width - 1, e.Bounds.Y + e.Bounds.Height);
            else
                e.Graphics.DrawLine(new Pen(oRAColours.Colour_BG_P1), e.Bounds.X - 1, e.Bounds.Y, e.Bounds.X - 1, e.Bounds.Y + e.Bounds.Height);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.SubItem.Text, oRAFonts.Font_SubDescription, e.ItemState.HasFlag(ListViewItemStates.Selected) ? new SolidBrush(oRAColours.Colour_Text_H) : new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + 22, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Item.Text, oRAFonts.Font_SubDescription).Height / 2);
        }

        private void HandleResize(object sender, EventArgs e)
        {
            if (Columns.Count == 0)
                return;
            int columnWidth = Columns.Cast<ColumnHeader>().Sum(cH => cH.Width);
            int widthDifference = Width - columnWidth - 1;

            if (widthDifference > 0 || widthDifference < 0)
            {
                int widthIncreaseAmnt = widthDifference / Columns.Count;
                for (int i = 0; i < Columns.Count; i++)
                {
                    Columns[i].Width += widthIncreaseAmnt;
                }
            }
        }
    }
}
