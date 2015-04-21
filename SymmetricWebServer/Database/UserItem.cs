using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserShared;

namespace WebServer.Database
{
    public class UserItem
    {
        public int ID { set; get; }
        public string FullName { set; get; }
        public byte SecurityLevel { set; get; }
        public string Description { set; get; }
        public byte MaxSecurityLevel { set; get; }
        public bool EnableSecurityLevel {  set; get; }
        public string StrEnableSecurityLevel
        {
            get
            {
                if (this.EnableSecurityLevel)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
        }

        public UserItem()
        {
            this.EnableSecurityLevel = true;
        }

        public UserItem(int id,
                        string fullname,
                        byte securityLevel,
                        string description)
            : this()
        {
            this.ID = id;
            this.FullName = fullname;
            this.SecurityLevel = securityLevel;
            this.Description = description;
        }

        public byte AdminValue
        {
            get
            {
                return User.AdminLevel;
            }
        }

        public byte ManagerValue
        {
            get
            {
                return User.ManagerLevel;
            }
        }

        public byte SupervisorValue
        {
            get
            {
                return User.SupervisorLevel;
            }
        }

        public byte SeniorOperatorValue
        {
            get
            {
                return User.SeniorOperatorLevel;
            }
        }

    }
}
