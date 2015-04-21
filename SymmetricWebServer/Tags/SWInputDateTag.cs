using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public class SWInputDateTag : SWBaseInputDateTimeTag
    {
        public const string Type = "inputdate";

        public SWInputDateTag()
            : base(SWInputDateTag.Type)
        {
        }

    }
}
