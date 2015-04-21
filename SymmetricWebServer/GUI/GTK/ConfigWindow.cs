using Gtk;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WebServer.GUI.GTK
{
	public class ConfigWindow : TemplateWindow
	{
        private Entry txtPort;
        private Label lblPort;
        private CheckButton cbStartup;
        private CheckButton cbMinimize;
        private ScrolledWindow swNetworkCards;
        private Table tNetworkCards;
        private List<NetworkCard> _lstNetworkCards;
        private Button btnApply;
        private Button btnCancel;

		public ConfigWindow (Window owner) :
			base ("config")
		{
            this.WidthRequest = 500;
            this.HeightRequest = 300;
            this.ShowMinimize = false;
            this.Modal = true;
            this.WindowPosition = Gtk.WindowPosition.None;
            int root_x, root_y;
            owner.GetPosition(out root_x, out root_y);
            this.Move(root_x + (owner.WidthRequest / 2) - (this.WidthRequest / 2),
                      root_y + (owner.HeightRequest / 2) - (this.HeightRequest / 2));

            this.lblPort = new global::Gtk.Label();
            this.lblPort.LabelProp = "Server Port:";
            this.GridMain.Add(this.lblPort);
            global::Gtk.Fixed.FixedChild w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.lblPort]));
            w2.X = 40;
            w2.Y = 62;

            this.txtPort = new global::Gtk.Entry();
            this.txtPort.WidthRequest = 100;
            this.GridMain.Add(this.txtPort);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.txtPort]));
            w2.X = 110;
            w2.Y = 60;

            this.cbStartup = new CheckButton();
            this.cbStartup.Label = "Start the web server when the PC starts up";
            this.GridMain.Add(this.cbStartup);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.cbStartup]));
            w2.X = 40;
            w2.Y = 85;

            this.cbMinimize = new CheckButton();
            this.cbMinimize.Label = "Minimize on startup";
            this.GridMain.Add(this.cbMinimize);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.cbMinimize]));
            w2.X = 40;
            w2.Y = 105;

            this.swNetworkCards = new ScrolledWindow();
            this.swNetworkCards.WidthRequest = 400;
            this.swNetworkCards.HeightRequest = 100;
            this.swNetworkCards.ShadowType = ((global::Gtk.ShadowType)(1));
            
            this.GridMain.Add(this.swNetworkCards);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.swNetworkCards]));
            w2.X = 40;
            w2.Y = 130;

            this.tNetworkCards = new Table(6, 2, false);
            this.tNetworkCards.RowSpacing = (6);
            this.tNetworkCards.ColumnSpacing = (6);

            global::Gtk.Viewport w1 = new global::Gtk.Viewport();
            w1.ShadowType = ((global::Gtk.ShadowType)(0));
            w1.Add(tNetworkCards);

            this.swNetworkCards.Add(w1);

            _lstNetworkCards = new List<NetworkCard>();
       
            List<string> ids = Globals.ReadEnvironmentVariables(Globals.V_NetworkID);
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            NetworkInterface[] netInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            uint i = 0;
            foreach (NetworkInterface adapter in netInterfaces)
            {
                var ipProps = adapter.GetIPProperties();
                foreach (var ip in ipProps.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        CheckButton cbNetworkCard = new CheckButton();
                        cbNetworkCard.Active = ids.Contains(adapter.Id);
                        cbNetworkCard.Label = String.Format("{0} - {1}", adapter.Name, ip.Address.ToString());
                        this.tNetworkCards.Add(cbNetworkCard);
                        tNetworkCards.Add(cbNetworkCard);
                        global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.tNetworkCards[cbNetworkCard]));
                        w3.TopAttach = i++;
                        w3.YOptions = ((global::Gtk.AttachOptions)(4));

                        NetworkCard networkCard = new NetworkCard();
                        networkCard.ID = adapter.Id;
                        networkCard.NetworkName = adapter.Name;
                        networkCard.IPAddress = ip.Address.ToString();
                        networkCard.CheckBox = cbNetworkCard;
                        _lstNetworkCards.Add(networkCard);
                    }
                }
            }

            this.btnApply = new Button();
            this.btnApply.Clicked += btnApply_Clicked;
            this.btnApply.Label = "Apply";
            this.btnApply.WidthRequest = 100;
            this.GridMain.Add(this.btnApply);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.btnApply]));
            w2.X = 150;
            w2.Y = 240;

            this.btnCancel = new Button();
            this.btnCancel.Clicked += btnCancel_Clicked;
            this.btnCancel.Label = "Cancel";
            this.btnCancel.WidthRequest = 100;
            this.GridMain.Add(this.btnCancel);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.btnCancel]));
            w2.X = 250;
            w2.Y = 240;

            //Apply settings
            txtPort.Text = Globals.ReadEnvironmentVariable<int>(Globals.V_WebServerPort).ToString();
            cbStartup.Active = Globals.ReadEnvironmentVariable<bool>(Globals.V_WebServerAutoStart);
            cbMinimize.Active = Globals.ReadEnvironmentVariable<bool>(Globals.V_WebServerHide);

            this.ShowObjects();
		}

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            this.CloseApp();
        }

        private void btnApply_Clicked(object sender, EventArgs e)
        {
            int i = 0;
            if (String.IsNullOrWhiteSpace(txtPort.Text) ||
                !int.TryParse(txtPort.Text.Trim(), out i))
            {
                ModernDialog.ShowMessage("Invalid server port.",
                                          "Input", ModernDialog.MessageBoxButton.OK, this);
                return;
            }
            Globals.WriteEnvironmentVariable(Globals.V_WebServerPort, txtPort.Text.Trim());
            Globals.WriteEnvironmentVariable(Globals.V_WebServerAutoStart, cbStartup.Active);
            Globals.WriteEnvironmentVariable(Globals.V_WebServerHide, cbMinimize.Active);

            List<object> ids = new List<object>();
            foreach (NetworkCard item in _lstNetworkCards)
            {
                if (item.CheckBox.Active)
                {
                    ids.Add(item.ID);
                }
            }
            Globals.WriteEnvironmentVariable(Globals.V_NetworkID, ids);

            this.CloseApp();
        }

	}
}

