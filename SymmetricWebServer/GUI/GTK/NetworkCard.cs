using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.GUI.GTK
{
    public class NetworkCard
    {
        public CheckButton CheckBox { set; get; }
        public string ID { set; get; }
        public string NetworkName { set; get; }
        public string IPAddress { set; get; }
    }
}
