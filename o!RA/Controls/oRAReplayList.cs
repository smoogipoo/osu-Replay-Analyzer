using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace o_RA.Controls
{
    class oRAReplayList : TreeView
    {
        private const int TVM_SETEXTENDEDSTYLE = 0x112C;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        private const int TVS_NOHSCROLL = 0x8000;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private readonly StringFormat format = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };

        public oRAReplayList()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawAll;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            //Little trick to force the control to double buffer
            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        protected override CreateParams CreateParams
        {
            //Disable the horizontal scroll bar
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= TVS_NOHSCROLL;
                return cp;
            }
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            SizeF textSize = e.Graphics.MeasureString(e.Node.Text, oRAFonts.Font_SubDescription);
            
            //Because the bounds of the replay list include vertical scrollbar bounds
            int actualWidth = Width - new VScrollBar().Width - 2;
            e.Graphics.FillRectangle(new SolidBrush(e.Node.IsSelected ? oRAColours.Colour_Item_BG_0 : oRAColours.Colour_BG_P0), new Rectangle(e.Bounds.Left, e.Bounds.Y, Width, e.Bounds.Height));
            e.Graphics.DrawString(e.Node.Text, oRAFonts.Font_SubDescription, new SolidBrush(e.Node.IsSelected ? oRAColours.Colour_Text_H : oRAColours.Colour_Text_N), new RectangleF(new PointF(e.Bounds.Left + 5, e.Bounds.Y + e.Bounds.Height / 2f - textSize.Height / 2), new SizeF(actualWidth - 5, textSize.Height)), format);
        }
    }
}
