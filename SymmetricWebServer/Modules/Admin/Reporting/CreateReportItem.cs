using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Database;
using WebServer.Tags;

namespace WebServer.Modules.Admin.Reporting
{
    public class CreateReportItem
    {
        public CreateReportItem(ReportItemBase reportItem)
        {
            this.Groups = new DBContent().ReportGroups;
            this.ReportItem = reportItem;
            this.Connections = new DBContent().BasicConnections;
            this.Forms = new DBContent().FullForms;
            this.Templates = new DBContent().FullTemplates;

            if (this.ReportItem == null)
            {
                this.ReportItem = new ReportItemBase(-1, "", "",
                                                     this.Connections.Count() > 0 ? new DBContent().GetConnection(this.Connections[0].ID) : null,
                                                     null,
                                                     this.Templates.Count() > 0 ?  new DBContent().GetTemplate(this.Templates[0].ID): null);
            }

            //Connections
            if (this.ReportItem.ConnectionItem != null)
            {
                foreach (OptionItem item in this.Connections)
                {
                    item.SetSelected(item.ID == ReportItem.ConnectionItem.ID);
                }
            }

            //Forms
            this.Forms.Insert(0, new BasicEntry(-1, "", ""));
            if (ReportItem.FormItem != null)
            {
                foreach (OptionItem item in this.Forms)
                {
                    item.SetSelected(item.ID == ReportItem.FormItem.ID);
                }
            }

            //Templates
            if (ReportItem.TemplateItem != null)
            {
                foreach (OptionItem item in this.Templates)
                {
                    item.SetSelected(item.ID == ReportItem.TemplateItem.ID);
                }
            }
        }

        public CreateReportItem()
            : this(null)
        {

        }

        public ReportItemBase ReportItem { private set; get; }

        public List<string> Groups { private set; get; }

        public List<BasicEntry> Connections { private set; get; }

        public List<BasicEntry> Forms { private set; get; }

        public List<BasicEntry> Templates { private set; get; }

        public int ID
        {
            get
            {
                return this.ReportItem.ID;
            }
        }

        public List<SWBaseTag> TemplateVarTags
        {
            get
            {
                return ReportItem.TemplateTags.Where(x => x is SWVarTag).Select(y => y).ToList();
            }
        }

        public List<SWBaseTag> TemplateQueryTags
        {
            get
            {
                return ReportItem.TemplateTags.Where(x => x is SWQueryTag).Select(y => y).ToList();
            }
        }

        public List<SWBaseTag> FormVarTags
        {
            get
            {
                return ReportItem.FormTags.Where(x => x is SWVarTag).Select(y => y).ToList();
            }
        }

        public List<SWBaseTag> FormSelectTags
        {
            get
            {
                return ReportItem.FormTags.Where(x => x is SWSelectTag).Select(y => y).ToList();
            }
        }
    }
}
