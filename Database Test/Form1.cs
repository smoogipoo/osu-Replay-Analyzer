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

            //UpdateReplays();
            //InsertBeatmaps();
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
        /// Updates a replay record if it exists, otherwise inserts it
        /// </summary>
        private async Task UpdateReplays()
        {
            await Task.Run(() =>
            {
                SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
                options |= SqlCeBulkCopyOptions.KeepNulls;

                DataTable replayData = DBHelper.CreateReplayDataTable();
                DataTable clickData = DBHelper.CreateReplayFrameTable();
                DataTable[] data = { replayData, clickData };

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
                                    if (replayData.AsEnumerable().All(row => r.ReplayHash != row.Field<string>("ReplayData_Hash")))
                                    {
                                        cmd.CommandText = "SELECT TOP 1 * FROM ReplayData WHERE Filename = @Filename;";
                                        cmd.Parameters["@Filename"].Value = file;
                                        rdr = cmd.ExecuteReader();
                                        if (rdr.Read())
                                        {
                                            if ((string)rdr["ReplayData_Hash"] != r.ReplayHash)
                                            {
                                                r.LoadReplayData();
                                                //Filename found, but hash is different
                                                //Delete this replay and its replayframes from the db
                                                DBHelper.DeleteRecords(conn, "ReplayData", "Filename", r.Filename);
                                                //Readd updated replay and its replayframes
                                                replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.ReplayLength);
                                                foreach (ReplayInfo rI in r.ClickFrames)
                                                {
                                                    clickData.Rows.Add(r.ReplayHash, rI.Time, rI.TimeDiff, rI.X, rI.Y, (int)rI.Keys);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            cmd.CommandText = String.Format("SELECT TOP 1 * FROM ReplayData WHERE ReplayData_Hash = '{0}';", r.ReplayHash);
                                            rdr = cmd.ExecuteReader();
                                            if (rdr.Read())
                                            {
                                                //Filename not found, but hash found
                                                //Update filename
                                                DBHelper.UpdateRecord(conn, "ReplayData", "Filename", r.Filename, "ReplayData_Hash", r.ReplayHash);
                                            }
                                            else
                                            {
                                                r.LoadReplayData();
                                                replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.ReplayLength);
                                                foreach (ReplayInfo rI in r.ClickFrames)
                                                {
                                                    clickData.Rows.Add(r.ReplayHash, rI.Time, rI.TimeDiff, rI.X, rI.Y, (int)rI.Keys);
                                                }
                                            }
                                        }
                                        //Limit memory usage
                                        if (replayData.Rows.Count >= 200 || clickData.Rows.Count > 100000)
                                        {
                                            DBHelper.BulkInsert(bC, data);
                                            replayData.Clear();
                                            clickData.Clear();
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
                            replayData.Clear();
                            clickData.Clear();
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
                   DataTable beatmapTagData = DBHelper.CreateBeatmapTagTable();
                   DataTable beatmapData_beatmapTagData = DBHelper.CreateBeatmapData_BeatmapTagTable();
                   DataTable[] data = { beatmapData, beatmapTagData, beatmapData_beatmapTagData };

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
                                   if (!DBHelper.RecordExists(conn, "BeatmapData", "BeatmapData_Hash", mapHash) && beatmapData.AsEnumerable().All(row => (mapHash != row.Field<string>("BeatmapData_Hash"))))
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
                                       beatmapData.Rows.Add(b.BeatmapHash, b.Creator ?? "", b.AudioFilename ?? "", b.Filename, b.HPDrainRate, b.CircleSize, b.OverallDifficulty, b.ApproachRate, b.Title ?? "", b.Artist ?? "", b.Version ?? "");
                                       foreach (var tag in b.Tags)
                                       {
                                           // only add tag if not already in the datatable or db
                                           if (tag.Length > 2 && !beatmapTagData.AsEnumerable().Any(row => String.Equals(row.Field<String>("BeatmapTag_Name"), tag, StringComparison.InvariantCultureIgnoreCase)) && !DBHelper.RecordExists(conn, "BeatmapTag", "BeatmapTag_Name", tag))
                                           {
                                               beatmapTagData.Rows.Add(tag);
                                               beatmapData_beatmapTagData.Rows.Add(b.BeatmapHash, tag);
                                           };
                                       }
                                       
                                       if (beatmapData.Rows.Count >= 1000)
                                       {
                                           DBHelper.BulkInsert(bC, data);
                                           beatmapData.Clear();
                                           beatmapTagData.Clear();
                                           beatmapData_beatmapTagData.Clear();
                                       }
                                   }
                               }
                               DBHelper.BulkInsert(bC, data);
                               //Flush any remaining data
                               beatmapData.Clear();
                               beatmapTagData.Clear();
                               beatmapData_beatmapTagData.Clear();
                           }
                       }
                   }
               });
        }

        /// <summary>
        /// Insert replays into an empty db
        /// </summary>
        private async Task InsertReplays()
        {
            Task.Run(() =>
              {
                  SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
                  options |= SqlCeBulkCopyOptions.KeepNulls;

                  DataTable replayData = DBHelper.CreateReplayDataTable();
                  DataTable clickData = DBHelper.CreateReplayFrameTable();
                  DataTable[] data = { replayData, clickData };

                  using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
                  {
                      conn.Open();
                      using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn, options))
                      {
                          progressBar1.BeginInvoke((MethodInvoker)delegate
                          {
                              progressBar1.Maximum = Directory.GetFiles(ReplayDir).Length;
                          });
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
                                  if (replayData.AsEnumerable().All(row => r.ReplayHash != row.Field<string>("ReplayData_Hash")))
                                  {
                                      r.LoadReplayData();

                                      replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.ReplayLength);
                                      progressBar1.BeginInvoke((MethodInvoker)delegate
                                      {
                                          progressBar1.Value += 1;
                                      });

                                      foreach (ReplayInfo rI in r.ClickFrames)
                                      {
                                          clickData.Rows.Add(r.ReplayHash, rI.Time, rI.TimeDiff, rI.X, rI.Y, (int)rI.Keys);
                                      }
                                      //Limit memory usage
                                      if (replayData.Rows.Count >= 200 || clickData.Rows.Count > 100000)
                                      {
                                          DBHelper.BulkInsert(bC, data);
                                          replayData.Clear();
                                          clickData.Clear();
                                      }
                                  }
                                  else
                                  {
                                      //TODO Offer to delete duplicate replay
                                  }
                              }
                          }
                          //Flush any remaining data
                          try
                          {
                              DBHelper.BulkInsert(bC, data);
                          }
                          catch (Exception ex)
                          {
                              MessageBox.Show(ex.Message + ex.StackTrace);
                          }
                          replayData.Clear();
                          clickData.Clear();
                      }
                  }
              });
        }

        /// <summary>
        /// Insert beatmaps into an empty db
        /// </summary>
        private void InsertBeatmaps()
        { }

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