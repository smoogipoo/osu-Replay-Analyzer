using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using BMAPI;
using ReplayAPI;

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

        DataClass p_Data { get; set; }
        ControlsClass p_Controls { get; set; }

        void Initialize();
        void Dispose();
    }

    public class DataClass
    {
        public delegate void ReplayChangedHandler(Replay r, Beatmap b);
        public event ReplayChangedHandler ReplayChanged;
        
        public void UpdateStatus(Replay Replay, Beatmap Beatmap)
        {
            if (ReplayChanged != null)
                ReplayChanged(Replay, Beatmap);
        }

        public Replay CurrentReplay { get; set; }
        public Beatmap CurrentBeatmap { get; set; }
        public Dictionary<string, string> BeatmapHashes { get; set; }
        public List<TreeNode> Replays { get; set; }
        public string ReplayDirectory { get; set; }
        public string BeatmapDirectory { get; set; }
        public double[] TimingWindows { get; set; }
    }

    public class ControlsClass
    {
        public ListBox FrameTimeline { get; set; }
        public ProgressBar Progress { get; set; }
        public ToolTip ProgressToolTip { get; set; }
    }
}
