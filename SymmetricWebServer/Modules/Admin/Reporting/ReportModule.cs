using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;
using WebServer.Database;
using WebServer.Tags;

namespace WebServer.Modules.Admin.Reporting
{
    public class ReportModule : EditControlModule
    {
        private const string Template = "template";
        private const string Form = "form";

        public ReportModule()
            : base(AccessLevels.Admin, "Reports", "report", "reports", "admin/reporting/configurereport")
        {
            Get["/testquery"] = _ =>
            {
                int connectionid = -1;
                if (this.Request.Query.connectionid != null)
                {
                    connectionid = this.Request.Query.connectionid;
                }
                string sql = "";
                if (this.Request.Query.sql != null)
                {
                    sql = this.Request.Query.sql;
                }
                
                string errorMessage = "";
                bool result = ConnectionItem.TestQuery(new DBContent().GetConnection(connectionid), sql, out errorMessage);
                return Response.AsJson(new TestQuery(result, "", errorMessage));
            };
        }

        protected override List<BasicEntry> Items()
        {
            return new DBContent().BasicReports;
        }

        protected override object ProcessAddItem()
        {
            this.Context.ViewBag.TemplateConst = ReportModule.Template;
            this.Context.ViewBag.FormConst = ReportModule.Form;
            return new CreateReportItem();
        }

        protected override bool ProcessDeleteItem(int id, out string errorMessage)
        {
            return new DBContent().DeleteReport(id, out errorMessage);
        }

        protected override object ProcessEditItem(int id)
        {
            this.Context.ViewBag.TemplateConst = ReportModule.Template;
            this.Context.ViewBag.FormConst = ReportModule.Form;

            string errormessage = "";
            ReportItemBase item = new DBContent().GetReport(id, out errormessage);
            if (item == null)
            {
                return null;
            }
            else
            {
                return new CreateReportItem(item);
            }
        }

        protected override ApplyResult ProcessApplyItem(ref object obj, out string errormessage, out string successmessage, out bool edited)
        {
            string action = "";
            if (this.Request.Form.action != null)
            {
                action = this.Request.Form.action.Value.ToLower();
            }
            errormessage = "";
            successmessage = "";
            edited = false;
            switch (action)
            {
                case "cancel":
                    return ApplyResult.Cancel;
            }

            this.Context.ViewBag.TemplateConst = ReportModule.Template;
            this.Context.ViewBag.FormConst = ReportModule.Form;

            int id = -1;
            if (this.Request.Form.objectID != null)
            {
                int.TryParse(this.Request.Form.objectID.Value.ToString(), out id);
            }

            string name = this.Request.Form.Name.Value;
            string group = this.Request.Form.GroupName.Value;

            ConnectionItem connection = null;
            if (this.Request.Form.Connection != null)
            {
                connection = new DBContent().GetConnection(int.Parse(this.Request.Form.Connection.Value));
            }

            FormItem form = null;
            int formID;
            if (int.TryParse(this.Request.Form.Form, out formID))
            {
                form = new DBContent().GetForm(formID);
            }

            TemplateItem template = null;
            int templateID = -1;
            if (int.TryParse(this.Request.Form.Template, out templateID))
            {
                template = new DBContent().GetTemplate(templateID);
            }

            obj = new CreateReportItem(new ReportItemBase(id,
                                       name,
                                       group,
                                       connection,
                                       form,
                                       template));

            foreach (KeyValuePair<string, object> kvp in (this.Request.Form as Nancy.DynamicDictionary).ToDictionary())
            {
                string[] split = kvp.Key.Split('.');
                if (split.Count() <= 1) continue;

                SWBaseTag tag = null;
                switch (split[0])
                {
                    case ReportModule.Form:
                        tag = (obj as CreateReportItem).ReportItem.FormTags.Where(x => x.Name.Equals(split[1])).FirstOrDefault();
                        if (tag != null)
                        {
                            tag.Value = kvp.Value.ToString();
                        }
                        break;
                    case ReportModule.Template:
                        tag = (obj as CreateReportItem).ReportItem.TemplateTags.Where(x => x.Name.Equals(split[1])).FirstOrDefault();
                        if (tag != null)
                        {
                            tag.Value = kvp.Value.ToString();
                        }
                        break;
                }
            }

            switch (action)
            {
                case "save":
                    bool result = new DBContent().SaveReport((obj as CreateReportItem).ReportItem, out errormessage);
                    if (result)
                    {
                        edited = (obj as CreateReportItem).ID > 0;
                        return ApplyResult.Save;
                    }
                    else
                    {
                        return ApplyResult.Message;
                    }
                default:
                    return ApplyResult.Message;
            }
        }
    }
}