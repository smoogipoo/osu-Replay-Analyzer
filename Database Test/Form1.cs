using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ReplayAPI;
using ErikEJ.SqlCe;

namespace Database_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string ReplayDir;
        private void Form1_Load(object sender, EventArgs e)
        {


            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = @"Select replay directory";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    ReplayDir = fbd.SelectedPath;
                    PutReplays();
                }
            }
        }


        private void PutReplays()
        {
            SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
            options |= SqlCeBulkCopyOptions.KeepNulls;

            DataTable replayData = new DataTable();
            replayData.Columns.Add(new DataColumn("Hash", typeof(string)));
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

            DataTable clickData = new DataTable();
            clickData.Columns.Add(new DataColumn("ReplayHash", typeof(string)));
            clickData.Columns.Add(new DataColumn("Time", typeof(int)));
            clickData.Columns.Add(new DataColumn("TimeDiff", typeof(int)));
            clickData.Columns.Add(new DataColumn("X", typeof(double)));
            clickData.Columns.Add(new DataColumn("Y", typeof(double)));
            clickData.Columns.Add(new DataColumn("KeyData", typeof(int)));

            //Specify an extremely large timeout otherwise connection will close 
            using (SqlCeBulkCopy bC = new SqlCeBulkCopy(@"Data Source='" + Path.Combine(Environment.CurrentDirectory, "db.sdf") + @"';Max Database Size=1024;Default Lock Timeout=9000000", options))
            {
                foreach (string file in Directory.GetFiles(ReplayDir))
                {
                    try
                    { 
                        Replay r = new Replay(file);

                        //Only add items to the datatable if there isn't any other item with the same hash
                        if (replayData.AsEnumerable().All(row => r.ReplayHash != row.Field<string>("Hash")))
                            replayData.Rows.Add(r.ReplayHash, (int)r.GameMode, r.Filename, r.MapHash, r.PlayerName, r.TotalScore, r.Count_300, r.Count_100, r.Count_50, r.Count_Geki, r.Count_Katu, r.Count_Miss, r.MaxCombo, r.IsPerfect, r.PlayTime.Ticks, r.ReplayLength);                     
                        
                        if (replayData.Rows.Count == 250) // This value may be changed depending on requirements - replays will only be put into the database when 250 of them are in replayData
                        {
                            bC.DestinationTableName = "ReplayData";
                            bC.WriteToServer(replayData); //Insert datatable into the database
                            replayData.Clear();
                        }
                        foreach (ReplayInfo rI in r.ClickFrames)
                        {
                            clickData.Rows.Add(r.ReplayHash, rI.Time, rI.TimeDiff, rI.X, rI.Y, (int)rI.Keys);

                            if (clickData.Rows.Count == 100000) //This value also may be changed, but 100 000 is pretty decent
                            {
                                bC.DestinationTableName = "ReplayFrame";
                                bC.WriteToServer(clickData);
                                clickData.Clear();
                            }
                        }
                    }
                    catch { }
                }

                //Now we flush the remaining items in the datatables to the database
                bC.DestinationTableName = "ReplayData";
                bC.WriteToServer(replayData);
                bC.DestinationTableName = "ReplayFrame";
                bC.WriteToServer(clickData);

                //Free some memory
                replayData.Clear();
                clickData.Clear();

                //Because we added a timeout, we need to close the BulkCopy (I think)
                bC.Close();
            }


        }
    }
}
