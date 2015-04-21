using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public class SWDateTag : SWBaseTag
    {
        public const string Type = "date";

        public SWDateTag()
            : base(BaseTagTypes.Template, SWDateTag.Type)
        {
        }
    }
}
