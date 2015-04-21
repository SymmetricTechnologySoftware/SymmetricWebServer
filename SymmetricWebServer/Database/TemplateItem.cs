using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace WebServer.Database
{
    public class TemplateItem : HTMLItem
    {
        public int ID { set; get; }
        public string Name { set; get; }

        public TemplateItem(int id, string name, string html)
            : base(html)
        {
            this.ID = id;
            this.Name = name;
        }

        public TemplateItem()
            : this(-1, "", TemplateItem.DefaultTemplate)
        {

        }

        public static string DefaultTemplate
        {
            get
            {
                if(File.Exists(Globals.DefaultTemplate))
                {
                    return File.ReadAllText(Globals.DefaultTemplate).Replace("/r/n", "").Replace("/t", "");
                }
                return "";
            }
        }
    }
}
