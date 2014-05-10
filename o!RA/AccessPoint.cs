#region About EasySqlCe
// EasySqlCe provides methods that enable easy access to an SqlCe datatable.
// 
// The following SQL-statements are implemented as methods:
// - CREATE TABLE ...
// - INSERT ...
// - SELECT ... WHERE ... (..LIKE..)
// - UPDATE ... SET ... WHERE ... (..LIKE..)
// - DELETE ... WHERE ... (..LIKE..)
// - ALTER TABLE (only to reset the Unique Identifier of an empty table)
// 
// Furthermore, a datatable can be read using tabledirect-mode.

// This software was written and tested by Bart-Jan Verhoeff in 2012 (c). 
// The software is provided as is, without any warranties and may be used 
// in compliance with the GNU General Public Licence. The functionality was
// first published on codeproject.com in 2011 as SQLCEtools(c). However, 
// SQLCEtools needed some thorough refactoring. The present project is the 
// result. 
//
// Main differences in use from SQLCEtools:
// - you need to reference EasySqlCe: using EasySqlCe.
// - you need to instantiate an 'AccessPoint', see Demo-project.
// - The filename, filepath and password are to be set in de respective
// properties in de instance of AccessPoint. 
// - Whether or not the Where-Clause is used with 'LIKE' instead of '=' 
// needs to be set in the property 'UseLikeStatement'. Also specify on
// which sides the wildcards should be used ('%').
// - DateTime needs to be nullable as well: 'DateTime?', this obviates
// the need for 'DateTimeNull' and a lot of extra code. I don't understand
// why I used it in the first place, probably because I started programming 
// in C# at that time.
//
// Important rules: 
// Declare a class in your application according to the following rules:
// 1. The name of the class must match the name of the datatable
// 2. The names of the properties must match the names of the fields in 
//    the datatable. The types must match the types of the fields.
// 3. The property that represents the unique identifier must have the 
//    attribute [UniqueIdentifier] to enable the write- and update-methods to 
//    recognize the identifier of the datatable
// 4. The Unique Identifier must autoincrement.
// 5. The methods will use all properties except those that are null. Hence, 
//    the types of all properties must be nullable. 
// 6. Check for conversion of types:
// http://msdn.microsoft.com/en-us/library/system.data.sqldbtype.aspx
// 7. Check for supported types:
// http://msdn.microsoft.com/en-us/library/ms172424(SQL.110).aspx
// NOTE: currently tested .NET types are: int, DateTime, string, byte[],
//    bool
// 
// Example:
// class Test : SQLCEtools.BaseCLass
// {
//   ..Constructor..
//
//   [UniqueIdentifier]
//   public int? TestID { get; set; }
//
//   public string TestString { get; set; }
//   public DateTime? TestDate { get; set; }
//   public bool? SendChristmasCard { get; set; }
// }
//
// Methods can be added if necessary, for example, to process data
// If you find any bugs please report to bverhoefff at yahoo.com
#endregion

#region Usings

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Data;
//using System.Data.SqlTypes;
using System.Data.SqlServerCe;
using System.IO;
using System.ComponentModel;
//using System.Runtime.Serialization;

#endregion

namespace o_RA
{
    #region Template class

    public class Template
    {
        public Template()
        {
            this.UID = null;
        }

        [UniqueIdentifier]
        public int? UID { get; set; }
    }

    #endregion

    #region Custom Attributes

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UniqueIdentifier : System.Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FieldInfo : System.Attribute
    {
        public int Length;
    }

    #endregion

    public class AccessPoint
    {
        #region Constructor

        public AccessPoint()
        {
            WhereClauseComparer = "=";
            WildCardLeft = true;
            WildCardRight = true;
            _UseLikeStatement = false;
            Filename = "";
            Filepath = "";
            password = "";
        }

        public AccessPoint(string Filepath, string Filename, string Password)
        {
            WhereClauseComparer = "=";
            WildCardLeft = true;
            WildCardRight = true;
            _UseLikeStatement = false;
            this.Filename = Filepath;
            this.Filepath = Filename;
            this.password = Password;
            InitiateConnection();
        }

        #endregion

        #region Public Properties

        private bool _UseLikeStatement;
        /// <summary>
        /// When true, the 'LIKE'-statement must be used in a 
        /// where-clause of an SQL-statement
        /// </summary>
        public bool UseLikeStatement
        {
            get
            {
                return _UseLikeStatement;
            }
            set
            {
                _UseLikeStatement = value;
                if (value) WhereClauseComparer = "LIKE";
                else WhereClauseComparer = "=";
            }
        }
        /// <summary>
        /// When true, '%' is added on the left side of a search string
        /// </summary>
        public bool WildCardLeft { get; set; }
        public bool WildCardRight { get; set; }
        public string Filepath { get; set; }
        public string Filename { get; set; }
        private string password { get; set; }
        public string Password { set { password = value; } }

        #endregion

        #region Private Properties

        string WhereClauseComparer;
        PropertyInfo[] PropertiesOfT;
        PropertyInfo CurrentUniqueIdentifier;
        ConnectionState PreviousConnectionState = new ConnectionState();
        SqlCeConnection Connection;

        #endregion

        #region Public Methods

        #region GetCurrentPath

        public string GetCurrentPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }

        #endregion

        #region CheckDatabase

        public bool CheckDatabase()
        {
            return File.Exists(FilePathAndName());
        }

        #endregion

        #region CreateDataBase

        public bool CreateDataBase(bool CheckIfExists)
        {
            if (!AccessPointReady()) return false;
            if (CheckIfExists && CheckDatabase()) return false;
            File.Delete(FilePathAndName());
            SqlCeEngine engine = new SqlCeEngine(ConnectionString() + "; Encrypt = TRUE;");
            try
            {
                engine.CreateDatabase();
                return true;
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region CheckTable

        public bool CheckTable<T>(T TableItem) where T : class, new()
        {
            SetPropertyInfosAndUniqueIdentifier(TableItem);
            string sqlStatement = ConstructSQLStatementCheckTable();
            try
            {
                PrepareAndOpenConnection();
                SqlCeCommand command = GetSqlCeCommand(Connection, sqlStatement);
                SqlCeResultSet ResultSet = command.ExecuteResultSet(ResultSetOptions.Scrollable);
                if (ResultSet.HasRows) while (ResultSet.Read())
                    {
                        object result = ResultSet.GetValue(2);
                        if ((string)result == typeof(T).Name) return true;
                    }
                return false;
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                ReturnToPreviousConnectionState();
            }
        }

        #endregion

        #region CreateTable
        /// <summary>
        /// Creates a new Table in a Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableItem"></param>
        /// <returns></returns>
        public bool CreateTable<T>(T TableItem) where T : class, new()
        {
            if (!AccessPointReady()) return false;
            if (TableItem == null) return false;
            SetPropertyInfosAndUniqueIdentifier(TableItem);
            string sqlStatement = ConstructSQLStatementCreateTable(TableItem);
            return ExecuteSqlStatement(sqlStatement);
        }

        #endregion

        #region Insert

        public int Insert<T>(T dataItem) where T : class, new()
        {
            List<T> dataList = new List<T>();
            dataList.Add(dataItem);
            return Insert(dataList);
        }

        public int Insert<T>(List<T> DataList) where T : class, new()
        {
            if (!AccessPointReady()) return 0;
            if (DataList == null || DataList.Count == 0) return 0;
            SetPropertyInfosAndUniqueIdentifier(DataList[0]);
            string insertStatement = ConstructSQLStatementInsert(DataList[0]);
            SqlCeConnection connection = new SqlCeConnection(ConnectionString());
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                SqlCeCommand command = GetSqlCeCommand(connection, insertStatement);
                AddParametersWithValuesFromProperties(DataList[0], command, Suffix.Insert);
                return InsertDataListAndRetrieveIdentifier(DataList, command);
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        #region Select

        public List<T> Select<T>(T SearchItem) where T : class, new()
        {
            if (!AccessPointReady()) return null;
            SetPropertyInfosAndUniqueIdentifier(SearchItem);
            List<T> dataList = new List<T>();
            string selectStatement = ConstructSQLStatementSelect(SearchItem);

            SqlCeConnection connection = new SqlCeConnection(ConnectionString());
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                SqlCeCommand command = GetSqlCeCommand(connection, selectStatement);
                AddParametersWithValuesFromProperties(SearchItem, command, Suffix.Where);
                SqlCeResultSet ResultSet = command.ExecuteResultSet(ResultSetOptions.Scrollable);
                if (ResultSet.HasRows) while (ResultSet.Read()) dataList.Add(FillObjectWithResultSet(new T(), ResultSet));
                return dataList;
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        #region CheckIndex
        /// <summary>
        /// Checks if an index exists for the property of T that is not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableItem"></param>
        /// <returns>the name of the index</returns>
        public string CheckIndex<T>(T TableItem) where T : class, new()
        {
            if (!AccessPointReady()) return null;
            SetPropertyInfosAndUniqueIdentifier(TableItem);
            string sqlStatement = ConstructSQLStatementCheckIndex();
            SqlCeConnection connection = new SqlCeConnection(ConnectionString());
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                SqlCeCommand command = GetSqlCeCommand(connection, sqlStatement);
                SqlCeResultSet ResultSet = command.ExecuteResultSet(ResultSetOptions.Scrollable);
                if (ResultSet.HasRows) while (ResultSet.Read())
                    {
                        object result = ResultSet.GetValue(5);
                        if ((string)result == GetIndexName(TableItem)) return (string)result;
                    }
                return "";
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        #region CreateIndex
        /// <summary>
        /// Creates an index of a single field that corresponds to the property in 
        /// object T that is not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="IndexItem"></param>
        /// <returns>true if successful</returns>
        public bool CreateIndex<T>(T IndexItem) where T : class, new()
        {
            if (!AccessPointReady()) return false;
            SetPropertyInfosAndUniqueIdentifier(IndexItem);
            string sqlStatement = ConstructSQLStatementCreateIndex(IndexItem);
            return ExecuteSqlStatement(sqlStatement);
        }

        #endregion

        #region TableDirectReader

        public List<T> TableDirectReader<T>(T SearchItem, T IndexItem) where T : class, new()
        {
            return TableDirectReader(SearchItem, GetIndexName(IndexItem));
        }

        public List<T> TableDirectReader<T>(T SearchItem, string IndexName) where T : class, new()
        {
            if (!AccessPointReady()) return null;
            SetPropertyInfosAndUniqueIdentifier(new T());
            SqlCeConnection connection = new SqlCeConnection(ConnectionString());
            SqlCeDataReader dataReader = null;
            List<T> resultsList = new List<T>();
            try
            {
                SqlCeCommand command = GetCommandTableDirect(connection, IndexName, typeof(T).Name);
                SetRangeOfTableDirect(SearchItem, command);
                connection.Open();
                dataReader = command.ExecuteReader(CommandBehavior.Default);
                DataReaderToResultsList(resultsList, dataReader);
                return resultsList;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                dataReader.Close();
                connection.Close();
            }
        }

        void SetRangeOfTableDirect<T>(T search, SqlCeCommand Command) where T : class, new()
        {
            object[] obj = new object[1];
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                object propertyValue = propertyOfT.GetValue(search, null);
                if (propertyValue != null) obj[0] = propertyValue;
            }
            Command.SetRange(DbRangeOptions.Match, obj, null);
        }

        void DataReaderToResultsList<T>(List<T> ResultsList, SqlCeDataReader DataReader) where T : class, new()
        {
            while (DataReader.Read())
            {
                var resultItem = new T();
                foreach (PropertyInfo propertyOfT in PropertiesOfT)
                {
                    object result = DataReader[propertyOfT.Name];
                    if (result is DBNull) propertyOfT.SetValue(resultItem, null, null);
                    else propertyOfT.SetValue(resultItem, result, null);
                }
                ResultsList.Add(resultItem);
            }
        }

        SqlCeCommand GetCommandTableDirect(SqlCeConnection Connection, string IndexName, string TableName)
        {
            SqlCeCommand command = Connection.CreateCommand();
            command.CommandType = CommandType.TableDirect;
            command.CommandText = TableName;
            command.IndexName = IndexName;
            return command;
        }

        #endregion

        #region Update

        public int Update<T>(T SetItem, T WhereItem) where T : class, new()
        {
            if (!AccessPointReady()) return 0;
            if (SetItem == null || WhereItem == null) return 0;
            SetPropertyInfosAndUniqueIdentifier(WhereItem);
            string sqlStatement = ConstructSQLStatementUpdate(SetItem, WhereItem);

            SqlCeConnection connection = new SqlCeConnection(ConnectionString());
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                SqlCeCommand command = GetSqlCeCommand(connection, sqlStatement);
                AddParametersWithValuesFromProperties(WhereItem, command, Suffix.Where);
                AddParametersWithValuesFromProperties(SetItem, command, Suffix.Insert);
                return command.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        #region Delete

        public int Delete<T>(T SearchItem) where T : class, new()
        {
            if (!AccessPointReady()) return 0;
            if (SearchItem == null) return 0;
            SetPropertyInfosAndUniqueIdentifier(SearchItem);
            string sqlStatement = ConstructSQLStatementDelete(SearchItem);

            SqlCeConnection connection = new SqlCeConnection(ConnectionString());
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                SqlCeCommand command = GetSqlCeCommand(connection, sqlStatement);
                AddParametersWithValuesFromProperties(SearchItem, command, Suffix.Where);
                return command.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        # region ResetIdentifier
        /// <summary>
        /// Resets identifier if the table is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SearchItem"></param>
        /// <returns>True if successful</returns>
        public bool ResetIdentifier<T>(T SearchItem) where T : class, new()
        {
            if (!AccessPointReady()) return false;
            List<T> searchResult = Select(new T());
            if (searchResult.Count == 0)
            {
                SetPropertyInfosAndUniqueIdentifier(SearchItem);
                SqlCeConnection connection = new SqlCeConnection(ConnectionString());
                try
                {
                    if (connection.State == ConnectionState.Closed) connection.Open();
                    string tableName = typeof(T).Name;
                    SqlCeCommand command = new SqlCeCommand("ALTER TABLE " + tableName + " ALTER COLUMN "
                        + CurrentUniqueIdentifier.Name + " IDENTITY (0,1)", connection);
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (SqlCeException sqlexception)
                {
                    MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                if (searchResult.Count > 0) MessageBox.Show("Table is not empty!", "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        #endregion

        #endregion

        #region Private Methods

        #region InitiateConnection

        void InitiateConnection()
        {
            Connection = new SqlCeConnection(ConnectionString());
        }

        #endregion

        #region ConnectionString

        string FilePathAndName()
        {
            return string.Format(Path.Combine(Filepath,Filename));
        }

        string ConnectionString()
        {
            return string.Format("DataSource='{0}'; Password='{1}'", FilePathAndName(), password);
        }

        bool AccessPointReady()
        {
            if (this.Filename.Length == 0) return false;
            return true;
        }

        #endregion

        #region Type conversion

        bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        Type GetUnderlyingTypeIfNullable(Type type)
        {
            if (IsNullableType(type))
            {
                NullableConverter nc = new NullableConverter(type);
                type = nc.UnderlyingType;
            }
            return type;
        }

        int GetAttributeFieldInfoLength(PropertyInfo PropertyOfInterest)
        {
            FieldInfo[] fieldInfoLength = (FieldInfo[])PropertyOfInterest.GetCustomAttributes(typeof(FieldInfo), true);
            if (fieldInfoLength.Length == 1) return fieldInfoLength[0].Length;
            return 0;
        }

        SqlDbType GetNTextOrNVarChar(ref int length)
        {
            if (length > 4000)
            {
                length = 0;  // length is not allowed with ntext
                return SqlDbType.NText;
            }
            if (length == 0) length = 256;  // default length
            return SqlDbType.NVarChar;
        }

        SqlDbType GetSqlDbTypeFromDotNetType(Type type)
        {
            if (type == typeof(byte[])) return SqlDbType.Image; // exception to the rule
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(DbType));
            type = GetUnderlyingTypeIfNullable(type);
            SqlCeParameter param = new SqlCeParameter();
            param.DbType = (DbType)typeConverter.ConvertFrom(type.Name);
            return param.SqlDbType;
        }

        SqlDbType GetSqlDbTypeFromProperty(PropertyInfo PropertyOfInterest, ref int Length)
        {
            Type type = PropertyOfInterest.PropertyType;
            if (type == typeof(byte[])) return SqlDbType.Image;
            Length = GetAttributeFieldInfoLength(PropertyOfInterest);
            if (type == typeof(string)) return GetNTextOrNVarChar(ref Length);
            return GetSqlDbTypeFromDotNetType(type);
        }

        string CheckLengthValue(SqlDbType DbType, int Length)
        {
            if (Length > 0) return string.Format(Enum.GetName(typeof(SqlDbType), DbType) + " ({0})", Length);
            else return Enum.GetName(typeof(SqlDbType), DbType);
        }

        string GetSqlCeTypeNameAndLength(PropertyInfo PropertyOfT)
        {
            int stringLength = 0;
            SqlDbType columnType = GetSqlDbTypeFromProperty(PropertyOfT, ref stringLength);
            return CheckLengthValue(columnType, stringLength).ToLower();
        }

        #endregion

        #region SqlExecution

        bool PrepareAndOpenConnection()
        {
            if (!AccessPointReady()) return false;
            if (!ConnectionExists()) InitiateConnection();
            OpenConnectionAndRememberPreviousState();
            return true;
        }

        bool ConnectionExists()
        {
            if (Connection == null) return false;
            return true;
        }

        void ChangeConnectionState(ConnectionState State)
        {
            try
            {
                if (State == ConnectionState.Open) Connection.Open();
                if (State == ConnectionState.Closed) Connection.Close();
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenConnectionAndRememberPreviousState()
        {
            PreviousConnectionState = Connection.State;
            if (Connection.State == ConnectionState.Closed) ChangeConnectionState(ConnectionState.Open);
        }

        void ReturnToPreviousConnectionState()
        {
            if (PreviousConnectionState != Connection.State) ChangeConnectionState(PreviousConnectionState);
        }

        bool ExecuteSqlStatement(string SqlStatement)
        {
            Connection = new SqlCeConnection(ConnectionString());
            OpenConnectionAndRememberPreviousState();
            try
            {
                SqlCeCommand command = GetSqlCeCommand(Connection, SqlStatement);
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlCeException sqlexception)
            {
                MessageBox.Show(sqlexception.Message + "\n\n" + sqlexception.StackTrace, "SQL-error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\n\n" + exception.StackTrace, "Error.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                ReturnToPreviousConnectionState();
            }
        }

        #endregion

        #region SQL-statements

        string GetNamesAndTypesFromProperties()
        {
            string betweenProperties = ", ";
            StringBuilder stringWithNamesAndTypesOfProperties = new StringBuilder();
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                if (propertyOfT.GetCustomAttributes(typeof(UniqueIdentifier), true).Length == 0)
                {
                    if (stringWithNamesAndTypesOfProperties.Length > 0)
                        stringWithNamesAndTypesOfProperties.Append(betweenProperties);
                    stringWithNamesAndTypesOfProperties.Append("[");
                    stringWithNamesAndTypesOfProperties.Append(propertyOfT.Name);
                    stringWithNamesAndTypesOfProperties.Append("] ");
                    stringWithNamesAndTypesOfProperties.Append(GetSqlCeTypeNameAndLength(propertyOfT));
                }
            }
            return stringWithNamesAndTypesOfProperties.ToString();
        }

        string GetNamesFromProperties(bool ExcludeUniqueIdentifier, string Prefix, Suffix AddToParameterName)
        {
            string betweenProperties = ", " + Prefix;
            StringBuilder nameString = new StringBuilder();
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                if (!ExcludeUniqueIdentifier || (propertyOfT.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length == 0)
                {
                    if (nameString.Length > 0) nameString.Append(betweenProperties);
                    else nameString.Append(Prefix);
                    nameString.Append(propertyOfT.Name);
                    if (AddToParameterName != Suffix.None) nameString.Append(AddToParameterName.ToString());
                }
            }
            return nameString.ToString();
        }

        string GetValueNamesFromProperties(bool ExcludeUniqueIdentifier)
        {
            return GetNamesFromProperties(ExcludeUniqueIdentifier, "@", Suffix.Insert).ToLower();
        }

        string GetFieldNamesFromProperties(bool ExcludeUniqueIdentifier)
        {
            return GetNamesFromProperties(ExcludeUniqueIdentifier, "", Suffix.None);
        }

        enum Suffix { Insert, Where, None }
        bool ExcludeUniqueIdentifier(Suffix AddToParameter)
        {
            return AddToParameter == Suffix.Insert ? true : false;
        }

        enum Separator { Comma, And }
        string Separate(Separator SeparateBy)
        {
            return SeparateBy == Separator.Comma ? ", " : " AND ";
        }

        string GetEquationsFromProperties<T>
            (T ObjectOfInterest,
            Suffix AddToParameterName,
            Separator SeparateBy) where T : class, new()
        {
            string betweenProperties = " " + this.WhereClauseComparer + " @";
            StringBuilder equations = new StringBuilder(4096);
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                object valueOfProperty = propertyOfT.GetValue(ObjectOfInterest, null);
                if (valueOfProperty != null &&
                    (!ExcludeUniqueIdentifier(AddToParameterName) ||
                        (propertyOfT.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length == 0))
                {
                    if (equations.Length > 0) equations.Append(Separate(SeparateBy));
                    equations.Append(propertyOfT.Name);
                    equations.Append(betweenProperties);
                    equations.Append(propertyOfT.Name.ToLower());
                    equations.Append(AddToParameterName.ToString());
                }
            }
            return equations.ToString();
        }

        string GetWhereClauseFromProperties<T>(T ObjectOfInterest) where T : class, new()
        {
            return GetEquationsFromProperties(ObjectOfInterest, Suffix.Where, Separator.And);
        }

        string GetSetClauseFromProperties<T>(T ObjectOfInterest) where T : class, new()
        {
            return GetEquationsFromProperties(ObjectOfInterest, Suffix.Insert, Separator.Comma);
        }

        string GetPropertyNameThatIsNotNull<T>(T ObjectOfInterest) where T : class, new()
        {
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                object valueOfProperty = propertyOfT.GetValue(ObjectOfInterest, null);
                if (valueOfProperty != null) return propertyOfT.Name;
            }
            throw new ArgumentException("One property must not be null!");
        }

        string ConstructSQLStatementCheckTable()
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("SELECT ");
            sqlStatement.Append("*");
            sqlStatement.Append(" FROM INFORMATION_SCHEMA.TABLES");
            return sqlStatement.ToString();
        }

        string ConstructSQLStatementCheckIndex()
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("SELECT ");
            sqlStatement.Append("*");
            sqlStatement.Append(" FROM INFORMATION_SCHEMA.INDEXES");
            return sqlStatement.ToString();
        }

        string ConstructSQLStatementCreateTable<T>(T ObjectOfInterest) where T : class, new()
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("CREATE TABLE ");
            sqlStatement.Append(typeof(T).Name);
            sqlStatement.Append("(");
            sqlStatement.Append(CurrentUniqueIdentifier.Name);
            sqlStatement.Append(" int IDENTITY (0,1) PRIMARY KEY, ");
            sqlStatement.Append(GetNamesAndTypesFromProperties());
            sqlStatement.Append(")");
            return sqlStatement.ToString();
        }

        string ConstructSQLStatementCreateIndex<T>(T ObjectOfInterest) where T : class, new()
        {
            StringBuilder sqlStatement = new StringBuilder();
            string fieldName = GetPropertyNameThatIsNotNull(ObjectOfInterest);
            sqlStatement.Append("CREATE INDEX ");
            sqlStatement.Append(GetIndexName(ObjectOfInterest));
            sqlStatement.Append(" ON ");
            sqlStatement.Append(typeof(T).Name);
            sqlStatement.Append(" (");
            sqlStatement.Append(fieldName);
            sqlStatement.Append(")");
            return sqlStatement.ToString();
        }

        string ConstructSQLStatementInsert<T>(T ObjectOfInterest) where T : class, new()
        {
            string fields = GetFieldNamesFromProperties(true);
            string values = GetValueNamesFromProperties(true);
            string sqlStatement = "INSERT INTO " + typeof(T).Name + " (" + fields + ") VALUES (" + values + ")";
            return sqlStatement;
        }

        string ConstructSQLStatementSelect<T>(T ObjectOfInterest) where T : class, new()
        {
            string sqlStatement = "SELECT " + GetFieldNamesFromProperties(false) + " FROM " + typeof(T).Name;
            string whereClause = GetWhereClauseFromProperties(ObjectOfInterest);
            sqlStatement += whereClause.Length == 0 ? "" : " WHERE " + whereClause;
            return sqlStatement;
        }

        string ConstructSQLStatementUpdate<T>(T SetObjectOfInterest, T WhereObjectOfInterest) where T : class, new()
        {
            string setclause = GetSetClauseFromProperties(SetObjectOfInterest);
            string whereClause = GetWhereClauseFromProperties(WhereObjectOfInterest);
            string sqlStatement = "UPDATE " + typeof(T).Name + " SET " + setclause;
            sqlStatement += whereClause.Length == 0 ? "" : " WHERE " + whereClause;
            return sqlStatement;
        }

        string ConstructSQLStatementDelete<T>(T ObjectOfInterest) where T : class, new()
        {
            string whereClause = GetWhereClauseFromProperties(ObjectOfInterest);
            string sqlStatement = "DELETE FROM " + typeof(T).Name;
            sqlStatement += whereClause.Length == 0 ? "" : " WHERE " + whereClause;
            return sqlStatement;
        }

        #endregion

        #region Property Functions

        void SetPropertyInfosAndUniqueIdentifier<T>(T SearchItem) where T : class, new()
        {
            PropertiesOfT = typeof(T).GetProperties();
            this.CurrentUniqueIdentifier = PropertiesOfT.First(p =>
                (p.GetCustomAttributes(typeof(UniqueIdentifier), true)).Length > 0);
        }

        #endregion

        #region SqlCe Functions

        int InsertDataListAndRetrieveIdentifier<T>(List<T> DataList, SqlCeCommand Command) where T : class, new()
        {
            string sqlStatement = Command.CommandText;
            int objectCounter = 0;
            // slow method, adds each value individually for each row
            foreach (var dataItem in DataList)
            {
                Command.CommandText = sqlStatement;
                AddParametersWithValuesFromProperties(dataItem, Command, Suffix.Insert);
                objectCounter += Command.ExecuteNonQuery();
                Command.CommandText = "SELECT @@IDENTITY";
                int propID = Convert.ToInt32((decimal)Command.ExecuteScalar());
                CurrentUniqueIdentifier.SetValue(dataItem, propID, null);
            }
            return objectCounter;
        }

        T FillObjectWithResultSet<T>(T dataItem,
            SqlCeResultSet ResultSet) where T : class, new()
        {
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                PropertyInfo singlepropinf = typeof(T).GetProperty(propertyOfT.Name);
                int ordinal = ResultSet.GetOrdinal(propertyOfT.Name);
                object result = ResultSet.GetValue(ordinal);
                if (result is DBNull) singlepropinf.SetValue(dataItem, null, null);
                else singlepropinf.SetValue(dataItem, result, null);
            }
            return dataItem;
        }

        SqlCeCommand GetSqlCeCommand(SqlCeConnection Connection, string SqlStatement)
        {
            SqlCeCommand command = new SqlCeCommand();
            command.Connection = Connection;
            command.CommandType = CommandType.Text;
            command.CommandText = SqlStatement;
            return command;
        }

        string AddWildCard(object PropertyValue)
        {
            string propertyString = PropertyValue.ToString();
            if (WildCardLeft) propertyString = "%" + propertyString;
            if (WildCardRight) propertyString = propertyString + "%";
            return propertyString;
        }

        void AddParametersWithValuesFromProperties<T>
            (T ObjectOfInterest,
            SqlCeCommand Command,
            Suffix AddToParameterName) where T : class, new()
        {
            bool excludeUniqueIdentifier = ExcludeUniqueIdentifier(AddToParameterName);
            foreach (PropertyInfo propertyOfT in PropertiesOfT)
            {
                if (!excludeUniqueIdentifier || propertyOfT != CurrentUniqueIdentifier)
                {
                    object propertyValue = propertyOfT.GetValue(ObjectOfInterest, null);
                    string parameterName = "@" + propertyOfT.Name.ToLower()
                        + AddToParameterName.ToString().ToLower();
                    if (propertyValue != null && _UseLikeStatement)
                        propertyValue = AddWildCard(propertyValue);
                    if (propertyValue == null) propertyValue = DBNull.Value;
                    if (!Command.Parameters.Contains(parameterName))
                        Command.Parameters.Add(parameterName,
                            GetSqlDbTypeFromDotNetType(propertyOfT.PropertyType));
                    Command.Parameters[parameterName].Value = propertyValue;
                }
            }
        }

        #endregion

        #region Index Functions

        string GetIndexName<T>(T ObjectOfInterest) where T : class, new()
        {
            return "IndexFor" + GetPropertyNameThatIsNotNull(ObjectOfInterest);
        }

        #endregion

        #endregion
    }
}
