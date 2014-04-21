using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Windows.Forms.DataVisualization.Charting;
using ReplayAPI;
using BMAPI;
using System.Xml;
using smgiFuncs;

namespace o_RA
{
    public partial class oRAMainForm : Form
    {
        public oRAMainForm()
        {
            InitializeComponent();
        }
        private XmlReader locale;
        public static Settings settings = new Settings();

        Replay replay;
        Beatmap beatmap;
        private readonly Dictionary<string, string> Language = new Dictionary<string, string>();
        readonly double[] TimingWindows = new double[3];
        readonly ToolTip ProgressTT = new ToolTip();
        string ProgressTTText = "";
        string ReplayDir = "", BeatmapDir = "";
        readonly List<TreeNode> Replays = new List<TreeNode>();
        readonly Dictionary<string, string> BeatmapHashes = new Dictionary<string, string>();

        private static readonly PluginServices Plugins = new PluginServices(); //Declares Plugins as global
        private const int PluginPriority = 10;

        private readonly Bitmap TimelineFrameImg = new Bitmap(18, 18);


        private void Form1_Load(object sender, EventArgs e)
        {     
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory + @"\Plugins\", "*.dll").Where(file => !settings.ContainsSetting("DisallowedPlugins") || !settings.GetSetting("DisallowedPlugins").Split(new[] { '|' }).Contains(file)))
            {
                sString fileString = file;
                fileString = fileString.SubString(fileString.LastIndexOf(@"\") + 1, fileString.LastIndexOf("."));
                Plugin p = Plugins.LoadPlugin(fileString.ToString());
                if (p.Instance.p_TabItem != null)
                {
                    TabPage newTabPage = new TabPage(p.Instance.p_Name);
                    newTabPage.Controls.Add(p.Instance.p_TabItem);
                    MainContainer.TabPages.Add(newTabPage);
                }
                if (p.Instance.p_MenuItem != null)
                {
                    PluginsMenuItem.DropDownItems.Add(p.Instance.p_MenuItem);
                }
            }
            Thread pluginUpdateThread = new Thread(UpdatePlugins)
            {
                IsBackground = true
            };
            pluginUpdateThread.Start();

            ReplayTimelineLB.ItemHeight = 20;

            if (!settings.ContainsSetting("ApplicationLocale") || settings.GetSetting("ApplicationLocale") == "")
            {
                LocaleSelectForm lsf = new LocaleSelectForm();
                lsf.ShowDialog();
            }
            try
            {
                locale = XmlReader.Create(File.OpenRead(Application.StartupPath + "\\locales\\" + settings.GetSetting("ApplicationLocale") + ".xml"));
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(@"Selected locale does not exist. Application will now exit.");
                Environment.Exit(1);
            }
            while (locale.Read())
            {
                    string n = locale.Name;
                    locale.Read();
                    if (!Language.ContainsKey(n))
                        Language.Add(n, locale.Value.Replace(@"\n", "\n").Replace(@"\t", "\t"));
            }
            if (settings.GetSetting("ApplicationLocale") != "enUS")
            {
                XmlReader enLocale = XmlReader.Create(File.OpenRead(Application.StartupPath + "\\locales\\enUS.xml"));
                while (enLocale.Read())
                {
                    string n = enLocale.Name;
                    enLocale.Read();
                    if (!Language.ContainsKey(n))
                        Language.Add(n, enLocale.Value.Replace(@"\n", "\n").Replace(@"\t", "\t"));
                }
            }
            Process[] procs = Process.GetProcessesByName("osu!");
            if (procs.Length != 0)
            {
                string gameDir = procs[0].Modules[0].FileName.Substring(0,procs[0].Modules[0].FileName.LastIndexOf("\\", StringComparison.Ordinal));
                ReplayDir = gameDir + "\\Replays";
                BeatmapDir = gameDir + "\\Songs";
            }
            else
            {

                if (MessageBox.Show(Language["info_osuClosed"], Language["info_osuClosedMessageBoxTitle"]) == DialogResult.OK)
                {
                   using (FolderBrowserDialog fd = new FolderBrowserDialog())
                   {
                       while (ReplayDir == "")
                       {
                           if (fd.ShowDialog() == DialogResult.OK)
                           {
                               if (Directory.Exists(fd.SelectedPath + "\\Replays") && Directory.Exists(fd.SelectedPath + "\\Songs"))
                               {
                                   ReplayDir = fd.SelectedPath + "\\Replays";
                                   BeatmapDir = fd.SelectedPath + "\\Songs";
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
            ProgressTTText = Language["info_PopReplays"];

            Thread populateListsThread = new Thread(PopulateLists);
            populateListsThread.IsBackground = true;
            populateListsThread.Start();

            FileSystemWatcher replayWatcher = new FileSystemWatcher(ReplayDir);
            replayWatcher.NotifyFilter = NotifyFilters.FileName;
            replayWatcher.Filter = "*.osr";
            FileSystemWatcher beatmapWatcher = new FileSystemWatcher(BeatmapDir);
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

        private void UpdatePlugins()
        {
            while(true)
            {
                foreach (Plugin p in Plugins.PluginCollection)
                {
                    p.Instance.p_BeatmapDirectory = BeatmapDir;
                    p.Instance.p_ReplayDirectory = ReplayDir;
                    p.Instance.p_CurrentBeatmap = beatmap;
                    p.Instance.p_CurrentReplay = replay;
                    p.Instance.p_FrameTimeline = ReplayTimelineLB;
                    p.Instance.p_MainTabControl = MainContainer;
                    p.Instance.p_Progress = Progress;
                    p.Instance.p_ProgressToolTipText = ProgressTTText;
                    p.Instance.p_TimingWindows = TimingWindows;
                }
                Thread.Sleep(1000 / PluginPriority);
            }
        }

        private void PopulateLists()
        {
            DirectoryInfo info = new DirectoryInfo(ReplayDir);
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
                Replays.Add(new TreeNode(file.Name));
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

            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.AddRange(Replays.ToArray())));
            ProgressTTText = Language["info_PopBeatmaps"];

            string[] beatmapFiles = Directory.GetFiles(BeatmapDir, "*.osu", SearchOption.AllDirectories);
            
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
                            BeatmapHashes.Add(file, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                                Progress.BeginInvoke((MethodInvoker)delegate
                                {
                                    Progress.Value += 1;
                                });        
                        }
                    }
                    catch { }
                }
            }
            Progress.BeginInvoke((MethodInvoker)delegate
            {
                Progress.Value = 0;
            });
            ProgressTTText = "All operations completed.";
        }

        private void ReplayCreated(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.Insert(0, e.Name)));
            
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
                    BeatmapHashes.Add(e.FullPath, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }             
            }
        }
        private void BeatmapDeleted(object sender, FileSystemEventArgs e)
        {
            BeatmapHashes.Remove(e.FullPath);
        }
        private void BeatmapRenamed(object sender, RenamedEventArgs e)
        {
            BeatmapHashes.Remove(e.OldFullPath);
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(e.FullPath))
                {
                    BeatmapHashes.Add(e.FullPath, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }
            }
        }

        private void ReplaysList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                replay = new Replay(ReplayDir + "\\" + e.Node.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language["info_RepLoadError"] + ex);
                return;
            }
            try //Todo: I don't know how
            {
                var file = BeatmapHashes.ToList().FirstOrDefault(kvp => kvp.Value.Contains(replay.mapHash));
                if (file.Key != null)
                {
                    beatmap = new Beatmap(file.Key);


                    /* Start Timing Windows tab */
                    //Determine the timing windows for 300,100,50
                    if ((replay.mods & Modifications.HardRock) == Modifications.HardRock)
                    {
                        beatmap.OverallDifficulty = Math.Min(beatmap.OverallDifficulty *= 1.4, 10);
                        beatmap.CircleSize = (int)(beatmap.CircleSize * 1.4);
                        foreach (BaseCircle hitObject in beatmap.HitObjects)
                        {
                            beatmap.HitObjects[beatmap.HitObjects.IndexOf(hitObject)].radius = 40 - 4 * (beatmap.CircleSize - 2);
                        }
                        beatmap.CircleSize = beatmap.CircleSize * 1.4;
                    }
                    if ((replay.mods & Modifications.DoubleTime) == Modifications.DoubleTime)
                    {
                        beatmap.OverallDifficulty = Math.Min(13.0 / 3.0 + (2.0 / 3.0) * beatmap.OverallDifficulty, 11);
                        beatmap.ApproachRate = Math.Min(13.0 / 3.0 + (2.0 / 3.0) * beatmap.ApproachRate, 11);
                    }
                    if ((replay.mods & Modifications.HalfTime) == Modifications.HalfTime)
                    {
                        beatmap.OverallDifficulty = (3.0 / 2.0) * beatmap.OverallDifficulty - 13.0 / 2.0;
                        beatmap.ApproachRate = (3.0 / 2.0) * beatmap.ApproachRate - 13.0 / 2.0;
                    }
                    if ((replay.mods & Modifications.Easy) == Modifications.Easy)
                    {
                        beatmap.OverallDifficulty = beatmap.OverallDifficulty / 2;
                    }

                    //Timing windows are determined by linear interpolation
                    for (int i = 2; i >= 0; i--)
                    {
                        TimingWindows[i] = beatmap.OverallDifficulty < 5 ? (200 - 60 * i) + (beatmap.OverallDifficulty) * ((150 - 50 * i) - (200 - 60 * i)) / 5 : (150 - 50 * i) + (beatmap.OverallDifficulty - 5) * ((100 - 40 * i) - (150 - 50 * i)) / 5;
                    }

                    //Get a list of all the individual clicks
                    List<ReplayInfo> realClicks = new List<ReplayInfo>();
                    for (int i = 0; i < replay.replayData.Count; i++)
                    {
                        if (replay.replayData[i].Keys != KeyData.None)
                        {
                            realClicks.Add(replay.replayData[i]);
                        }
                        for (int n = i; n < replay.replayData.Count; n++)
                        {
                            if (replay.replayData[n].Keys != replay.replayData[i].Keys)
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
                    foreach (BaseCircle hitObject in beatmap.HitObjects)
                    {
                        ReplayInfo c = realClicks.Find(click => (Math.Abs(click.Time - hitObject.startTime) < TimingWindows[2]) && !iteratedObjects.Contains(click)) ??
                                        realClicks.Find(click => (Math.Abs(click.Time - hitObject.startTime) < TimingWindows[1]) && !iteratedObjects.Contains(click)) ??
                                        realClicks.Find(click => (Math.Abs(click.Time - hitObject.startTime) < TimingWindows[0]) && !iteratedObjects.Contains(click));
                        if (c != null)
                        {
                            iteratedObjects.Add(c);
                            TWChart.Series[0].Points.AddXY(inc, c.Time - hitObject.startTime);
                            errAvg += c.Time - hitObject.startTime;
                            if (c.Time - hitObject.startTime > 0)
                            {
                                posErrAvg += c.Time - hitObject.startTime;
                                posErrCount += 1;
                            }
                            else
                            {
                                negErrAvg += c.Time - hitObject.startTime;
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
                    foreach (var spinner in beatmap.HitObjects.Where(o => o.GetType() == typeof(SpinnerInfo)))
                    {
                        PointInfo currentPosition = new PointInfo(-500,-500);
                        Dictionary<double, int> RPMCount = new Dictionary<double, int>();
                        double currentTime = 0;
                        foreach (ReplayInfo repPoint in replay.replayData.Where(repPoint => repPoint.Time < ((SpinnerInfo)spinner).endTime && repPoint.Time > spinner.startTime))
                        {
                            if ((int)currentPosition.x == -500)
                            {
                                currentPosition.x = repPoint.X;
                                currentPosition.y = repPoint.Y;
                            }
                            else
                            {
                                currentTime += repPoint.TimeDiff;
                                double ptsDist = currentPosition.DistanceTo(new PointInfo(repPoint.X, repPoint.Y));
                                double p1CDist = currentPosition.DistanceTo(spinner.location);
                                double p2CDist = new PointInfo(repPoint.X, repPoint.Y).DistanceTo(spinner.location);
                                double travelDegrees = Math.Acos((Math.Pow(p1CDist, 2) + Math.Pow(p2CDist, 2) - Math.Pow(ptsDist, 2)) / (2 * p1CDist * p2CDist)) * (180 / Math.PI);
                                RPMCount.Add(currentTime, (int)Math.Min((travelDegrees / (0.006 * repPoint.TimeDiff)), 477));
                                currentPosition.x = repPoint.X;
                                currentPosition.y = repPoint.Y;
                            }
                        }
                        int count = 0;
                        int valueAmnt = 0;
                        Series spinnerSeries = new Series();
                        spinnerSeries.ChartType = SeriesChartType.Spline;
                        spinnerSeries.BorderWidth = 2;
                        spinnerSeries.Name = "Spinner " + currentSpinnerNumber;
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
                    MapInfoTB.Text = "\n";
                    MapInfoTB.AppendText(Language["info_Format"] + beatmap.Format + "\n");
                    MapInfoTB.AppendText(Language["info_FName"] + beatmap.Filename + "\n");
                    MapInfoTB.AppendText(Language["info_FSize"] + File.OpenRead(beatmap.Filename).Length + " bytes\n");
                    MapInfoTB.AppendText(Language["info_FHash"] + file.Value + "\n");
                    MapInfoTB.AppendText("\tTotal hitobjects:\t\t" + beatmap.HitObjects.Count);
                    MapInfoTB.AppendText("\n");
                    MapInfoTB.AppendText(Language["info_MapAFN"] + beatmap.AudioFilename + "\n");
                    MapInfoTB.AppendText(beatmap.Title != null ? Language["info_MapName"] + beatmap.Title + (beatmap.TitleUnicode != "" && beatmap.TitleUnicode != beatmap.Title ? "(" + beatmap.TitleUnicode + ")" : "") + "\n" : "");
                    MapInfoTB.AppendText(beatmap.Artist != null ? Language["info_MapArtist"] + beatmap.Artist + (beatmap.ArtistUnicode != "" && beatmap.ArtistUnicode != beatmap.Artist ? "(" + beatmap.ArtistUnicode + ")" : "") + "\n" : "");
                    MapInfoTB.AppendText(beatmap.Source != null ? Language["info_MapSource"] + beatmap.Source + "\n" : "");
                    MapInfoTB.AppendText(beatmap.Creator != null ? Language["info_MapCreator"] + beatmap.Creator + "\n" : "");
                    MapInfoTB.AppendText(beatmap.Version != null ? Language["info_MapVersion"] + beatmap.Version + "\n" : "");
                    MapInfoTB.AppendText(beatmap.BeatmapID != null ? Language["info_MapID"] + beatmap.BeatmapID + "\n" : "");
                    MapInfoTB.AppendText(beatmap.BeatmapSetID != null ? Language["info_MapSetID"] + beatmap.BeatmapSetID + "\n" : "");
                    MapInfoTB.AppendText(beatmap.Tags.Count != 0 ? Language["info_MapTags"] + string.Join(", ", beatmap.Tags) + "\n" : "\n");
                    MapInfoTB.AppendText("\n");
                    MapInfoTB.AppendText(Language["info_MapOD"] + beatmap.OverallDifficulty.ToString(".00") + "\n");
                    MapInfoTB.AppendText(Language["info_MapAR"] + beatmap.ApproachRate.ToString(".00") + "\n");
                    MapInfoTB.AppendText(Language["info_MapHP"] + beatmap.HPDrainRate.ToString(".00") + "\n");
                    MapInfoTB.AppendText(Language["info_MapCS"] + beatmap.CircleSize.ToString(".00") + "\n");
                    foreach (ComboInfo combo in beatmap.ComboColours)
                    {
                        MapInfoTB.AppendText(Language["info_MapComboColour"] + " " + combo.comboNumber + ":\t\t\t");
                        int pos = MapInfoTB.TextLength;
                        MapInfoTB.AppendText(combo.colour.r + @", " + combo.colour.g + @", " + combo.colour.b + "\n");
                        MapInfoTB.Select(pos, MapInfoTB.TextLength);
                        MapInfoTB.SelectionColor = Color.FromArgb(255, combo.colour.r, combo.colour.g, combo.colour.b);
                    }
                    MapInfoTB.AppendText("\n");
                    int totalTime = beatmap.HitObjects[beatmap.HitObjects.Count - 1].startTime - beatmap.HitObjects[0].startTime;
                    MapInfoTB.AppendText(Language["info_MapTotalTime"] + TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") + "\n");
                    MapInfoTB.AppendText(Language["info_MapDrainTime"]);
                    totalTime = beatmap.Events.Where(brk => brk.GetType() == typeof (BreakInfo)).Aggregate(totalTime, (current, brk) => current - (((BreakInfo) brk).endTime - brk.startTime));
                    MapInfoTB.AppendText(TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") + "\n");

                    //Replay Info
                    ReplayInfoTB.Text = "\n";
                    ReplayInfoTB.AppendText(Language["info_Format"] + replay.fileFormat + "\n");
                    ReplayInfoTB.AppendText(Language["info_FName"] + ReplayDir + "\\" + e.Node.Text + "\n");
                    ReplayInfoTB.AppendText(Language["info_FSize"] + File.OpenRead(ReplayDir + "\\" + e.Node.Text).Length + " bytes\n");
                    ReplayInfoTB.AppendText(Language["info_FHash"] + replay.replayHash + "\n");
                    ReplayInfoTB.AppendText("\tReplay frames:\t\t" + replay.replayData.Count);
                    ReplayInfoTB.AppendText("\n");
                    ReplayInfoTB.AppendText(Language["info_RepMode"] + replay.gameMode + "\n");
                    ReplayInfoTB.AppendText(Language["info_RepPlayer"] + replay.playerName + "\n");
                    ReplayInfoTB.AppendText(Language["info_RepScore"] + replay.totalScore + "\n");
                    ReplayInfoTB.AppendText(Language["info_RepCombo"] + replay.maxCombo + "\n");
                    ReplayInfoTB.AppendText(Language["info_Rep300Count"] + replay.count_300 + "\n");
                    ReplayInfoTB.AppendText(Language["info_Rep100Count"] + replay.count_100 + "\n");
                    ReplayInfoTB.AppendText(Language["info_Rep50Count"] + replay.count_50 + "\n");
                    ReplayInfoTB.AppendText(Language["info_RepMissCount"] + replay.count_miss + "\n");
                    ReplayInfoTB.AppendText(Language["info_RepGekiCount"] + replay.count_geki + "\n");
                    ReplayInfoTB.AppendText(Language["info_RepKatuCount"] + replay.count_katu + "\n");
                    ReplayInfoTB.AppendText("\n");
                    ReplayInfoTB.AppendText(Language["info_RepMods"] + replay.mods + "\n");
                    ReplayInfoTB.AppendText(Language["info_UnstableRate"] + "-" + uRate + "ms\n");
                    ReplayInfoTB.AppendText(Language["info_ErrorRate"] + "-" + Math.Abs(negErrAvg).ToString(".00") + "ms - " + "+" + posErrAvg.ToString(".00") + "ms\n");
                    ReplayInfoTB.AppendText(Language["info_OverallErrorRate"] + Math.Abs(errAvg).ToString(".00") + "ms\n");
                    /* End Info tabs */
                }
            }
            catch { }
        }

        private void Progress_MouseEnter(object sender, EventArgs e)
        {
            ProgressTT.Show(ProgressTTText, Progress, 0);
        }

        private void Progress_MouseLeave(object sender, EventArgs e)
        {
            ProgressTT.Hide(Progress);
        }

        private void ReplayTimelineLB_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            string text = ReplayTimelineLB.Items[e.Index].ToString();
            e.Graphics.DrawImageUnscaled(TimelineFrameImg, e.Bounds.Left + 1, e.Bounds.Height + 1);
            e.Graphics.DrawString(text, new Font("Segoe UI", 8), Brushes.Black, e.Bounds.Left + 22, e.Bounds.Top + 10 - e.Graphics.MeasureString(text, new Font("Segoe UI", 8)).Height / 2);
        }

        private void ReplayTimelineLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            TWChart.Series[1].Points.Clear();
            TWChart.Series[1].Points.AddXY(ReplayTimelineLB.SelectedIndex, TWChart.ChartAreas[0].AxisY.Maximum - 5);
            TWChart.Series[1].Points.AddXY(ReplayTimelineLB.SelectedIndex, TWChart.ChartAreas[0].AxisY.Minimum + 5);
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
    }
}
