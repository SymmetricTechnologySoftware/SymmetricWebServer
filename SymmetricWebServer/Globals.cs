using UserShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Database;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Diagnostics;

namespace WebServer
{
    public static class Globals
    {
        public const string LicenseFolder = "Licenses\\";
        public const string ApplicationTitle = "Symmetric Web Server";
		public const string ContentPath = "Web_Content\\";
        public const string Scripts = Globals.ContentPath + "scripts\\";
        public const string DefaultScripts = Globals.Scripts + "defaultscripts\\";
        public const string DefaultForm = DefaultScripts + "form.html";
        public const string DefaultTemplate = DefaultScripts + "template.html";

        private static string _rootPath;
        public static string RootPath
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_rootPath))
                {
                    return _rootPath;
                }
                _rootPath = AppDomain.CurrentDomain.BaseDirectory + ContentPath;
             //   switch (Environment.OSVersion.Platform)
             //   {
              //      case PlatformID.Unix:
                        _rootPath = _rootPath.Replace('\\', '/');
              //          break;
              //  }
                if (!Directory.Exists(_rootPath))
                {
                    Directory.CreateDirectory(_rootPath);
                }
                return _rootPath;
            }
        }

        private static string _version;
        public static string Version
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_version))
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _version = String.Format("Version {0}\n", fvi.ProductVersion);
                }
                return _version;
            }
        }

        private static string _applicationDirectory;
        public static string ApplicationDirectory
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_applicationDirectory))
                {
                    return _applicationDirectory;
                }

                _applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
                _applicationDirectory = _applicationDirectory.Replace('\\', '/');
                return _applicationDirectory;
            }
        }

        private static string _licenseDirectory;
        public static string LicenseDirectory
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_licenseDirectory))
                {
                    _licenseDirectory = Globals.ApplicationDirectory + Globals.LicenseFolder;
                }
                return _licenseDirectory;
            }
        }

		public static string ApplicationEnvironmentVariables
		{
			get
			{
                return Globals.RootPath + "EnvironmentVariables.xml";
			}
		}

        private static WebServerUserDB _userDB;
        public static WebServerUserDB UserDB
        {
            get
            {
                if (_userDB == null)
                {
                    _userDB = new WebServerUserDB(Globals.RootPath);
                }
                return _userDB;
            }
        }

        public static Dictionary<NetworkInterface, List<IPAddressInformation>> GetValidIPAddress()
        {
            Dictionary<NetworkInterface, List<IPAddressInformation>> result = new Dictionary<NetworkInterface, List<IPAddressInformation>>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            NetworkInterface[] netInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in netInterfaces)
            {
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;

                var ipProps = adapter.GetIPProperties();
                foreach (IPAddressInformation ip in ipProps.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                        IPAddress.IsLoopback(ip.Address))
                    {
                        continue;
                    }

                    if (!System.Net.Dns.GetHostEntry(String.Empty).AddressList.Contains(ip.Address)) continue;

                    if (!result.ContainsKey(adapter))
                    {
                        result.Add(adapter, new List<IPAddressInformation>());
                    }

                    result[adapter].Add(ip);
                }
            }
            return result;
        }

		private static Dictionary<string, object> _lstDefaults;

		public const int VARCHAR_StandardSize = 255;

		//Used to save default data in the applications XML File
		public const string Variable_WebServerAutoStart = "WEBSERVERAUTOSTART";
		public const string Variable_WebServerPort = "WEBSERVERPORT";
        public const string Variable_NetworkID = "NETWORKID";
        public const string Variable_RestGetUsers = "RESTGETUSERS";
	
		public enum SyncSelection { App, Project, User }

		private static void SetupDefaults()
		{
			if (_lstDefaults != null) return;

			_lstDefaults = new Dictionary<string, object>();
			_lstDefaults.Add(Variable_WebServerAutoStart, false);
		}

		public static void WriteEnvironmentVariable(string key, object value)
		{
			XmlWrapper.WriteVariable(Globals.ApplicationEnvironmentVariables, key, value);
		}

		public static void WriteEnvironmentVariable(string key, List<object> values)
		{
			XmlWrapper.WriteVariable(Globals.ApplicationEnvironmentVariables, key, values);
		}

		public static T ReadEnvironmentVariable<T>(string key)
		{
			if(String.IsNullOrWhiteSpace(key))
			{
				return default(T);
			}
			key = key.ToUpper();
			SetupDefaults();

			T defaultValue = default(T);
			if (_lstDefaults.ContainsKey(key))
			{
				try
				{
					defaultValue = (T)Convert.ChangeType(_lstDefaults[key].ToString(), typeof(T));
				}
				catch
				{
					//ignore
				}
			}
			return XmlWrapper.ReadVariable<T>(Globals.ApplicationEnvironmentVariables, key, defaultValue);
		}

		public static List<string> ReadEnvironmentVariables(string key)
		{
			return XmlWrapper.ReadVariable(Globals.ApplicationEnvironmentVariables, key);
		}
    }
}
