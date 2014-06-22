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
            await Task.WhenAll(UpdateBeatmaps(), UpdateReplays());
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

                DataTable[] data = { replayData, frameData };

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
                            foreach (string file in Directory.GetFiles(ReplayDir))
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
                                    if (replayData.AsEnumerable().All(row => r.ReplayHash != row.Field<string>("Replay_Data_Hash")))
                                    {
                                        cmd.CommandText = "SELECT TOP 1 * FROM Replay_Data WHERE Filename = @Filename;";
                                        cmd.Parameters["@Filename"].Value = file;
                                        rdr = cmd.ExecuteReader();
                                        if (rdr.Read())
                                        {
                                            if ((string)rdr["Replay_Data_Hash"] != r.ReplayHash)
                                            {
                                                r.LoadReplayData();
                                                //Filename found, but hash is different
                                                //Delete this replay and its replayframes from the db
                                                DBHelper.DeleteRecords(conn, "Replay_Data", "Filename", r.Filename);
                                                //Readd updated replay and its replayframes
                                                replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.Mods, r.ReplayLength);
                                                foreach (ReplayInfo rI in r.ClickFrames)
                                                {
                                                    frameData.Rows.Add(r.ReplayHash, rI.Time, rI.TimeDiff, rI.X, rI.Y, (int)rI.Keys);
                                                }
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
                                            }
                                            else
                                            {
                                                r.LoadReplayData();
                                                replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.Mods, r.ReplayLength);
                                                foreach (ReplayInfo rI in r.ClickFrames)
                                                {
                                                    frameData.Rows.Add(r.ReplayHash, rI.Time, rI.TimeDiff, rI.X, rI.Y, (int)rI.Keys);
                                                }
                                            }
                                        }
                                        //Limit memory usage
                                        if (replayData.Rows.Count >= 200 || frameData.Rows.Count > 100000)
                                        {
                                            DBHelper.BulkInsert(bC, data);
                                            foreach (DataRow dr in replayData.Rows)
                                            {
                                                InsertReplay(dr["Filename"].ToString());
                                            }
                                            replayData.Clear();
                                            frameData.Clear();
                                        }
                                    }
                                    else
                                    {
                                        //TODO Offer to delete duplicate replay
                                    }
                                }
                            }
                            rdr.Close();
                            //Flush any remaining data
                            DBHelper.BulkInsert(bC, data);
                            foreach (DataRow dr in replayData.Rows)
                            {
                                InsertReplay(dr["Filename"].ToString());
                            }
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
                                    beatmapData.Rows.Add(b.BeatmapHash, b.Mode ?? 0, b.Creator ?? "", b.AudioFilename ?? "", b.Filename, b.HPDrainRate, b.CircleSize, b.OverallDifficulty, b.ApproachRate, b.Title ?? "", b.Artist ?? "", b.Version ?? "");
                                    foreach (var tag in b.Tags)
                                    {
                                        TagData.Rows.Add(b.BeatmapHash, tag);
                                    }
                                       
                                    if (beatmapData.Rows.Count >= 1000)
                                    {
                                        DBHelper.BulkInsert(bC, data);
                                        foreach (DataRow dr in beatmapData.Rows)
                                        {
                                            InsertBeatmap(dr["Filename"].ToString(), dr["Beatmap_Data_Hash"].ToString());
                                        }
                                        beatmapData.Clear();
                                        TagData.Clear();
                                    }
                                }
                            }
                            //Flush any remaining data
                            DBHelper.BulkInsert(bC, data);
                            foreach (DataRow dr in beatmapData.Rows)
                            {
                                InsertBeatmap(dr["Filename"].ToString(), dr["Beatmap_Data_Hash"].ToString());
                            }
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