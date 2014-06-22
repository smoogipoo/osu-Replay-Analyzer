using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ErikEJ.SqlCe;
using Microsoft.Win32;
using ReplayAPI;
using BMAPI;
using System.Security.Cryptography;
using System.Threading.Tasks;
namespace Database_Test
{
    public partial class Form1 : Form
    {
        private string ReplayDir;
        private string BeatmapDir;

        public Form1()
        {
            InitializeComponent();
        }

        private static string FindOsuPath()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu!\\DefaultIcon");
                if (key != null)
                {
                    object o = key.GetValue(null);
                    if (o != null)
                    {
                        var filter = new Regex(@"(?<="")[^\""]*(?="")");
                        return Path.GetDirectoryName(filter.Match(o.ToString()).ToString());
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
            return "";
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ReplayDir = Path.Combine(FindOsuPath(), "Replays");
            BeatmapDir = Path.Combine(FindOsuPath(), "Songs");

            await Handler();
        }

        private async Task Handler()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            await Task.WhenAll(UpdateReplays(), UpdateBeatmaps());
            watch.Stop();
            MessageBox.Show(watch.Elapsed.ToString());
        }
    
        /// <summary>
        /// Gets the replays in the database and performs operations on them
        /// </summary>
        private async Task GetReplays()
        {
            await Task.Run(() =>
            {
                using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
                {
                    conn.Open();
                    foreach (DataRow dr in DBHelper.GetRecords(conn, "Replay_Data", "Filename").Rows)
                    {
                        InsertReplay(dr["Filename"].ToString());                     
                    }
                }
            });
        }

        private void InsertReplay(string filename)
        {
            //Todo: Do something with replays here
        }

        /// <summary>
        /// Gets the beatmaps in the database and performs operations on them
        /// </summary>
        private async Task GetBeatmaps()
        {
            await Task.Run(() =>
            {
                using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
                {
                    conn.Open();
                    foreach (DataRow dr in DBHelper.GetRecords(conn, "Beatmap_Data", "Beatmap_Data_Hash,Filename").Rows)
                    {
                        InsertBeatmap(dr["Filename"].ToString(), dr["Beatmap_Data_Hash"].ToString());
                    }
                }
            });
        }

        private void InsertBeatmap(string filename, string hash)
        {
            //Todo: Do something with beatmaps here       
        }

        /// <summary>
        /// Updates a replay record if it exists, otherwise inserts it
        /// </summary>
        private async Task UpdateReplays()
        {
            await Task.Run(() =>
            {
                SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
                options |= SqlCeBulkCopyOptions.KeepNulls;

                DataTable replayData = DBHelper.CreateReplayDataTable();
                DataTable frameData = DBHelper.CreateReplayFrameTable();
                DataTable lifeData = DBHelper.CreateReplayLifeDataTable();

                DataTable[] data = { replayData, frameData, lifeData };

                using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
                {
                    conn.Open();
                    using (SqlCeCommand cmd = new SqlCeCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Filename" });
                        using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn, options))
                        {
                            SqlCeDataReader rdr = null;
                            Stopwatch s = new Stopwatch();
                            foreach (string file in Directory.GetFiles(ReplayDir, "*.osr"))
                            {
                                using (Replay r = new Replay())
                                {
                                    try
                                    {
                                        r.Open(file);
                                        r.LoadMetadata();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Failed to load replay {0}\nStacktrace:{1}", file, ex.StackTrace);
                                        continue;
                                    }

                                    //Only add items to the datatable if there isn't any other item with the same hash
                                    //We will try to avoid as much user interraction as possible. If we can load the replay
                                    //it is not broken. If it has the same replay hash and map hash, then it they are the same
                                    //if one is different, then one will not work inside osu!, so we tell the user that
                                    bool skipReplay = false;
                                    for (int i = 0; i < replayData.Rows.Count; i++)
                                    {
                                        if (replayData.Rows[i].Field<string>("Replay_Data_Hash") == r.ReplayHash)
                                        {
                                            if (replayData.Rows[i].Field<string>("MapHash") == r.MapHash)
                                            {
                                                skipReplay = true;
                                                break;
                                            }
                                            //Todo: Improve this with more options
                                            if (MessageBox.Show(String.Format("A duplicate replay has been found. Additional information:\n" +
                                                                              "---Old replay---\n" + "Filename: {0}\n" + "Player Name: {1}\n" + "Total Score: {2}\n" +
                                                                              "---New replay---\n" + "Filename: {3}\n" + "Player Name: {4}\n" + "Total Score: {5}\n" +
                                                                              "One of these replays will not load in osu! - we encourage you to try both of them before proceeding.\n" +
                                                                              "Would you like to delete the new replay?",
                                                replayData.Rows[i].Field<string>("Filename"), replayData.Rows[i].Field<string>("PlayerName"), replayData.Rows[i].Field<int>("TotalScore"), r.Filename, r.PlayerName, r.TotalScore), @"Replay action required", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                            {
                                                r.Dispose();
                                                File.Delete(file);
                                            }
                                            skipReplay = true;
                                            break;
                                        }
                                    }
                                    if (skipReplay)
                                        continue;

                                    cmd.CommandText = "SELECT TOP 1 * FROM Replay_Data WHERE Filename = @Filename;";
                                    cmd.Parameters["@Filename"].Value = file;
                                    rdr = cmd.ExecuteReader();
                                    if (rdr.Read())
                                    {
                                        if ((string)rdr["Replay_Data_Hash"] != r.ReplayHash)
                                        {
                                            //Filename found, but hash is different
                                            //Delete this replay from the db before re-adding
                                            //Filename is unique column
                                            DBHelper.DeleteRecords(conn, "Replay_Data", "Filename", r.Filename);
                                        }
                                        else
                                        {
                                            //All good here, replayhash and replay filename match
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        cmd.CommandText = String.Format("SELECT TOP 1 * FROM Replay_Data WHERE Replay_Data_Hash = '{0}';", r.ReplayHash);
                                        rdr = cmd.ExecuteReader();
                                        if (rdr.Read())
                                        {
                                            //Filename not found, but hash found
                                            //Update filename
                                            DBHelper.UpdateRecord(conn, "Replay_Data", "Filename", r.Filename, "Replay_Data_Hash", r.ReplayHash);
                                            //No need to re-insert frames as hashes are identical
                                            continue;
                                        }
                                    }
                                    r.LoadReplayData();

                                    //Insert replay metadata
                                    replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.Mods, r.ReplayLength);
                                    
                                    //Insert replay frames
                                    for (int i = 0; i < r.ClickFrames.Count; i++)
                                        frameData.Rows.Add(r.ReplayHash, r.ClickFrames[i].Time, r.ClickFrames[i].TimeDiff, r.ClickFrames[i].X, r.ClickFrames[i].Y, (int)r.ClickFrames[i].Keys);
                                    
                                    //Insert life data
                                    for (int i = 0; i < r.LifeData.Count; i++)
                                        lifeData.Rows.Add(r.ReplayHash, r.LifeData[i].Time, r.LifeData[i].Percentage);

                                    //Limit memory usage
                                    if (frameData.Rows.Count >= 200000)
                                    {
                                        DBHelper.BulkInsert(bC, data);
                                        replayData.Clear();
                                        frameData.Clear();
                                    } 
                                }
                            }
                            rdr.Close();
                            //Flush any remaining data
                            DBHelper.BulkInsert(bC, data);
                            replayData.Clear();
                            frameData.Clear();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Updates a beatmap record if it exists, otherwise inserts it
        /// </summary>
        private async Task UpdateBeatmaps()
        {
            await Task.Run(() =>
            {
                SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
                options |= SqlCeBulkCopyOptions.KeepNulls;

                DataTable beatmapData = DBHelper.CreateBeatmapDataTable();
                DataTable TagData = DBHelper.CreateBeatmapTagTable();
                DataTable[] data = { beatmapData, TagData };

                string[] beatmapFiles = Directory.GetFiles(BeatmapDir, "*.osu", SearchOption.AllDirectories);

                using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
                {
                    conn.Open();
                    using (SqlCeCommand cmd = new SqlCeCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Filename" });
                        using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn, options))
                        {
                            Beatmap b;
                            foreach (string file in beatmapFiles)
                            {
                                string mapHash = MD5FromFile(file);
                                if (!DBHelper.RecordExists(conn, "Beatmap_Data", "Beatmap_Data_Hash", mapHash) && beatmapData.AsEnumerable().All(row => (mapHash != row.Field<string>("Beatmap_Data_Hash"))))
                                {
                                    try
                                    {
                                        b = new Beatmap(file);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("Beatmap failed loading: " + ex.StackTrace);
                                        continue;
                                    }

                                    //Can't have null values here - this might be fixed in BMAPI in a later version
                                    //Insert the beatmap metadata
                                    beatmapData.Rows.Add(b.BeatmapHash, b.Mode ?? 0, b.Creator ?? "", b.AudioFilename ?? "", b.Filename, b.HPDrainRate, b.CircleSize, b.OverallDifficulty, b.ApproachRate, b.Title ?? "", b.Artist ?? "", b.Version ?? "");
                                    
                                    //Insert beatmap tags
                                    for (int i = 0; i < b.Tags.Count; i++)
                                        TagData.Rows.Add(b.BeatmapHash, b.Tags[i]);

                                    //Todo: Insert beatmap objecst

                                    if (beatmapData.Rows.Count >= 1000)
                                    {
                                        DBHelper.BulkInsert(bC, data);
                                        beatmapData.Clear();
                                        TagData.Clear();
                                    }
                                }
                            }
                            //Flush any remaining data
                            DBHelper.BulkInsert(bC, data);
                            beatmapData.Clear();
                            TagData.Clear();
                        }
                    }
                }
            });
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
    }
}