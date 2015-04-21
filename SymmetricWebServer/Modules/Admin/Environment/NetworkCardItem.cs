using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Modules.Admin.Environment
{
    public class NetworkCardItem
    {
        public string Alias { set; get; }
        public bool IsActive { set; get; }
        public string StrIsActive
        {
            get
            {
                if (this.IsActive)
                {
                    return "checked";
                }
                else
                {
                    return "";
                }
            }
        }
        public string NetworkCardFullName { set; get; }
    }
}
