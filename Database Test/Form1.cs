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

        private DataTable CreateBeatmapData_BeatmapTagTable()
        {
            DataTable beatmapData_BeatmapTag = new DataTable("BeatmapData_BeatmapTag");
            beatmapData_BeatmapTag.Columns.Add(new DataColumn("BeatmapData_Hash", typeof(string)));
            beatmapData_BeatmapTag.Columns.Add(new DataColumn("BeatmapTag_Id", typeof(int)));
            return beatmapData_BeatmapTag;
        }

        private DataTable CreateBeatmapDataTable()
        {
            DataTable beatmapData = new DataTable("BeatmapData");
            beatmapData.Columns.Add(new DataColumn("BeatmapData_Hash", typeof(string)));
            beatmapData.Columns.Add(new DataColumn("Creator", typeof(string)));
            beatmapData.Columns.Add(new DataColumn("AudioFilename", typeof(string)));
            beatmapData.Columns.Add(new DataColumn("Filename", typeof(string)));
            beatmapData.Columns.Add(new DataColumn("HPDrainRate", typeof(double)));
            beatmapData.Columns.Add(new DataColumn("CircleSize", typeof(double)));
            beatmapData.Columns.Add(new DataColumn("OverallDifficulty", typeof(double)));
            beatmapData.Columns.Add(new DataColumn("ApproachRate", typeof(double)));
            beatmapData.Columns.Add(new DataColumn("Title", typeof(string)));
            beatmapData.Columns.Add(new DataColumn("Artist", typeof(string)));
            beatmapData.Columns.Add(new DataColumn("Version", typeof(string)));
            return beatmapData;
        }

        private DataTable CreateBeatmapTagTable()
        {
            DataTable beatmapTag = new DataTable("BeatmapTag");
            beatmapTag.Columns.Add(new DataColumn("Name", typeof(string)));
            return beatmapTag;
        }

        private DataTable CreateReplayDataTable()
        {
            DataTable replayData = new DataTable("ReplayData");
            replayData.Columns.Add(new DataColumn("ReplayData_Hash", typeof(string)));
            replayData.Columns.Add(new DataColumn("GameMode", typeof(int)));
            replayData.Columns.Add(new DataColumn("Filename", typeof(string)));
            replayData.Columns.Add(new DataColumn("MapHash", typeof(string)));
            replayData.Columns.Add(new DataColumn("PlayerName", typeof(string)));
            replayData.Columns.Add(new DataColumn("TotalScore", typeof(int)));
            replayData.Columns.Add(new DataColumn("Count_300", typeof(int)));
            replayData.Columns.Add(new DataColumn("Count_100", typeof(int)));
            replayData.Columns.Add(new DataColumn("Count_50", typeof(int)));
            replayData.Columns.Add(new DataColumn("Count_Geki", typeof(int)));
            replayData.Columns.Add(new DataColumn("Count_Katu", typeof(int)));
            replayData.Columns.Add(new DataColumn("Count_Miss", typeof(int)));
            replayData.Columns.Add(new DataColumn("MaxCombo", typeof(int)));
            replayData.Columns.Add(new DataColumn("IsPerfect", typeof(int)));
            replayData.Columns.Add(new DataColumn("PlayTime", typeof(long)));
            replayData.Columns.Add(new DataColumn("ReplayLength", typeof(int)));
            return replayData;
        }

        private DataTable CreateReplayFrameTable()
        {
            DataTable clickData = new DataTable("ReplayFrame");
            clickData.Columns.Add(new DataColumn("ReplayData_Hash", typeof(string)));
            clickData.Columns.Add(new DataColumn("Time", typeof(int)));
            clickData.Columns.Add(new DataColumn("TimeDiff", typeof(int)));
            clickData.Columns.Add(new DataColumn("X", typeof(double)));
            clickData.Columns.Add(new DataColumn("Y", typeof(double)));
            clickData.Columns.Add(new DataColumn("KeyData", typeof(int)));
            return clickData;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReplayDir = Path.Combine(FindOsuPath(), "Replays");
            UpdateReplays();
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

            DataTable replayData = CreateReplayDataTable();
            DataTable clickData = CreateReplayFrameTable();
            DataTable[] data = new DataTable[] { replayData, clickData };

            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (SqlCeConnection conn = new SqlCeConnection(@"Data Source='" + Path.Combine(Environment.CurrentDirectory, "db.sdf") + @"';Max Database Size=1024;"))
            {
                conn.Open();
                using (SqlCeCommand cmd = new SqlCeCommand())
                {
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SqlCeParameter() { ParameterName = "@Filename" });
                    using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn, options))
                    {
                        SqlCeDataReader rdr = null;
                        foreach (string file in Directory.GetFiles(ReplayDir))
                        {
                            Replay r = new Replay(file);
                            //Only add items to the datatable if there isn't any other item with the same hash
                            if (replayData.AsEnumerable().All(row => r.ReplayHash != row.Field<string>("ReplayData_Hash")))
                            {
                                try
                                {
                                    cmd.CommandText = "SELECT 1 FROM ReplayData WHERE Filename = @Filename;";
                                    cmd.Parameters["@Filename"].Value = r.Filename;
                                    rdr = cmd.ExecuteReader();
                                    if (rdr.Read())
                                    {
                                        if (rdr["ReplayData_Hash"] != r.ReplayHash)
                                        {
                                            //delete this replay and its replayframes from the db
                                            //add replay and its replayframes
                                        }
                                    }
                                    else
                                    {
                                        cmd.CommandText = String.Format("SELECT 1 FROM ReplayData WHERE ReplayData_Hash = '{0}';", r.ReplayHash);
                                        rdr = cmd.ExecuteReader();
                                        if (rdr.Read())
                                        {
                                            //update filename
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
                                    if (replayData.Rows.Count >= 200)
                                    {
                                        BulkInsert(bC, data);
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
                        BulkInsert(bC, data);
                        replayData.Clear();
                        clickData.Clear();
                    }
                }
            }
            watch.Stop();
            MessageBox.Show(watch.Elapsed.ToString());
        }

        private void BulkInsert(SqlCeBulkCopy bC, DataTable[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                bC.DestinationTableName = data[i].TableName;
                bC.WriteToServer(data[i]);
            }
        }
    }
}