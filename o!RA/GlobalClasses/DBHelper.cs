//Can't pass some values as parameters
//See http://stackoverflow.com/questions/6843065/error-with-sqlce-parameters

using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using ErikEJ.SqlCe;

namespace o_RA.GlobalClasses
{
    internal static class DBHelper
    {
        public static readonly string dbPath = @"Data Source='" + System.IO.Path.Combine(System.Environment.CurrentDirectory, "db.sdf") + @"';Max Database Size=1024;";

        /// <summary>
        /// Inserts a DataTable object using SqlCeBulkCopy
        /// </summary>
        /// <param name="bC">The SqlBulkCopy connection.</param>
        /// <param name="data">The data to insert</param>
        public static void Insert(SqlCeBulkCopy bC, DataTable data)
        {
            bC.DestinationTableName = data.TableName;
            bC.WriteToServer(data);
        }

        /// <summary>
        /// Inserts DataTable objects using SqlCeBulkCopy
        /// </summary>
        /// <param name="bC">The SqlBulkCopy connection.</param>
        /// <param name="data">The array of data to insert</param>
        public static void MultiInsert(SqlCeBulkCopy bC, DataTable[] data)
        {
            foreach (DataTable t in data)
            {
                Insert(bC, t);
            }
        }

        public static DataTable CreateBeatmapDataTable()
        {
            DataTable beatmapData = new DataTable("Beatmaps");
            beatmapData.Columns.Add(new DataColumn { ColumnName = "Hash", DataType = typeof(string), Unique = true});
            beatmapData.Columns.Add(new DataColumn("Filename", typeof(string)));
            return beatmapData;
        }

        /// <summary>
        /// Get first matching record
        /// </summary>
        /// <returns>First record that matches a condition</returns>
        public static DataRow GetRecord(SqlCeConnection conn, string table, string searchColumn, string searchValue)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TOP 1 * FROM [" + EscapeLiteral(table) + "] WHERE [" + EscapeLiteral(searchColumn) + "] = @Value;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Value", Value = searchValue });
                using (SqlCeDataAdapter da = new SqlCeDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
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
        /// Gets all records that match a condition
        /// </summary>
        /// <returns>All records that match a condition</returns>
        public static DataTable GetRecords(SqlCeConnection conn, string table, string searchColumn, string searchValue)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT * FROM [" + EscapeLiteral(table) + "] WHERE [" + EscapeLiteral(searchColumn) + "] = @Value;";
                cmd.Parameters.Add(new SqlCeParameter { ParameterName = "@Value", Value = searchValue });
                using (SqlCeDataAdapter da = new SqlCeDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
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
                using (SqlCeDataAdapter da = new SqlCeDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
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

