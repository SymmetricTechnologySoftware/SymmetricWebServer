using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class DefaultAdminUser : User
    {
        public const int DefaultAdminID = 1;
        public const string DefaultAdminDescription = "This is the default admin user";
        public const string DefaultAdminName = "Admin";
        public const byte DefaultAdminSecurityLevel = 100;

        public DefaultAdminUser(byte[] password)
            : base(DefaultAdminUser.DefaultAdminID, DefaultAdminUser.DefaultAdminDescription,
                   DefaultAdminUser.DefaultAdminName, password, DefaultAdminUser.DefaultAdminSecurityLevel, null)
        {
            
        }

        public DefaultAdminUser(byte[] password, bool passwordChanged, string newPassword)
            : base(DefaultAdminUser.DefaultAdminID, DefaultAdminUser.DefaultAdminDescription,
                   DefaultAdminUser.DefaultAdminName, password, DefaultAdminUser.DefaultAdminSecurityLevel, null, 
                   passwordChanged, newPassword)
        {
            
        }
    }
}
