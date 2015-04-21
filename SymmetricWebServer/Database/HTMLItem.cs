using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Database
{
    public abstract class HTMLItem
    {
        public string HTML { private set; get; }

        protected HTMLItem(string html)
        {
            this.HTML = html;
        }
    }
}
