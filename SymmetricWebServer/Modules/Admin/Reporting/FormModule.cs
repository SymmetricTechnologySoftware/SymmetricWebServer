using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using WebServer.Database;
using WebServer.Modules.Admin.Reporting;
using WebServer.Tags;

namespace WebServer.Modules.Admin.Reporting
{
    public class FormModule : EditControlModule
    {
        public FormModule()
            : base(AccessLevels.Admin, "Forms", "form", "forms", "admin/reporting/configureform")
        {

        }

        protected override List<BasicEntry> Items()
        {
            return new DBContent().BasicForms;
        }

        protected override object ProcessAddItem()
        {
            return new FormItem();
        }

        protected override bool ProcessDeleteItem(int id, out string errormessage)
        {
            return new DBContent().DeleteForm(id, out errormessage);
        }

        protected override object ProcessEditItem(int id)
        {
            return new DBContent().GetForm(id);
        }

        protected override ApplyResult ProcessApplyItem(ref object obj, out string errormessage, out string successmessage, out bool edited)
        {
            errormessage = "";
            successmessage = "";
            edited = false;
            string action = this.Request.Form.action.Value.ToLower();
            int objectID = -1;
            if (this.Request.Form.objectID != null)
            {
                objectID = int.Parse(this.Request.Form.objectID.Value.ToString());
            }

            string html = this.Request.Form.HTML.Value;

            FormItem item = new FormItem(objectID,
                                         this.Request.Form.Name.Value,
                                         html);
            obj = item;



            switch (action)
            {
                case BaseWebModule.PostSave:

                    List<SWBaseTag> tags = SWBaseTag.GetTags(html, SWBaseTag.BaseTagTypes.Form);
                    foreach (SWBaseTag t1 in tags)
                    {
                        foreach (SWBaseTag t2 in tags)
                        {
                            if (!t1.Equals(t2) &&
                                t1.Name.Equals(t2.Name))
                            {
                                errormessage = String.Format("There are one or more tags with the same name {0}.", t1.Name);
                                return ApplyResult.Message;
                            }
                        }
                    }

                    bool result = false;
                    result = new DBContent().SaveForm(item, out errormessage);
                    edited = item.ID > 0;

                    if (result)
                    {
                        return ApplyResult.Save;
                    }
                    break;
                case BaseWebModule.PostCancel:
                    return ApplyResult.Cancel;
            }
            return ApplyResult.Message;
        }
    }
}
