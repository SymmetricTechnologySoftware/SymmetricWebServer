using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace UserShared
{
    public class FingerPrintHandle
    {
        private Assembly _DLLHandle;
        private Type _DLLType;
        private MethodInfo _miInitBase;
        private MethodInfo _miReadPacket;
        private MethodInfo _miDeInitBase;
        private object _instance;

        public bool DLLoaded { private set; get; }
        
        /// <summary>
        /// Checks if a class derives from type FingerPrintTemplateDLL
        /// </summary>
        /// <param name="toCheck">class to check.</param>
        /// <returns>True if the class does derive.</returns>
        private static bool IsSubclassOfRawGeneric(Type toCheck)
        {
            while (toCheck != null)
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (typeof(FingerPrintTemplateDLL) == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Loads the fingerprint DLL.
        /// </summary>
        /// <param name="initType">The type to initialise the driver with.</param>
        /// <returns>A message if the DLL failed to load else returns an empty string.</returns>
        public string LoadDLL(FingerPrintTemplateDLL.InitTypes initType)
        {
            return LoadDLL(initType, null);
        }

        /// <summary>
        /// Loads the fingerprint DLL.
        /// </summary>
        /// <param name="initType">The type to initialise the driver with.</param>
        /// <param name="users">The users that you want to load into the driver.</param>
        /// <returns>A message if the DLL failed to load else returns an empty string.</returns>
        public string LoadDLL(FingerPrintTemplateDLL.InitTypes initType, List<User> users)
        {
            string message = "";
            try
            {
                if (this.DLLoaded)
                {
                    message = this.Init(initType, users);
                    if (!String.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }
                    return "";
                }
                this.DLLoaded = false;
                if (!File.Exists("FingerPrint.dll"))
                {
                    return "Cannot find FingerPrint.dll.";
                }
                _DLLHandle = Assembly.LoadFrom("FingerPrint.dll");

                if (_DLLHandle == null) return "Cannot find FingerPrint.dll.";

                Type[] DLLTypes = _DLLHandle.GetTypes();
                _DLLType = null;
                foreach (Type t in DLLTypes)
                {
                    if (!IsSubclassOfRawGeneric(t)) continue;
                    _miInitBase = t.GetMethod("InitBase", new Type[] { typeof(FingerPrintTemplateDLL.InitTypes), typeof(Dictionary<int, Dictionary<int, byte[]>>), typeof(string).MakeByRefType() });//return bool
                    _miReadPacket = t.GetMethod("ReadPacket");//return FingerPrintPacket
                    _miDeInitBase = t.GetMethod("DeInitBase");
                    if (_miInitBase == null ||
                        _miReadPacket == null ||
                        _miDeInitBase == null) continue;
                    _DLLType = t;
                }

                if (_DLLType == null)
                {
                    return "Cannot find the entry point for the FingerPrint.dll.";
                }
                this.DLLoaded = true;
                message = this.Init(initType, users);
                if (!String.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "FingerPrint.LoadDLL: " + ex.Message + ".";
            }
        }

        /// <summary>
        /// Initializes the fingerprint DLL.
        /// </summary>
        /// <returns>A message if the DLL failed to initialize else returns an empty string.</returns>
        private string Init(FingerPrintTemplateDLL.InitTypes initType, List<User> users)
        {
            string message = "";
            object[] args = null;
            try
            {
                _instance = Activator.CreateInstance(_DLLType);
                Dictionary<int, Dictionary<int, byte[]>> fingerPrints = new Dictionary<int, Dictionary<int, byte[]>>();
                if (users != null)
                {
                    foreach (User usr in users)
                    {
                        fingerPrints.Add(usr.ID, new Dictionary<int, byte[]>());
                        foreach (FingerPrint fp in usr.FingerPrints)
                        {
                            fingerPrints[usr.ID].Add(fp.PrintNumber, fp.Print);
                        }
                    }
                }

                args = new object[] { initType, fingerPrints, "" };
                bool result = (bool)_miInitBase.Invoke(_instance, args);
                if (!result)
                {
                    message = "Failed to initialize \"FingerPrint.dll\".";
                    if (args != null && args.Length > 2)
                    {
                        message += " " + args[2].ToString();
                    }
                    return message;
                }
                return "";
            }
            catch (Exception ex)
            {
                message = "Failed To Load \"Includes\\FingerPrint.dll\". " + ex.Message;
            }
            finally
            {
                args = null;
            }
            return message;
        }

        /// <summary>
        /// Unloads the DLL if it is loaded.
        /// </summary>
        public void UnLoadDLL()
        {
            if (!this.DLLoaded) return;

            try
            {
                _miDeInitBase.Invoke(_instance, null);
            }
            catch
            {
                //ignore
            }
            finally
            {
                _instance = null;
                this.DLLoaded = false;
            }
        }

        public FingerPrintPacket ReadPacket(out string message)
        {
            message = "";
            if (!this.DLLoaded) return null;
            try
            {
                object pckt = _miReadPacket.Invoke(_instance, null);
                if ((pckt != null) && (pckt is FingerPrintPacket))
                {
                    return pckt as FingerPrintPacket;
                }
                return null;
            }
            catch (Exception ex)
            {
                message = "GrabFingerPrint: " + ex.Message;
                return null;
            }
        }
    }
}
