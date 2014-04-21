using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using o_RA.Properties;
using System.Xml;

namespace o_RA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Public objects:
        Replay replay;
        Beatmap beatmap;
        XmlReader locale;
        readonly Dictionary<string, string> Language = new Dictionary<string, string>();
        readonly double[] timingWindows = new double[3];
        readonly ToolTip progressTT = new ToolTip();
        string progressTTText = "";
        string replayDir = "", beatmapDir = "";
        //Lists
        readonly List<TreeNode> replays = new List<TreeNode>();
        readonly Dictionary<string, string> beatmapHashes = new Dictionary<string, string>();

        //Private objects
        private readonly Bitmap timelineFrameImg = new Bitmap(18, 18);
        private readonly Series TWDataSeries = new Series();
        private Series TLSelectionSeries = new Series();

        private void Form1_Load(object sender, EventArgs e)
        {
            ReplayTimelineLB.ItemHeight = 20;

            if (Settings.Default.ApplicationLocale == "")
            {
                LocaleSelectForm lsf = new LocaleSelectForm();
                lsf.ShowDialog();
            }
            try
            {
                locale = XmlReader.Create(File.OpenRead(Application.StartupPath + "\\locales\\" + Settings.Default.ApplicationLocale + ".xml"));
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
            if (Settings.Default.ApplicationLocale != "enUS")
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
                replayDir = gameDir + "\\Replays";
                beatmapDir = gameDir + "\\Songs";
            }
            else
            {

                if (MessageBox.Show(Language["info_osuClosed"], Language["info_osuClosedMessageBoxTitle"]) == DialogResult.OK)
                {
                   using (FolderBrowserDialog fd = new FolderBrowserDialog())
                   {
                       while (replayDir == "")
                       {
                           if (fd.ShowDialog() == DialogResult.OK)
                           {
                               if (Directory.Exists(fd.SelectedPath + "\\Replays") && Directory.Exists(fd.SelectedPath + "\\Songs"))
                               {
                                   replayDir = fd.SelectedPath + "\\Replays";
                                   beatmapDir = fd.SelectedPath + "\\Songs";
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
            progressTTText = Language["info_PopReplays"];

            Thread populateListsThread = new Thread(populateLists);
            populateListsThread.IsBackground = true;
            populateListsThread.Start();

            FileSystemWatcher replayWatcher = new FileSystemWatcher(replayDir);
            replayWatcher.NotifyFilter = NotifyFilters.FileName;
            replayWatcher.Filter = "*.osr";
            FileSystemWatcher beatmapWatcher = new FileSystemWatcher(beatmapDir);
            replayWatcher.NotifyFilter = NotifyFilters.FileName;
            beatmapWatcher.Filter = "*.osu";
            beatmapWatcher.IncludeSubdirectories = true;

            replayWatcher.Created += replayCreated;
            replayWatcher.Deleted += replayDeleted;
            replayWatcher.Renamed += replayRenamed;
            beatmapWatcher.Created += beatmapCreated;
            beatmapWatcher.Deleted += beatmapDeleted;
            beatmapWatcher.Renamed += beatmapRenamed;

            replayWatcher.EnableRaisingEvents = true;
            beatmapWatcher.EnableRaisingEvents = true;
        }

        private void populateLists()
        {
            DirectoryInfo info = new DirectoryInfo(replayDir);
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
                replays.Add(new TreeNode(file.Name));
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

            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.AddRange(replays.ToArray())));
            progressTTText = Language["info_PopBeatmaps"];

            string[] beatmapFiles = Directory.GetFiles(beatmapDir, "*.osu", SearchOption.AllDirectories);
            
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
                            beatmapHashes.Add(file, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
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
            progressTTText = "All operations completed.";
        }

        private void replayCreated(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.Insert(0, e.Name)));
            
        }
        private void replayDeleted(object sender, FileSystemEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)(() => ReplaysList.Nodes.RemoveByKey(e.Name)));
        }
        private void replayRenamed(object sender, RenamedEventArgs e)
        {
            ReplaysList.BeginInvoke((MethodInvoker)delegate
            {
                ReplaysList.Nodes.RemoveByKey(e.OldName);
                ReplaysList.Nodes.Insert(0, e.Name);
            });
        }

        private void beatmapCreated(object sender, FileSystemEventArgs e)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(e.FullPath))
                {
                    beatmapHashes.Add(e.FullPath, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }             
            }
        }
        private void beatmapDeleted(object sender, FileSystemEventArgs e)
        {
            beatmapHashes.Remove(e.FullPath);
        }
        private void beatmapRenamed(object sender, RenamedEventArgs e)
        {
            beatmapHashes.Remove(e.OldFullPath);
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(e.FullPath))
                {
                    beatmapHashes.Add(e.FullPath, BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                }
            }
        }

        private void ReplaysList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                replay = new Replay(replayDir + "\\" + e.Node.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Language["info_RepLoadError"] + ex);
                return;
            }
            try //Todo: I don't know how
            {
                var file = beatmapHashes.ToList().FirstOrDefault(kvp => kvp.Value.Contains(replay.mapHash));
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
                        timingWindows[i] = beatmap.OverallDifficulty < 5 ? (200 - 60 * i) + (beatmap.OverallDifficulty) * ((150 - 50 * i) - (200 - 60 * i)) / 5 : (150 - 50 * i) + (beatmap.OverallDifficulty - 5) * ((100 - 40 * i) - (150 - 50 * i)) / 5;
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
                    
                    //Match up beatmap objects to replay clicks
                    List<ReplayInfo> iteratedObjects = new List<ReplayInfo>();
                    TWDataSeries.Points.Clear();
                    TLSelectionSeries.Points.Clear();
                    foreach (BaseCircle hitObject in beatmap.HitObjects)
                    {
                        ReplayInfo c = realClicks.Find(click => (Math.Abs(click.Time - hitObject.startTime) < timingWindows[2]) && !iteratedObjects.Contains(click)) ??
                                        realClicks.Find(click => (Math.Abs(click.Time - hitObject.startTime) < timingWindows[1]) && !iteratedObjects.Contains(click)) ??
                                        realClicks.Find(click => (Math.Abs(click.Time - hitObject.startTime) < timingWindows[0]) && !iteratedObjects.Contains(click));
                        if (c != null)
                        {
                            iteratedObjects.Add(c);
                            TWDataSeries.Points.AddXY(inc, c.Time - hitObject.startTime);
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
                    TWChart.Series.Clear();
                    TWChart.Series.Add(TWDataSeries);
                    ReplayTimelineLB.SelectedIndex = 0;
                    /* End Timing Windows tab */


                    /* Start Spinner RPM tab */
                    foreach (object spinner in beatmap.HitObjects.Where(o => o.GetType() == typeof(SpinnerInfo)))
                    {
                        PointInfo currentPosition = new PointInfo(-500,-500);
                        List<int> RPMCount = new List<int>();

                        foreach (ReplayInfo repPoint in replay.replayData.Where(repPoint => repPoint.Time < ((SpinnerInfo)spinner).endTime && repPoint.Time > ((SpinnerInfo)spinner).startTime))
                        {
                            if ((int)currentPosition.x == -500)
                            {
                                currentPosition.x = repPoint.X;
                                currentPosition.y = repPoint.Y;
                            }
                            else
                            {
                                double ptsDist = currentPosition.DistanceTo(new PointInfo(repPoint.X, repPoint.Y));
                                double p1CDist = currentPosition.DistanceTo(((SpinnerInfo)spinner).location);
                                double p2CDist = new PointInfo(repPoint.X, repPoint.Y).DistanceTo(((SpinnerInfo)spinner).location);
                                double travelDegrees = Math.Acos((Math.Pow(p1CDist, 2) + Math.Pow(p2CDist, 2) - Math.Pow(ptsDist, 2)) / (2 * p1CDist * p2CDist)) * (180 / Math.PI);
                                RPMCount.Add((int)Math.Min((travelDegrees / (0.006 * repPoint.TimeDiff)), 477));
                                currentPosition.x = repPoint.X;
                                currentPosition.y = repPoint.Y;
                            }
                        }
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
                    MapInfoTB.AppendText(Language["info_FHash"] + file.Value);
                    MapInfoTB.AppendText("\tTotal objects:\t\t" + beatmap.HitObjects.Count);
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
                    foreach (var brk in beatmap.Events)
                    {
                        if (brk.GetType() == typeof(BreakInfo))
                        {
                            totalTime -= (((BreakInfo)brk).endTime - brk.startTime);
                        }
                    }
                    MapInfoTB.AppendText(TimeSpan.FromMilliseconds(totalTime).Minutes + ":" + TimeSpan.FromMilliseconds(totalTime).Seconds.ToString("00") + "\n");

                    //Replay Info
                    ReplayInfoTB.Text = "\n";
                    ReplayInfoTB.AppendText(Language["info_Format"] + replay.fileFormat + "\n");
                    ReplayInfoTB.AppendText(Language["info_FName"] + replayDir + "\\" + e.Node.Text + "\n");
                    ReplayInfoTB.AppendText(Language["info_FSize"] + File.OpenRead(replayDir + "\\" + e.Node.Text).Length + " bytes\n");
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
            progressTT.Show(progressTTText, Progress, 0);
        }

        private void Progress_MouseLeave(object sender, EventArgs e)
        {
            progressTT.Hide(Progress);
        }

        private void ReplayTimelineLB_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            string text = ReplayTimelineLB.Items[e.Index].ToString();
            e.Graphics.DrawImageUnscaled(timelineFrameImg, e.Bounds.Left + 1, e.Bounds.Height + 1);
            e.Graphics.DrawString(text, new Font("Segoe UI", 8), Brushes.Black, e.Bounds.Left + 22, e.Bounds.Top + 10 - e.Graphics.MeasureString(text, new Font("Segoe UI", 8)).Height / 2);
        }

        private void ReplayTimelineLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TWChart.Series.Contains(TLSelectionSeries))
                TWChart.Series.Remove(TLSelectionSeries);
            TLSelectionSeries.Points.Clear();
            TLSelectionSeries.Points.AddXY(ReplayTimelineLB.SelectedIndex, TWChart.ChartAreas[0].AxisY.Maximum - 5);
            TLSelectionSeries.Points.AddXY(ReplayTimelineLB.SelectedIndex, TWChart.ChartAreas[0].AxisY.Minimum + 5);
            TWChart.Series.Add(TLSelectionSeries);
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
    }
}
