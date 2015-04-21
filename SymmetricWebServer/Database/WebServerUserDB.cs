using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using UserShared;

namespace WebServer.Database
{
    public class WebServerUserDB : DBUserShared
    {
        public WebServerUserDB(string contentFolder) :
            base(contentFolder)
        {

        }

        public UserItem GetUserItem(int id, out string errormessage)
        {
            errormessage = "";
            SqliteConnection conn = null;
            SqliteDataReader reader = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();
                SqliteCommand cmd = new SqliteCommand(conn);

                cmd.CommandText = "SELECT id, fullname, securityLevel, description  FROM User";
                cmd.CommandText += " WHERE id = " + id;
                
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    return new UserItem(int.Parse(reader[0].ToString()),
                                        reader[1].ToString(),
                                        byte.Parse(reader[2].ToString()),
                                        reader[3].ToString());
                }
                
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
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
            return null;
        }

        public List<BasicEntry> BasicUsers(int currentSecurityLevel, int currentUserID)
        {
                List<BasicEntry> result = new List<BasicEntry>();
                SqliteConnection conn = null;
                SqliteDataReader reader = null;
                try
                {
                    conn = new SqliteConnection(@"data source=" + this.UsersFile);
                    conn.Open();
                    SqliteCommand cmd = new SqliteCommand(conn);

                    cmd.CommandText = "SELECT id, fullname, securityLevel FROM User";
                    cmd.CommandText += String.Format(" WHERE deleted = 0 AND (securityLevel < {0} OR id = {1})", currentSecurityLevel, currentUserID); 
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result.Add(new BasicEntry(int.Parse(reader[0].ToString()),
                                reader[1].ToString(), User.SecurityName(byte.Parse(reader[2].ToString()))));
                    }
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
                return result;
        }


        public string LoadMasterUsers(out List<OptionItem> users, int selectedID)
        {
            users = new List<OptionItem>();
            SqliteConnection conn = null;
            SqliteDataReader reader = null;
            try
            {
                conn = new SqliteConnection(@"data source=" + this.UsersFile);
                conn.Open();
                SqliteCommand cmd = new SqliteCommand(conn);

                cmd.CommandText = "SELECT id, fullname FROM User WHERE deleted = 0 ";
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = int.Parse(reader[0].ToString());
                    OptionItem user = new OptionItem(id, 
                                                     reader[1].ToString(),
                                                     id == selectedID);
                    users.Add(user);
                }
                reader.Close();
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
    }
}
