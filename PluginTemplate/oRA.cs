﻿using System.Windows.Forms;
using oRAInterface;

namespace PluginTemplate
{
    public class oRA : IPlugin
    {
        #region Plugin Constants (Set these)
        public static string Name = "My first o!RA Plugin";
        public static string Description = "My plugin is beautiful.";
        public static string Author = "Me!";
        public static string Version = "1.0.0";
        public static string HomePage = "";
        #endregion

        #region Exposed items
        public object Host { get; set; }

        public static ToolStripMenuItem PluginMenuItem;
        public static UserControl PluginTabItem;
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
            //Add your custom initialization procedure here
        }
        public void Dispose()
        {
            //Add your custom disposal here
        }
    }
}
