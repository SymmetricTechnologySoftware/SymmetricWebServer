using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class DBUserShared
    {
        public const int VARCHAR_StandardSize = 255;
        private static readonly byte[] TheGuy = { 106, 78, 98, 72, 113, 66, 84, 104, 77, 54, 76, 117, 82, 56, 101, 98 };
        private static readonly byte[] TheSecondGuy = { 113, 66, 84, 104, 77, 54, 76, 117 };

        private const int FingerCount = 10;
        private SqliteTransaction _customTransaction;
        private SqliteConnection _tempConn;

        public DBUserShared(string contentFolder)
        {
            this.ContentFolder = contentFolder;
            if (!this.ContentFolder.EndsWith("\\"))
            {
                this.ContentFolder += "\\";
            }

            this.CreateUserDB();
        }

        public const string UserFileName = "user.db";
        public string UsersFile
        {
            get
            {
                return this.ContentFolder + UserFileName;
            }
        }

        public string ContentFolder { private set; get; }

        public static string GetUsersFile(string contentFolder)
        {
            return contentFolder + UserFileName;
        }

        private string CreateUserDB()
        {
            SqliteConnection conn = null;
            try
            {
                if (File.Exists(this.UsersFile))
                {
                    return "";
                }

                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();

                SqliteCommand cmd = new SqliteCommand(conn);

                cmd.CommandText = DBUserShared.GetDBSQL();
                cmd.ExecuteNonQuery();

                return "";
            }
            catch (Exception ex)
            {
                try
                {
                    File.Delete(this.UsersFile);
                }
                catch
                {
                    return "Please manually remove: " + this.UsersFile + ", " + ex.Message;//should not get in here
                }
                return "Error creating user database: " + ex.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        public string DeleteAllUsers()
        {
            SqliteConnection conn = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();
                SqliteCommand cmd = new SqliteCommand(conn);
                cmd.CommandText = "DELETE FROM User";
                cmd.ExecuteNonQuery();
                return "";
            }
            catch (Exception ex)
            {
                return "Failed to delete users: " + ex.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        public bool DeleteUser(int id, out string errorMessage)
        {
            errorMessage = "";
            SqliteConnection conn = null;
            SqliteDataReader reader = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();

                SqliteCommand cmd = new SqliteCommand(conn);
                cmd.Parameters.Add(new SqliteParameter("@id", id));
                //check if the user exists
                cmd.CommandText = "SELECT id FROM User WHERE id = @id";
                reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    errorMessage = "Couldn't find a user with the ID " + id + ".";
                    return false;
                }
                reader.Close();

                if (id == DefaultAdminUser.DefaultAdminID)
                {
                    errorMessage = "You cannot delete the Default Admin.";
                    return false;
                }

                cmd.CommandText = "UPDATE User SET deleted = 1 WHERE ID = @id";
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to delete user: " + ex.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }

                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }
            return false;
        }

        public User ValidLogin(int id, string password, out string message)
        {
            User user = null;

            message = this.GetUser(out user, id);
            if (!String.IsNullOrWhiteSpace(message)) return null;
            return DBUserShared.ValidLogin(password, user);
        }

        public static User ValidLogin(string password, User user)
        {
            byte[] newPassword = null;
            if (!String.IsNullOrWhiteSpace(password))
            {
                newPassword = User.GenerateSHA256(password);
            }

            if (user == null) return null;

            //  User foundUser = users.Where(x => x.ID == id).FirstOrDefault();
            //   if (foundUser == null) return null;

            if ((user.Password == null ||
                user.Password.Length == 0))
            {
                if (String.IsNullOrWhiteSpace(password))
                {
                    return user;
                }
            }
            else
            {
                if (newPassword != null &&
                    user.Password.SequenceEqual(newPassword))
                {
                    return user;
                }
            }
            return null;
        }

        public string AddEditUser(User user)
        {
            return AddEditUser(user, false, false, false);
        }

        public string AddEditUser(User user, bool customTransaction, bool updateUserLastUpdated, bool customID)
        {
            if (user == null) return "User cannot be null.";

            if (String.IsNullOrWhiteSpace(user.FullName))
            {
                return "The user's fullname cannot be empty.";
            }

            if (DefaultUser.DefaultUserName.ToUpper().Equals(user.FullName.ToUpper()))
            {
                return String.Format("The user's name cannot be {0}.", DefaultUser.DefaultUserName);
            }

            if (user.PasswordChanged && (String.IsNullOrWhiteSpace(user.NewPassword) || user.NewPassword.Length > VARCHAR_StandardSize))
            {
                return "The user's password cannot be empty.";
            }


            if (user.Description != null && user.Description.Length > VARCHAR_StandardSize)
            {
                return "The user's description is too long.";
            }

            SqliteDataReader reader = null;
            SqliteConnection conn = null;
            try
            {
                SqliteTransaction transaction = null;
                if (customTransaction)
                {
                    if (_tempConn == null)
                    {
                        _tempConn = new SqliteConnection(@"data source=" + this.UsersFile);
                        _tempConn.Open();
                        _customTransaction = _tempConn.BeginTransaction();
                    }
                    conn = _tempConn;
                }
                else
                {
                    conn = new SqliteConnection(@"data source=" + this.UsersFile);
                    conn.Open();
                    transaction = conn.BeginTransaction();
                }

                SqliteCommand cmd = new SqliteCommand(conn);
                cmd.Parameters.Add(new SqliteParameter("@userID", user.ID));

                string description = user.Description;
                string fullName = user.FullName;
                byte securityLevel = user.SecurityLevel;
                if (user.ID > 0)
                {
                    if (!customID)
                    {
                        cmd.CommandText = "SELECT id FROM User WHERE id = @userID";
                        reader = cmd.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            return "Couldn't find a user with the ID " + user.ID;
                        }
                        reader.Close();
                    }

                    cmd.Parameters.Add(new SqliteParameter("@id", user.ID));
                    //force the admin properties to stay the same
                    if (user.ID == DefaultAdminUser.DefaultAdminID)
                    {
                        description = DefaultAdminUser.DefaultAdminDescription;
                        fullName = DefaultAdminUser.DefaultAdminName;
                        securityLevel = DefaultAdminUser.DefaultAdminSecurityLevel;
                    }
                }
                cmd.Parameters.Add(new SqliteParameter("@description", description));

                cmd.Parameters.Add(new SqliteParameter("@fullname", fullName));

                byte[] password = user.Password;
                if (user.PasswordChanged)
                {
                    if (String.IsNullOrWhiteSpace(user.NewPassword))
                    {
                        return "The password cannot be blank.";
                    }
                    password = User.GenerateSHA256(user.NewPassword);
                }
                cmd.Parameters.Add(new SqliteParameter("password", password));
                cmd.Parameters.Add(new SqliteParameter("@securityLevel", securityLevel));


                if (user.ID > 0 && !customID)
                {
                    cmd.CommandText = "UPDATE User SET description = @description, fullname = @fullname, password = @password, securityLevel = @securityLevel";
                }
                else
                {
                    cmd.CommandText = "INSERT INTO User(";
                    if (customID)
                    {
                        cmd.CommandText += "id,";
                    }
                    cmd.CommandText += "description, fullname, password, securityLevel";
                }

                for (int i = 0; i < DBUserShared.FingerCount; ++i)
                {
                    if (user.ID > 0 && !customID)
                    {
                        cmd.CommandText += String.Format(" ,fingerprint{0} = @fingerprint{0}", i);
                    }
                    else
                    {
                        cmd.CommandText += String.Format(" ,fingerprint{0}", i);
                    }
                    cmd.Parameters.Add(new SqliteParameter(String.Format("@fingerprint{0}", i), DBNull.Value));
                }

                foreach (FingerPrint fingerprint in user.FingerPrints)
                {
                    string fp = String.Format("@fingerprint{0}", fingerprint.PrintNumber);
                    if (cmd.Parameters.Contains(fp) && fingerprint.Print != null && fingerprint.Print.Length != 0)
                    {
                        cmd.Parameters[fp].Value = DBUserShared.AES_Encrypt(fingerprint.Print);
                    }
                }

                if (user.ID > 0 && !customID)
                {
                    cmd.CommandText += " WHERE ID = @id";
                }
                else
                {
                    cmd.CommandText += ") VALUES(";
                    if(customID)
                    {
                        cmd.CommandText += "@userID,";
                    }
                    cmd.CommandText += "@description, @fullname, @password, @securityLevel";

                    for (int i = 0; i < DBUserShared.FingerCount; ++i)
                    {
                        cmd.CommandText += String.Format(" ,@fingerprint{0}", i);
                    }
                    cmd.CommandText += ")";
                }

                cmd.ExecuteNonQuery();

                if (user.ID < 0 && !customID)
                {
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        user.ID = int.Parse(reader[0].ToString());
                        cmd.Parameters["@userID"].Value = user.ID;
                    }
                    reader.Close();
                }

                if (updateUserLastUpdated)
                {
                    cmd.CommandText = "DELETE FROM UserLastUpdated";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO UserLastUpdated(Value) VALUES(@value)";
                    cmd.Parameters.Add(new SqliteParameter("@value", DateTime.Now));
                    cmd.ExecuteNonQuery();
                }

                if (!customTransaction)
                {
                    transaction.Commit();
                }
                return "";
            }
            catch (Exception ex)
            {
                return "Failed to add or edit user: " + ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
                if (!customTransaction)
                {
                    if (conn != null)
                    {
                        conn.Close();
                        conn = null;
                    }
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

        public string CommitTransaction()
        {
            try
            {
                if (_customTransaction != null)
                {
                    _customTransaction.Commit();
                    _customTransaction = null;
                }

                if (_tempConn != null)
                {
                    _tempConn.Close();
                    _tempConn = null;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "CommitTransaction failed: " + ex.Message;
            }
        }

        public string GetUser(out User user, int id)
        {
            List<User> users = null;
            user = null;
            try
            {
                return this.LoadUserBase(out users, true, id);
            }
            finally
            {
                if (users != null && users.Count() > 0)
                {
                    user = users[0];
                }
            }
        }

        public string LoadUsers(out List<User> users)
        {
            return this.LoadUserBase(out users, false, -1);
        }

        private string LoadUserBase(out List<User> users, bool single, int id)
        {
            users = new List<User>();
            SqliteConnection conn = null;
            SqliteDataReader reader = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();
                SqliteCommand cmd = new SqliteCommand(conn);

                cmd.CommandText = "SELECT id, description, fullname, password, securityLevel";
                for (int i = 0; i < DBUserShared.FingerCount; ++i)
                {
                    cmd.CommandText += String.Format(", fingerprint{0}", i);
                }
                cmd.CommandText += " FROM User WHERE deleted = 0";
                if (single)
                {
                    cmd.CommandText += " AND id = " + id;
                }

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Dictionary<int, byte[]> lstFingerPrints = new Dictionary<int, byte[]>();

                    for (int i = 0; i < DBUserShared.FingerCount; ++i)
                    {
                        byte[] bytes = null;
                        int index = 5 + i;
                        if (!reader.IsDBNull(index))
                        {
                            try
                            {
                                bytes = DBUserShared.AES_Decrypt((byte[])reader[index]);
                                lstFingerPrints.Add(i, bytes);
                            }
                            catch
                            {
                                //ignore but should not get in here
                                continue;
                            }
                        }
                    }
                    byte[] password = null;
                    if (reader[3] != System.DBNull.Value)
                    {
                        password = (byte[])reader[3];
                    }
                    User user = new User(int.Parse(reader[0].ToString()),
                                        reader[1].ToString(),
                                        reader[2].ToString(),
                                        password,
                                        byte.Parse(reader[4].ToString()),
                                        lstFingerPrints);
                    users.Add(user);
                }
                return "";
            }
            catch (Exception ex)
            {
                return "Error reading user database: " + ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }

                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        public DateTime ReadUserLastUpdated(out string message)
        {
            message = "";

            SqliteDataReader reader = null;
            SqliteConnection conn = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();
                SqliteCommand cmd = new SqliteCommand(conn);
                cmd.CommandText = "SELECT Value FROM UserLastUpdated";
                reader = cmd.ExecuteReader();

                //Safety, so there is always a date
                if (!reader.HasRows)
                {
                    reader.Close();
                    DateTime dt = DateTime.Now;
                    cmd.CommandText = " INSERT INTO UserLastUpdated(Value) VALUES(@date);";
                    cmd.Parameters.Add(new SqliteParameter("@date", dt));
                    cmd.ExecuteNonQuery();
                    return dt;
                }

                while (reader.Read())
                {
                    DateTime dt;
                    if (DateTime.TryParse(reader[0].ToString(), out dt))
                    {
                        return dt;
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                    reader = null;
                }

                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
            return DateTime.Now;
        }


        public string SaveUserLastUpdated(DateTime dt)
        {
            SqliteConnection conn = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    SqliteCommand cmd = new SqliteCommand(conn);
                    cmd.CommandText = "DELETE FROM UserLastUpdated";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO UserLastUpdated(Value) VALUES(@value)";
                    cmd.Parameters.Add(new SqliteParameter("@value", dt));
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        public static string GetDBSQL()
        {
            string result = " CREATE TABLE [User](id INTEGER PRIMARY KEY AUTOINCREMENT,";
            result += " description VARCHAR(" + DBUserShared.VARCHAR_StandardSize + ") collate nocase,";
            result += " fullname VARCHAR(" + DBUserShared.VARCHAR_StandardSize + ") NOT NULL collate nocase,";
            result += " password BLOB,";
            result += " deleted INTEGER DEFAULT 0,";
            result += " securityLevel INTEGER NOT NULL";
            for (int i = 0; i < DBUserShared.FingerCount; ++i)
            {
                result += String.Format(" ,fingerprint{0} BLOB", i);
            }
            result += " );";

            result += " CREATE TABLE UserLastUpdated(Value TEXT NOT NULL);";

            result += " INSERT INTO [User](id, description, fullname, securityLevel)";
            result += String.Format(" VALUES({0}, \"{1}\", \"{2}\", {3});",
                                    DefaultAdminUser.DefaultAdminID,
                                    DefaultAdminUser.DefaultAdminDescription,
                                    DefaultAdminUser.DefaultAdminName,
                                    DefaultAdminUser.DefaultAdminSecurityLevel);

            return result;
        }
    }
}
