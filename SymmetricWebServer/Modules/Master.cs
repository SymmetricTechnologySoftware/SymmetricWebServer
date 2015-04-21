using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using UserShared;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Collections;
using WebServer.Database;

namespace WebServer.Modules
{
    public class Master : BaseWebModule
    {
        private const string LoginFailed = "failed";

        public Master()
            : this(AccessLevels.None)
        {

        }

        public Master(AccessLevels accessLevel)
        {
            Get["/"] = Index;

            Get["/index"] = Index;

            Post["/login"] = parameters =>
            {
                int id = -1;
                if (!int.TryParse(this.Request.Form.Fullname.Value, out id))
                {
                    return Response.AsRedirect(String.Format("Index?{0}={1}&{2}={3}", BaseWebModule.Login, BaseWebModule.Failed, BaseWebModule.ID, id));
                }

                string password = this.Request.Form.UserPassword;
                if (this.LoginUser(id, password))
                {
                    return Response.AsRedirect("Index");
                }
                else
                {
                    return Response.AsRedirect(String.Format("Index?{0}={1}&{2}={3}", BaseWebModule.Login, BaseWebModule.Failed, BaseWebModule.ID, id));
                }
            };

            Post["/logout"] = parameters =>
            {
                this.Logout();
                return Response.AsRedirect("Index");
            };
        }

        private dynamic Index(dynamic parameters)
        {
            int id = -1;
            if (this.Request.Query[BaseWebModule.ID])
            {
                int.TryParse(this.Request.Query[BaseWebModule.ID], out id);
            }

            this.Context.ViewBag.IsIndex = true;
            switch ((string)this.Request.Query[BaseWebModule.Login])
            {
                case Master.Failed:
                    this.Context.ViewBag.MasterPageError = "Invalid password.";
                    break;
                case Master.RequiredUser:
                    this.Context.ViewBag.MasterPageError = "You need to login before accessing this page.";
                    break;
                case Master.RequiredManager:
                    this.Context.ViewBag.MasterPageError = "You need to login as an manager before accessing this page.";
                    break;
                case Master.RequiredAdmin:
                    this.Context.ViewBag.MasterPageError = "You need to login as an admin before accessing this page.";
                    break;
            }
            return View["Index", this.SetupUsers(id)];
        }

        private dynamic SetupUsers(int id)
        {
            if (IsValidUser()) return null;

            List<OptionItem> users = null;
            string message = Globals.UserDB.LoadMasterUsers(out users, id);
            if (!String.IsNullOrWhiteSpace(message))
            {
                this.Context.ViewBag.MasterPageError = message;
                return null;
            }

            users.Sort(delegate(OptionItem a, OptionItem b)
            {
                return a.Name.CompareTo(b.Name.ToString());
            });

            dynamic model = new System.Dynamic.ExpandoObject();
            model.Users = users;
            return model;
        }
    }
}
