using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class DefaultUser : User
    {
        public const int DefaultUserID = -2;
        public const string DefaultUserDescription = "This is the default user when a workstation cannot connect to the web server";
        public const string DefaultUserName = "Default User";
        public const int DefaultUserSecurityLevel = 100;

        public DefaultUser()
            : base(DefaultUser.DefaultUserID, DefaultUser.DefaultUserDescription,
                   DefaultUser.DefaultUserName, null, DefaultUser.DefaultUserSecurityLevel, null)
        {
            
        }
    }
}
