using Microsoft.Win32;
using System;

namespace WebServer
{

    public static class VersionVerification
    {
        public static Version DotNetV4_5 = new Version("4.5");

        private static Version GetMaxVersion()
        {
            Version maxVersion = new Version();
            // Opens the registry key for the .NET Framework entry. 
            using (RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                // As an alternative, if you know the computers you will query are running .NET Framework 4.5  
                // or later, you can use: 
                // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,  
                // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))

                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (!String.IsNullOrWhiteSpace(install)) //no install info, must be later.
                        {
                            if (name != "" && install == "1")
                            {
                                Version version = null;
                                if (Version.TryParse(name, out version))
                                {
                                    if (version.CompareTo(maxVersion) > 0)
                                    {
                                        maxVersion = new Version(name);
                                    }
                                }
                            }
                        }

                        if (!String.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            install = subKey.GetValue("Install", "").ToString();
                            if (String.IsNullOrWhiteSpace(install) ||
                                install != "1") continue;

                            Version version = null;
                            if (Version.TryParse(name, out version))
                            {
                                if (version.CompareTo(maxVersion) > 0)
                                {
                                    maxVersion = new Version(name);
                                }
                            }
                        }
                    }
                }
            }
            return maxVersion;
        }

        public static bool VerifyDotNet(out string message, Version assemblyDotNETVersion)
        {
            if (assemblyDotNETVersion == null)
            {
                message = "Invalid executing assembly.";
                return false;
            }

            message = "";
            try
            {
                Version maxVersion = GetMaxVersion();
                int compare = assemblyDotNETVersion.CompareTo(maxVersion);
                if (compare > 0)
                {
                    message = String.Format("You have an outdated version of the .NET framework (v{0}).\nPlease update to version {1}.",
                                            maxVersion, assemblyDotNETVersion);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
    }
}