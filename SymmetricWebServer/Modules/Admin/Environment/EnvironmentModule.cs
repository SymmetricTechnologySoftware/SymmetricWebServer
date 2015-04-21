using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Modules.Admin.Environment
{
    public class EnvironmentModule : BaseWebModule
    {
        public EnvironmentModule()
            : base(AccessLevels.Admin)
        {
            Get["/environment"] = _ =>
            {
                return View["admin/environment", GetItem()];
            };

            Post["/environment"] = _ =>
            {
                return View["admin/environment", PostEnviroment()];
            };
        }


        private EnvironmentItem GetItem()
        {
            EnvironmentItem item = new EnvironmentItem();
            item.Port = Globals.ReadEnvironmentVariable<string>(Globals.Variable_WebServerPort);
            item.StartOnLogin = Globals.ReadEnvironmentVariable<bool>(Globals.Variable_WebServerAutoStart);
            List<string> ids = Globals.ReadEnvironmentVariables(Globals.Variable_NetworkID);
            item.NetworkCards = this.GetNetworkCardItems(ids);
            item.EnableRestGetUsers = Globals.ReadEnvironmentVariable<bool>(Globals.Variable_RestGetUsers);
            return item;
        }

        private List<NetworkCardItem> GetNetworkCardItems(List<string> ids)
        {
            List<NetworkCardItem> result = new List<NetworkCardItem>();
            foreach (KeyValuePair<NetworkInterface, List<IPAddressInformation>> kvp in Globals.GetValidIPAddress())
            {
                bool isActive = ids.Contains(kvp.Key.Id);
                foreach (IPAddressInformation ip in kvp.Value)
                {
                    result.Add(new NetworkCardItem()
                    {
                        Alias = ip.Address.ToString().Replace(".", ""),
                        IsActive = isActive,
                        NetworkCardFullName = kvp.Key.Name + "(" + ip.Address.ToString() + ")"
                    });
                }
            }
            return result;
        }

        private EnvironmentItem PostEnviroment()
        {
            EnvironmentItem item = new EnvironmentItem();
            if (this.Request.Form.Port != null)
            {
                item.Port = this.Request.Form.Port.ToString();
            }

            string errormessage = "";
            int port = 0;
            if (!int.TryParse(item.Port, out port))
            {
                errormessage = "Invalid Port";
            }

            if (this.Request.Form.startonlogin != null)
            {
                item.StartOnLogin = this.Request.Form.startonlogin == "on";
            }

            if (this.Request.Form.enableRestGetUsers != null)
            {
                item.EnableRestGetUsers = this.Request.Form.enableRestGetUsers == "on";
            }

            List<object> ids = new List<object>();
            foreach (KeyValuePair<NetworkInterface, List<IPAddressInformation>> kvp in Globals.GetValidIPAddress())
            {
                foreach (IPAddressInformation ip in kvp.Value)
                {
                    if ((this.Request.Form as Nancy.DynamicDictionary).ContainsKey(ip.Address.ToString().Replace(".", "")))
                    {
                        string value = (this.Request.Form as Nancy.DynamicDictionary)[ip.Address.ToString().Replace(".", "")];
                        if (value == "on")
                        {
                            ids.Add(kvp.Key.Id);
                            break;
                        }
                    }
                }
            }
            item.NetworkCards = this.GetNetworkCardItems(ids.Select(x=>x.ToString()).ToList<string>());

            try
            {
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (item.StartOnLogin)
                {
                    Key.SetValue(Globals.ApplicationTitle, "\"" + System.Reflection.Assembly.GetEntryAssembly().Location + "\"");
                }
                else
                {
                    if (Key.GetValue(Globals.ApplicationTitle) != null)
                    {
                        Key.DeleteValue(Globals.ApplicationTitle);
                    }
                    //  Key.SetValue("AppName", System.Reflection.Assembly.GetEntryAssembly().Location);
                }
                Key.Close();
            }
            catch (Exception ex)
            {
                errormessage = ex.Message;
            }

            if (!String.IsNullOrWhiteSpace(errormessage))
            {
                this.SetErrorMessage(errormessage);
                return item;
            }
            else
            {
                Globals.WriteEnvironmentVariable(Globals.Variable_RestGetUsers, item.EnableRestGetUsers);
                Globals.WriteEnvironmentVariable(Globals.Variable_WebServerPort, port);
                Globals.WriteEnvironmentVariable(Globals.Variable_WebServerAutoStart, item.StartOnLogin);
                Globals.WriteEnvironmentVariable(Globals.Variable_NetworkID, ids);

                this.SetSuccessMessage("Please restart for changes to take place.");
                return this.GetItem();
            }
        }
    }
}
