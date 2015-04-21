using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public class SWSelectTag : SWBaseSQLTag
    {
        public const string Type = "select";

        public SWSelectTag()
            : base(BaseTagTypes.Both ,SWSelectTag.Type)
        {
        }
    }
}
