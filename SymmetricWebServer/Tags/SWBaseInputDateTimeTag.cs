using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public abstract class SWBaseInputDateTimeTag : SWBaseTag
    {
        public SWBaseInputDateTimeTag(string tagType)
            : base(BaseTagTypes.Both, tagType)
        {
        }
    }
}
