using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserShared;
using Nancy;
using WebServer.Database;
using System.Reflection;
using System.Diagnostics;
using System.Net;

namespace WebServer.Modules
{
    public abstract class BaseWebModule : BaseModule
    {
        protected const string PostAction = "action";
        protected const string PostID = "id";
        protected const string PostDelete = "delete";
        protected const string PostApply = "apply";
        protected const string PostAdd = "add";
        protected const string PostEdit = "edit";
        protected const string PostSave = "save";
        protected const string PostCancel = "cancel";

        protected const string Add = "add";
        protected const string Edit = "edit";
        protected const string Login = "login";
      
        protected const string Success = "success";
        protected const string Failed = "failed";

        protected const string NavBar = "navbar";
        protected const string Hidden = "hidden";
        protected const string Visible = "visible";

        protected const string ID = "id";

        protected const string Failed_InvalidID = "id";

        public enum AccessLevels { None, User, Manager, Admin,  }
        public AccessLevels AccessLevel { private set; get; }

        public BaseWebModule()
            : this(AccessLevels.None)
        {
            
        }

        public BaseWebModule(AccessLevels accessLevel)
        {
            this.AccessLevel = accessLevel;

            Before += ctx =>
            {
                if (this.IsValidUser())
                {
                    this.Context.ViewBag.IsValidUser = true;
                    this.Context.ViewBag.FullName = this.FullName;

                    if (IsValidManager())
                    {
                        this.Context.ViewBag.IsValidUserManager = true;
                    }

                    if (IsValidAdmin())
                    {
                        this.Context.ViewBag.IsValidUserAdmin = true;
                    }
                }


                this.SetupFooter();
                switch (this.AccessLevel)
                {
                    case AccessLevels.Admin:
                        if (!this.IsValidAdmin())
                        {
                            return Response.AsRedirect(String.Format("index?{0}={1}", BaseWebModule.Login, BaseWebModule.RequiredAdmin));
                        }
                        break;
                    case AccessLevels.Manager:
                        if (!this.IsValidManager())
                        {
                            return Response.AsRedirect(String.Format("index?{0}={1}", BaseWebModule.Login, BaseWebModule.RequiredManager));
                        }
                        break;
                    case AccessLevels.User:
                        if (!this.IsValidUser())
                        {
                            return Response.AsRedirect(String.Format("index?{0}={1}", BaseWebModule.Login, BaseWebModule.RequiredUser));
                        }
                        break;
                    case AccessLevels.None:
                        break;
                }

                if (this.Request.Query[Master.NavBar] != null)
                {
                    switch (((string)this.Request.Query[Master.NavBar]).ToLower())
                    {
                        case Master.Hidden:
                            this.Context.ViewBag.IsNavBarHidden = true;
                            break;
                    }
                }
                return null;
            };
        }

        private void SetupFooter()
        {
            this.Context.ViewBag.VersionInfo = Globals.Version;
        }

        protected void SetSuccessMessage(string message)
        {
            this.Context.ViewBag.MasterPageSuccess = message;
        }

        protected void SetErrorMessage(string message)
        {
            this.Context.ViewBag.MasterPageError = message;
        }

        protected void StretchContainer(bool value)
        {
            this.Context.ViewBag.StretchContainer = value ? "true" : null;
        }
    }
}
