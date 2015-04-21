using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class User
    {
        private static readonly byte[] salt = { (byte)'i', (byte)'n', 
                                                (byte)'3', (byte)'y', 
                                                (byte)'g', (byte)'o', 
                                                (byte)'x', (byte)'-', 
                                                (byte)'v' };

        public const byte SeniorOperatorLevel = 20;
        public const byte SupervisorLevel = 40;
        public const byte ManagerLevel = 60;
        public const byte AdminLevel = 80;

        private List<FingerPrint> _lstFingerPrints;
        public ReadOnlyCollection<FingerPrint> FingerPrints
        {
            get
            {
                return _lstFingerPrints.AsReadOnly();
            }
        }

        public int ID { set; get; }
        public string Description { private set; get; }
        public string FullName { private set; get; }
        public byte[] Password { private set; get; }
        public byte SecurityLevel { private set; get; }
        public bool PasswordChanged { private set; get; }
        public string NewPassword { private set; get; }

        public bool IsSeniorOperator
        {
            get
            {
                return User.IsUserSeniorOperator(this.SecurityLevel);
            }
        } 

        public bool IsSupervisor
        {
            get
            {
                return User.IsUserSupervisor(this.SecurityLevel);
            }
        } 

        public bool IsManager
        {
            get
            {
                return User.IsUserManager(this.SecurityLevel);
            }
        } 
        
        public bool IsAdmin
        {
            get
            {
                return User.IsUserAdmin(this.SecurityLevel);
            }
        }

        private static bool IsUserSeniorOperator(byte level)
        {
            return level >= User.SeniorOperatorLevel;
        }

        private static bool IsUserSupervisor(byte level)
        {
            return level >= User.SupervisorLevel;
        }

        private static bool IsUserManager(byte level)
        {
            return level >= User.ManagerLevel;
        }

        private static bool IsUserAdmin(byte level)
        {
            return level >= User.AdminLevel;
        }

        public static string SecurityName(byte level)
        {
            if (User.IsUserAdmin(level))
            {
                return "Administrator";
            }
            else if (User.IsUserManager(level))
            {
                return "Manager";
            }
            else if (User.IsUserSupervisor(level))
            {
                return "Supervisor";
            }
            else if (User.IsUserSeniorOperator(level))
            {
                return "Senior Operator";
            }
            return "Junior Operator";
        }

        public static byte[] GenerateSHA256(byte[] password)
        {
            if (password == null ||
                password.Length == 0) return null;

            byte[] combined = new byte[password.Length + salt.Length];

            for (int i = 0; i < password.Length; ++i)
            {
                combined[i] = password[i];
            }

            for (int i = 0; i < salt.Length; ++i)
            {
                combined[password.Length + i] = salt[i];
            }

            using (SHA256Managed sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(combined);
            }
        }

        public static byte[] GenerateSHA256(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            byte[] tempPassword = Encoding.UTF8.GetBytes(password);
            return GenerateSHA256(tempPassword);
        }

        public User()
        {

        }

        public User(int id, string description, string fullName,
                    byte[] password, byte securityLevel, Dictionary<int, byte[]> fingerPrints)
            : this(id, description, fullName, password, securityLevel, fingerPrints, false, null)
        {

        }

        public User(int id, string description, string fullName,
                    byte[] password, byte securityLevel, Dictionary<int, byte[]> fingerPrints,
                    bool passwordChanged, string newPassword)
        {
            this.ID = id;
            this.Description = description;
            this.FullName = fullName;
            this.Password = password;
            this._lstFingerPrints = new List<FingerPrint>();
            if (fingerPrints != null)
            {
                foreach (KeyValuePair<int, byte[]> pair in fingerPrints)
                {
                    this._lstFingerPrints.Add(new FingerPrint(pair.Key, pair.Value));
                }
            }
            this.SecurityLevel = securityLevel;
            this.PasswordChanged = passwordChanged;
            this.NewPassword = newPassword;
        }  
    }
}
