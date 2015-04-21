using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Database;

namespace WebServer.Tags
{
    public class SWQueryTag : SWBaseSQLTag
    {
        public const string Type = "query";

        public SWQueryTag()
            : base(BaseTagTypes.Template, SWQueryTag.Type)
        {
        }
    }
}
