using Nancy.Responses.Negotiation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Modules
{
    public abstract class ViewControlModule : BaseControlModule
    {
        protected const string PostDisplay = "display";

        public string DisplayPage { private set; get; }

        public ViewControlModule() :
            this(AccessLevels.None,  "", "", "", "", true)
        {

        }

        public ViewControlModule(AccessLevels accessLevel,
                                string heading, string alias,
                                string mainPage, string displayPage)
            : this(accessLevel, heading, alias, mainPage, displayPage, false)
        {

        }

        private ViewControlModule(AccessLevels accessLevel,
                                string heading, string alias,
                                string mainPage, string displayPage,
                                bool ignore)
            : base(accessLevel, heading, alias, mainPage)
        {
            if (ignore) return;

            this.DisplayPage = displayPage;

            Get["/" + this.MainPage] = parameters =>
            {
                return this.GetView();
            };

            Post["/" + this.MainPage] = parameters =>
            {
                return GetView();
            };
        }

        private Negotiator GetView()
        {
            if (this.Request.Query[Master.PostAction] != null)
            {
                int id = -1;
                if (this.Request.Query[Master.PostID] != null)
                {
                    int.TryParse(this.Request.Query[Master.PostID], out id);
                }
                else if (this.Request.Form.objectID != null)
                {
                    id = this.Request.Form.objectID;
                }

                switch ((string)this.Request.Query[Master.PostAction])
                {
                    case ViewControlModule.PostDisplay:
                        return this.DisplayItem(id);
                }
            }
            this.Model.Items = this.SortedList();
            return View["controls/viewcontrol", this.Model];
        }

        private Negotiator DisplayItem(int id)
        {
            this.Context.ViewBag.MainPage = this.MainPage;
            this.Context.ViewBag.DisplayPage = this.DisplayPage;

            string errorMessage;
            object obj = this.ProcessDisplayItem(id, out errorMessage);

            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                this.Context.ViewBag.MasterPageError = errorMessage;
            }
            return View["controls/displaycontrol", obj];
        }

        protected abstract object ProcessDisplayItem(int id, out string errorMessage);
    }
}
