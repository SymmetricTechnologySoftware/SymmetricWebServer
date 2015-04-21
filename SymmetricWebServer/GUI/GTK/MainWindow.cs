using System;
using Gtk;
using System.Diagnostics;

namespace WebServer.GUI.GTK
{
	public class MainWindow : TemplateWindow
	{
        private Label lblStatus;
		private MetroButton btnStart;
		private MetroButton btnStop;
		private MetroButton btnConfig;
		private DrawingArea line;
        private StatusIcon _trayIcon;
        public event EventHandler StartClick;
        public event EventHandler StopClick;

		public MainWindow () :
			base("web server")
		{
            this.WidthRequest = 550;
            this.HeightRequest = 350;
            // Creation of the Icon
            _trayIcon = new StatusIcon(global::Gdk.Pixbuf.LoadFromResource("CrossPlatformWebServer.Resources.app.png"));
            _trayIcon.Visible = true;
            // Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
            _trayIcon.Activate += delegate 
            { 
                this.Visible = !this.Visible; 
            };
            // Show a pop up menu when the icon has been right clicked.
            _trayIcon.PopupMenu += OnTrayIconPopup;

            // A Tooltip for the Icon
            _trayIcon.Tooltip = "Cross Platform Web Server";

            this.btnStart = new global::WebServer.GUI.GTK.MetroButton(global::Gdk.Pixbuf.LoadFromResource("CrossPlatformWebServer.Resources.Play.png"),
				global::Gdk.Pixbuf.LoadFromResource ("CrossPlatformWebServer.Resources.Play2.png"));
			this.btnStart.TooltipText = "Start Server";
            this.btnStart.Clicked += btnStart_Clicked;
            this.GridMain.Add (this.btnStart);
			global::Gtk.Fixed.FixedChild w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain [btnStart]));
			w2.X = 215;
			w2.Y = 145;

            this.btnStop = new global::WebServer.GUI.GTK.MetroButton(global::Gdk.Pixbuf.LoadFromResource("CrossPlatformWebServer.Resources.Stop.png"),
				global::Gdk.Pixbuf.LoadFromResource ("CrossPlatformWebServer.Resources.Stop2.png"));
			this.btnStop.TooltipText = "Stop Server";
            this.btnStop.Clicked += btnStop_Clicked;
			this.GridMain.Add (this.btnStop);
			w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain [btnStop]));
			w2.X = 275;
			w2.Y = 145;

            this.btnConfig = new global::WebServer.GUI.GTK.MetroButton(global::Gdk.Pixbuf.LoadFromResource("CrossPlatformWebServer.Resources.Config.png"),
				global::Gdk.Pixbuf.LoadFromResource ("CrossPlatformWebServer.Resources.Config2.png"));
			this.btnConfig.TooltipText = "Configuration";
            this.btnConfig.Clicked += btnConfig_Clicked;
			this.GridMain.Add (this.btnConfig);
			w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain [btnConfig]));
			w2.X = 20;
			w2.Y = 50;

			this.line = new global::Gtk.DrawingArea ();
			this.line.Name = "line";
			this.GridMain.Add (this.line);
			this.line.WidthRequest = 1;
			this.line.HeightRequest = 280;
			this.line.ModifyBg (StateType.Normal, new Gdk.Color (128, 128, 128)); 
			w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain [this.line]));
			w2.X = 55;
			w2.Y = 50;

            this.lblStatus = new Label();
            this.lblStatus.Wrap = true;
            this.lblStatus.Justify = Justification.Left;
            this.lblStatus.WidthRequest = 470;
            this.lblStatus.HeightRequest = 50;
            this.GridMain.Add(this.lblStatus);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.lblStatus]));
            w2.X = 69;
            w2.Y = 270;

            this.ServerStatus = WebServerHandle.ServerStatuses.Stopped;

			this.ShowObjects ();
		}

        public string Status
        {
            set
            {
                this.lblStatus.LabelProp = value;
            }
            get
            {
                return this.lblStatus.LabelProp;
            }
        }

        private WebServerHandle.ServerStatuses _serverStatus;
        public WebServerHandle.ServerStatuses ServerStatus
        {
            set
            {

                _serverStatus = value;
                switch(value)
                {
                    case WebServerHandle.ServerStatuses.Started:
                        btnStart.Sensitive = false;
                        btnStop.Sensitive = true;
                        btnConfig.Sensitive = false;
                        break;
                    case WebServerHandle.ServerStatuses.Stopping:
                    case WebServerHandle.ServerStatuses.Starting:
                        btnStart.Sensitive = false;
                        btnStop.Sensitive = false;
                        btnConfig.Sensitive = false;
                        break;
                    case WebServerHandle.ServerStatuses.Stopped:
                        btnStart.Sensitive = true;
                        btnStop.Sensitive = false;
                        btnConfig.Sensitive = true;
                        break;
                }
            }
            get
            {
                return _serverStatus;
            }
        }

        private void btnStop_Clicked(object sender, EventArgs e)
        {
            if (this.StopClick != null)
            {
                this.StopClick(sender, e);
            }
        }

        private void btnStart_Clicked(object sender, EventArgs e)
        {
            if (this.StartClick != null)
            {
                this.StartClick(sender, e);
            }
        }

        private void btnConfig_Clicked(object sender, EventArgs e)
        {
            ConfigWindow win = new ConfigWindow(this);
            win.Show();
            Application.Run();
            win.Hide();
            win = null;
        }

        private void OnTrayIconPopup(object o, EventArgs args)
        {
            Menu popupMenu = new Menu();
            ImageMenuItem menuItemQuit = new ImageMenuItem("Quit");
            Gtk.Image appimg = new Gtk.Image(Stock.Quit, IconSize.Menu);
            menuItemQuit.Image = appimg;
            popupMenu.Add(menuItemQuit);
            // Quit the application when quit has been clicked.
            menuItemQuit.Activated += menuItemQuit_Activated;
            popupMenu.ShowAll();
            popupMenu.Popup();
        }

        protected override void CloseApp()
        {
            if (this.ServerStatus == WebServerHandle.ServerStatuses.Started ||
                this.ServerStatus == WebServerHandle.ServerStatuses.Starting)
            {
                if (ModernDialog.ShowMessage("Are you sure you want to shutdown the web server?",
                                                "Shutdown", ModernDialog.MessageBoxButton.YesNo, this) == ResponseType.Yes)
                {
                    base.CloseApp();
                }
            }
            else
            {
                base.CloseApp();
            }
        }

        private void menuItemQuit_Activated(object sender, EventArgs e)
        {
            this.CloseApp();
        }
	}
}

