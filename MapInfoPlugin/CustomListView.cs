using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MapInfoPlugin
{
    class CustomListView : ListView
    {
        public CustomListView()
        {
            DoubleBuffered = true;
            OwnerDraw = true;

            DrawColumnHeader += HandleDrawHeader;
        }

        private static void HandleDrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(o_RA.oRAColours.Colour_BG_Main), e.Bounds);
            e.Graphics.FillRectangle(new SolidBrush(o_RA.oRAColours.Colour_BG_P0), new Rectangle(e.Bounds.X + e.Bounds.Width - 1, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Header.Text, o_RA.oRAFonts.Font_Description, new SolidBrush(o_RA.oRAColours.Colour_Text_N), e.Bounds.Left + e.Bounds.Width / 2 - e.Graphics.MeasureString(e.Header.Text, o_RA.oRAFonts.Font_Description).Width / 2, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Header.Text, o_RA.oRAFonts.Font_Description).Height / 2);

        }
    }
}
