using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebServer.GUI
{
    public partial class FrmAbout : Form
    {
        private bool _canClose;
        public FrmAbout()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.app;
            _canClose = false;

            lblAppName.Text = Globals.ApplicationTitle;
            lblVersion.Text = Globals.Version;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        public void CloseForm()
        {
            _canClose = true;
            this.Close();
        }

        private void FrmAbout_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_canClose)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void llLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(Globals.LicenseDirectory);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
