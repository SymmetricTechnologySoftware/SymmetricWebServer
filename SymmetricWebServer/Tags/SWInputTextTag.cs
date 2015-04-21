using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public class SWInputTextTag : SWBaseSQLTag
    {
        public const string Type = "inputtext";

        public SWInputTextTag()
            : base(BaseTagTypes.Both, SWInputTextTag.Type)
        {
        }
    }
}
