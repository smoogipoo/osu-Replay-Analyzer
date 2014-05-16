using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ErikEJ.SqlCe;
using Microsoft.Win32;
using ReplayAPI;

namespace Database_Test
{
    public partial class Form1 : Form
    {
        private string ReplayDir;

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

        private void Form1_Load(object sender, EventArgs e)
        {
            ReplayDir = Path.Combine(FindOsuPath(), "Replays");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            UpdateReplays();
            watch.Stop();
            MessageBox.Show(watch.Elapsed.ToString());
        }

        /// <summary>
        /// Updates a replay record if it exists, otherwise inserts it
        /// </summary>
        private void UpdateReplays()
        {
            //Logic used
            //if (filename exists in table)
            //{
            //    if (hash for that entry is different)
            //    {
            //        //Update that db entry
            //    }
            //}
            //else
            //{
            //    if (hash exists in table)
            //    {
            //        //Update filename
            //    }
            //    else
            //    {
            //        //Add to db
            //    }
            //}
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
                            Replay r;
                            try
                            {
                                r = new Replay(file);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Replay failed loading: " + ex.StackTrace);
                                return;
                            }
                            //Only add items to the datatable if there isn't any other item with the same hash
                            if (replayData.AsEnumerable().All(row => r.ReplayHash != row.Field<string>("ReplayData_Hash")))
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT TOP 1 * FROM ReplayData WHERE Filename = @Filename;";
                                    cmd.Parameters["@Filename"].Value = r.Filename;
                                    rdr = cmd.ExecuteReader();
                                    if (rdr.Read())
                                    {
                                        if ((string)rdr["ReplayData_Hash"] != r.ReplayHash)
                                        {
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
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message + ex.StackTrace);
                                }
                            }
                            else
                            {
                                //TODO Offer to delete duplicate replay
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
        }
    }
}