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
        internal static Settings Settings = new Settings();
        internal static Updater Updater = new Updater();
        internal static SqlCeConnection DBConnection = new SqlCeConnection(DBHelper.dbPath);
        internal Replay CurrentReplay;
        internal Beatmap CurrentBeatmap;
        internal XmlReader Locale;

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
                    Settings.AddSetting("GameDir", procs[0].Modules[0].FileName);
                    Settings.Save();
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
                                    string langString = Language["info_FoundosuDirectory"];
                                    if (MessageBox.Show(langString.Substring(0, langString.IndexOf('|')) + @": " + path + '\n' + langString.Substring(langString.IndexOf('|') + 1), @"o!RA", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                        using (FolderBrowserDialog fd = new FolderBrowserDialog())
                        {
                            fd.Description = Language["info_osuDirectory"];
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
                                        MessageBox.Show(Language["info_osuWrongDir"], @"o!RA");
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
            FileInfo[] files = new DirectoryInfo(oRAData.ReplayDirectory).GetFiles().Where(f => f.Extension == ".osr").OrderBy(f => f.CreationTime).Reverse().ToArray();

            //Add replays
            ReplaysList.BeginInvoke((Action)(() => ReplaysList.Nodes.AddRange(files.Select(f => new TreeNode{ Text = f.Name, Name = f.FullName }).ToArray())));
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
                
                //Get the hashes that are currently in the database
                DataTable existingRows = DBHelper.GetRecords(conn, "Beatmaps", "*");

                //Hashset performance >>>>> List performance
                HashSet<string> existingHashes = new HashSet<string>(existingRows.AsEnumerable().Select(row => row.Field<string>("Hash")));
                HashSet<string> existingFiles = new HashSet<string>(existingRows.AsEnumerable().Select(row => row.Field<string>("Filename")));

                using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn))
                {
                    foreach (string file in beatmapFiles)
                    {
                        string beatmapHash = MD5FromFile(file);
                        //Check if hash exists in database
                        if (!existingHashes.Contains(beatmapHash))
                        {
                            if (existingFiles.Contains(file))
                            {
                                //Remove the old file from the database
                                DBHelper.DeleteRecords(conn, "Beatmaps", "Filename", file);
                            }

                            //Add the new file
                            beatmapData.Rows.Add(beatmapHash, file);
                            existingHashes.Add(beatmapHash);

                            //Increment the progressbar
                            Progress.BeginInvoke((Action)(() => Progress.Value += 1));

                            //Free memory by pushing the rows
                            //We don't want to set this too low or we spend more time
                            //pushing to the database. But we don't want to set it too
                            //high or user won't have his beatmaps for a long time
                            if (beatmapData.Rows.Count >= 3000)
                            {
                                DBHelper.Insert(bC, beatmapData);
                                beatmapData.Clear();
                            }
                        }
                    }
                    //Flush any remaining data
                    DBHelper.Insert(bC, beatmapData);
                    beatmapData.Clear();
                    existingHashes.Clear(); //Final cleanup

                    //Set the first replay in the replayslist as the current replay
                    ReplaysList.BeginInvoke((Action)(() =>
                    {
                        if (ReplaysList.Nodes.Count > 0 && ReplaysList.SelectedNode == null)
                            ReplaysList.SelectedNode = ReplaysList.Nodes[0];
                    }));

                }
            }
            Progress.BeginInvoke((Action)(() => Progress.Value = 0));
            oRAControls.ProgressToolTip.Tag = Language["info_OperationsCompleted"];
        }

        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((Action)(() =>
            {
                ReplaysList.Nodes.Insert(0, new TreeNode { Text = e.Name, Name = e.FullPath });
                ReplaysList.SelectedNode = ReplaysList.Nodes[0];
            }));
        }
        private void ReplayDeleted(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((Action)(() => ReplaysList.Nodes.RemoveByKey(e.FullPath)));
        }
        private void ReplayRenamed(object sender, RenamedEventArgs e)
        {
            int index = ReplaysList.Nodes.IndexOfKey(e.OldFullPath);
            ReplaysList.BeginInvoke((Action)(() =>
            {
                ReplaysList.Nodes[index].Text = e.Name;
                ReplaysList.Nodes[index].Name = e.FullPath;
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
                        //Remove old record if it exists
                        if (DBHelper.RecordExists(conn, "Beatmaps", "Filename", e.FullPath))
                            DBHelper.DeleteRecords(conn, "Beatmaps", "Filename", e.FullPath);
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
            //Failsafe
            if (!File.Exists(Path.Combine(oRAData.ReplayDirectory, e.Node.Text)))
            {
                MessageBox.Show(Language["info_ReplayInexistent"] + '\n' + e.Node.Name);
                return;
            }
            
            using (CurrentReplay = new Replay(Path.Combine(oRAData.ReplayDirectory, e.Node.Text)))
            {
                DataRow dR = DBHelper.GetRecord(DBConnection, "Beatmaps", "Hash", CurrentReplay.MapHash);
                if (dR != null)
                {
                    CurrentBeatmap = new Beatmap(dR["Filename"].ToString());

                    /* Start Timing Windows tab */

                    //Determine the timing windows for 300,100,50
                    //First modify the beatmap attributes according by player mods
                    if ((CurrentReplay.Mods & Modifications.HardRock) == Modifications.HardRock)
                    {
                        CurrentBeatmap.OverallDifficulty = Math.Min(CurrentBeatmap.OverallDifficulty *= 1.4, 10);
                        CurrentBeatmap.CircleSize = CurrentBeatmap.CircleSize * 1.4;
                    }
                    if ((CurrentReplay.Mods & Modifications.DoubleTime) == Modifications.DoubleTime)
                    {
                        CurrentBeatmap.OverallDifficulty = Math.Min(13.0 / 3.0 + (2.0 / 3.0) * CurrentBeatmap.OverallDifficulty, 11);
                        CurrentBeatmap.ApproachRate = Math.Min(13.0 / 3.0 + (2.0 / 3.0) * CurrentBeatmap.ApproachRate, 11);
                    }
                    if ((CurrentReplay.Mods & Modifications.HalfTime) == Modifications.HalfTime)
                    {
                        CurrentBeatmap.OverallDifficulty = (3.0 / 2.0) * CurrentBeatmap.OverallDifficulty - 13.0 / 2.0;
                        CurrentBeatmap.ApproachRate = (3.0 / 2.0) * CurrentBeatmap.ApproachRate - 13.0 / 2.0;
                    }
                    if ((CurrentReplay.Mods & Modifications.Easy) == Modifications.Easy)
                    {
                        CurrentBeatmap.OverallDifficulty = CurrentBeatmap.OverallDifficulty / 2;
                    }

                    //Timing windows are determined by linear interpolation
                    for (int i = 2; i >= 0; i--)
                    {
                        oRAData.TimingWindows[i] = CurrentBeatmap.OverallDifficulty < 5 ? (200 - 60 * i) + (CurrentBeatmap.OverallDifficulty) * ((150 - 50 * i) - (200 - 60 * i)) / 5 : (150 - 50 * i) + (CurrentBeatmap.OverallDifficulty - 5) * ((100 - 40 * i) - (150 - 50 * i)) / 5;
                    }

                    oRAData.ReplayObjects.Clear();

                    if (CurrentReplay.ReplayFrames.Count == 0)
                        return;

                    //Match up beatmap objects to replay clicks
                    HashSet<ReplayInfo> iteratedObjects = new HashSet<ReplayInfo>();
                    for (int i = 0; i < CurrentBeatmap.HitObjects.Count; i++)
                    {
                        //Todo: Consider if hitobject containspoint
                        ReplayInfo c = CurrentReplay.ClickFrames.Find(click => (Math.Abs(click.Time - CurrentBeatmap.HitObjects[i].StartTime) < oRAData.TimingWindows[2]) && !iteratedObjects.Contains(click)) ??
                                        CurrentReplay.ClickFrames.Find(click => (Math.Abs(click.Time - CurrentBeatmap.HitObjects[i].StartTime) < oRAData.TimingWindows[1]) && !iteratedObjects.Contains(click)) ??
                                        CurrentReplay.ClickFrames.Find(click => (Math.Abs(click.Time - CurrentBeatmap.HitObjects[i].StartTime) < oRAData.TimingWindows[0]) && !iteratedObjects.Contains(click));
                        if (c != null)
                        {
                            iteratedObjects.Add(c);
                            oRAData.ReplayObjects.Add(new ReplayObject { Frame = c, Object = CurrentBeatmap.HitObjects[i] });
                        }
                    }

                    ReplayTimeline.DataSource = iteratedObjects.ToList();
                    if (ReplayTimeline.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn c in ReplayTimeline.Columns)
                        {
                            c.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }
                    }
                    if (ReplayTimeline.Rows.Count > 0)
                        ReplayTimeline.Rows[0].Selected = true;

                    oRAData.UpdateStatus(CurrentReplay, CurrentBeatmap);
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
