using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using System.Net;
using Nancy.Responses.Negotiation;
using WebServer.Database;
using WebServer.Tags;

namespace WebServer.Modules.ViewReport
{
    public class ViewReportModule : ViewControlModule
    {
        public ViewReportModule()
            : base(AccessLevels.User, "View Reports", "report", "viewreports", "viewreport")
        {
            Before += ctx =>
            {
                this.Context.ViewBag.IsViewReport = true;
                return null;
            };

            Post["/getreport"] = _ =>
            {
                int id = -1;
                if (this.Request.Form["objectID"] != null &&
                    int.TryParse(this.Request.Form["objectID"], out id))
                {
                    string errormessage;

                    DisplayReportItem item = GetDisplayItem(id, out errormessage);

                    if (!String.IsNullOrWhiteSpace(errormessage))
                    {
                        return Response.AsJson(errormessage);
                    }
                  
                    item.CreateTemplate(out errormessage);
                    if (!String.IsNullOrWhiteSpace(errormessage))
                    {
                        return Response.AsJson(errormessage);
                    }
                    return Response.AsJson(item.TemplateHTML);
                }
                return Response.AsJson("Invalid id");
            };
        }

        protected override List<BasicEntry> Items()
        {
            return new DBContent().BasicReports;
        }

        protected override object ProcessDisplayItem(int id, out string errormessage)
        {
            this.StretchContainer(true);
            Dictionary<string, object> formValues = (this.Request.Form as Nancy.DynamicDictionary).ToDictionary();
            DisplayReportItem item = this.GetDisplayItem(id, out errormessage);
            if (!String.IsNullOrWhiteSpace(errormessage)) return null;

            if (item.ReportItem.FormItem != null)
            {
                item.CreateForm(out errormessage);
                if (!String.IsNullOrWhiteSpace(errormessage)) return null;
            }
            return item;
        }

        private DisplayReportItem GetDisplayItem(int id, out string errormessage)
        {
            ReportItemBase reportBase = new DBContent().GetReport(id, out errormessage);
            if (!String.IsNullOrWhiteSpace(errormessage))
            {
                return null;
            }
     
           Dictionary<string, object> formValues = (this.Request.Form as Nancy.DynamicDictionary).ToDictionary();
              
            return new DisplayReportItem(reportBase, formValues);
        }
    }
}
