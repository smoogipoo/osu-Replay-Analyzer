using System.Collections.Generic;
using System.Windows.Forms;
using oRAInterface;
using ReplayAPI;
using BMAPI;

namespace CursorTracer
{
    public class Class1 : IPlugin
    {
        #region "Plugin Constants" (Set these)
        public static string Name = "Cursor Tracer";
        public static string Description = "Provides a cursor trace.";
        public static string Author = "smoogipooo";
        public static string Version = "1.0.0";
        public static string UpdateURL = "";
        #endregion

        #region "Exposed items"
        public IPluginHost Host { get; set; }

        public static ToolStripMenuItem MenuItem;
        public static UserControl TabItem = new mainFrm();

        public static string ReplayDirectory = "";
        public static string BeatmapDirectory = "";
        public static string ProgressToolTipText = "";
        public static double[] TimingWindows;
        public static Replay CurrentReplay;
        public static Beatmap CurrentBeatmap;
        public static Dictionary<string, string> BeatmapHashes;
        public static TabControl MainTabControl;
        public static ListBox FrameTimeline;
        public static ProgressBar Progress;
        #endregion
        #region "Exposed item interfaces"
        public string p_Name
        {
            get { return Name; }
        }
        public string p_Author
        {
            get { return Author; }
        }
        public string p_Description
        {
            get { return Description; }
        }
        public string p_Version
        {
            get { return Version; }
        }
        public string p_UpdateURL
        {
            get { return UpdateURL; }
        }
        public ToolStripMenuItem p_MenuItem
        {
            get { return MenuItem; }
        }
        public UserControl p_TabItem
        {
            get { return TabItem; }
        }
        public string p_ReplayDirectory
        {
            get { return ReplayDirectory; }
            set { ReplayDirectory = value; }
        }
        public string p_BeatmapDirectory
        {
            get { return BeatmapDirectory; }
            set { BeatmapDirectory = value; }
        }
        public string p_ProgressToolTipText
        {
            get { return ProgressToolTipText; }
            set { ProgressToolTipText = value; }
        }
        public double[] p_TimingWindows
        {
            get { return TimingWindows; }
            set { TimingWindows = value; }
        }
        public Replay p_CurrentReplay
        {
            get { return CurrentReplay; }
            set { CurrentReplay = value; }
        }
        public Beatmap p_CurrentBeatmap
        {
            get { return CurrentBeatmap; }
            set { CurrentBeatmap = value; }
        }
        public Dictionary<string, string> p_BeatmapHashes
        {
            get { return BeatmapHashes; }
            set { BeatmapHashes = value; }
        }
        public TabControl p_MainTabControl
        {
            get { return MainTabControl; }
            set { MainTabControl = value; }
        }
        public ListBox p_FrameTimeline
        {
            get { return FrameTimeline; }
            set { FrameTimeline = value; }
        }
        public ProgressBar p_Progress
        {
            get { return Progress; }
            set { Progress = value; }
        }
        #endregion     

        public void Initialize()
        {
            //Add your custom initialization procedure here
        }
        public void Dispose()
        {
            //Add your custom disposal here
        }
    }
}
