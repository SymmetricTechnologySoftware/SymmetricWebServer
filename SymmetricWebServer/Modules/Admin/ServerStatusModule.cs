using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Modules.Admin
{
    public class ServerStatusModule : BaseWebModule
    {
        public ServerStatusModule()
            : base(AccessLevels.Admin)
        {
            Get["/serverstatus"] = _ =>
            {
                return View["admin/serverstatus", Program.Handle.URIs.Select(x => x.ToString()).ToList<string>()];
            };
        }


    }
}
