using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Tags;

namespace WebServer.Database
{
    public class ReportItemBase
    {
        public ReportItemBase()
        {

        }

        public ReportItemBase(int id,
                              string name,
                              string groupName,
                              ConnectionItem connection,
                              FormItem form,
                              TemplateItem template)
        {
            this.ID = id;
            this.Name = name;
            this.GroupName = groupName;
            this.ConnectionItem = connection;
            this.FormItem = form;
            this.TemplateItem = template;
        }

        public int ID { private set; get; }
        public string Name { private set; get; }
        public string GroupName { private set; get; }
        public ConnectionItem ConnectionItem { private set; get; }
        private FormItem _formItem;
        public FormItem FormItem 
        {
            private set
            {
                _formItem = value;
                if (_formItem != null)
                {
                    this._formTags = SWBaseTag.GetTags(_formItem.HTML, SWBaseTag.BaseTagTypes.Form);
                }

                if (_formTags == null)
                {
                    this._formTags = new List<SWBaseTag>();
                }
            }
            get
            {
                return _formItem;
            }
        }

        private TemplateItem _templateItem;
        public TemplateItem TemplateItem
        {
            private set
            {
                _templateItem = value;
                if (_templateItem != null)
                {
                    this._templateTags = SWBaseTag.GetTags(_templateItem.HTML, SWBaseTag.BaseTagTypes.Template);
                }

                if (_templateTags == null)
                {
                    this._templateTags = new List<SWBaseTag>();
                }
            }
            get
            {
                return _templateItem;
            }
        }

        private List<SWBaseTag> _formTags;
        public ReadOnlyCollection<SWBaseTag> FormTags
        {
            get
            {
                return _formTags.AsReadOnly();
            }
        }

        private List<SWBaseTag> _templateTags;
        public ReadOnlyCollection<SWBaseTag> TemplateTags 
        {
            get
            {
                return _templateTags.AsReadOnly();
            }
        }
    }
}
