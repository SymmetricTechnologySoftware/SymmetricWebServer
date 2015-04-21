using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Modules.Admin.Environment
{
    public class EnvironmentItem
    {
        public string Port { set; get; }
        public bool StartOnLogin { set; get; }
        public string StrStartOnLogin
        {
            get
            {
                if (this.StartOnLogin)
                {
                    return "checked";
                }
                else
                {
                    return "";
                }
            }
        }
        public bool EnableRestGetUsers { set; get; }
        public string StrEnableRestGetUsers
        {
            get
            {
                if (this.EnableRestGetUsers)
                {
                    return "checked";
                }
                else
                {
                    return "";
                }
            }
        }
        public List<NetworkCardItem> NetworkCards { set; get; }


        public EnvironmentItem()
        {
            NetworkCards = new List<NetworkCardItem>();
        }
    }
}
