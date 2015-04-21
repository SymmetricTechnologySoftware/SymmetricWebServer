using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.ModelBinding;
using WebServer.Modules.Admin;
using WebServer.Database;

namespace WebServer.Modules
{
    public abstract class EditControlModule : BaseControlModule
    {
        protected enum ApplyResult { Cancel, Save, Message }

        public string ConfigPage { private set; get; }

        public EditControlModule() :
            this(AccessLevels.None,  "", "", "", "", true)
        {

        }

        public EditControlModule(AccessLevels accessLevel, string heading, 
                                string alias, string mainPage, 
                                string configPage)
            : this(accessLevel, heading, alias, mainPage, configPage, false)
        {

        }

        private EditControlModule(AccessLevels accessLevel, string heading, 
                                  string alias, string mainPage, 
                                  string configPage, bool ignore)
            : base(accessLevel, heading, alias, mainPage)
        {
            if (ignore) return;
            this.ConfigPage = configPage;

            this.Model.ConfigPage = configPage;

            Get["/" + this.MainPage] = parameters =>
            {
                if (this.Request.Query[Master.Success] != null)
                {
                    switch ((string)this.Request.Query[Master.Success])
                    {
                        case Master.PostAdd:
                            this.SetSuccessMessage(String.Format("Successfully added a {0}.", this.Alias));
                            break;
                        case Master.PostEdit:
                            this.SetSuccessMessage(String.Format("Successfully edited a {0}.", this.Alias));
                            break;
                        case Master.PostDelete:
                            this.SetSuccessMessage(String.Format("Successfully deleted a {0}.", this.Alias));
                            break;
                    }
                }

                return this.PostView();
            };

            Post["/" + this.MainPage] = parameters =>
            {
                return PostView();   
            };
        }

        private object PostView()
        {
            if (this.Request.Query[Master.PostAction] != null)
            {
                int id = -1;
                if (this.Request.Query[Master.PostID] != null)
                {
                    int.TryParse(this.Request.Query[Master.PostID], out id);
                }
                else if (this.Request.Form.id != null)
                {
                    id = this.Request.Form.id;
                }

                switch ((string)this.Request.Query[Master.PostAction])
                {
                    case Master.PostAdd:
                        return this.AddItem();
                    case Master.PostEdit:
                        return this.EditItem(id);
                    case Master.PostApply:
                        return this.ApplyItem();
                    case Master.PostDelete:
                        return this.DeleteItem(id);
                }
            }

            return this.DefaultView();
        }

        private Negotiator AddItem()
        {
            this.Context.ViewBag.MainPage = this.MainPage;
            this.Context.ViewBag.ConfigPage = this.ConfigPage;
            return View["controls/configurecontrol", this.ProcessAddItem()];
        }

        protected abstract object ProcessAddItem();

        private object EditItem(int id)
        {
            object editingObject = ProcessEditItem(id);
            if (editingObject == null)
            {
                this.SetErrorMessage("Invalid editing ID.");
                return this.DefaultView();
            }
            this.Context.ViewBag.MainPage = this.MainPage;
            this.Context.ViewBag.ConfigPage = this.ConfigPage;
            return View["controls/configurecontrol", editingObject];
        }

        protected abstract object ProcessEditItem(int id);

        private object ApplyItem()
        {
            object obj = null;
            string errorMessage = "";
            string successMessage = "";
            bool edited;

            ApplyResult result = this.ProcessApplyItem(ref obj, out errorMessage, out successMessage, out edited);

            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                this.SetErrorMessage(errorMessage);
            }

            if(!String.IsNullOrWhiteSpace(successMessage))
            {
                this.SetSuccessMessage(successMessage);
            }

            switch(result)
            {
                case ApplyResult.Cancel:
                    return Response.AsRedirect(this.MainPage);
                case ApplyResult.Message:
                    break;
                case ApplyResult.Save:
                    return Response.AsRedirect(String.Format("{0}?{1}={2}", this.MainPage, Master.Success, edited ? Master.PostEdit : Master.PostAdd));
            }

            this.Context.ViewBag.MainPage = this.MainPage;
            this.Context.ViewBag.ConfigPage = this.ConfigPage;
            return View["controls/configurecontrol", obj];
        }

        protected abstract ApplyResult ProcessApplyItem(ref object obj, out string errorMessage, out string successMessage, out bool edited);

        private Negotiator DeleteItem(int id)
        {
            string errorMessage;
            if (this.ProcessDeleteItem(id, out errorMessage))
            {
                this.Context.ViewBag.MasterPageSuccess = String.Format("Successfully deleted a {0}.", this.Alias);
            }
            else
            {
                this.Context.ViewBag.MasterPageError = errorMessage;
            }

            return this.DefaultView();
        }

        private Negotiator DefaultView()
        {
            this.Model.Items = this.SortedList();
            return View["controls/editcontrol", this.Model];
        }

        protected abstract bool ProcessDeleteItem(int id, out string errorMessage);

    }
}
