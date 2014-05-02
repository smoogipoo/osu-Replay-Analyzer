using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Linq;
using System.Drawing;
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

        public o_RA.Settings Settings { get; set; }
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
        public ToolTip ProgressToolTip { get; set; }
    }
}
namespace o_RA
{
    public class oRAFonts
    {
        public oRAFonts() { }

        public static Font Font_Title = new Font("Segoe UI", 12, FontStyle.Bold);
        public static Font Font_Description = new Font("Segoe UI", 10);
        public static Font Font_SubDescription = new Font("Segoe UI", 8);
    }
    public class oRAColours
    {
        public oRAColours() { }

        public static Color Colour_BG_Main = Color.FromArgb(255, 232, 232, 232);
        public static Color Colour_BG_P0 = Color.FromArgb(255, 43, 43, 43);
        public static Color Colour_BG_P1 = Color.FromArgb(255, 26, 26, 26);
        public static Color Colour_Text_N = Color.FromArgb(255, 109, 109, 109);
        public static Color Colour_Text_H = Color.FromArgb(255, 232, 232, 232);
        public static Color Colour_Highlight = Color.FromArgb(255, 255, 255, 255);
        public static Color Colour_Item_BG_0 = Color.FromArgb(255, 0, 120, 255);
        public static Color Colour_Item_BG_1 = Color.FromArgb(127, 0, 120, 255);
    }

    public class Settings
    {
        internal readonly Dictionary<string, string> s_settings = new Dictionary<string, string>();
        FileStream s_file;

        public Settings()
        {
            LoadSettings();
        }
        public void LoadSettings()
        {
            s_file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\settings.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (StreamReader sR = new StreamReader(s_file))
            {
                while (sR.Peek() != -1)
                {
                    string s = sR.ReadLine();
                    if (s != null)
                        s_settings.Add(s.Substring(0, s.IndexOf(":", StringComparison.Ordinal)), s.Substring(s.IndexOf(":", StringComparison.Ordinal) + 1));
                }
            }
            s_file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\settings.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
        public bool ContainsSetting(string name)
        {
            return s_settings.ContainsKey(name);
        }

        public List<string> GetKeys()
        {
            lock (this)
            {
                return s_settings.Keys.ToList();
            }
        }
        public void AddSetting(string name, string value, bool overwrite = true)
        {
            lock (this)
            {
                if ((s_settings.ContainsKey(name)) & (overwrite))
                {
                    s_settings[name] = value;
                }
                else if (s_settings.ContainsKey(name) == false)
                {
                    s_settings.Add(name, value);
                }
            }

        }
        public string GetSetting(string name)
        {
            lock (this)
            {
                return s_settings.ContainsKey(name) ? s_settings[name] : "";
            }
        }

        public void DeleteSetting(string name)
        {
            lock (this)
            {
                if (s_settings.ContainsKey(name))
                {
                    s_settings.Remove(name);
                }
            }

        }
        public void Save()
        {
            lock (this)
            {
                string constructedString = s_settings.Aggregate("", (str, di) => str + (di.Key + ":" + di.Value + Environment.NewLine));
                if (constructedString != "")
                {
                    constructedString = constructedString.Substring(0, constructedString.LastIndexOf(Environment.NewLine, StringComparison.Ordinal));
                }
                s_file.SetLength(constructedString.Length);
                s_file.Position = 0;
                byte[] bytesToWrite = System.Text.Encoding.ASCII.GetBytes(constructedString);
                s_file.Write(bytesToWrite, 0, bytesToWrite.Length);
                s_file.Flush();
            }

        }
    }
}