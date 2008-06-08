using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace CloverleafShared.TestInXSP
{
    public partial class XSPOptions : Form
    {
        String projectDirectory;

        public XSPOptions(String projDir)
        {
            projectDirectory = projDir;

            InitializeComponent();
            lblProjectName.Text = projectDirectory.Substring(1 +
                    projectDirectory.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            nudXSPPort.Value = CloverleafEnvironment.DefaultXSPPort;
            chkHTTPS.Checked = CloverleafEnvironment.XSPUsesHTTPS;
            chkBrowserAutostart.Checked = CloverleafEnvironment.XSPBrowserAutostart;
        }

        private void cmdGo_Click(object sender, EventArgs e)
        {
            CloverleafEnvironment.DefaultXSPPort = (Int32) nudXSPPort.Value;
            CloverleafEnvironment.XSPUsesHTTPS = chkHTTPS.Checked;
            CloverleafEnvironment.XSPBrowserAutostart = chkBrowserAutostart.Checked;

            ProcessStartInfo ps = new ProcessStartInfo();
            ps.FileName = Path.Combine(CloverleafEnvironment.MonoBinPath, "xsp2.bat");
            ps.Arguments = "--port " + CloverleafEnvironment.DefaultXSPPort.ToString();
            if (CloverleafEnvironment.XSPUsesHTTPS == true)
            {
                ps.Arguments += " --https";
            }
            ps.WorkingDirectory = Environment.CurrentDirectory;
            ps.UseShellExecute = true;

            Process p = new Process();
            p.StartInfo = ps;
            p.Start();

            if (CloverleafEnvironment.XSPBrowserAutostart == true)
            {
                ps = new ProcessStartInfo();
                ps.FileName = "http://127.0.0.1:" + CloverleafEnvironment.DefaultXSPPort.ToString();
                ps.UseShellExecute = true;
                p = new Process();
                p.StartInfo = ps;
                p.Start();
            }

            Application.Exit();
        }
    }
}
