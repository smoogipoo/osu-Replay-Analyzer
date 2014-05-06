using System.Drawing;
using System.Windows.Forms;
using oRAInterface;

namespace MapInfoPlugin
{
    public class oRA : IPlugin
    {
        #region Plugin Constants (Set these)
        public static string Name = "Beatmap Information";
        public static string Description = "Displays information about the selected beatmap.";
        public static string Author = "o!RA Developers";
        public static string Version = "1.0.0";
        public static string HomePage = "";

        public static ToolStripMenuItem PluginMenuItem;
        public static UserControl PluginTabItem = new MainForm();
        public static Bitmap PluginTabIcon_Normal = Properties.Resources.Icon_N;
        public static Bitmap PluginTabIcon_Hot = Properties.Resources.Icon_H;
        #endregion

        #region Exposed items
        public object Host { get; set; }

        public static DataClass Data;
        public static ControlsClass Controls;
        #endregion

        #region Exposed item interfaces
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
        public string p_HomePage
        {
            get { return HomePage; }
        }
        public ToolStripMenuItem p_PluginMenuItem
        {
            get { return PluginMenuItem; }
        }
        public UserControl p_PluginTabItem
        {
            get { return PluginTabItem; }
        }
        public Bitmap p_PluginTabIcon_N
        {
            get { return PluginTabIcon_Normal; }
        }
        public Bitmap p_PluginTabIcon_H
        {
            get { return PluginTabIcon_Hot; }
        }
        public DataClass p_Data
        {
            get { return Data; }
            set { Data = value; }
        }
        public ControlsClass p_Controls
        {
            get { return Controls; }
            set { Controls = value; }
        }
        #endregion

        public void Initialize()
        {
        }

        public void Dispose()
        {
            //Add your custom disposal here
        }
    }
}
