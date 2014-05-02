using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Windows.Forms.DataVisualization.Charting;
using ReplayAPI;
using BMAPI;
using System.Xml;
using oRAInterface;

namespace o_RA
{
    public partial class oRAMainForm : Form
    {
        public oRAMainForm()
        {
            InitializeComponent();
        }
        private XmlReader Locale;
        public static Settings Settings = new Settings();


        Replay Replay;
        Beatmap Beatmap;
        private readonly Dictionary<string, string> Language = new Dictionary<string, string>();
        static DataClass oRAData;
        static ControlsClass oRAControls;

        public static readonly PluginServices Plugins = new PluginServices();
        private readonly Bitmap TimelineFrameImg = new Bitmap(18, 18);
        internal Chart TWChart = new Chart();
        internal Chart SRPMChart = new Chart();
        internal oRAListView ReplayInfoLV = new oRAListView();
        internal oRAListView MapInfoLV = new oRAListView();

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeLocale();
            InitializeControls();
            InitializePlugins();

            ReplayTimelineLB.ItemHeight = 20;


            Process[] procs = Process.GetProcessesByName("osu!");
            if (procs.Length != 0)
            {
                string gameDir = procs[0].Modules[0].FileName.Substring(0,procs[0].Modules[0].FileName.LastIndexOf("\\", StringComparison.Ordinal));
                oRAData.ReplayDirectory = gameDir + "\\Replays";
                oRAData.BeatmapDirectory = gameDir + "\\Songs";
            }
            else
            {

                if (MessageBox.Show(Language["info_osuClosed"], Language["info_osuClosedMessageBoxTitle"]) == DialogResult.OK)
                {
                   using (FolderBrowserDialog fd = new FolderBrowserDialog())
                   {
                       while (oRAData.ReplayDirectory == null)
                       {
                           if (fd.ShowDialog() == DialogResult.OK)
                           {
                               if (Directory.Exists(fd.SelectedPath + "\\Replays") && Directory.Exists(fd.SelectedPath + "\\Songs"))
                               {
                                   oRAData.ReplayDirectory = fd.SelectedPath + "\\Replays";
                                   oRAData.BeatmapDirectory = fd.SelectedPath + "\\Songs";
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
            oRAControls.ProgressToolTip.Tag = Language["info_PopReplays"];

            Thread populateListsThread = new Thread(PopulateLists);
            populateListsThread.IsBackground = true;
            populateListsThread.Start();

            FileSystemWatcher replayWatcher = new FileSystemWatcher(oRAData.ReplayDirectory);
            replayWatcher.NotifyFilter = NotifyFilters.FileName;
            replayWatcher.Filter = "*.osr";
            FileSystemWatcher beatmapWatcher = new FileSystemWatcher(oRAData.BeatmapDirectory);
            replayWatcher.NotifyFilter = NotifyFilters.FileName;
            beatmapWatcher.Filter = "*.osu";
            beatmapWatcher.IncludeSubdirectories = true;

            replayWatcher.Created += ReplayCreated;
            replayWatcher.Deleted += ReplayDeleted;
            replayWatcher.Renamed += ReplayRenamed;
            beatmapWatcher.Created += BeatmapCreated;
            beatmapWatcher.Deleted += BeatmapDeleted;
            beatmapWatcher.Renamed += BeatmapRenamed;

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
                if (!Language.ContainsKey(n))
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

        private void InitializeControls()
        {
            //Timing Windows Chart
            ChartArea chartArea1 = new ChartArea();
            ChartArea chartArea2 = new ChartArea();
            Legend legend1 = new Legend();
            Legend legend2 = new Legend();
            Series series1 = new Series();
            Series series2 = new Series();
            oRAPage tabPage1 = new oRAPage();
            oRAPage tabPage2 = new oRAPage();
            oRAPage tabPage3 = new oRAPage();
            oRAPage tabPage4 = new oRAPage();
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorGrid.LineColor = oRAColours.Colour_BG_P0;
            chartArea1.BackColor = oRAColours.Colour_BG_Main;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            legend1.Alignment = StringAlignment.Center;
            legend1.Docking = Docking.Bottom;
            legend1.Font = oRAFonts.Font_Description;
            legend1.IsTextAutoFit = false;
            legend1.BackColor = oRAColours.Colour_BG_Main;
            legend1.ForeColor = oRAColours.Colour_BG_P1;
            legend1.Name = "Legend1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = Language["text_TimingWindow"];
            series2.ChartArea = "ChartArea1";
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Caret";
            series2.XValueType = ChartValueType.Int32;
            TWChart.BackColor = oRAColours.Colour_BG_Main;
            TWChart.ChartAreas.Add(chartArea1);
            TWChart.Dock = DockStyle.Fill;
            TWChart.Legends.Add(legend1);
            TWChart.Name = "TWChart";
            TWChart.Palette = ChartColorPalette.None;
            TWChart.Series.Add(series1);
            TWChart.Series.Add(series2);
            TWChart.TabIndex = 9;
            TWChart.Text = @"Timing Windows Chart";
            TWChart.MouseDown += TWChart_MouseDown;
            TWChart.MouseMove += TWChart_MouseMove;
            tabPage1.Name = Language["tab_TimingWindows"];
            tabPage1.Description = "";
            tabPage1.Contents = TWChart;
            //Spinner RPM Chart
            chartArea2.BackColor = oRAColours.Colour_BG_Main;
            chartArea2.CursorX.IsUserSelectionEnabled = true;
            chartArea2.Name = "ChartArea2";
            legend2.Alignment = StringAlignment.Center;
            legend2.Docking = Docking.Bottom;
            legend2.Font = oRAFonts.Font_Description;
            legend2.IsTextAutoFit = false;
            legend2.BackColor = oRAColours.Colour_BG_Main;
            legend2.ForeColor = oRAColours.Colour_BG_P1;
            legend2.Name = "Legend2";
            SRPMChart.BackColor = oRAColours.Colour_BG_Main;
            SRPMChart.ChartAreas.Add(chartArea2);
            SRPMChart.Dock = DockStyle.Fill;
            SRPMChart.Legends.Add(legend2);
            SRPMChart.Name = "SRPMChart";
            SRPMChart.Palette = ChartColorPalette.None;
            SRPMChart.TabIndex = 10;
            SRPMChart.Text = @"Spinner RPM Chart";
            SRPMChart.MouseDown += SRPMChart_MouseDown;
            SRPMChart.MouseMove += SRPMChart_MouseMove;
            tabPage2.Name = Language["tab_SpinnerRPM"];
            tabPage2.Contents = SRPMChart;
            //Replay Info ListView
            ColumnHeader columnHeader1 = new ColumnHeader();
            ColumnHeader columnHeader2 = new ColumnHeader();
            ColumnHeader PropertyHeader = new ColumnHeader();
            ColumnHeader InfoHeader = new ColumnHeader();
            columnHeader1.Text = Language["header_Property"];
            columnHeader1.Width = 250;
            columnHeader2.Text = Language["header_Information"];
            columnHeader2.Width = 600;
            PropertyHeader.Text = Language["header_Property"];
            PropertyHeader.Width = 250;
            InfoHeader.Text = Language["header_Information"];
            InfoHeader.Width = 600;
            ReplayInfoLV.Location = new Point(0, 0);
            ReplayInfoLV.Columns.AddRange(new[] { columnHeader1, columnHeader2 });
            ReplayInfoLV.Dock = DockStyle.Fill;
            ReplayInfoLV.Name = "ReplayInfoLV";
            ReplayInfoLV.TabIndex = 1;
            ReplayInfoLV.UseCompatibleStateImageBehavior = false;
            ReplayInfoLV.FullRowSelect = true;
            ReplayInfoLV.View = View.Details;
            ReplayInfoLV.AllowColumnReorder = false;
            ReplayInfoLV.GridLines = false;
            ReplayInfoLV.BackColor = oRAColours.Colour_BG_P1;
            ReplayInfoLV.OwnerDraw = true;
            ReplayInfoLV.DrawColumnHeader += ListView_DrawColumnHeader;
            ReplayInfoLV.DrawSubItem += ListView_DrawItem;
            tabPage4.Name = Language["tab_ReplayInformation"];
            tabPage4.Contents = ReplayInfoLV;
            //Beatmap Info ListView
            MapInfoLV.Columns.AddRange(new[] { PropertyHeader , InfoHeader });
            MapInfoLV.Dock = DockStyle.Fill;
            MapInfoLV.Name = "MapInfoLV";
            MapInfoLV.TabIndex = 0;
            MapInfoLV.UseCompatibleStateImageBehavior = false;
            MapInfoLV.FullRowSelect = true;
            MapInfoLV.View = View.Details;
            MapInfoLV.AllowColumnReorder = false;
            MapInfoLV.GridLines = false;
            MapInfoLV.BackColor = oRAColours.Colour_BG_P1;
            MapInfoLV.OwnerDraw = true;
            MapInfoLV.DrawColumnHeader += ListView_DrawColumnHeader;
            MapInfoLV.DrawSubItem += ListView_DrawItem;
            tabPage3.Name = Language["tab_BeatmapInformation"];
            tabPage3.Contents = MapInfoLV;

            MainContainer.TabPages.Add(tabPage1);
            MainContainer.TabPages.Add(tabPage2);
            MainContainer.TabPages.Add(tabPage3);
            MainContainer.TabPages.Add(tabPage4);
        }

        private void InitializePlugins()
        {

            //Initialize plugin interface
            oRAData = new DataClass();
            oRAControls = new ControlsClass();
            oRAData.Replays = new List<TreeNode>();
            oRAData.BeatmapHashes = new Dictionary<string, string>();
            oRAData.TimingWindows = new double[3];
            oRAControls.ProgressToolTip = new ToolTip();
            oRAControls.FrameTimeline = ReplayTimelineLB;

            //Load Plugins
            if (Directory.Exists(Environment.CurrentDirectory + @"\Plugins\"))
            {
                foreach (string pluginFile in Directory.GetDirectories(Environment.CurrentDirectory + @"\Plugins\").SelectMany(dir => Directory.GetFiles(dir, "*.dll").Where(file => !Settings.ContainsSetting("DisabledPlugins") || !Settings.GetSetting("DisabledPlugins").Split(new[] { '|' }).Contains(file))))
                {
                    Plugin p = Plugins.LoadPlugin(pluginFile);
                    if (p == null)
                        continue;
                    p.Instance.p_Data = oRAData;
                    p.Instance.p_Controls = oRAControls;
                    if (p.Instance.p_PluginTabItem != null)
                    {
                        p.Instance.p_PluginTabItem.Dock = DockStyle.Fill;
                        oRAPage page = new oRAPage();
                        page.Description = p.Instance.p_Description;
                        page.Name = p.Instance.p_Name;
                        page.Contents = p.Instance.p_PluginTabItem;
                        MainContainer.TabPages.Add(page);
                    }
                    if (p.Instance.p_PluginMenuItem != null)
                    {
                        PluginsMenuItem.DropDownItems.Add(p.Instance.p_PluginMenuItem);
                    }
                }
            }
        }

        private void PopulateLists()
        {
            DirectoryInfo info = new DirectoryInfo(oRAData.ReplayDirectory);
            FileInfo[] files = info.GetFiles().Where(f => f.Extension == ".osr").OrderBy(f => f.CreationTime).Reverse().ToArray();

            try
            {
                Progress.BeginInvoke((MethodInvoker)delegate
                {
                    Progress.Maximum = files.Length;
                });
            }
            catch
            {
                return;
            }
            foreach (FileInfo file in files)
            {
                oRAData.Replays.Add(new TreeNode(file.Name));
                try
                { 
                    Progress.BeginInvoke((MethodInvoker)delegate
                    {
                        Progress.Value += 1;
                    });                  
                }
                catch
                {
                    return;
                }
            }

            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.AddRange(oRAData.Replays.ToArray())));
            oRAControls.ProgressToolTip.Tag = Language["info_PopBeatmaps"];

            string[] beatmapFiles = Directory.GetFiles(oRAData.BeatmapDirectory, "*.osu", SearchOption.AllDirectories);
            
            try
            {
                Progress.BeginInvoke((MethodInvoker)delegate
                {
                    Progress.Value = 0;
                    Progress.Maximum = beatmapFiles.Length;
                });                
            }
            catch
            {
                return;
            }
            using (var md5 = MD5.Create())
            {
                foreach (string file in beatmapFiles)
                {
                    try
                    {
                        using (var stream = File.OpenRead(file))
                        {
                            oRAData.BeatmapHashes.Add(file, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                            Progress.BeginInvoke((MethodInvoker)delegate
                            {
                                Progress.Value += 1;
                            });        
                        }
                    }
                    catch { }
                }
            }
            Progress.Value = 0;
            oRAControls.ProgressToolTip.Tag = Language["info_OperationsCompleted"];
        }

        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)delegate
            {
                ReplaysList.Nodes.Insert(0, e.Name);
                ReplaysList.SelectedNode = ReplaysList.Nodes[0];
            });
        }
        private void ReplayDeleted(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.RemoveByKey(e.Name)));
        }
        private void ReplayRenamed(object sender, RenamedEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)delegate
            {
                ReplaysList.Nodes.RemoveByKey(e.OldName);
                ReplaysList.Nodes.Insert(0, e.Name);
            });
        }

        private void BeatmapCreated(object sender, FileSystemEventArgs e)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(e.FullPath))
                {
                    oRAData.BeatmapHashes.Add(e.FullPath, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }             
            }
        }
        private static void BeatmapDeleted(object sender, FileSystemEventArgs e)
        {
            oRAData.BeatmapHashes.Remove(e.FullPath);
        }
        private static void BeatmapRenamed(object sender, RenamedEventArgs e)
        {
            oRAData.BeatmapHashes.Remove(e.OldFullPath);
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(e.FullPath))
                {
                    oRAData.BeatmapHashes.Add(e.FullPath, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }
            }
        }

        private void ReplaysList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                Replay = new Replay(oRAData.ReplayDirectory + "\\" + e.Node.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language["info_RepLoadError"] + ex);
                return;
            }
            try //Todo: I don't know how
            {
                var file = oRAData.BeatmapHashes.ToList().FirstOrDefault(kvp => kvp.Value.Contains(Replay.MapHash));
                if (file.Key != null)
                {
                    Beatmap = new Beatmap(file.Key);
                    oRAData.UpdateStatus(Replay, Beatmap);

                    /* Start Timing Windows tab */
                    //Determine the timing windows for 300,100,50
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

                    //Get a list of all the individual clicks
                    List<ReplayInfo> realClicks = new List<ReplayInfo>();
                    for (int i = 0; i < Replay.ReplayData.Count; i++)
                    {
                        if (Replay.ReplayData[i].Keys != KeyData.None)
                        {
                            realClicks.Add(Replay.ReplayData[i]);
                        }
                        for (int n = i; n < Replay.ReplayData.Count; n++)
                        {
                            if (Replay.ReplayData[n].Keys != Replay.ReplayData[i].Keys)
                            {
                                i = n;
                                break;
                            }
                        }
                    }

                    double posErrAvg = 0, negErrAvg = 0, errAvg = 0;
                    int posErrCount = 0, negErrCount = 0;
                    int inc = 0;
                    TWChart.Series[0].Points.Clear();

                    //Match up beatmap objects to replay clicks
                    List<ReplayInfo> iteratedObjects = new List<ReplayInfo>();
                    foreach (BaseCircle hitObject in Beatmap.HitObjects)
                    {
                        ReplayInfo c = realClicks.Find(click => (Math.Abs(click.Time - hitObject.StartTime) < oRAData.TimingWindows[2]) && !iteratedObjects.Contains(click)) ??
                                        realClicks.Find(click => (Math.Abs(click.Time - hitObject.StartTime) < oRAData.TimingWindows[1]) && !iteratedObjects.Contains(click)) ??
                                        realClicks.Find(click => (Math.Abs(click.Time - hitObject.StartTime) < oRAData.TimingWindows[0]) && !iteratedObjects.Contains(click));
                        if (c != null)
                        {
                            iteratedObjects.Add(c);
                            TWChart.Series[0].Points.AddXY(inc, c.Time - hitObject.StartTime);
                            errAvg += c.Time - hitObject.StartTime;
                            if (c.Time - hitObject.StartTime > 0)
                            {
                                posErrAvg += c.Time - hitObject.StartTime;
                                posErrCount += 1;
                            }
                            else
                            {
                                negErrAvg += c.Time - hitObject.StartTime;
                                negErrCount += 1;
                            }
                            inc += 1;
                        }
                    }
                    ReplayTimelineLB.Items.Clear();
                    ReplayTimelineLB.Items.AddRange(iteratedObjects.Select((t, i) => "Frame " + i + ":" + (i < 10? "\t\t" : "\t") + "{X=" + t.X + ", Y=" + t.Y + "; Keys: " + t.Keys + "}").ToArray());
                    ReplayTimelineLB.SelectedIndex = 0;
                    /* End Timing Windows tab */


                    /* Start Spinner RPM tab */
                    SRPMChart.Series.Clear();
                    int currentSpinnerNumber = 1;
                    foreach (var spinner in Beatmap.HitObjects.Where(o => o.GetType() == typeof(SpinnerInfo)))
                    {
                        PointInfo currentPosition = new PointInfo(-500,-500);
                        Dictionary<double, int> RPMCount = new Dictionary<double, int>();
                        double currentTime = 0;
                        foreach (ReplayInfo repPoint in Replay.ReplayData.Where(repPoint => repPoint.Time < ((SpinnerInfo)spinner).EndTime && repPoint.Time > spinner.StartTime))
                        {
                            if ((int)currentPosition.X == -500)
                            {
                                currentPosition.X = repPoint.X;
                                currentPosition.Y = repPoint.Y;
                            }
                            else
                            {
                                currentTime += repPoint.TimeDiff;
                                double ptsDist = currentPosition.DistanceTo(new PointInfo(repPoint.X, repPoint.Y));
                                double p1CDist = currentPosition.DistanceTo(spinner.Location);
                                double p2CDist = new PointInfo(repPoint.X, repPoint.Y).DistanceTo(spinner.Location);
                                double travelDegrees = Math.Acos((Math.Pow(p1CDist, 2) + Math.Pow(p2CDist, 2) - Math.Pow(ptsDist, 2)) / (2 * p1CDist * p2CDist)) * (180 / Math.PI);
                                RPMCount.Add(currentTime, (int)Math.Min((travelDegrees / (0.006 * repPoint.TimeDiff)), 477));
                                currentPosition.X = repPoint.X;
                                currentPosition.Y = repPoint.Y;
                            }
                        }
                        int count = 0;
                        int valueAmnt = 0;
                        Series spinnerSeries = new Series();
                        spinnerSeries.ChartType = SeriesChartType.Spline;
                        spinnerSeries.BorderWidth = 2;
                        spinnerSeries.Name = Language["text_Spinner"] + " " + currentSpinnerNumber;
                        foreach (var frame in RPMCount)
                        {
                            valueAmnt += frame.Value;
                            count += 1;
                            spinnerSeries.Points.AddXY(frame.Key, valueAmnt / count);
                        }
                        SRPMChart.Series.Add(spinnerSeries);
                        currentSpinnerNumber += 1;
                    }


                    /* End Spinner RPM tab */

                    /* Start Info tabs */
                    int uRate = TWChart.Series[0].Points.Count != 0 ? Convert.ToInt32(TWChart.Series[0].Points.FindMaxByValue().YValues[0] - TWChart.Series[0].Points.FindMinByValue().YValues[0]) : 0;
                    posErrAvg = posErrCount != 0 ? posErrAvg / posErrCount : 0;
                    negErrAvg = negErrCount != 0 ? negErrAvg / negErrCount : 0;
                    errAvg = (negErrCount != 0 || posErrCount != 0) ? errAvg / (negErrCount + posErrCount) : 0;

                    //Map Info
                    MapInfoLV.Items.Clear();
                    MapInfoLV.Items.Add(new ListViewItem());
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_Format"], Beatmap.Format.ToString() }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_FName"], Beatmap.Filename }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_FSize"], File.OpenRead(Beatmap.Filename).Length + " bytes" }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_FHash"], file.Value }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_TotalHitObjects"], Beatmap.AudioFilename }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapAFN"], Beatmap.HitObjects.Count.ToString(CultureInfo.InvariantCulture) }));
                    MapInfoLV.Items.Add(new ListViewItem());
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapName"], Beatmap.Title + (!string.IsNullOrEmpty(Beatmap.TitleUnicode) && Beatmap.TitleUnicode != Beatmap.Title ? "(" + Beatmap.TitleUnicode + ")" : "") }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapArtist"], Beatmap.Artist + (!string.IsNullOrEmpty(Beatmap.ArtistUnicode) && Beatmap.ArtistUnicode != Beatmap.Artist ? "(" + Beatmap.ArtistUnicode + ")" : "") }));
                    if (Beatmap.Source != null)
                        MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapSource"], Beatmap.Source }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapCreator"], Beatmap.Creator }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapVersion"], Beatmap.Version }));
                    if (Beatmap.BeatmapID != null)
                        MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapID"], Beatmap.BeatmapID.ToString() }));
                    if (Beatmap.BeatmapID != null)
                        MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapSetID"], Beatmap.BeatmapSetID.ToString() }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapTags"], string.Join(", ", Beatmap.Tags) }));
                    MapInfoLV.Items.Add(new ListViewItem());
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapOD"], Beatmap.OverallDifficulty.ToString(".00").Substring(Beatmap.OverallDifficulty.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? Beatmap.OverallDifficulty.ToString(CultureInfo.InvariantCulture) : Beatmap.OverallDifficulty.ToString(".00") }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapAR"], Beatmap.ApproachRate.ToString(".00").Substring(Beatmap.ApproachRate.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? Beatmap.ApproachRate.ToString(CultureInfo.InvariantCulture) : Beatmap.CircleSize.ToString(".00") }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapHP"], Beatmap.HPDrainRate.ToString(".00").Substring(Beatmap.HPDrainRate.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? Beatmap.HPDrainRate.ToString(CultureInfo.InvariantCulture) : Beatmap.HPDrainRate.ToString(".00") }));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapCS"], Beatmap.CircleSize.ToString(".00").Substring(Beatmap.CircleSize.ToString(".00").LastIndexOf(".", StringComparison.InvariantCulture) + 1) == "00" ? Beatmap.CircleSize.ToString(CultureInfo.InvariantCulture) : Beatmap.CircleSize.ToString(".00") }));
                    foreach (ComboInfo combo in Beatmap.ComboColours)
                    {
                        ListViewItem li = new ListViewItem(Language["info_MapComboColour"] + " " + combo.ComboNumber + ":");
                        ListViewItem.ListViewSubItem colorItem = new ListViewItem.ListViewSubItem();
                        colorItem.Text = combo.Colour.R + @", " + combo.Colour.G + @", " + combo.Colour.B;
                        colorItem.ForeColor = Color.FromArgb(255, combo.Colour.R, combo.Colour.G, combo.Colour.B);
                        li.SubItems.Add(colorItem);
                        MapInfoLV.Items.Add(li);
                    }
                    MapInfoLV.Items.Add(new ListViewItem());
                    int totalTime = Beatmap.HitObjects[Beatmap.HitObjects.Count - 1].StartTime - Beatmap.HitObjects[0].StartTime;
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapTotalTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));
                    totalTime = Beatmap.Events.Where(brk => brk.GetType() == typeof (BreakInfo)).Aggregate(totalTime, (current, brk) => current - (((BreakInfo) brk).EndTime - brk.StartTime));
                    MapInfoLV.Items.Add(new ListViewItem(new[] { Language["info_MapDrainTime"], TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") }));


                    //Replay Info
                    ReplayInfoLV.Items.Clear();
                    ReplayInfoLV.Items.Add(new ListViewItem());
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_Format"], Replay.FileFormat.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_FName"], oRAData.ReplayDirectory + "\\" + e.Node.Text }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_FSize"], File.OpenRead(oRAData.ReplayDirectory + "\\" + e.Node.Text).Length + " " + Language["text_bytes"] }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_FHash"], Replay.ReplayHash }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_ReplayFrames"], Replay.ReplayData.Count.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem());
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepMode"], Replay.GameMode.ToString() }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepPlayer"], Replay.PlayerName }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepScore"], Replay.TotalScore.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepCombo"], Replay.MaxCombo.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_Rep300Count"], Replay.Count_300.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_Rep100Count"], Replay.Count_100.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_Rep50Count"], Replay.Count_50.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepMissCount"], Replay.Count_Miss.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepGekiCount"], Replay.Count_Geki.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepKatuCount"], Replay.Count_Katu.ToString(CultureInfo.InvariantCulture) }));
                    ReplayInfoLV.Items.Add(new ListViewItem());
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_RepMods"], Replay.Mods.ToString() }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_UnstableRate"], uRate + "ms" }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_ErrorRate"], Math.Abs(negErrAvg).ToString(".00") + "ms - " + "+" + posErrAvg.ToString(".00") + "ms" }));
                    ReplayInfoLV.Items.Add(new ListViewItem(new[] { Language["info_OverallErrorRate"], Math.Abs(errAvg).ToString(".00") + "ms" }));
                    /* End Info tabs */
                }
            }
            catch { }
        }

        private void Progress_MouseEnter(object sender, EventArgs e)
        {
            oRAControls.ProgressToolTip.Show((string)oRAControls.ProgressToolTip.Tag, Progress, 0);
        }

        private void Progress_MouseLeave(object sender, EventArgs e)
        {
            oRAControls.ProgressToolTip.Hide(Progress);
        }

        private void TWChart_MouseDown(object sender, MouseEventArgs e)
        {
            //Check for overflow and set the current position
            if ((int)TWChart.ChartAreas[0].AxisX.PixelPositionToValue(e.X) < ReplayTimelineLB.Items.Count && (int)TWChart.ChartAreas[0].AxisX.PixelPositionToValue(e.X) >= 0)
                ReplayTimelineLB.SelectedIndex = (int)TWChart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
        }

        private void TWChart_MouseMove(object sender, MouseEventArgs e)
        {
            HitTestResult result = TWChart.HitTest(e.X, e.Y);

            if (result.PointIndex != -1 && result.Series != null && result.PointIndex < TWChart.Series[0].Points.Count)
            {
                if (ChartToolTip.Tag == null || (int)ChartToolTip.Tag != (int)TWChart.Series[0].Points[result.PointIndex].XValue)
                {
                    ChartToolTip.Tag = (int)TWChart.Series[0].Points[result.PointIndex].XValue;
                    ChartToolTip.SetToolTip(TWChart, TWChart.Series[0].Points[result.PointIndex].YValues[0] + "ms");
                }
            }
            else
            {
                ChartToolTip.Hide(TWChart);
            }
        }

        private void SRPMChart_MouseMove(object sender, MouseEventArgs e)
        {
            HitTestResult result = SRPMChart.HitTest(e.X, e.Y);
            if (result.PointIndex != -1 && result.Series != null && result.Series.BorderWidth != 0)
            {
                foreach (Series s in SRPMChart.Series.Where(s => !Equals(s, result.Series) && s.BorderWidth != 0))
                {
                    SRPMChart.Series[SRPMChart.Series.IndexOf(s)].BorderWidth = 2;
                }
                result.Series.BorderWidth = 3;
            }
            else
            {
                foreach (Series s in SRPMChart.Series.Where(s => !Equals(s.Tag, "1") && s.BorderWidth != 0))
                {
                    SRPMChart.Series[SRPMChart.Series.IndexOf(s)].BorderWidth = 2;
                }
            }

            if (result.PointIndex != -1 && result.Series != null && result.PointIndex < SRPMChart.Series[0].Points.Count && !Equals(result.Series.Tag, "0"))
            {
                if (ChartToolTip.Tag == null || (int)ChartToolTip.Tag != (int)result.Series.Points[result.PointIndex].XValue)
                {
                    ChartToolTip.Tag = (int)result.Series.Points[result.PointIndex].XValue;
                    ChartToolTip.SetToolTip(SRPMChart, result.Series.Points[result.PointIndex].YValues[0] + "RPM");
                }
            }
            else
            {
                ChartToolTip.Hide(SRPMChart);
            }
        }

        private void SRPMChart_MouseDown(object sender, MouseEventArgs e)
        {
            HitTestResult result = SRPMChart.HitTest(e.X, e.Y);
            if (result.PointIndex != -1 && result.Series != null)
            {
                if (result.Series.BorderWidth == 0)
                {
                    foreach (Series s in SRPMChart.Series)
                    {
                        SRPMChart.Series[SRPMChart.Series.IndexOf(s)].BorderWidth = 2;
                        SRPMChart.Series[SRPMChart.Series.IndexOf(s)].IsVisibleInLegend = true;
                        SRPMChart.Series[SRPMChart.Series.IndexOf(s)].Tag = "";
                    }
                }
                else
                {
                    foreach (Series s in SRPMChart.Series.Where(s => !Equals(s, result.Series)))
                    {
                        SRPMChart.Series[SRPMChart.Series.IndexOf(s)].BorderWidth = 0;
                        SRPMChart.Series[SRPMChart.Series.IndexOf(s)].IsVisibleInLegend = false;
                        SRPMChart.Series[SRPMChart.Series.IndexOf(s)].Tag = "0";
                    }
                    result.Series.BorderWidth = 3;
                    result.Series.Tag = "1";
                }

            }
            else
            {
                foreach (Series s in SRPMChart.Series)
                {
                    SRPMChart.Series[SRPMChart.Series.IndexOf(s)].BorderWidth = 2;
                    SRPMChart.Series[SRPMChart.Series.IndexOf(s)].IsVisibleInLegend = true;
                    SRPMChart.Series[SRPMChart.Series.IndexOf(s)].Tag = "";
                }
            }
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

        private void ReplayTimelineLB_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), e.Bounds);
            if (e.State.HasFlag(DrawItemState.Selected))
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_1), e.Bounds);
                e.Graphics.DrawRectangle(new Pen((new SolidBrush(oRAColours.Colour_Item_BG_0))), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            string text = ReplayTimelineLB.Items[e.Index].ToString();
            e.Graphics.DrawImageUnscaled(TimelineFrameImg, e.Bounds.Left + 1, e.Bounds.Height + 1);
            e.Graphics.DrawString(text, oRAFonts.Font_SubDescription, (e.State & DrawItemState.Selected) == DrawItemState.Selected ? new SolidBrush(oRAColours.Colour_Text_H) : new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + 22, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(text, oRAFonts.Font_SubDescription).Height / 2);
        }

        private void ReplayTimelineLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            TWChart.Series[1].Points.Clear();
            TWChart.Series[1].Points.AddXY(ReplayTimelineLB.SelectedIndex, TWChart.ChartAreas[0].AxisY.Maximum - 5);
            TWChart.Series[1].Points.AddXY(ReplayTimelineLB.SelectedIndex, TWChart.ChartAreas[0].AxisY.Minimum + 5);
        }

        private void ReplaysList_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Index == -1)
                return;
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), e.Bounds);                
            if (e.State.HasFlag(TreeNodeStates.Selected))
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_1), e.Bounds);
                e.Graphics.DrawRectangle(new Pen(oRAColours.Colour_Item_BG_0), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Node.Text, oRAFonts.Font_SubDescription, e.State.HasFlag(TreeNodeStates.Selected) ? new SolidBrush(oRAColours.Colour_Text_H) : new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + 22, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Node.Text, oRAFonts.Font_SubDescription).Height / 2);
        }

        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_Main), e.Bounds);
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), new Rectangle(e.Bounds.X + e.Bounds.Width - 1, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Header.Text, oRAFonts.Font_Description, new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + e.Bounds.Width / 2 - e.Graphics.MeasureString(e.Header.Text, oRAFonts.Font_Description).Width / 2, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Header.Text, oRAFonts.Font_Description).Height / 2);
        }

        private static void ListView_DrawItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ItemIndex == -1)
                return;
            e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_BG_P0), e.Bounds);
            if (e.ItemState.HasFlag(ListViewItemStates.Selected))
            {
                e.Graphics.FillRectangle(new SolidBrush(oRAColours.Colour_Item_BG_1), e.Bounds);
                e.Graphics.DrawRectangle(new Pen(oRAColours.Colour_Item_BG_0), e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(e.Item.Text, oRAFonts.Font_SubDescription, e.ItemState.HasFlag(ListViewItemStates.Selected) ? new SolidBrush(oRAColours.Colour_Text_H) : new SolidBrush(oRAColours.Colour_Text_N), e.Bounds.Left + 22, e.Bounds.Top + e.Bounds.Height / 2 - e.Graphics.MeasureString(e.Item.Text, oRAFonts.Font_SubDescription).Height / 2);
        }

    }
}
