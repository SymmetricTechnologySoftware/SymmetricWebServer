using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebServer
{
    public class Program
    {
        public static WebServerHandle Handle { private set; get; }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string message = "";
            if (!VersionVerification.VerifyDotNet(out message, VersionVerification.DotNetV4_5))
            {
                System.Windows.Forms.MessageBox.Show(message, ".NET");
                Application.Exit();
                return;
            }

            try
            {
                Handle = new WebServerHandle();
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                Handle.Run();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
