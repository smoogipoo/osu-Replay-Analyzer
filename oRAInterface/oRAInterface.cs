using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using ReplayAPI;
using BMAPI;

namespace oRAInterface
{
    public interface IPlugin
    {
        object Host { get; set; }

        string p_Name { get; }
        string p_Author { get; }
        string p_Description { get; }
        string p_Version { get; }
        string p_HomePage { get; }

        ToolStripMenuItem p_PluginMenuItem { get; }
        UserControl p_PluginTabItem { get; }
        Bitmap p_PluginTabIcon_N { get; }
        Bitmap p_PluginTabIcon_H { get; }

        DataClass p_Data { get; set; }
        ControlsClass p_Controls { get; set; }

        void Initialize();
        void Dispose();
    }

    public class DataClass
    {
        public delegate void ReplayChangedHandler(Replay r, Beatmap b);
        public delegate void FrameChangedHandler(int FrameIndex);
        public event ReplayChangedHandler ReplayChanged;
        public event FrameChangedHandler FrameChanged;
        
        public void UpdateStatus(Replay Replay, Beatmap Beatmap)
        {
            if (ReplayChanged != null)
                ReplayChanged(Replay, Beatmap);
        }
        public void ChangeFrame(int FrameIndex)
        {
            if (FrameChanged != null)
                FrameChanged(FrameIndex);
            CurrentFrame = FrameIndex;
        }

        public Dictionary<string, string> Language { get; set; }
        public ConcurrentDictionary<string, string> BeatmapHashes { get; set; }
        public List<TreeNode> Replays { get; set; }
        public List<ReplayObject> ReplayObjects { get; set; }
        public string ReplayDirectory { get; set; }
        public string BeatmapDirectory { get; set; }
        public double[] TimingWindows { get; set; }
        public int CurrentFrame { get; set; }
    }

    public class ControlsClass
    {
        public DataGridView FrameTimeline { get; set; }
        public ToolTip ProgressToolTip { get; set; }
    }

    public struct ReplayObject
    {
        public ReplayInfo Frame { get; set; }
        public BaseCircle Object { get; set; }
    }
}
namespace o_RA
{
    public class oRAFonts
    {
        public static Font Font_Title = new Font("Segoe UI", 10, FontStyle.Bold);
        public static Font Font_Description = new Font("Segoe UI", 9);
        public static Font Font_SubDescription = new Font("Segoe UI", 8);
    }
    public class oRAColours
    {
        public static Color Colour_BG_Main = Color.FromArgb(255, 232, 232, 232);
        public static Color Colour_BG_P0 = Color.FromArgb(255, 43, 43, 43);
        public static Color Colour_BG_P1 = Color.FromArgb(255, 26, 26, 26);
        public static Color Colour_Text_N = Color.FromArgb(255, 109, 109, 109);
        public static Color Colour_Text_H = Color.FromArgb(255, 232, 232, 232);
        public static Color Colour_Highlight = Color.FromArgb(255, 255, 255, 255);
        public static Color Colour_Item_BG_0 = Color.FromArgb(255, 0, 120, 255);
        public static Color Colour_Item_BG_1 = Color.FromArgb(127, 0, 120, 255);
    }
}