using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;

namespace o_RA
{
    public class DBHelper
    {
        private readonly SqlCeConnection LocalConnection = null;
        private static string _connString = @"Data Source='" + System.IO.Path.Combine(Environment.CurrentDirectory, "db.sdf") + @"';Max Database Size=1024;";

        public DBHelper()
        {
            LocalConnection = new SqlCeConnection(_connString);
            LocalConnection.Open(); ;
        }

        //one-way access, good for large amoutns of data
        #region ExecuteDataReader
        public SqlCeDataReader ExecuteDataReader(string query)
        {
            SqlCeDataReader localReader = null;
            SqlCeCommand localCommand = null;

            try
            {
                LocalConnection.Open(); ;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                localReader = localCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
            }
            return (localReader);
        }

        public SqlCeDataReader ExecuteDataReader(string query, ArrayList parameters)
        {
            SqlCeDataReader localReader = null;
            SqlCeCommand localCommand = null;

            try
            {
                LocalConnection.Open(); ;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlCeParameter localParam in parameters)
                        localCommand.Parameters.Add(localParam);

                    localReader = localCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
            }
            return (localReader);
        }
        #endregion

        //uses in-memory objects
        #region DataSet
        public DataSet ExecuteDataSet(string query)
        {
            DataSet localDataSet = null;
            SqlCeCommand localCommand = null;
            SqlCeDataAdapter localDataAdapter = null;

            try
            {
                LocalConnection.Open(); ;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                localDataAdapter = new SqlCeDataAdapter();
                localDataAdapter.SelectCommand = localCommand;
                localDataSet = new DataSet();

                localDataAdapter.Fill(localDataSet, "DATA");
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
            }
            return (localDataSet);
        }

        public DataSet ExecuteDataSet(string query, ArrayList parameters)
        {
            DataSet localDataSet = null;
            SqlCeCommand localCommand = null;
            SqlCeDataAdapter localDataAdapter = null;

            try
            {
                LocalConnection.Open(); ;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                localDataAdapter = new SqlCeDataAdapter();
                localDataAdapter.SelectCommand = localCommand;
                localDataSet = new DataSet();

                if (parameters != null)
                {
                    foreach (SqlCeParameter localParam in parameters)
                        localCommand.Parameters.Add(localParam);

                    localDataAdapter.Fill(localDataSet, "DATA");
                }
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                localCommand.Dispose();
            }
            return (localDataSet);
        }
        #endregion

        //for scalar results
        #region ExecuteScalar
        public object ExecuteScalar(string query)
        {
            object localScalarObject = string.Empty;
            SqlCeCommand localCommand = null;

            try
            {
                LocalConnection.Open(); ;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                localScalarObject = localCommand.ExecuteScalar();
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
                LocalConnection.Close();
            }
            return localScalarObject;
        }

        public object ExecuteScalar(string query, ArrayList parameters)
        {
            object localScalarObject = string.Empty;
            SqlCeCommand localCommand = null;

            try
            {
                LocalConnection.Open();;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlCeParameter localParam in parameters)
                        localCommand.Parameters.Add(localParam);

                    localScalarObject = localCommand.ExecuteScalar();
                }
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
                LocalConnection.Close();
            }
            return localScalarObject;
        }
        #endregion

        //for insert/delete queries
        #region ExecuteNonQuery
        public int ExecuteNonQuery(string query)
        {
            int rowsAffected = 0;
            SqlCeCommand localCommand = null;

            try
            {
                LocalConnection.Open();;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                rowsAffected = localCommand.ExecuteNonQuery();
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
                LocalConnection.Close();
            }

            return (rowsAffected);
        }

        public int ExecuteNonQuery(string query, ArrayList parameters)
        {
            int rowsAffected = 0;
            SqlCeCommand localCommand = null;

            try
            {
                LocalConnection.Open(); ;

                localCommand = new SqlCeCommand(query, LocalConnection);
                localCommand.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (SqlCeParameter localParam in parameters)
                        localCommand.Parameters.Add(localParam);
                }
                rowsAffected = localCommand.ExecuteNonQuery();
            }
            catch (SqlCeException sqlCeEx)
            {
                throw (sqlCeEx);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (localCommand != null) localCommand.Dispose();
                LocalConnection.Close();
            }
            return (rowsAffected);
        }
        #endregion
    }
}