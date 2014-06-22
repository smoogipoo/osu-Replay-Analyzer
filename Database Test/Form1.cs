using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ErikEJ.SqlCe;
using Microsoft.Win32;
using BMAPI;
using System.Threading.Tasks;
namespace Database_Test
{
    public partial class Form1 : Form
    {
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
            BeatmapDir = Path.Combine(FindOsuPath(), "Songs");

            await Handler();
        }

        private async Task Handler()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            await UpdateBeatmaps();
            watch.Stop();
            MessageBox.Show(watch.Elapsed.ToString());
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
                    foreach (DataRow dr in DBHelper.GetRecords(conn, "Beatmaps", "Hash,Filename").Rows)
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
        /// Updates a beatmap record if it exists, otherwise inserts it
        /// </summary>
        private async Task UpdateBeatmaps()
        {
            await Task.Run(() =>
            {
                SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
                options |= SqlCeBulkCopyOptions.KeepNulls;

                DataTable beatmapData = DBHelper.CreateBeatmapDataTable();
                DataTable[] data = { beatmapData };

                string[] beatmapFiles = Directory.GetFiles(BeatmapDir, "*.osu", SearchOption.AllDirectories);

                using (SqlCeConnection conn = new SqlCeConnection(DBHelper.dbPath))
                {
                    conn.Open();
                    using (SqlCeBulkCopy bC = new SqlCeBulkCopy(conn, options))
                    {
                        foreach (string file in beatmapFiles)
                        {
                            string beatmapHash = MD5FromFile(file);
                            if (!DBHelper.RecordExists(conn, "Beatmaps", "Hash", beatmapHash) && beatmapData.AsEnumerable().All(row => (beatmapHash != row.Field<string>("Hash"))))
                            {
                                beatmapData.Rows.Add(beatmapHash, file);

                                if (beatmapData.Rows.Count >= 1000)
                                {
                                    DBHelper.BulkInsert(bC, data);
                                    beatmapData.Clear();
                                }
                            }
                        }
                        //Flush any remaining data
                        DBHelper.BulkInsert(bC, data);
                        beatmapData.Clear();
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