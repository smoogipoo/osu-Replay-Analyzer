

//Can't pass some values as parameters
//See http://stackoverflow.com/questions/6843065/error-with-sqlce-parameters

using System.Data;
using System.Data.SqlServerCe;
using ErikEJ.SqlCe;

namespace Database_Test
{
    internal static class DBHelper
    {
        public static readonly string dbPath = @"Data Source='" + System.IO.Path.Combine(System.Environment.CurrentDirectory, "db.sdf") + @"';Max Database Size=1024;";

        /// <summary>
        /// Inserts DataTable objects using SqlCeBulkCopy
        /// </summary>
        /// <param name="bC"></param>
        /// <param name="data"></param>
        public static void BulkInsert(SqlCeBulkCopy bC, DataTable[] data)
        {
            foreach (DataTable t in data)
            {
                bC.DestinationTableName = t.TableName;
                bC.WriteToServer(t);
            }
        }

        public static DataTable CreateBeatmapDataTable()
        {
            DataTable beatmapData = new DataTable("BeatmapData");
            beatmapData.Columns.Add(new DataColumn { ColumnName = "BeatmapData_Hash", DataType = typeof(string), Unique = true});
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

        public static DataTable CreateBeatmapTagTable()
        {
            DataTable beatmapTag = new DataTable("BeatmapTag");
            beatmapTag.Columns.Add(new DataColumn("BeatmapData_Hash", typeof(string)));
            beatmapTag.Columns.Add(new DataColumn("BeatmapTag_Name", typeof(string)));
            return beatmapTag;
        }

        public static DataTable CreateReplayDataTable()
        {
            DataTable replayData = new DataTable("ReplayData");
            replayData.Columns.Add(new DataColumn { ColumnName = "ReplayData_Hash", DataType = typeof(string), Unique = true });
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

        public static DataTable CreateReplayFrameTable()
        {
            DataTable clickData = new DataTable("ReplayFrame");
            clickData.Columns.Add(new DataColumn { ColumnName = "ReplayData_Hash", DataType = typeof(string)});
            clickData.Columns.Add(new DataColumn("Time", typeof(int)));
            clickData.Columns.Add(new DataColumn("TimeDiff", typeof(int)));
            clickData.Columns.Add(new DataColumn("X", typeof(double)));
            clickData.Columns.Add(new DataColumn("Y", typeof(double)));
            clickData.Columns.Add(new DataColumn("KeyData", typeof(int)));
            return clickData;
        }

        /// <summary>
        /// Deletes all matching records
        /// </summary>
        /// <returns>Amount of deleted Records</returns>
        public static int DeleteRecords(SqlCeConnection conn, string table, string searchColumn, string searchValue)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "DELETE FROM [" + EscapeLiteral(table) + "] WHERE [" + EscapeLiteral(searchColumn) + "] = @Value;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Value", Value = searchValue });
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Get first matching record
        /// </summary>
        /// <returns>First record that matches a condition</returns>
        public static SqlCeDataReader GetRecord(SqlCeConnection conn, string table, string searchColumn, string searchValue)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TOP 1 * FROM [" + EscapeLiteral(table) + "] WHERE [" + EscapeLiteral(searchColumn) + "] = @Value;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Value", Value = searchValue });
                return cmd.ExecuteReader();
            }
        }


        /// <summary>
        /// Gets all records that match a condition
        /// </summary>
        /// <returns>All records that match a condition</returns>
        public static SqlCeDataReader GetRecords(SqlCeConnection conn, string table, string searchColumn, string searchValue)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT * FROM [" + EscapeLiteral(table) + "] WHERE [" + EscapeLiteral(searchColumn) + "] = @Value;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Value", Value = searchValue });
                return cmd.ExecuteReader();
            }
        }

        /// <summary>
        /// Get all records in a column of the table
        /// </summary>
        /// <returns>All records from the column searchColumn in the table</returns>
        public static DataTable GetRecords(SqlCeConnection conn, string table, string searchColumn)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT " + EscapeLiteral(searchColumn) + " FROM [" + EscapeLiteral(table) + "];";
                SqlCeDataAdapter da = new SqlCeDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                da.Dispose();
                return dt;
            }
        }

        /// <summary>
        /// Checks if Records satisfying a condition exist
        /// </summary>
        /// <returns>True if at least one record is found, else false</returns>
        public static bool RecordExists(SqlCeConnection conn, string table, string searchColumn, string value)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT 1 FROM [" + EscapeLiteral(table) + "] WHERE [" + EscapeLiteral(searchColumn) + "] = @Value;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Value", Value = value });
                return cmd.ExecuteReader().Read();
            }
        }

        /// <summary>
        /// Updates a record
        /// </summary>
        /// <returns>1 if the record was updated, else 0</returns>
        public static int UpdateRecord(SqlCeConnection conn, string table, string targetColumn, string targetValue, string searchColumn, string searchValue)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "UPDATE " + EscapeLiteral(table) + " SET [" + EscapeLiteral(targetColumn) + "] = @TargetValue WHERE [" + EscapeLiteral(searchColumn) + "] = @SearchValue;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@TargetValue", Value = targetValue });
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@SearchValue", Value = searchValue });
                return cmd.ExecuteNonQuery();
            }
        }

        private static string EscapeLiteral(string value)
        {
            return value.Replace("'", "''");
        }
    }
}

