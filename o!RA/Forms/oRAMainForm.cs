using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using BMAPI;
using ErikEJ.SqlCe;
using Microsoft.Win32;
using oRAInterface;
using o_RA.Controls;
using o_RA.GlobalClasses;
using ReplayAPI;

namespace o_RA.Forms
{
    public partial class oRAMainForm : Form
    {
        public oRAMainForm()
        {
            InitializeComponent();
        }
        private XmlReader Locale;
        public static Settings Settings = new Settings();
        internal static Updater Updater = new Updater();
        internal static SqlCeConnection DBConnection = new SqlCeConnection(DBHelper.dbPath);

        private Replay Replay;
        private Beatmap Beatmap;
        private readonly Dictionary<string, string> Language = new Dictionary<string, string>();
        private static DataClass oRAData;
        private static ControlsClass oRAControls;

        public static readonly PluginServices Plugins = new PluginServices();

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeLocale();
            InitializePlugins();
            InitializeGameDirs();
            Task.Factory.StartNew(() => Updater.Start(Settings));
            Task.Factory.StartNew(PopulateReplays);
            Task.Factory.StartNew(UpdateBeatmaps);

            FileSystemWatcher replayWatcher = new FileSystemWatcher(oRAData.ReplayDirectory)
            {
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.osr",
            };
            FileSystemWatcher beatmapWatcher = new FileSystemWatcher(oRAData.BeatmapDirectory)
            {
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.osu",
                IncludeSubdirectories = true,
            };

            replayWatcher.Created += ReplayCreated;
            replayWatcher.Deleted += ReplayDeleted;
            replayWatcher.Renamed += ReplayRenamed;
            beatmapWatcher.Created += BeatmapCreated;

            replayWatcher.EnableRaisingEvents = true;
            beatmapWatcher.EnableRaisingEvents = true;
        }

        private void InitializeLocale()
        {
            if (!Settings.ContainsSetting("ApplicationLocale") || Settings.GetSetting("ApplicationLocale") == "")
            {
                LocaleSelectForm lsf = new LocaleSelectForm();
                lsf.ShowDialog();
            }
            try
            {
                Locale = XmlReader.Create(File.OpenRead(Application.StartupPath + "\\Locales\\" + Settings.GetSetting("ApplicationLocale") + ".xml"));
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(@"Selected locale does not exist. Application will now exit.");
                Environment.Exit(1);
            }
            while (Locale.Read())
            {
                string n = Locale.Name;
                Locale.Read();
                if (!Language.ContainsKey(n) && Locale.Value != "")
                    Language.Add(n, Locale.Value.Replace(@"\n", "\n").Replace(@"\t", "\t"));
            }
            if (Settings.GetSetting("ApplicationLocale") != "en")
            {
                XmlReader enLocale = XmlReader.Create(File.OpenRead(Application.StartupPath + "\\locales\\en.xml"));
                while (enLocale.Read())
                {
                    string n = enLocale.Name;
                    enLocale.Read();
                    if (!Language.ContainsKey(n))
                        Language.Add(n, enLocale.Value.Replace(@"\n", "\n").Replace(@"\t", "\t"));
                }
            }
        }

        private void InitializePlugins()
        {
            //Initialize plugin interface
            oRAData = new DataClass();
            oRAControls = new ControlsClass();
            oRAData.Language = Language;
            oRAData.Replays = new List<TreeNode>();
            oRAData.TimingWindows = new double[3];
            oRAData.ReplayObjects = new List<ReplayObject>();
            oRAControls.ProgressToolTip = new ToolTip();
            oRAControls.FrameTimeline = ReplayTimeline;
            oRAData.FrameChanged += HandleFrameChanged;

            //Load Plugins
            if (Directory.Exists(Environment.CurrentDirectory + @"\Plugins\"))
            {
                foreach (string pluginFile in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Plugins"), "*.dll", SearchOption.AllDirectories))
                {
                    if (!Settings.ContainsSetting("DisabledPlugins") || !Settings.GetSetting("DisabledPlugins").Split(new[] { '|' }).Contains(pluginFile))
                    {
                        Plugin p = Plugins.LoadPlugin(pluginFile);
                        if (p == null)
                            continue;
                        p.Instance.p_Data = oRAData;
                        p.Instance.p_Controls = oRAControls;
                        if (p.Instance.p_PluginTabItem != null)
                        {
                            p.Instance.p_PluginTabItem.Dock = DockStyle.Fill;
                            oRAPage page = new oRAPage
                            {
                                Description = p.Instance.p_Description,
                                Name = p.Instance.p_Name,
                                Contents = p.Instance.p_PluginTabItem,
                                Icon_Hot = p.Instance.p_PluginTabIcon_H,
                                Icon_Normal = p.Instance.p_PluginTabIcon_N,
                            };
                            MainContainer.TabPages.Add(page);
                        }
                        if (p.Instance.p_PluginMenuItem != null)
                        {
                            PluginsMenuItem.DropDownItems.Add(p.Instance.p_PluginMenuItem);
                        }
                    }
                }
            }
        }

        private void InitializeGameDirs()
        {
            if (!IsOsuPath(Settings.GetSetting("GameDir")))
            {
                //Try get the osu! path from processes
                Process[] procs = Process.GetProcessesByName("osu!");
                if (procs.Length != 0)
                {
                    string gameDir = Path.GetDirectoryName(procs[0].Modules[0].FileName);
                    if (IsOsuPath(gameDir))
                    {
                        Settings.AddSetting("GameDir", gameDir);
                        Settings.Save();
                    }
                    else
                    {
                        MessageBox.Show(Language["info_osuWrongDir"], Language["info_osuClosedMessageBoxTitle"]);
                    }
                }
                else
                {
                    //Try to get osu! path from registry
                    try
                    {
                        RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu!\\DefaultIcon");
                        if (key != null)
                        {
                            object o = key.GetValue(null);
                            if (o != null)
                            {
                                var filter = new Regex(@"(?<="")[^\""]*(?="")");
                                string path = Path.GetDirectoryName(filter.Match(o.ToString()).ToString());
                                if (IsOsuPath(path))
                                {
                                    if (MessageBox.Show(@"Found osu! directory at: " + path + '\n' + @"Is this correct?", @"osu! Directory", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        Settings.AddSetting("GameDir", Path.GetDirectoryName(filter.Match(o.ToString()).ToString()));
                                    else
                                        throw new Exception();
                                }
                                else
                                    throw new Exception();
                            }
                        }
                    }
                    catch
                    {
                        //Get the user to select osu! path
                        if (MessageBox.Show(Language["info_osuClosed"], Language["info_osuClosedMessageBoxTitle"]) == DialogResult.OK)
                        {
                            using (FolderBrowserDialog fd = new FolderBrowserDialog())
                            {
                                while (!IsOsuPath(fd.SelectedPath))
                                {
                                    if (fd.ShowDialog() == DialogResult.OK)
                                    {
                                        if (IsOsuPath(fd.SelectedPath))
                                        {
                                            Settings.AddSetting("GameDir", fd.SelectedPath);
                                            Settings.Save();
                                        }
                                        else
                                        {
                                            MessageBox.Show(Language["info_osuWrongDir"], Language["info_osuClosedMessageBoxTitle"]);
                                        }
                                    }
                                    else
                                    {
                                        Environment.Exit(0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            oRAData.ReplayDirectory = Path.Combine(Settings.GetSetting("GameDir"), "Replays");
            oRAData.BeatmapDirectory = Path.Combine(Settings.GetSetting("GameDir"), "Songs");
        }

        private bool IsOsuPath(string gameDir)
        {
            return Directory.Exists(Path.Combine(gameDir, "Replays")) && Directory.Exists(Path.Combine(gameDir, "Songs"));
        }

        /// <summary>
        /// Populates the replays listbox
        /// </summary>
        private void PopulateReplays()
        {
            DirectoryInfo info = new DirectoryInfo(oRAData.ReplayDirectory);
            FileInfo[] files = info.GetFiles().Where(f => f.Extension == ".osr").OrderBy(f => f.CreationTime).Reverse().ToArray();

            //Add replays
            foreach (FileInfo file in files)
            {
                oRAData.Replays.Add(new TreeNode(file.Name));
            }
            ReplaysList.BeginInvoke((Action)(() => ReplaysList.Nodes.AddRange(oRAData.Replays.ToArray())));


            ReplaysList.BeginInvoke((Action)(() =>
            {
                if (oRAData.Replays.Count > 0 && ReplaysList.SelectedNode == null)
                {
                    ReplaysList.SelectedNode = ReplaysList.Nodes[0];
                    ReplaysList.Select();
                }
            }));

        }
        /// <summary>
        /// Updates a beatmap record if it exists, otherwise inserts it
        /// </summary>
        private void UpdateBeatmaps()
        {
            oRAControls.ProgressToolTip.Tag = Language["info_PopBeatmaps"];


            DataTable beatmapData = DBHelper.CreateBeatmapDataTable();
            string[] beatmapFiles = Directory.GetFiles(oRAData.BeatmapDirectory, "*.osu", SearchOption.AllDirectories);

            Progress.BeginInvoke((Action)(() => Progress.Maximum = beatmapFiles.Length));

            using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
            {
                conn.Open();
                using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn))
                {
                    foreach (string file in beatmapFiles)
                    {
                        string beatmapHash = MD5FromFile(file);
                        if (!DBHelper.RecordExists(conn, "Beatmaps", "Hash", beatmapHash) && beatmapData.AsEnumerable().All(row => (beatmapHash != row.Field<string>("Hash"))))
                        {
                            beatmapData.Rows.Add(beatmapHash, file);
                            Progress.BeginInvoke((Action)(() => Progress.Value += 1));

                            //Free memory by pushing the rows
                            if (beatmapData.Rows.Count >= 2000)
                            {
                                DBHelper.Insert(bC, beatmapData);
                                beatmapData.Clear();
                            }
                        }
                    }
                    //Flush any remaining data
                    DBHelper.Insert(bC, beatmapData);
                    beatmapData.Clear();
                }
            }
            Progress.BeginInvoke((Action)(() => Progress.Value = 0));
            oRAControls.ProgressToolTip.Tag = Language["info_OperationsCompleted"];
        }
        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((Action)(() =>
            {
                ReplaysList.Nodes.Insert(0, e.Name);
                ReplaysList.SelectedNode = ReplaysList.Nodes[0];
            }));
        }
        private void ReplayDeleted(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((Action)(() => ReplaysList.Nodes.RemoveByKey(e.Name)));
        }
        private void ReplayRenamed(object sender, RenamedEventArgs e)
        {
            ReplaysList.BeginInvoke((Action)(() =>
            {
                ReplaysList.Nodes.RemoveByKey(e.OldName);
                ReplaysList.Nodes.Insert(0, e.Name);
            }));
        }

        private static void BeatmapCreated(object sender, FileSystemEventArgs e)
        {
            SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
            options |= SqlCeBulkCopyOptions.KeepNulls;
            using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
            {
                conn.Open();
                using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn, options))
                {
                    string beatmapHash = MD5FromFile(e.FullPath);
                    if (!DBHelper.RecordExists(conn, "Beatmaps", "Hash", beatmapHash))
                    {
                        DataTable dT = DBHelper.CreateBeatmapDataTable();
                        dT.Rows.Add(beatmapHash, e.FullPath);
                        DBHelper.Insert(bC, dT);
                    }
                }
            }
        }

        private static string MD5FromFile(string fileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        private void ReplaysList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            using (Replay = new Replay(Path.Combine(oRAData.ReplayDirectory, e.Node.Text)))
            {
                DataRow dR = DBHelper.GetRecord(DBConnection, "Beatmaps", "Hash", Replay.MapHash);
                if (dR != null)
                {
                    Beatmap = new Beatmap(dR["Filename"].ToString());

                    /* Start Timing Windows tab */

                    //Determine the timing windows for 300,100,50
                    //First modify the beatmap attributes according by player mods
                    if ((Replay.Mods & Modifications.HardRock) == Modifications.HardRock)
                    {
                        Beatmap.OverallDifficulty = Math.Min(Beatmap.OverallDifficulty *= 1.4, 10);
                        Beatmap.CircleSize = (int)(Beatmap.CircleSize * 1.4);
                        foreach (BaseCircle hitObject in Beatmap.HitObjects)
                        {
                            Beatmap.HitObjects[Beatmap.HitObjects.IndexOf(hitObject)].Radius = 40 - 4 * (Beatmap.CircleSize - 2);
                        }
                        Beatmap.CircleSize = Beatmap.CircleSize * 1.4;
                    }
                    if ((Replay.Mods & Modifications.DoubleTime) == Modifications.DoubleTime)
                    {
                        Beatmap.OverallDifficulty = Math.Min(13.0 / 3.0 + (2.0 / 3.0) * Beatmap.OverallDifficulty, 11);
                        Beatmap.ApproachRate = Math.Min(13.0 / 3.0 + (2.0 / 3.0) * Beatmap.ApproachRate, 11);
                    }
                    if ((Replay.Mods & Modifications.HalfTime) == Modifications.HalfTime)
                    {
                        Beatmap.OverallDifficulty = (3.0 / 2.0) * Beatmap.OverallDifficulty - 13.0 / 2.0;
                        Beatmap.ApproachRate = (3.0 / 2.0) * Beatmap.ApproachRate - 13.0 / 2.0;
                    }
                    if ((Replay.Mods & Modifications.Easy) == Modifications.Easy)
                    {
                        Beatmap.OverallDifficulty = Beatmap.OverallDifficulty / 2;
                    }

                    //Timing windows are determined by linear interpolation
                    for (int i = 2; i >= 0; i--)
                    {
                        oRAData.TimingWindows[i] = Beatmap.OverallDifficulty < 5 ? (200 - 60 * i) + (Beatmap.OverallDifficulty) * ((150 - 50 * i) - (200 - 60 * i)) / 5 : (150 - 50 * i) + (Beatmap.OverallDifficulty - 5) * ((100 - 40 * i) - (150 - 50 * i)) / 5;
                    }

                    oRAData.ReplayObjects.Clear();

                    if (Replay.ReplayFrames.Count == 0)
                        return;

                    //Match up beatmap objects to replay clicks
                    List<ReplayInfo> iteratedObjects = new List<ReplayInfo>();
                    for (int i = 0; i < Beatmap.HitObjects.Count; i++)
                    {
                        //Todo: Consider if hitobject containspoint
                        ReplayInfo c = Replay.ClickFrames.Find(click => (Math.Abs(click.Time - Beatmap.HitObjects[i].StartTime) < oRAData.TimingWindows[2]) && !iteratedObjects.Contains(click)) ??
                                        Replay.ClickFrames.Find(click => (Math.Abs(click.Time - Beatmap.HitObjects[i].StartTime) < oRAData.TimingWindows[1]) && !iteratedObjects.Contains(click)) ??
                                        Replay.ClickFrames.Find(click => (Math.Abs(click.Time - Beatmap.HitObjects[i].StartTime) < oRAData.TimingWindows[0]) && !iteratedObjects.Contains(click));
                        if (c != null)
                        {
                            iteratedObjects.Add(c);
                            oRAData.ReplayObjects.Add(new ReplayObject { Frame = c, Object = Beatmap.HitObjects[i] });
                        }
                    }

                    ReplayTimeline.DataSource = iteratedObjects;
                    if (ReplayTimeline.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn c in ReplayTimeline.Columns)
                        {
                            c.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }
                    }
                    if (ReplayTimeline.Rows.Count > 0)
                        ReplayTimeline.Rows[0].Selected = true;

                    oRAData.UpdateStatus(Replay, Beatmap);
                }
            }
        }
        private void ReplayTimeline_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected) return;
            if (e.Row.Index != -1 && e.Row.Index != oRAData.CurrentFrame)
            {
                oRAData.ChangeFrame(e.Row.Index);
            }
        }
        private void HandleFrameChanged(int index)
        {
            if (index > ReplayTimeline.Rows.Count - 1)
                return;
            ReplayTimeline.CurrentCell = ReplayTimeline.Rows[index].Cells[0];
        }
        private void Progress_MouseEnter(object sender, EventArgs e)
        {
            oRAControls.ProgressToolTip.Show((string)oRAControls.ProgressToolTip.Tag, Progress, 0);
        }

        private void Progress_MouseLeave(object sender, EventArgs e)
        {
            oRAControls.ProgressToolTip.Hide(Progress);
        }

        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PluginSettingsForm psf = new PluginSettingsForm();
            psf.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ReplaysList_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Index == -1)
                return;
            e.Graphics.FillRectangle(new SolidBrush(e.State.HasFlag(TreeNodeStates.Selected) ? oRAColours.Colour_Item_BG_0 : oRAColours.Colour_BG_P0), e.Bounds);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Node.Text, oRAFonts.Font_SubDescription, e.State.HasFlag(TreeNodeStates.Selected) ? new SolidBrush(oRAColours.Colour_Text_H) : new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + 22, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Node.Text, oRAFonts.Font_SubDescription).Height / 2);
        }


        private void ToolStripMenuItem_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), e.ClipRectangle);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (((ToolStripMenuItem)sender).Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_1), e.ClipRectangle);
                e.Graphics.DrawRectangle(new Pen(oRAColours.Colour_Item_BG_0), new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1));
                SizeF stringSize = e.Graphics.MeasureString(((ToolStripMenuItem)sender).Text, oRAFonts.Font_SubDescription);
                e.Graphics.DrawString(((ToolStripMenuItem)sender).Text, oRAFonts.Font_SubDescription, new SolidBrush(oRAColours.Colour_Text_H), new PointF(e.ClipRectangle.X + e.ClipRectangle.Width / 2 - stringSize.Width / 2, e.ClipRectangle.Y + e.ClipRectangle.Height / 2 - stringSize.Height / 2));
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P1), e.ClipRectangle);
                SizeF stringSize = e.Graphics.MeasureString(((ToolStripMenuItem)sender).Text, oRAFonts.Font_SubDescription);
                e.Graphics.DrawString(((ToolStripMenuItem)sender).Text, oRAFonts.Font_SubDescription, new SolidBrush(oRAColours.Colour_Text_N), new PointF(e.ClipRectangle.X + e.ClipRectangle.Width / 2 - stringSize.Width / 2, e.ClipRectangle.Y + e.ClipRectangle.Height / 2 - stringSize.Height / 2));
            }
        }

        private void oRAMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Plugin pS in Plugins.PluginCollection.ToArray())
            {
                Plugins.UnloadPlugin(pS.AssemblyFile);
            }
            Application.ExitThread();
        }


    }
}
