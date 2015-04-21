using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public class SWInputTimeTag : SWBaseInputDateTimeTag
    {
        public const string Type = "inputtime";

        public SWInputTimeTag()
            : base(SWInputTimeTag.Type)
        {
        }
    }
}
