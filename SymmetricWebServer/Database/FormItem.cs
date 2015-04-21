using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace WebServer.Database
{
    public class FormItem : HTMLItem
    {
        public int ID { set; get; }
        public string Name { set; get; }

        public FormItem(int id, string name, string html)
            : base(html)
        {
            this.ID = id;
            this.Name = name;
        }

        public FormItem()
            : this(-1, "", FormItem.DefaultForm)
        {

        }

        public static string DefaultForm
        {
            get
            {
                if (File.Exists(Globals.DefaultForm))
                {
                    return File.ReadAllText(Globals.DefaultForm).Replace("/r/n", "").Replace("/t", "");
                }
                return "";
            }
        }
    }
}
