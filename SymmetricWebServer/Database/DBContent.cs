using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Security.Cryptography;
using WebServer.Tags;

namespace WebServer.Database
{
    public class DBContent
    {
        private static readonly byte[] TheGuy = { 106, 78, 98, 72, 113, 66, 84, 104, 77, 54, 76, 117, 82, 56, 101, 98 };
        private static readonly byte[] TheSecondGuy = { 113, 66, 84, 104, 77, 54, 76, 117 };

        public enum ConnectionTypes { MySQL = 0, MSSQL = 1 }
        public enum TagValueItemTypes { Form = 0, Template = 1 }
        private static int CurrentVersion = 0;

        private const int VARCHAR_StandardSize = 255;
        private const int BaseDBVersion = 1;
        private const int DBLatestVersion = 2;

        private SqliteConnection _conn;
        private SqliteDataReader _reader;
        private SqliteCommand _cmd;

        private static string DatabaseFileName
        {
            get
            {
                return Globals.RootPath + "content.db";
            }
        }

        public DBContent()
        {
            this.Initialize();
        }

        #region Basic Functions

        private void Initialize()
        {
            this.CreateDB();

            if (_conn == null)
            {
                _conn = new SqliteConnection(@"data source=" + DBContent.DatabaseFileName);
                _conn.Open();
            }
            if (_cmd == null)
            {
                _cmd = new SqliteCommand(_conn);
            }
            _reader = null;
        }

        private void CreateDB()
        {
            SqliteCommand cmd = null;
            try
            {
                bool fileExists = File.Exists(DBContent.DatabaseFileName);
                cmd = new SqliteCommand();
                cmd.Connection = new SqliteConnection(@"data source=" + DBContent.DatabaseFileName);
                cmd.Connection.Open();

                if (fileExists)
                {
                    DBContent.CheckAgainstVersion(cmd);
                    return;
                }

                string sql = "";
                sql = "CREATE TABLE Version(versionInfo INTEGER);";
                sql += String.Format(" INSERT INTO Version VALUES({0});", DBContent.BaseDBVersion);

                sql += " CREATE TABLE [ReportingConnections](id INTEGER PRIMARY KEY,";
                sql += " name VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL UNIQUE collate nocase,";
                sql += " address VARCHAR(" + DBContent.VARCHAR_StandardSize + ") collate nocase,";
                sql += " port INTEGER DEFAULT 3306 NOT NULL,";
                sql += " connectionType INTEGER DEFAULT 0 NOT NULL,"; // ConnectionTypes
                sql += " defaultDatabase VARCHAR(" + DBContent.VARCHAR_StandardSize + ") DEFAULT '' NOT NULL,";
                sql += " userName VARCHAR(" + DBContent.VARCHAR_StandardSize + ") collate nocase,";
                sql += " password BLOB NULL);";

                sql += " CREATE TABLE [ReportingTemplate](id INTEGER PRIMARY KEY,";
                sql += " name VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL UNIQUE collate nocase,";
                sql += " bodyHTML TEXT DEFAULT '' NOT NULL);";

                sql += " CREATE TABLE [ReportGroup](id INTEGER PRIMARY KEY,";
                sql += " name VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL UNIQUE collate nocase);";

                sql += " CREATE TABLE [Report](id INTEGER PRIMARY KEY,";
                sql += " name VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL UNIQUE collate nocase,";
                sql += " fk_connection INTEGER NOT NULL,";
                sql += " fk_group INTEGER NOT NULL,";
                sql += " fk_template INTEGER NOT NULL,";
                sql += " FOREIGN KEY(fk_group) REFERENCES [ReportGroup](id),";
                sql += " FOREIGN KEY(fk_template) REFERENCES [ReportingTemplate](id),";
                sql += " FOREIGN KEY(fk_connection) REFERENCES [ReportingConnections](id));";

                sql += " CREATE TABLE [Tag](id INTEGER PRIMARY KEY,";
                sql += " type VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL UNIQUE collate nocase);";

                sql += " CREATE TABLE [TagValue](id INTEGER PRIMARY KEY,";
                sql += " name VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL collate nocase,";
                sql += " itemType INTEGER DEFAULT 0 NOT NULL,"; // TagValueItemTypes
                sql += " value TEXT DEFAULT '' NOT NULL,";
                sql += " fk_tag INTEGER NOT NULL,";
                sql += " fk_report INTEGER NOT NULL,";
                sql += " FOREIGN KEY(fk_report) REFERENCES [Report](id),";
                sql += " FOREIGN KEY(fk_tag) REFERENCES [Tag](id));";

                sql += " CREATE TABLE [ReportingForm](id INTEGER PRIMARY KEY,";
                sql += " name VARCHAR(" + DBContent.VARCHAR_StandardSize + ") NOT NULL UNIQUE collate nocase,";
                sql += " bodyHTML TEXT DEFAULT '' NOT NULL);";

                sql += " INSERT INTO [ReportingForm](name, bodyHTML) VALUES('Default Form', @defaultForm);";
                cmd.Parameters.Add(new SqliteParameter("@defaultForm", FormItem.DefaultForm));

                sql += " ALTER TABLE [Report] ADD COLUMN fk_form INTEGER NULL REFERENCES [ReportingForm](id);";

                // ***** Defaults here *****

                sql += " INSERT INTO [ReportingConnections](name, address, userName) VALUES('Default Database', 'localhost', 'root');";

                cmd.Parameters.Add(new SqliteParameter("@defaultTemplate", TemplateItem.DefaultTemplate));
                sql += " INSERT INTO [ReportingTemplate](name, bodyHTML) VALUES('Default Template', @defaultTemplate);";


                sql += " INSERT INTO [ReportGroup](name) VALUES('Default Group');";

                sql += " INSERT INTO [Report](name, fk_connection, fk_group, fk_template) VALUES('Default Report', 1, 1, 1);";

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                DBContent.CheckAgainstVersion(cmd);
            }
            catch (Exception ex)
            {
                try
                {
                    File.Delete(DBContent.DatabaseFileName);
                }
                catch
                {
                    throw new Exception("Please manually remove: " + DBContent.DatabaseFileName + ", " + ex.Message);//should not get in here
                }
                throw new Exception("Error creating database: " + ex.Message);
            }
            finally
            {
                if (cmd != null)
                {
                    if (cmd.Connection != null)
                    {
                        cmd.Connection.Close();
                        cmd.Connection.Dispose();
                        cmd.Connection = null;
                    }
                    cmd.Dispose();
                    cmd = null;
                }
            }
        }

        private void ClearParameters()
        {
            _cmd.Parameters.Clear();
        }

        private void AddParameter(string name, object value)
        {
            if (String.IsNullOrWhiteSpace(name)) return;
            name = name.ToLower();

            if (_cmd.Parameters.IndexOf(name) >= 0)
            {
                _cmd.Parameters[name].Value = value;
            }
            else
            {
                _cmd.Parameters.Add(new SqliteParameter(name, value));
            }
        }

        private int ExecuteNonQuery(string sql)
        {
            if (_reader != null)
            {
                if (!_reader.IsClosed)
                {
                    _reader.Close();
                }
            }
            _cmd.CommandText = sql;
            return _cmd.ExecuteNonQuery();
        }

        public SqliteDataReader ExecuteReader(string sql)
        {
            _cmd.CommandText = sql;
            _reader = _cmd.ExecuteReader();
            return _reader;
        }

        private void CleanUp()
        {
            if (_reader != null)
            {
                if (!_reader.IsClosed)
                {
                    _reader.Close();
                }
                _reader.Dispose();
                _reader = null;
            }

            if (_cmd != null)
            {
                _cmd.Dispose();
                _cmd = null;
            }

            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
                _conn = null;
            }
        }


        private static void CheckAgainstVersion(SqliteCommand cmd)
        {
            if (DBContent.CurrentVersion == DBContent.DBLatestVersion) return;

            SqliteDataReader reader = null;
            try
            {
                cmd.CommandText = "SELECT versionInfo FROM Version";
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int versionNr = int.Parse(reader[0].ToString());
                    reader.Close();
                    /*switch (versionNr)
                    {
                    case DatabaseHandle.BaseDBVersion:

                        cmd.CommandText += " DELETE FROM Version;";

                        cmd.CommandText += " INSERT INTO Version VALUES(2);";
                        cmd.ExecuteNonQuery();
                        break;
                    }*/
                    DBContent.CurrentVersion = DBContent.DBLatestVersion;
                }
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

        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted)
        {
            byte[] encryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(TheGuy, TheSecondGuy, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted)
        {
            try
            {
                byte[] decryptedBytes = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(TheGuy, TheSecondGuy, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }

                return decryptedBytes;
            }
            catch
            {
                return new byte[0];
            }
        }

        #endregion

        #region Connections

        public List<BasicEntry> BasicConnections
        {
            get
            {
                List<BasicEntry> result = new List<BasicEntry>();
                try
                {
                    SqliteDataReader reader = this.ExecuteReader("SELECT ID, name FROM [ReportingConnections]");
                    while (reader.Read())
                    {
                        result.Add(new BasicEntry(int.Parse(reader[0].ToString()), reader[1].ToString(), ""));
                    }
                    return result;
                }
                finally
                {
                    this.CleanUp();
                }
            }
        }

        public bool SaveConnection(ConnectionItem item, out string errormessage)
        {
            errormessage = "";
            try
            {
                if (item == null)
                {
                    errormessage = "Invalid item.";
                    return false;
                }
                if (String.IsNullOrWhiteSpace(item.Name))
                {
                    errormessage = "The connection name cannot be blank.";
                    return false;
                }

                int connectionType = 0; // mysql
                switch (item.ConnectionType)
                {
                    case ConnectionItem.ConnectionTypes.MSSQL:
                        connectionType = 1;
                        break;
                }

                this.AddParameter("@id", item.ID);
                this.AddParameter("@name", item.Name);
                this.AddParameter("@address", item.Host);
                this.AddParameter("@port", item.ConnPort);
                this.AddParameter("@connectionType", connectionType);
                this.AddParameter("@username", item.Username);
                this.AddParameter("@password", DBContent.AES_Encrypt(System.Text.Encoding.UTF8.GetBytes(item.Password)));
                this.AddParameter("@defaultDatabase", item.DefaultDatabase);

                string sql = "";
                if (item.ID > 0)
                {
                    SqliteDataReader reader = this.ExecuteReader("SELECT id FROM ReportingConnections WHERE id != @id AND name LIKE @name");
                    if (reader.HasRows)
                    {
                        errormessage = "A reporting connection with that name already exists.";
                        return false;
                    }
                    sql = "UPDATE ReportingConnections SET name = @name, address = @address, port = @port, ";
                    sql += "connectionType = @connectionType, username=@username, password = @password,";
                    sql += "defaultDatabase = @defaultDatabase, password = @password WHERE id = @id";
                }
                else
                {
                    SqliteDataReader reader = this.ExecuteReader("SELECT id FROM ReportingConnections WHERE name LIKE @name");
                    if (reader.HasRows)
                    {
                        errormessage = "A reporting connection with that name already exists.";
                        return false;
                    }
                    sql = "INSERT INTO ReportingConnections(name, address, port, connectionType, username, password, defaultDatabase)";
                    sql += " VALUES(@name, @address, @port, @connectionType, @username, @password, @defaultDatabase)";
                }
                this.ExecuteNonQuery(sql);
                return true;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public bool DeleteConnection(int id, out string errormessage)
        {
            errormessage = "";
            try
            {
                this.AddParameter("@id", id);

                SqliteDataReader reader = this.ExecuteReader("SELECT fk_connection FROM [Report] WHERE fk_connection=@id");
                if (reader.HasRows)
                {
                    errormessage = "We cannot delete this connection as one or more reports are using it.";
                    return false;
                }

                this.ExecuteNonQuery("DELETE FROM [ReportingConnections] WHERE id = @id");
                return true;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public ConnectionItem GetConnection(int id)
        {
            try
            {
                string sql = " SELECT ID, name, address, port, userName,";
                sql += " password, connectionType, defaultDatabase FROM [ReportingConnections] WHERE ID=" + id;
                SqliteDataReader reader = this.ExecuteReader(sql);
                while (reader.Read())
                {
                    ConnectionItem.ConnectionTypes connType = ConnectionItem.ConnectionTypes.MySQL;
                    switch (reader[6].ToString())
                    {
                        case "1":
                            connType = ConnectionItem.ConnectionTypes.MSSQL;
                            break;
                    }

                    string strPassword = "";
                    if (reader[5] != DBNull.Value)
                    {
                        strPassword = System.Text.Encoding.UTF8.GetString(DBContent.AES_Decrypt((byte[])(reader[5])));
                    }

                    return new ConnectionItem(int.Parse(reader[0].ToString()),
                                              reader[1].ToString(),
                                              reader[2].ToString(),
                                              int.Parse(reader[3].ToString()),
                                              reader[4].ToString(),
                                              strPassword,
                                              connType,
                                              reader[7].ToString());
                }
            }
            finally
            {
                this.CleanUp();
            }
            return null;
        }

        #endregion

        #region Templates

        public List<BasicEntry> BasicTemplates
        {
            get
            {
                return this.GetTemplates(true);
            }
        }

        public List<BasicEntry> FullTemplates
        {
            get
            {
                return this.GetTemplates(false);
            }
        }

        private List<BasicEntry> GetTemplates(bool basic)
        {
            List<BasicEntry> result = new List<BasicEntry>();
            try
            {
                string sql = "SELECT ID, name";
                if (!basic)
                {
                    sql += ", bodyHTML";
                }
                sql += " FROM [ReportingTemplate]";
                _reader = this.ExecuteReader(sql);

                while (_reader.Read())
                {
                    BasicEntry entry = new BasicEntry(int.Parse(_reader[0].ToString()), _reader[1].ToString(), "");
                    result.Add(entry);
                    if (!basic)
                    {
                        entry.Tag = _reader[2].ToString();
                    }
                }
                return result;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public TemplateItem GetTemplate(int id)
        {
            try
            {
                SqliteDataReader reader = this.ExecuteReader("SELECT ID, name, bodyHTML FROM [ReportingTemplate] WHERE id = " + id);
                while (reader.Read())
                {
                    return new TemplateItem(int.Parse(reader[0].ToString()),
                                            reader[1].ToString(),
                                            reader[2].ToString());
                }
            }
            finally
            {
                this.CleanUp();
            }
            return null;
        }

        public bool DeleteTemplate(int id, out string errormessage)
        {
            errormessage = "";
            try
            {
                this.AddParameter("@id", id);
                SqliteDataReader reader =  this.ExecuteReader("SELECT id FROM [Report] WHERE fk_template=@id");
                if (reader.HasRows)
                {
                    errormessage = "We cannot delete this template as one or more reports are using it.";
                    return false;
                }
                reader.Close();

                this.ExecuteNonQuery("DELETE FROM [ReportingTemplate] WHERE id = @id");
                return true;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public bool SaveTemplate(TemplateItem item, out string errormessage)
        {
            errormessage = "";
            if (item == null)
            {
                errormessage = "Invalid item.";
                return false;
            }
            if (String.IsNullOrWhiteSpace(item.Name))
            {
                errormessage = "The template name cannot be blank.";
                return false;
            }

            try
            {
                this.AddParameter("@name", item.Name);
                this.AddParameter("@bodyHTML", item.HTML);
                this.AddParameter("@id", item.ID);
                SqliteDataReader reader = null;
                if (item.ID > 0)
                {
                    reader = this.ExecuteReader("SELECT id FROM ReportingTemplate WHERE id != @id AND name LIKE @name");
                    if (reader.HasRows)
                    {
                        errormessage = "A reporting template with that name already exists.";
                        return false;
                    }
                    reader.Close();
                    this.ExecuteNonQuery("UPDATE ReportingTemplate SET name = @name, bodyHTML = @bodyHTML WHERE id = @id");
                }
                else
                {
                    reader = this.ExecuteReader("SELECT id FROM ReportingTemplate WHERE name LIKE @name");
                    if (reader.HasRows)
                    {
                        errormessage = "A reporting template with that name already exists.";
                        return false;
                    }
                    reader.Close();
                    this.ExecuteNonQuery("INSERT INTO ReportingTemplate(name, bodyHTML) VALUES(@name, @bodyHTML)");
                }
                return true;
            }
            finally
            {
                this.CleanUp();
            }
        }

        #endregion

        #region Forms

        public List<BasicEntry> FullForms
        {
            get
            {
                return this.GetForms(false);
            }
        }

        public List<BasicEntry> BasicForms
        {
            get
            {
                return this.GetForms(true);
            }
        }

        private List<BasicEntry> GetForms(bool basic)
        {
            List<BasicEntry> result = new List<BasicEntry>();
            try
            {
                string sql = "SELECT ID, Name";
                if (!basic)
                {
                    sql += ", BodyHTML";
                }
                sql += " FROM [ReportingForm]";
                SqliteDataReader reader = this.ExecuteReader(sql);
                while (reader.Read())
                {
                    BasicEntry entry = new BasicEntry(int.Parse(reader[0].ToString()), reader[1].ToString(), "");
                    result.Add(entry);
                    if (!basic)
                    {
                        entry.Tag = reader[2].ToString();
                    }
                }
                return result;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public bool DeleteForm(int id, out string errormessage)
        {
            errormessage = "";
            try
            {
                this.AddParameter("@id", id);

                SqliteDataReader reader = this.ExecuteReader("SELECT id FROM [Report] WHERE fk_form=@id");
                if (reader.HasRows)
                {
                    errormessage = "We cannot delete this form as one or more reports are using it.";
                    return false;
                }
                reader.Close();
                this.ExecuteNonQuery("DELETE FROM [ReportingForm] WHERE id = @id");
                return true;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public FormItem GetForm(int? id)
        {
            if (id == null) return null;
            SqliteDataReader reader = null;
            try
            {
                reader = this.ExecuteReader("SELECT ID, name, bodyHTML FROM [ReportingForm] WHERE id = " + id);
                while (reader.Read())
                {
                    return new FormItem(int.Parse(reader[0].ToString()),
                                            reader[1].ToString(),
                                            reader[2].ToString());
                }
            }
            finally
            {
                this.CleanUp();
            }
            return null;
        }

        public bool SaveForm(FormItem item, out string errormessage)
        {
            errormessage = "";
            if (item == null)
            {
                errormessage = "Invalid form.";
                return false;
            }
            if (String.IsNullOrWhiteSpace(item.Name))
            {
                errormessage = "The form name cannot be blank.";
                return false;
            }

            SqliteDataReader reader = null;
            try
            {
                this.AddParameter("@name", item.Name);
                this.AddParameter("@bodyHTML", item.HTML);
                this.AddParameter("@id", item.ID);

                if (item.ID > 0)
                {
                    reader = this.ExecuteReader("SELECT id FROM ReportingForm WHERE id != @id AND name LIKE @name");
                    if (reader.HasRows)
                    {
                        errormessage = "A reporting form with that name already exists.";
                        return false;
                    }
                    reader.Close();
                    this.ExecuteNonQuery("UPDATE ReportingForm SET name = @name, bodyHTML = @bodyHTML WHERE id = @id");
                }
                else
                {
                    reader = this.ExecuteReader("SELECT id FROM ReportingForm WHERE name LIKE @name");
                    if (reader.HasRows)
                    {
                        errormessage = "A reporting form with that name already exists.";
                        return false;
                    }
                    reader.Close();
                    this.ExecuteNonQuery("INSERT INTO ReportingForm(name, bodyHTML) VALUES(@name, @bodyHTML)");
                }
                return true;
            }
            finally
            {
                this.CleanUp();
            }
        }

        #endregion

        #region Reports

        public List<BasicEntry> BasicReports
        {
            get
            {
                List<BasicEntry> result = new List<BasicEntry>();
                try
                {
                    string sql = "SELECT [Report].ID, [Report].name, [ReportGroup].name FROM [Report]";
                    sql += "INNER JOIN [ReportGroup] ON [Report].fk_group = [ReportGroup].id";
                    _reader = this.ExecuteReader(sql);
                    while (_reader.Read())
                    {
                        result.Add(new BasicEntry(int.Parse(_reader[0].ToString()), _reader[1].ToString(), _reader[2].ToString()));
                    }
                    return result;
                }
                finally
                {
                    this.CleanUp();
                }
            }
        }

        public bool DeleteReport(int id, out string errormessage)
        {
            errormessage = "";
            try
            {
                this.AddParameter("@id", id);

                string sql = " DELETE FROM [TagValue] WHERE fk_report = @id;";
                sql += " DELETE FROM [Report] WHERE id = @id;";
                sql += " DELETE FROM [ReportGroup] WHERE id NOT IN (SELECT fk_group FROM Report);";

                this.ExecuteNonQuery(sql);
                return true;
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
            }
            finally
            {
                this.CleanUp();
            }
            return false;
        }

        public List<string> ReportGroups
        {
            get
            {
                List<string> result = new List<string>();
                try
                {
                    _reader = this.ExecuteReader("SELECT name FROM [ReportGroup]");
                    while (_reader.Read())
                    {
                        result.Add(_reader[0].ToString());
                    }
                    return result;
                }
                finally
                {
                    this.CleanUp();
                }
            }
        }

        public bool SaveReport(ReportItemBase item, out string errormessage)
        {
            errormessage = "";
            if (item == null)
            {
                errormessage = "Invalid report.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(item.GroupName))
            {
                errormessage = "The report group cannot be blank.";
                return false;
            }

            if (String.IsNullOrWhiteSpace(item.Name))
            {
                errormessage = "The report name cannot be blank.";
                return false;
            }

            if (item.ConnectionItem == null)
            {
                errormessage = "Connection cannot be null.";
                return false;
            }

            if (item.TemplateItem == null)
            {
                errormessage = "Template cannot be null.";
                return false;
            }

            try
            {
                string sql = "";
                using (var trans = _conn.BeginTransaction())
                {
                    //setup the group ID
                    int groupID = -1;
                    sql = "SELECT id FROM [ReportGroup] WHERE name LIKE @groupname";
                    this.AddParameter("@groupname", item.GroupName);
                    _reader = this.ExecuteReader(sql);
                    bool hasRows = false;
                    while (_reader.Read())
                    {
                        hasRows = true;
                        groupID = int.Parse(_reader[0].ToString());
                        break;
                    }
                    _reader.Close();

                    if (!hasRows)
                    {
                        sql = "INSERT INTO [ReportGroup](name) VALUES (@groupname)";
                        this.ExecuteNonQuery(sql);

                        sql = "SELECT id FROM [ReportGroup] WHERE name LIKE @groupname";
                        _reader = this.ExecuteReader(sql);
                        while (_reader.Read())
                        {
                            groupID = int.Parse(_reader[0].ToString());
                        }
                        _reader.Close();
                        this.ClearParameters();
                    }

                    this.AddParameter("@id", item.ID);
                    this.AddParameter("@name", item.Name);
                    this.AddParameter("@fk_connection",  item.ConnectionItem.ID);
                    this.AddParameter("@fk_group", groupID);
                    this.AddParameter("@fk_template", item.TemplateItem.ID);

                    if (item.FormItem == null)
                    {
                        this.AddParameter("@fk_form", DBNull.Value);
                    }
                    else
                    {
                        this.AddParameter("@fk_form", item.FormItem.ID);
                    }
                    if (item.ID > 0)
                    {
                        sql = "SELECT id FROM Report WHERE id != @id AND name LIKE @name";
                        _reader = this.ExecuteReader(sql);
                        if (_reader.HasRows)
                        {
                            errormessage = "A report with that name already exists.";
                            return false;
                        }
                        _reader.Close();
                        sql = "UPDATE Report SET name = @name, fk_connection = @fk_connection, fk_group = @fk_group, fk_template = @fk_template, fk_form = @fk_form WHERE id = @id";
                    }
                    else
                    {
                        sql = "SELECT id FROM Report WHERE name LIKE @name";
                        _reader = this.ExecuteReader(sql);
                        if (_reader.HasRows)
                        {
                            errormessage = "A report with that name already exists.";
                            return false;
                        }
                        _reader.Close();
                        sql = "INSERT INTO Report(name, fk_connection, fk_group, fk_template, fk_form)";
                        sql += " VALUES(@name, @fk_connection, @fk_group, @fk_template, @fk_form)";
                    }
                    this.ExecuteNonQuery(sql);

                    sql = "DELETE FROM ReportGroup WHERE id NOT IN (SELECT fk_group FROM Report)";
                    this.ExecuteNonQuery(sql);

                    sql = "DELETE FROM TagValue WHERE fk_report=@id";
                    this.ExecuteNonQuery(sql);

                    int reportingID = -1;
                    sql = "SELECT id FROM Report WHERE name LIKE @name";
                    _reader = this.ExecuteReader(sql);
                    while (_reader.Read())
                    {
                        reportingID = int.Parse(_reader[0].ToString());
                    }
                    _reader.Close();
                    this.AddParameter("@id", reportingID);

                    List<string> tagTypes = new List<string>();
                    sql = "SELECT type FROM [Tag]";
                    _reader = this.ExecuteReader(sql);
                    while (_reader.Read())
                    {
                        tagTypes.Add(_reader[0].ToString().ToLower());
                    }
                    _reader.Close();

                    this.AddParameter("@tag", "");

                    List<SWBaseTag> combined = new List<SWBaseTag>();
                    if (item.TemplateTags != null)
                    {
                        combined.AddRange(item.TemplateTags);
                    }

                    if (item.FormTags != null)
                    {
                        combined.AddRange(item.FormTags);
                    }

                    foreach (SWBaseTag tag in combined)
                    {
                        if (!tagTypes.Contains(tag.TagType))
                        {
                            sql = "INSERT INTO [Tag](type) VALUES(@tag);";
                            this.AddParameter("@tag", tag.TagType);
                            this.ExecuteNonQuery(sql);
                            tagTypes.Add(tag.TagType);
                        }
                    }

                    this.AddParameter("@tagName", "");
                    this.AddParameter("@tagValue", "");
                    this.AddParameter("@itemType", "");
                    foreach (SWBaseTag tag in combined)
                    {
                        sql = "INSERT INTO [TagValue](name, itemType, value, fk_report, fk_tag)";
                        sql += " VALUES(@tagName, @itemType, @tagValue, @id, ";
                        sql += " (SELECT id FROM [Tag] WHERE type = @tag))";
                        this.AddParameter("@tagName", tag.Name);
                        DBContent.TagValueItemTypes type;
                        if (item.TemplateTags.Contains(tag))
                        {
                            type = DBContent.TagValueItemTypes.Template;
                        }
                        else
                        {
                            type = DBContent.TagValueItemTypes.Form;
                        }
                        this.AddParameter("@itemType", (int)type);
                        this.AddParameter("@tagValue", tag.Value);
                        this.AddParameter("@tag", tag.TagType);
                        this.ExecuteNonQuery(sql);
                    }

                    trans.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
            }
            finally
            {
                this.CleanUp();
            }
            return false;
        }

        public ReportItemBase GetReport(int id,
                                        out string errormessage)
        {
            errormessage = "";
            ConnectionItem connection = null;
            FormItem form = null;
            TemplateItem template = null;
            List<SWBaseTag> formTags = new List<SWBaseTag>();
            List<SWBaseTag> templateTags = new List<SWBaseTag>();

            try
            {
                string sql = "SELECT fk_connection, fk_template, fk_form, [Report].name, [ReportGroup].name FROM [Report]";
                sql += " LEFT JOIN [ReportGroup] ON [ReportGroup].id = [Report].fk_group";
                sql += " WHERE [Report].ID = " + id;

                _reader = this.ExecuteReader(sql);

                int connectionID = -1;
                int formID = -1;
                int templateID = -1;
                string reportName = "";
                string groupName = "";

                if (!_reader.HasRows)
                {
                    _reader.Close();
                    errormessage = "Invalid report.";
                    return null;
                }

                while (_reader.Read())
                {
                    connectionID = int.Parse(_reader[0].ToString());
                    templateID = int.Parse(_reader[1].ToString());

                    if (_reader[2] != DBNull.Value)
                    {
                        formID = int.Parse(_reader[2].ToString());
                    }
                    reportName = _reader[3].ToString();
                    groupName = _reader[4].ToString();
                    break;
                }
                _reader.Close();

                connection = new DBContent().GetConnection(connectionID);
                if (connection == null)
                {
                    errormessage = "Invalid connection.";
                    return null;
                }

                form = new DBContent().GetForm(formID);

                template = new DBContent().GetTemplate(templateID);
                if (template == null)
                {
                    errormessage = "Invalid template.";
                    return null;
                }

                sql = "SELECT [TagValue].itemType, [Tag].type, [TagValue].name, [TagValue].value FROM [Tag]";
                sql += " INNER JOIN [TagValue] ON [TagValue].fk_tag = [Tag].id";
                sql += " WHERE [TagValue].fk_report = " + id;
                _reader = this.ExecuteReader(sql);
                while (_reader.Read())
                {
                    int itemType = 0;
                    if (!int.TryParse(_reader[0].ToString(), out itemType)) continue;

                    string type = _reader[1].ToString().ToLower();
                    string name = _reader[2].ToString().ToLower();
                    string value = _reader[3].ToString();

                    SWBaseTag tag = SWBaseTag.GetTag(type);
                    tag.Name = name;
                    tag.Value = value;
                    if (tag == null) continue;
                    switch (itemType)
                    {
                        case (int)DBContent.TagValueItemTypes.Form:
                            if (tag.BaseTagType == SWBaseTag.BaseTagTypes.Form ||
                                tag.BaseTagType == SWBaseTag.BaseTagTypes.Both)
                            {
                                formTags.Add(tag);
                            }
                            break;
                        case (int)DBContent.TagValueItemTypes.Template:
                            if (tag.BaseTagType == SWBaseTag.BaseTagTypes.Template ||
                                tag.BaseTagType == SWBaseTag.BaseTagTypes.Both)
                            {
                                templateTags.Add(tag);
                            }
                            break;
                        default:
                            continue;
                    }
                }

                ReportItemBase item = new ReportItemBase(id, reportName, groupName, connection, form, template);
                foreach (SWBaseTag dbtag in formTags)
                {
                    foreach (SWBaseTag itemTag in item.FormTags)
                    {
                        if (dbtag.Name.Equals(itemTag.Name) &&
                            dbtag.BaseTagType.Equals(itemTag.BaseTagType) &&
                            dbtag.TagType.Equals(itemTag.TagType))
                        {
                            itemTag.Value = dbtag.Value;
                            break;
                        }
                    }
                }

                foreach (SWBaseTag dbtag in templateTags)
                {
                    foreach (SWBaseTag itemTag in item.TemplateTags)
                    {
                        if (dbtag.Name.Equals(itemTag.Name) &&
                            dbtag.BaseTagType.Equals(itemTag.BaseTagType) &&
                            dbtag.TagType.Equals(itemTag.TagType))
                        {
                            itemTag.Value = dbtag.Value;
                            break;
                        }
                    }
                }
                return item;
            }
            finally
            {
                this.CleanUp();
            }
        }
#endregion
    }
}
