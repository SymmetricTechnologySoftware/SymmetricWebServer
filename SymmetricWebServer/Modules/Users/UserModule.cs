using Nancy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserShared;
using Nancy.ModelBinding;
using WebServer.Database;

namespace WebServer.Modules.Users
{
    public class UserModule : EditControlModule
    {
        public UserModule()
            : base(AccessLevels.Manager, "Users", "user", "users", "configureuser")
        {
            Before += ctx =>
            {
                this.Context.ViewBag.IsUsers = true;
                return null;
            };
        }

        protected override List<BasicEntry> Items()
        {
            return Globals.UserDB.BasicUsers(this.SecurityLevel, this.UserID);
        }

        protected override object ProcessAddItem()
        {
            return new UserItem() { SecurityLevel = (byte)(this.SecurityLevel - 1), MaxSecurityLevel = (byte)(this.SecurityLevel - 1) };
            // return new ConnectionItem();
        }

        protected override bool ProcessDeleteItem(int id, out string errorMessage)
        {
            UserItem user = Globals.UserDB.GetUserItem(id, out errorMessage);
            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                return false;
            }

            if (user == null)
            {
                errorMessage = "Invalid user.";
                return false;
            }

            if (this.UserID == user.ID)
            {
                errorMessage = "You cannot delete yourself.";
                return false;
            }

            if (!this.CheckModifyUser(id, user.SecurityLevel, out errorMessage))
            {
                return false;
            }
            return Globals.UserDB.DeleteUser(id, out errorMessage);
        }

        protected override object ProcessEditItem(int id)
        {
            string errormessage = "";
            UserItem user = Globals.UserDB.GetUserItem(id, out errormessage);
            if (!String.IsNullOrWhiteSpace(errormessage))
            {
                return null;
            }

            if (user != null)
            {
                if (!this.CheckModifyUser(id, user.SecurityLevel, out errormessage))
                {
                    user = null;
                }
                else
                {
                    if (this.UserID == id)
                    {
                        user.EnableSecurityLevel = false;
                        user.MaxSecurityLevel = this.SecurityLevel;
                    }
                    else
                    {
                        user.MaxSecurityLevel = (byte)(this.SecurityLevel - 1);
                        user.EnableSecurityLevel = true;
                    }
                }
            }
            return user;
        }

        protected override ApplyResult ProcessApplyItem(ref object obj, out string errormessage, out string successmessage, out bool edited)
        {
            string action = this.Request.Form.action.Value.ToLower();

            errormessage = "";
            successmessage = "";
            edited = false;

            switch (action)
            {
                case "save":
                    int id = int.Parse(this.Request.Form.objectID.Value);
                    string fullname = this.Request.Form.fullname;
                    string description = this.Request.Form.description;
                    byte securityLevel;
                    if (this.UserID == id)
                    {
                        securityLevel = this.SecurityLevel;
                    }
                    else
                    {
                        securityLevel = byte.Parse(this.Request.Form.securitylevel);
                    }
                    
                    string password = this.Request.Form.password;
                    string confirmpassword = this.Request.Form.confirmpassword;

                    obj = new UserItem(id, fullname, securityLevel, description) { MaxSecurityLevel = (byte)(this.SecurityLevel - 1) };
                    (obj as UserItem).EnableSecurityLevel = (this.UserID != id);
                    if(!this.CheckModifyUser(id, securityLevel, out errormessage))
                    {
                        return ApplyResult.Message;
                    }
                    
                    errormessage = Globals.UserDB.AddEditUser(
                                            new User(id,
                                                description,
                                                fullname,
                                                null,
                                                securityLevel,
                                                null, true, password), false, true, false);
                    
                    if (!String.IsNullOrWhiteSpace(errormessage))
                    {
                        return ApplyResult.Message;
                    }
                    return ApplyResult.Save;
                case "cancel":
                    return ApplyResult.Cancel;
            }
            return ApplyResult.Message;
        }
    }
}
