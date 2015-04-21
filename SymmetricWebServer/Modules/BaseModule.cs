using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserShared;
using WebServer.Database;

namespace WebServer.Modules
{
    public abstract class BaseModule : NancyModule
    {
        private const string Session_UserID = "UserID";
        private const string Session_UserFullName = "UserFullName";
        private const string Session_UserSecurityLevel = "UserSecurityLevel";
        protected const string RequiredUser = "requireduser";
        protected const string RequiredAdmin = "requiredadmin";
        protected const string RequiredManager = "requiredmananger";

        public int UserID
        {
            get
            {
                object obj = Request.Session[Session_UserID];
                if (obj == null)
                {
                    return 0;
                }
                else
                {
                    return (int)obj;
                }
            }
        }

        public string FullName
        {
            get
            {
                object obj = Request.Session[Session_UserFullName];
                if (obj == null)
                {
                    return "";
                }
                else
                {
                    return obj.ToString();
                }
            }
        }

        public byte SecurityLevel
        {
            get
            {
                object obj = Request.Session[Session_UserSecurityLevel];
                if (obj == null)
                {
                    return 0;
                }
                else
                {
                    return (byte)obj;
                }
            }
        }

       /* public DBContent ContentDatabase
        {
            get
            {
                object obj = Request.Session[Session_DBContent];
                if (obj == null)
                {
                    return null;
                }
                else
                {
                    return (DBContent)obj;
                }
            }
        }*/

        public bool IsValidUser()
        {
            return this.UserID > 0;
        }

        public bool IsValidManager()
        {
            return this.IsValidUser() &&
                this.SecurityLevel >= User.ManagerLevel;
        }

        public bool IsValidAdmin()
        {
            return this.IsValidUser() &&
                   this.SecurityLevel >= User.AdminLevel;
        }

        protected bool LoginUser(int id, string password)
        {
            string message;
            User user = Globals.UserDB.ValidLogin(id, password, out message);
            if (!String.IsNullOrWhiteSpace(message)) return false;
            
            if (user == null)
            {
                Logout();
                return false;
            }
            else
            {
                Request.Session[Session_UserID] = id;
                Request.Session[Session_UserFullName] = user.FullName;
                Request.Session[Session_UserSecurityLevel] = user.SecurityLevel;
                return true;
            }
        }

        protected void Logout()
        {
            Request.Session.DeleteAll();
        }

        public bool CheckModifyUser(int modifyID, int modifySecurityLevel,
                                    out string errormessage)
        {
            errormessage = "";
            if (!this.IsValidUser())
            {
                errormessage = "Please log in to continue.";
                return false;
            }

            if (this.SecurityLevel < User.ManagerLevel)
            {
                errormessage = "Only a security level of manager or higher can edit users.";
                return false;
            }

            if(this.SecurityLevel < modifySecurityLevel && 
                this.UserID == modifyID)
            {
                errormessage = "You cannot lower your own security level.";
                return false;
            }

            if (modifySecurityLevel > this.SecurityLevel)
            {
                errormessage = "You are not authorised to set this users security level the same or above your own.";
                return false;
            }

            return true;
        }
    }


}
