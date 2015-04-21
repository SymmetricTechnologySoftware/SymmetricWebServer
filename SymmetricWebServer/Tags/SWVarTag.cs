using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public class SWVarTag : SWBaseTag
    {
        public const string Type = "var";

        public SWVarTag()
            : base(BaseTagTypes.Both, SWVarTag.Type)
        {

        }
    }
}
