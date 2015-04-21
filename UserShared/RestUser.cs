using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class RestUser
    {
        public bool IsDefaultAdmin { set; get; }
        public int UserID { set; get; }
        public string Description { set; get; }
        public string FullName { set; get; }
        public List<byte> Password { set; get; }
        public byte SecurityLevel { set; get; }
        public bool PasswordChanged { set; get; }
        public string NewPassword { set; get; }

        public List<RestFingerPrint> FingerPrints { set; get; }
        public RestUser()
        {
            FingerPrints = new List<RestFingerPrint>();
            Password = new List<byte>();
        }

        public static User RestToUser(RestUser restUser)
        {
            Dictionary<int, byte[]> fingerprints = new Dictionary<int, byte[]>();
            foreach (RestFingerPrint fp in restUser.FingerPrints)
            {
                byte[] print = null;
                if (fp.Print != null)
                {
                    print = fp.Print.ToArray();
                }
                fingerprints.Add(fp.PrintNumber, print);
            }

            byte[] password = null;
            if (restUser.Password != null)
            {
                password = restUser.Password.ToArray();
            }

            if (restUser.IsDefaultAdmin)
            {
                return new DefaultAdminUser(password,
                                            restUser.PasswordChanged,
                                            restUser.NewPassword);
            }
            else
            {
                return new User(restUser.UserID,
                                 restUser.Description,
                                 restUser.FullName,
                                 password,
                                 restUser.SecurityLevel,
                                 fingerprints,
                                 restUser.PasswordChanged,
                                 restUser.NewPassword);
            }
        }

        public static List<User> RestToUser(List<RestUser> restUsers)
        {
            if (restUsers == null) return null;
            List<User> result = new List<User>();
            foreach (RestUser restUser in restUsers)
            {
                result.Add(RestToUser(restUser));
            }
            return result;
        }

        public static RestUser UserToRest(User usr)
        {
            return UserToRest(usr, true);
        }

        public static RestUser UserToRest(User usr, bool usePassword)
        {
            if (usr == null) return null;
            RestUser restUser = new RestUser()
            {
                UserID = usr.ID,
                Description = usr.Description,
                FullName = usr.FullName,
                SecurityLevel = usr.SecurityLevel
            };

            if (usePassword)
            {
                if (usr.Password != null)
                {
                    restUser.Password = usr.Password.ToList();
                }
                restUser.PasswordChanged = usr.PasswordChanged;
                restUser.NewPassword = usr.NewPassword;
            }

            if (usr is DefaultAdminUser)
            {
                restUser.IsDefaultAdmin = true;
            }

            foreach (FingerPrint fp in usr.FingerPrints)
            {
                List<byte> print = null;
                if (fp.Print != null)
                {
                    print = fp.Print.ToList();
                }
                restUser.FingerPrints.Add(new RestFingerPrint() { PrintNumber = fp.PrintNumber, Print = print });
            }
            return restUser;
        }
    }
}
