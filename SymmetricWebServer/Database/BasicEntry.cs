using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Database
{
    public class BasicEntry : OptionItem
    {
        public string GroupName { set; get; }

        public BasicEntry(int id, string name, string groupName)
            : base(id, name)
        {
            this.GroupName = groupName;
        }
    }
}
