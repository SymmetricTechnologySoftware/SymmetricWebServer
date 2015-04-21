using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebServer.Tags
{
    public abstract class SWBaseSQLTag : SWBaseTag
    {
        protected SWBaseSQLTag(BaseTagTypes baseTagType, string tagType)
            : base(baseTagType, tagType)
        {

        }

        public Dictionary<string, object> GetParameters()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (Match match in Regex.Matches(this.Value, @"(?<!\w)@\w+"))
            {
                string key = match.Value.ToLower();
                if (!result.ContainsKey(key))
                {
                    result.Add(key, "");
                }
            }
            return result;
        }
    }
}
