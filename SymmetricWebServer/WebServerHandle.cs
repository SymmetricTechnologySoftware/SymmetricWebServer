using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using WebServer.Database;
using System.Windows.Forms;
using WebServer.GUI;
using System.Collections.ObjectModel;
using Microsoft.Win32;
//using NetFwTypeLib; // Located in FirewallAPI.dll

namespace WebServer
{
    public class WebServerHandle
    {
        public const string DefaultAddress = "127.0.0.1";
        public const int DefaultPort = 12860;

        private List<Uri> _lstUri;
        private DebugLogger _debugLogger;
        private NancyHost _host;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _ct;
        private NotifyIcon _trayIcon;
        private ContextMenu _trayMenu;
        private FrmAbout _frmAbout;
       // private Form _frmMain;
        private int _iPort = WebServerHandle.DefaultPort;

        public bool EnableGetUsers { private set; get; }

        public ReadOnlyCollection<Uri> URIs
        {
            get
            {
                return _lstUri.AsReadOnly();
            }
        }

        public WebServerHandle()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);

            _frmAbout = new FrmAbout();
            _frmAbout.Visible = false;
            _frmAbout.WindowState = FormWindowState.Minimized;

            _lstUri = new List<Uri>();
            _debugLogger = new DebugLogger();
          //  _frmMain = new Form() { WindowState = FormWindowState.Minimized, Visible = false, ShowInTaskbar = false };
            // Create a simple tray menu with only one item.
            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add("Open Server", OnOpenServer);
            _trayMenu.MenuItems.Add("Restart", OnRestart);
            _trayMenu.MenuItems.Add("About", OnAbout);
            _trayMenu.MenuItems.Add("Shutdown Server", OnExit);
            _trayIcon = new NotifyIcon();
            _trayIcon.MouseDoubleClick += TrayMouseDoubleClick;
            _trayIcon.Text = Globals.ApplicationTitle;
            _trayIcon.Icon = Properties.Resources.app;
            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;

             this.StartHost();


         /*   INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Description = "Allow " + Globals.ApplicationName;
            firewallRule.ApplicationName = Globals.ApplicationFullPath;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = Globals.ApplicationName;

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(firewallRule);*/
        }

        private void TrayMouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.OpenServer();
        }

        private void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            _debugLogger.WriteLine(e.Message);
        }

        private void OnOpenServer(object sender, EventArgs e)
        {
            this.OpenServer();
        }

        private void OpenServer()
        {
            Process.Start(String.Format("http://{0}:{1}", WebServerHandle.DefaultAddress, _iPort));
        }

        private void OnRestart(object sender, EventArgs e)
        {
            this.StopHost();
            this.StartHost();
        }

        private void OnAbout(object sender, EventArgs e)
        {
            _frmAbout.WindowState = FormWindowState.Normal;
            _frmAbout.BringToFront();
            _frmAbout.Show();
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayIcon.ShowBalloonTip(2000, Globals.ApplicationTitle, "Server is shutting down...", ToolTipIcon.Info);
            this.Exit();
        }

        private async void Exit()
        {
             await Task.Delay(2000);
             _frmAbout.CloseForm();
             Application.Exit();
        }

        public void Run()
        {
            try
            {
                Application.Run();
            }
            finally
            {
                Application.Exit();
                _trayIcon.Visible = false;
                this.StopHost();
                _debugLogger.CloseFile();
            }
        }

        private void StopHost()
        {
            try
            {
                if (_host != null)
                {
                    _host.Stop();
                }
            }
            catch
            {
                //ignore
            }
        }

        private delegate void OutAction<T1>(ref T1 a);

        private async void StartHost()
        {
            try
            {
                _trayIcon.ShowBalloonTip(2000, Globals.ApplicationTitle, "Server is starting...", ToolTipIcon.Info);
                _debugLogger.WriteLine("Starting Server..");
                OutAction<int> action = RunAsyncHostStart;
                int result = 0;
                Task t1 = new Task(() => action(ref result), _ct);
                t1.Start();
                _tokenSource = new CancellationTokenSource();
                _ct = _tokenSource.Token;
                await t1;

                switch (result)
                {
                    case 0:
                        _trayIcon.ShowBalloonTip(2000, Globals.ApplicationTitle, "Server is running", ToolTipIcon.Info);
                        break;
                    case 1:
                    case 2:
                        _trayIcon.ShowBalloonTip(2000, Globals.ApplicationTitle, "Server started with errors", ToolTipIcon.Warning);
                        break;
                    default:
                        _trayIcon.ShowBalloonTip(2000, Globals.ApplicationTitle, "Server failed to start", ToolTipIcon.Error);
                        break;

                }

                _tokenSource = null;
            }
            catch (Exception ex)
            {
                _debugLogger.WriteLine("Starting Server error: " + ex.Message);
            }
        }

        /// <summary>
        /// Start the nancy server.
        /// </summary>
        /// <param name="result">
        /// 0 = Success
        /// 1 = Failed to start first time
        /// 2 = Faield to start 2nd time with only local url
        /// 3 = Other
        /// </param>
        private void RunAsyncHostStart(ref int result)
        {
            bool bInvalidOperationException = false;
            try
            {
                this.StopHost();
                _lstUri.Clear();

                this.EnableGetUsers = Globals.ReadEnvironmentVariable<bool>(Globals.Variable_RestGetUsers);
                bool portSet = true;
                {
                    string port = Globals.ReadEnvironmentVariable<string>(Globals.Variable_WebServerPort);

                    if (String.IsNullOrWhiteSpace(port))
                    {
                        portSet = false;
                    }

                    if (String.IsNullOrWhiteSpace(port) ||
                       !int.TryParse(port, out _iPort))
                    {
                        _iPort = WebServerHandle.DefaultPort;
                    }
                }

                int i = 0;
                while (PortInUse(_iPort))
                {
                    portSet = false;
                    if (++i == 20)
                    {
                        _debugLogger.WriteLine("Failed to assign port: " + _iPort);
                        result = 3;
                        return;
                    }
                    ++_iPort;
                }

                if (!portSet)
                {
                    Globals.WriteEnvironmentVariable(Globals.Variable_WebServerPort, _iPort);
                }

                List<string> ids;
                if (result == 2)
                {
                    ids = new List<string>();
                }
                else
                {
                    ids = Globals.ReadEnvironmentVariables(Globals.Variable_NetworkID);
                }

                if (ids != null &&
                    ids.Count > 0)
                {
                    foreach (KeyValuePair<NetworkInterface, List<IPAddressInformation>> kvp in Globals.GetValidIPAddress())
                    {
                        bool isActive = ids.Contains(kvp.Key.Id);
                        if (!isActive) continue;
                        foreach (IPAddressInformation ip in kvp.Value)
                        {
                            _lstUri.Add(new Uri(String.Format("http://{0}:{1}", ip.Address.ToString(), _iPort)));
                        }
                    }
                }

                _lstUri.Add(new Uri(String.Format("http://{0}:{1}", WebServerHandle.DefaultAddress, _iPort)));

               // lstUri.Add(new Uri(String.Format("http://{0}:{1}", "Braden-Desktop", _iPort)));
                /*ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh");
                procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                foreach (Uri uri in lstUri)
                {
                     procStartInfo.Arguments = String.Format("http delete urlacl url={0}", uri.ToString());
                     Process.Start(procStartInfo).WaitForExit();

                      procStartInfo.Arguments = String.Format("http add urlacl url=h{0} user=Everyone", uri.ToString());
                      Process.Start(procStartInfo).WaitForExit();
                }*/

                HostConfiguration hostConfig = new HostConfiguration()
                {
                    AllowChunkedEncoding = false,
                    UrlReservations = new UrlReservations()
                    {
                        CreateAutomatically = true,
                        // User = "Everyone"
                    }
                };
                Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;
                _host = new NancyHost(hostConfig, _lstUri.ToArray());
                _host.Start();

                _debugLogger.WriteLine("Server has started.");
                return;
            }
            catch (InvalidOperationException ex)
            {
                bInvalidOperationException = true;
                _debugLogger.WriteLine("Failed to start server: " + ex.Message);
            }
            catch (Exception ex)
            {
                _debugLogger.WriteLine("Failed to start server: " + ex.Message);
            }

            result++;

            if (result == 1)
            {
                if(!bInvalidOperationException)
                {
                    ++result;
                }
                this.RunAsyncHostStart(ref result);
            }
            else if (result == 2)
            {
                 this.RunAsyncHostStart(ref result);
            }
        }

        private bool PortInUse(int port)
        {
            TcpClient tcpClient = null;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(WebServerHandle.DefaultAddress, port);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }
            }
        }

    }
}
