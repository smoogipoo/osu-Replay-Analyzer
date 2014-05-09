#region About SQLCEtools
// SQLCEtools provides static methods for CRUD operations on SQL server
// compact edition database tables. This software was written and tested by
// Bart-Jan Verhoeff in 2011 (c). The software is provided as is, without any 
// warranties and may be used in compliance with the GNU General Public Licence.
// SQLCEtools was published on codeproject.com
//
// Usage: 
// Declare a class in your application according to the following rules:
// 1. The name of the class must match the name of the datatable
// 2. The names of the properties must match the names of the fields in 
//    the datatable. The types must match the types of the fields.
// 3. The property that represents the unique identifier must have the 
//    attribute [UniqueIdentifier] to enable the write- and update-methods to 
//    recognize the identifier of the datatable
// 4. The methods will use all properties except those that are null. Hence, 
//    the types of all properties must be nullable. 
// 5. DateTime is an exception to rule 4. If DateTime is used, a constructor
//    must be declared that equals DateTime to SQLCEtools.DateTimeNull.
// 
// Example:
// class Test : SQLCEtools.BaseCLass
// {
//   [UniqueIdentifier]
//   public int? TestID { get; set; }
//
//   public string TestString { get; set; }
//   public DateTime TestDate {get; set; }
//   
//   public Test()
//   {
//     TestID = null;
//     TestString = null;
//     TestDate = SQLCEtools.DateTimeNull;
//   }
// }
//
// Methods can be added if necessary, for example, to process data
// If you find any bugs please report to bverhoefff-at-yahoo.nl
#endregion

#region usings
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlServerCe;
using System.IO;
using System.ComponentModel;
#endregion

namespace o_RA
{
    #region Attributes
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UniqueIdentifier : System.Attribute
    {
        //public readonly bool Seed;

        //public UniqueIdentifier(bool seed)
        //{
        //    this.Seed = seed;
        //}
    }
    #endregion

    class SQLCEtools
    {
        #region BaseClass
        public class BaseClass
        {
            public bool Equals(BaseClass search)
            {
                throw new NotImplementedException();
            }

            public bool IsIdentifierNull(BaseClass search)
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Variables
        private static DateTime dateTimeNull = new DateTime(1753, 1, 1);
        public static DateTime DateTimeNull { get { return dateTimeNull; } }
        #endregion

        #region showproperties
        private static void showproperties(PropertyInfo[] props)
        {
            string prostr = "";
            PropertyInfo[] prop = props[0].GetType().GetProperties();
            foreach (PropertyInfo p in prop)
            {
                prostr += p.Name + "\n";
            }
            MessageBox.Show(prostr);
        }
        #endregion

        #region ConnectString
        /// <summary>
        /// Create a connectionstring
        /// </summary>
        /// <param name="filePath">path of the database</param>
        /// <param name="fileName">name of the database</param>
        /// <param name="password">password</param>
        /// <returns></returns>
        public static string ConnectString(string filePath, string fileName, string password)
        {
            string connectionString;
            connectionString = string.Format(
            "DataSource={0}{1}; Password='{2}'", filePath, fileName, password);
            return connectionString;
        }
        #endregion

        #region Read(Like)Data
        /// <summary>
        /// Reads datarows from database and adds them to list containing objects of type T.
        /// Note that the properties of T should match the fields of the database table.
        /// </summary>
        /// <param name="data">List containing objects of type T with properties matching fields 
        /// in table.</param>
        /// <param name="search">Object of type T with (some) properties containing search 
        /// constraints, others should be null. Unused DateTime should be 1753-01-01.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>-1 if exception was thrown or the number of records (objects of type T) 
        /// otherwise</returns>
        public static int ReadData<T>(List<T> data, T search, string connect)
            where T : BaseClass, new()
        {
            return BaseRead(data, search, connect, "=");
        }
        /// <summary>
        /// Reads datarows from database and adds them to list containing objects of type T.
        /// Note that the properties of T should match the fields of the database table.
        /// </summary>
        /// <param name="search">Object of type T with (some) properties containing search 
        /// constraints, others should be null. Unused DateTime should be 1753-01-01.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>List of objects of type T containing the searchresults or null if an 
        /// exception was thrown.</returns>
        public static List<T> ReadData<T>(T search, string connect)
            where T : BaseClass, new()
        {
            List<T> data = new List<T>();
            int result = BaseRead(data, search, connect, "=");
            return result >= 0 ? data : null;
        }
        /// <summary>
        /// Reads datarows from database and adds them to list containing objects of type T.
        /// Note that the properties of T should match the fields of the database table.
        /// </summary>
        /// <param name="data">List containing objects of type T with properties matching fields 
        /// in table.</param>
        /// <param name="search">Object of type T with (some) properties containing search 
        /// constraints, others should be null. Unused DateTime should be 1753-01-01.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>-1 if exception was thrown or the number of records (objects of type T) 
        /// otherwise</returns>
        public static int ReadLikeData<T>(List<T> data, T search, string connect)
            where T : BaseClass, new()
        {
            return BaseRead(data, search, connect, "LIKE");
        }
        /// <summary>
        /// Reads datarows from database and adds them to list containing objects of type T.
        /// Note that the properties of T should match the fields of the database table.
        /// </summary>
        /// <param name="search">Object of type T with (some) properties containing search 
        /// constraints, others should be null. Unused DateTime should be 1753-01-01.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>List of objects of type T containing the searchresults or null if an 
        /// exception was thrown.</returns>
        public static List<T> ReadLikeData<T>(T search, string connect)
            where T : BaseClass, new()
        {
            List<T> data = new List<T>();
            int result = BaseRead(data, search, connect, "LIKE");
            return result >= 0 ? data : null;
        }
        private static int BaseRead<T>(List<T> data, T search, string connect,
            string comparer) where T : BaseClass, new()
        {
            // Abort if insufficient arguments
            if (data == null || connect == "") return 0;
            // Make sure List<T> data is empty
            data.Clear();
            // Retrieve name of object of type T (which equals table name)
            string table = typeof(T).Name;
            // Retrieve properties from object of type T 
            PropertyInfo[] propinfs = typeof(T).GetProperties();

            // -----------------------------------------
            // Create string that contains SQL-statement
            // -----------------------------------------
            string fields = ""; string wherestr = "";
            // Retrieve fields from propinf
            foreach (PropertyInfo p in propinfs)
            {
                fields += fields == "" ? p.Name : ", " + p.Name;
                dynamic propvalue = p.GetValue(search, null);
                // Solutions for properties of type DateTime
                DateTime dt = new DateTime();
                Type type = propvalue != null ? propvalue.GetType() : null;
                if (propvalue != null && propvalue.GetType() == dt.GetType()) dt = propvalue;
                // DateTime 1753-01-01 equals null (= DateTimeNull)
                if (propvalue != null && dt != DateTimeNull)
                    wherestr += wherestr == "" ? p.Name + " " + comparer + " @" + p.Name.ToLower()
                        : " AND " + p.Name + " " + comparer + " @" + p.Name.ToLower();
            }
            // Create SQL SELECT statement with properties and search
            string sql = "SELECT " + fields + " FROM " + table;
            sql += wherestr == "" ? "" : " WHERE " + wherestr;

            // -------------------
            // Database operations
            // -------------------
            SqlCeConnection cn = new SqlCeConnection(connect);
            if (cn.State == ConnectionState.Closed) cn.Open();
            try
            {
                SqlCeCommand cmd = new SqlCeCommand(sql, cn);
                cmd.CommandType = CommandType.Text;
                // Add propertyvalues to WHERE-statement using reflection
                foreach (PropertyInfo p in propinfs)
                {
                    dynamic propvalue = p.GetValue(search, null);
                    // Except for DateTime values 1753-01-01 (defined as null)
                    if (propvalue != null && !(propvalue.GetType() is DateTime
                        && propvalue != DateTimeNull))
                    {
                        if (comparer == "LIKE") propvalue = "%" + propvalue + "%";
                        cmd.Parameters.AddWithValue("@" + p.Name.ToLower(), propvalue);
                    }
                }
                SqlCeResultSet rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
                if (rs.HasRows)  // Only if database is not empty
                {
                    while (rs.Read()) // Read next row in database
                    {
                        // Instantiate single item of List data
                        var dataitem = new T();  // Object to put the field-values in
                        foreach (PropertyInfo p in propinfs)
                        {
                            // Read database fields using reflection
                            PropertyInfo singlepropinf = typeof(T).GetProperty(p.Name);
                            int ordinal = rs.GetOrdinal(p.Name);
                            dynamic result = rs.GetValue(ordinal);
                            // Conversion to null in case field is DBNull
                            if (result is DBNull)
                            {
                                if (singlepropinf.PropertyType.Equals(typeof(DateTime)))
                                {
                                    // Fill data item with datetimenull
                                    singlepropinf.SetValue(dataitem, DateTimeNull, null);
                                }
                                else
                                {
                                    // Fill data item with null
                                    singlepropinf.SetValue(dataitem, null, null);
                                }
                            }
                            else
                            {
                                // Or fill data item with value
                                singlepropinf.SetValue(dataitem, result, null);
                            }
                        }
                        data.Add(dataitem);  // And add the record to List<T> data.
                    }
                }
                else
                {
                    //MessageBox.Show("No records matching '" + wherestr + "'!");
                    return 0;
                }
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "SQL-error.", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                cn.Close();
            }
            // Return number of objects (should equal number of retrieved records)
            return data.Count();
        }
        #endregion

        #region DeleteData
        ///----------------------------------------------------------------------
        /// <summary>
        /// Deletes datarows from database. Note that the properties of T should 
        /// match the fields of the database table.
        /// </summary>
        /// <param name="table">Table in database.</param>
        /// <param name="search">Object of type T with (some) properties 
        /// containing search constraints, others should be null. Unused DateTime 
        /// should be 1753-01-01.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>-1 if exception was thrown or the number of affected records 
        /// otherwise</returns>
        ///----------------------------------------------------------------------
        public static int DeleteData<T>(T search, string connect) where T : BaseClass, new()
        {
            string table = typeof(T).Name;
            return DeleteData<T>(table, search, connect);
        }
        public static int DeleteData<T>(string table, T search, string connect)
            where T : BaseClass, new()
        {
            // Abort if insufficient arguments
            if (table == "" || connect == "") return 0;
            // Retrieve properties from object of type T 
            PropertyInfo[] propinfs = typeof(T).GetProperties();

            // -----------------------------------------
            // Create string that contains SQL-statement
            // -----------------------------------------
            string wherestr = "";
            // Retrieve fields from propinf
            foreach (PropertyInfo p in propinfs)
            {
                dynamic propvalue = p.GetValue(search, null);
                // Solutions for properties of type DateTime
                long dateticks = 0; DateTime dt = new DateTime();
                Type type = propvalue != null ? propvalue.GetType() : null;
                if (propvalue != null && propvalue.GetType() == dt.GetType())
                {
                    dt = propvalue;
                    dateticks = dt.Ticks;
                }
                // DateTime 1753-01-01 equals null (hey, it's better than nothing...)
                if (propvalue != null && dt != DateTimeNull)
                    wherestr += wherestr == "" ? p.Name + " = @" + p.Name.ToLower()
                        : " AND " + p.Name + " = @" + p.Name.ToLower();
            }
            // Create SQL DELETE statement with properties and search
            string sql = "DELETE FROM " + table;
            sql += wherestr == "" ? "" : " WHERE " + wherestr;

            // -------------------
            // Database operations
            // -------------------
            SqlCeConnection cn = new SqlCeConnection(connect);
            if (cn.State == ConnectionState.Closed) cn.Open();
            int result = 0;
            try
            {
                SqlCeCommand cmd = new SqlCeCommand(sql, cn);
                cmd.CommandType = CommandType.Text;
                // Add propertyvalues to WHERE-statement using reflection
                foreach (PropertyInfo p in propinfs)
                {
                    dynamic propvalue = p.GetValue(search, null);
                    // Except for DateTime values 1753-01-01 (defined as null)
                    if (propvalue != null && !(propvalue.GetType() is DateTime
                        && propvalue != DateTimeNull))
                        cmd.Parameters.AddWithValue("@" + p.Name.ToLower(), propvalue);
                }
                result = cmd.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "SQL-error.", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                cn.Close();
            }
            // Return number of objects (should equal number of retrieved records)
            return result;
        }
        #endregion

        #region UpdateData
        ///----------------------------------------------------------------------
        /// <summary>
        /// Updates a datarow in a database table. Unique identifier must be in 
        /// first column. Properties/Fields may not be null.
        /// </summary>
        /// <param name="data">Objects with properties matching fields in table.</param>
        /// <param name="table">Table in database.</param>
        /// <param name="search">Substring of SQL-statement that follows 'WHERE'.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>-1 if exception was thrown or the result of ExecuteNonQuery otherwise</returns>
        ///----------------------------------------------------------------------
        public static int UpdateData<T>(T data, T search, string connect) where T : BaseClass, new()
        {
            string table = typeof(T).Name;
            return UpdateData(data, table, search, connect);
        }
        private static int UpdateData<T>(T data, string table, T search, string connect)
            where T : BaseClass, new()
        {
            // Abort if insufficient arguments
            if (data == null || table == "" || search == null || connect == "") return 0;
            // Number of Ticks that equal 1753-01-01 in DateTime (is defined as null)
            // Retrieve properties from object of type T 
            PropertyInfo[] propinfs = typeof(T).GetProperties();

            // -----------------------------------------
            // Create string that contains SQL-statement
            // -----------------------------------------
            string fields = ""; string wherestr = "";
            // Retrieve fields from propinf
            foreach (PropertyInfo p in propinfs)
            {
                // don't change unique identifier
                if ((p.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length == 0)
                {
                    string field = p.Name + "=@" + p.Name.ToLower();
                    fields += fields == "" ? field : ", " + field;
                }
                dynamic propvalue = p.GetValue(search, null);
                // Solutions for properties of type DateTime
                long dateticks = 0; DateTime dt = new DateTime();
                Type type = propvalue != null ? propvalue.GetType() : null;
                if (propvalue != null && propvalue.GetType() == dt.GetType())
                {
                    dt = propvalue;
                    dateticks = dt.Ticks;
                }
                // DateTime 1753-01-01 equals null (hey, it's better than nothing...)
                if (propvalue != null && dt != DateTimeNull)
                    wherestr += wherestr == "" ? p.Name + " = @" + p.Name.ToLower() + "1" : ", " +
                        p.Name + " = @" + p.Name.ToLower();
            }
            // Create SQL UPDATE statement with properties and search
            string sql = "UPDATE " + table + " SET " + fields;
            if (wherestr == "") return 0;
            sql += wherestr == "" ? "" : " WHERE " + wherestr;

            // -------------------
            // Database operations
            // -------------------
            SqlCeConnection cn = new SqlCeConnection(connect);
            if (cn.State == ConnectionState.Closed) cn.Open();
            try
            {
                SqlCeCommand cmd = new SqlCeCommand(sql, cn);
                cmd.CommandType = CommandType.Text;
                // Add propertyvalues to WHERE-statement using reflection
                foreach (PropertyInfo p in propinfs)
                {
                    dynamic propvalue = p.GetValue(search, null);
                    // Except for DateTime values 1753-01-01 (defined as null)
                    if (propvalue != null && !(propvalue.GetType() is DateTime
                        && propvalue.Ticks != DateTimeNull))
                        cmd.Parameters.AddWithValue("@" + p.Name.ToLower() + "1", propvalue);
                }
                //SqlCeResultSet rs = cmd.ExecuteResultSet(ResultSetOptions.Scrollable);
                // Instantiate single item of List data
                var dataitem = new T();  // Object to put the field-values in
                foreach (PropertyInfo p in propinfs)
                {
                    if ((p.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length == 0)
                    {
                        // Use dynamic type to substitute propertyvalues
                        PropertyInfo singlepropinf = typeof(T).GetProperty(p.Name);
                        dynamic propertyvalue = singlepropinf.GetValue(data, null);
                        // Check DateTime values
                        long dateticks = 0; DateTime dt = new DateTime();
                        Type type = propertyvalue != null ? propertyvalue.GetType() : null;
                        if (propertyvalue != null && propertyvalue.GetType() == dt.GetType())
                        {
                            dt = propertyvalue;
                            dateticks = dt.Ticks;
                        }
                        // If propertyvalue is not DateTime OR if DateTime is not DateTimeNull
                        if (dateticks == 0 || (dateticks != 0 && dt != DateTimeNull))
                        {
                            // check if propertyvalue is null => convert to DBNull.Value
                            propertyvalue = (dateticks == 0 && propertyvalue == null) ?
                                DBNull.Value : propertyvalue;
                            cmd.Parameters.AddWithValue("@" + p.Name.ToLower(), propertyvalue);
                        }
                        else
                        {
                            // If DateTime is DateTimeNull then substitute with null
                            SqlDateTime dbdt = SqlDateTime.Null;
                            cmd.Parameters.AddWithValue("@" + p.Name.ToLower(), dbdt);
                        }
                    }
                }
                if (cmd.ExecuteNonQuery() != 1)
                {
                    return 0;
                }
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "SQL-error.", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                cn.Close();
            }
            // Should have succeeded now
            return 1;
        }
        #endregion

        #region WriteData
        ///----------------------------------------------------------------------
        /// <summary>
        /// Writes datarows from list to database. Unique identifier must be in 
        /// first column. Properties/Fields may not be null.
        /// </summary>
        /// <param name="data">Object with properties matching fields in table.</param>
        /// <param name="table">Table in database.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>-1 if exception was thrown or the result of ExecuteNonQuery 
        /// otherwise</returns>
        ///----------------------------------------------------------------------
        public static int WriteData<T>(T data, string connect) where T : BaseClass, new()
        {
            string table = typeof(T).Name;
            return WriteData(data, table, connect);
        }
        public static int WriteData<T>(T data, string table, string connect)
            where T : BaseClass, new()
        {
            List<T> listdata = new List<T>();
            listdata.Add(data);
            return WriteData(listdata, table, connect);
        }

        ///----------------------------------------------------------------------
        /// <summary>
        /// Writes datarows from list to database. Unique identifier must be in 
        /// first column. Properties/Fields may not be null.
        /// </summary>
        /// <param name="data">List of objects with properties matching fields in table.</param>
        /// <param name="table">Table in database.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>-1 if exception was thrown or the result of ExecuteNonQuery 
        /// otherwise</returns>
        ///----------------------------------------------------------------------
        public static int WriteData<T>(List<T> data, string connect) where T : BaseClass, new()
        {
            string table = typeof(T).Name;
            return WriteData(data, table, connect);
        }
        public static int WriteData<T>(List<T> data, string table, string connect)
            where T : BaseClass, new()
        {
            // success counter
            int success = 0;
            // Return if missing input
            if (data == null || table == "" || connect == "") return 0;
            // Retrieve properties from object of type T using reflection
            PropertyInfo[] propinfs = typeof(T).GetProperties();

            // --------------------------------
            // Create SQL INSERT INTO statement
            // --------------------------------
            string fields = ""; string values = "";
            foreach (PropertyInfo p in propinfs)
            {
                // Only add to sqlstatement if not a Unique Identifier
                if ((p.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length == 0)
                {
                    fields += fields == "" ? p.Name : ", " + p.Name;
                    values += values == "" ? p.Name.ToLower() : ", @" + p.Name.ToLower();
                }
            }
            // Combine strings in SQL-statement
            string sql = "INSERT INTO " + table + " (" + fields + ") VALUES (@" + values + ")";

            // -------------------
            // Database operations
            // -------------------
            SqlCeConnection cn = new SqlCeConnection(connect);
            if (cn.State == ConnectionState.Closed)
            {
                cn.Open();
            }
            try
            {
                SqlCeCommand cmd = new SqlCeCommand(sql, cn);
                cmd.CommandType = CommandType.Text;
                bool firstRecord = true;
                foreach (var dat in data)
                {
                    PropertyInfo ID = null;
                    foreach (PropertyInfo p in propinfs)
                    {
                        // Unique identifier is remembered to put ID value in, 
                        // rest is used for insert statement.
                        if ((p.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length > 0)
                        {
                            ID = p;
                        }
                        else
                        {
                            // Use dynamic type to substitute propertyvalues
                            PropertyInfo singlepropinf = typeof(T).GetProperty(p.Name);
                            dynamic propertyvalue = singlepropinf.GetValue(dat, null);
                            // Check DateTime values
                            long dateticks = 0; DateTime dt = new DateTime();
                            Type type = propertyvalue != null ? propertyvalue.GetType() : null;
                            // check for type DateTime (dateticks == 0 means no DateTime)
                            if (propertyvalue != null && propertyvalue.GetType() == dt.GetType())
                            {
                                dt = propertyvalue;
                                dateticks = dt.Ticks;
                            }
                            // If propertyvalue is not DateTime OR if DateTime is not 1753-01-01
                            if (dateticks == 0 || (dateticks != 0 && dt != DateTimeNull))
                            {
                                // check if propertyvalue is null => convert to DBNull.Value
                                propertyvalue = (dateticks == 0 && propertyvalue == null) ?
                                    DBNull.Value : propertyvalue;
                                if (firstRecord) cmd.Parameters.AddWithValue("@" + p.Name.ToLower(),
                                    propertyvalue);
                                else cmd.Parameters["@" + p.Name.ToLower()].Value = propertyvalue;
                            }
                            else
                            {
                                // If DateTime is DateTimeNull then substitute with null
                                SqlDateTime dbdt = SqlDateTime.Null;
                                if (firstRecord) cmd.Parameters.AddWithValue
                                    ("@" + p.Name.ToLower(), dbdt);
                                else cmd.Parameters["@" + p.Name.ToLower()].Value = dbdt;
                            }
                        }
                    }
                    // execute INSERT-command
                    success += cmd.ExecuteNonQuery();
                    // only add parameters to sqlstatement if this was not done before
                    firstRecord = false;
                    // now create new command to get identity
                    cmd.CommandText = "SELECT @@IDENTITY";
                    // Get new identifier value, convert to int32
                    int propID = Convert.ToInt32((decimal)cmd.ExecuteScalar());
                    // And put identifier value in ID property for later reference
                    ID.SetValue(dat, propID, null);
                    // change commandtext back to original text
                    cmd.CommandText = sql;
                }
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message, "SQL-error.", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                cn.Close();
            }
            // Successful
            return success;
        }
        #endregion

        # region ResetIdentifier
        ///----------------------------------------------------------------------
        /// <summary>
        /// Resets the unique identifier to 0 and the autoincrement value to 1.
        /// Use with care. (usually only if table is empty!)
        /// </summary>
        /// <param name="idcolumn">Name of the unique identifier.</param>
        /// <param name="table">Table in database.</param>
        /// <param name="connect">Connectionstring.</param>
        /// <returns>true if successful</returns>
        ///----------------------------------------------------------------------
        public static bool ResetIdentifier<T>(T data, string idcolumn, string table, string connect)
            where T : BaseClass, new()
        {
            List<T> list = new List<T>();
            T search = new T();
            int readdata = ReadData(list, search, connect);
            if (readdata == 0)
            {
                SqlCeConnection cn = new SqlCeConnection(connect);
                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }
                try
                {
                    SqlCeCommand cmd = new SqlCeCommand("ALTER TABLE " + table + " ALTER COLUMN "
                        + idcolumn + " IDENTITY (0,1)", cn);
                    cmd.ExecuteNonQuery();
                }
                catch (SqlCeException sqlexception)
                {
                    MessageBox.Show(sqlexception.Message, "SQL-error.", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    cn.Close();
                }
                return true;
            }
            else
            {
                if (readdata > 0) MessageBox.Show("Table is not empty!", "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        #endregion

        #region GetSqlDBTypeFromType
        /// <summary>
        /// Gets the correct SqlDBType for a given .NET type. Useful for working with SQL CE.
        /// </summary>
        /// <param name="type">The .Net Type used to find the SqlDBType.</param>
        /// <returns>The correct SqlDbType for the .Net type passed in.</returns>
        public static SqlDbType GetSqlDBTypeFromType(Type type)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(DbType));
            DbType dbType = (DbType)tc.ConvertFrom(type.Name);
            SqlCeParameter param = new SqlCeParameter();
            param.DbType = dbType;
            return param.SqlDbType;
        }
        #endregion

        #region ReadFast
        /// <summary>
        /// This method has not yet been thoroughly tested! Has more or less the same functionality 
        /// as readdata but with tabledirect and using an index. The performance benefit probably 
        /// only starts to become significant when more than +-2500 records are retrieved. In this 
        /// method the penalty for using reflection becomes apparent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="search"></param>
        /// <param name="indexname"></param>
        /// <param name="connect"></param>
        /// <returns></returns>
        public static int ReadFast<T>(List<T> data, T search, string indexname, string connect)
            where T : BaseClass, new()
        {
            // Prepare connection
            SqlCeConnection conn = new SqlCeConnection(connect);
            SqlCeCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.TableDirect;
            SqlCeDataReader rdr = null;

            // Make sure List<T> data is empty
            data.Clear();

            // Flag error
            bool error = false;

            // Retrieve properties from object of type T 
            PropertyInfo[] propinfs = typeof(T).GetProperties();

            try
            {
                // This is the name of the base table. 
                string table = typeof(T).Name;
                cmd.CommandText = table;

                //Assume: Index contains one column, value is provided by 'search'
                cmd.IndexName = indexname;

                // Instantiate object containing searchterm
                object[] obj = new object[1];

                // Add propertyvalues to WHERE-statement using reflection
                foreach (PropertyInfo p in propinfs)
                {
                    dynamic propvalue = p.GetValue(search, null);
                    // Except for DateTime values 1753-01-01 (defined as null)
                    if (propvalue != null)
                    {
                        Type t = propvalue.GetType();
                        if (t.Name == "DateTime")
                        {
                            if (propvalue != DateTimeNull) obj[0] = propvalue;
                        }
                        else obj[0] = propvalue;
                    }
                }

                cmd.SetRange(DbRangeOptions.Match, obj, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception caught initiating SqlCeCommand: " + ex.ToString(),
                    "Exception");
                error = true;
            }

            if (!error) try
                {
                    conn.Open();

                    rdr = cmd.ExecuteReader(CommandBehavior.Default);

                    while (rdr.Read())
                    {
                        // Instantiate single item of List data
                        var dataitem = new T();  // Object to put the field-values in
                        foreach (PropertyInfo p in propinfs)
                        {
                            // Read database fields using reflection
                            PropertyInfo singlepropinf = typeof(T).GetProperty(p.Name);
                            //int ordinal = rs.GetOrdinal(p.Name);
                            dynamic result = rdr[p.Name];
                            // Conversion to null in case field is DBNull
                            if (result is DBNull)
                            {
                                if (singlepropinf.PropertyType.Equals(typeof(DateTime)))
                                {
                                    // Fill data item with datetimenull
                                    singlepropinf.SetValue(dataitem, DateTimeNull, null);
                                }
                                else
                                {
                                    // Fill data item with null
                                    singlepropinf.SetValue(dataitem, null, null);
                                }
                            }
                            else
                            {
                                // Or fill data item with value
                                singlepropinf.SetValue(dataitem, result, null);
                            }
                        }
                        data.Add(dataitem);  // And add the record to List<T> data.
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception caught reading table: " + ex.ToString(), "Exception");
                    error = true;
                }
                finally
                {
                    rdr.Close();
                    conn.Close();
                }
            if (error) return -1;
            return data.Count;
        }
        #endregion
    }
}