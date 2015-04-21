using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace WebServer.Database
{
    public class ConnectionItem
    {
        public enum ConnectionTypes { MySQL, MSSQL }
        public int ID { set; get; }
        public ConnectionTypes ConnectionType { set; get; }
        public string Name { set; get; }
        public string Host { set; get; }
        public int ConnPort { set; get; }
        public string Username { set; get; }
        public string Password { set; get; }
        public string DefaultDatabase { set; get; }
        public List<OptionItem> Databases { set; get; }

        public bool IsMySQL
        {
            get
            {
                return this.ConnectionType == ConnectionTypes.MySQL;
            }
        }

        public ConnectionItem()
        {
            this.ID = -1;
            this.ConnPort = 3306;
            this.ConnectionType = ConnectionTypes.MySQL;
            this.Databases = new List<OptionItem>();
        }

        public ConnectionItem(int id, string name, string host, int connPort, 
                              string username, string password, 
                              ConnectionTypes connectionType, string defaultConnection)
            : this()
        {
            this.ID = id;
            this.Name = name;
            this.Host = host;
            this.ConnPort = connPort;
            this.Username = username;
            this.Password = password;
            this.ConnectionType = connectionType;
            this.DefaultDatabase = defaultConnection;
        }

        private static DbConnection CreateConnection(ConnectionItem item, out string message)
        {
            message = "";
            if (item == null)
            {
                message = "Invalid item.";
                return null;
            }
            DbConnection result = null;
            try
            {
                string database = "";
                
                
                switch (item.ConnectionType)
                {
                    case ConnectionTypes.MySQL:
                        if (!String.IsNullOrWhiteSpace(item.DefaultDatabase))
                        {
                            database = "; Database=" + item.DefaultDatabase + ";";
                        }
                        result = new MySqlConnection(String.Format("Server={0}; {4} Port={1}; Uid={2}; Pwd={3} {4}", item.Host, item.ConnPort, item.Username, item.Password, database));
                        result.Open();
                        break;
                    case ConnectionTypes.MSSQL:
                        if (!String.IsNullOrWhiteSpace(item.DefaultDatabase))
                        {
                            database = "; Initial Catalog=" + item.DefaultDatabase + ";";
                        }
                        result = new SqlConnection(String.Format("Server={0}; User Id={1}; Password={2} {3}", item.Host, item.Username, item.Password, database));
                        result.Open();
                        break;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return result;
        }

        public static DbDataReader GetReader(ConnectionItem item, string sql, out string message, out DbConnection conn,
                                             Dictionary<string, object> dbParams)
        {
            conn = null;
            if (String.IsNullOrWhiteSpace(sql))
            {
                message = "The query cannot be blank.";
                return null;
            }
            conn = CreateConnection(item, out message);
            if (conn == null || !String.IsNullOrWhiteSpace(message)) return null;
            DbCommand cmd = null;
            try
            {
                switch (item.ConnectionType)
                {
                    case ConnectionTypes.MSSQL:
                        cmd = new SqlCommand(sql, conn as SqlConnection);
                        if (dbParams != null)
                        {
                            foreach (KeyValuePair<string, object> kvp in dbParams)
                            {
                                cmd.Parameters.Add(new SqlParameter(kvp.Key, kvp.Value));
                            }
                        }
                        break;
                    case ConnectionTypes.MySQL:
                        cmd = new MySqlCommand(sql, conn as MySqlConnection);
                        if (dbParams != null)
                        {
                            foreach (KeyValuePair<string, object> kvp in dbParams)
                            {
                                cmd.Parameters.Add(new MySqlParameter(kvp.Key, kvp.Value));
                            }
                        }
                        break;
                }

                if (cmd != null)
                {
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return null;
        }

        public static bool TestQuery(ConnectionItem item, string sql, out string message)
        {
            if (String.IsNullOrWhiteSpace(sql))
            {
                message = "The query cannot be blank.";
                return false;
            }

            DbConnection conn = CreateConnection(item, out message);
            if (conn == null || !String.IsNullOrWhiteSpace(message)) return false;
            DbDataReader reader = null;
            DbCommand cmd = null;
            try
            {
                switch (item.ConnectionType)
                {
                    case ConnectionTypes.MSSQL:
                        cmd = new SqlCommand(sql, conn as SqlConnection);
                        foreach (Match match in Regex.Matches(sql, @"(?<!\w)@\w+"))
                        {
                            if (!cmd.Parameters.Contains(match.Value))
                            {
                                cmd.Parameters.Add(new SqlParameter(match.Value, ""));
                            }
                        }
                        reader = cmd.ExecuteReader();
                        break;
                    case ConnectionTypes.MySQL:
                        cmd = new MySqlCommand(sql, conn as MySqlConnection);
                        foreach (Match match in Regex.Matches(sql, @"(?<!\w)@\w+"))
                        {
                            if (!cmd.Parameters.Contains(match.Value))
                            {
                                cmd.Parameters.Add(new MySqlParameter(match.Value, ""));
                            }
                        }
                        reader = cmd.ExecuteReader();
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }

                conn.Close();
                conn.Dispose();
                conn = null;
            }
            return false;
        }

        public bool RefreshDatabases()
        {
            string message;
            return this.RefreshDatabases(out message);
        }

        public bool RefreshDatabases(out string message)
        {
            string selectedDatabase = this.DefaultDatabase;
            if (selectedDatabase == null)
            {
                selectedDatabase = "";
            }
            else
            {
                selectedDatabase = selectedDatabase.ToUpper();
            }

            this.Databases.Clear();
        
            DbConnection conn = CreateConnection(this, out message);
            if (conn == null || !String.IsNullOrWhiteSpace(message)) return false;
            try
            {
                if (conn is MySqlConnection)
                {
                    MySqlDataReader reader = null;
                    try
                    {
                        MySqlCommand cmd = new MySqlCommand("SHOW DATABASES;", conn as MySqlConnection);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.Databases.Add(new OptionItem(-1, reader[0].ToString(), selectedDatabase.Equals(reader[0].ToString().ToUpper())));
                        }
                        return true;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                            reader.Dispose();
                            reader = null;
                        }
                    }
                }
                else if (conn is SqlConnection)
                {
                    SqlDataReader reader = null;
                    try
                    {
                        SqlCommand cmd = new SqlCommand("SELECT name FROM master..sysdatabases", conn as SqlConnection);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            this.Databases.Add(new OptionItem(-1, reader[0].ToString(), selectedDatabase.Equals(reader[0].ToString().ToUpper())));
                        }
                        return true;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                            reader.Dispose();
                            reader = null;
                        }
                    }
                }
                else
                {
                    message = "Unknown connection.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            return false;
        }

        public static bool TestConnection(ConnectionItem item, out string message)
        {
            DbConnection conn = CreateConnection(item, out message);
            if (conn == null || !String.IsNullOrWhiteSpace(message)) return false;
            try
            {
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            return false;
        }
    }
}
