using System.Drawing;
using System.Windows.Forms;

namespace o_RA.oRAControls
{
    public class oRALabel : Label
    {
        public Bitmap Icon_Normal { get; set; }
        public Bitmap Icon_Hot { get; set; }
        public Color Colour_Normal = oRAColours.Colour_BG_P0;
        public Color Colour_Hot = oRAColours.Colour_Item_BG_0;
        public int Index = -1;
        public bool Activated;
        public bool HasEllipsis;
    }
}
