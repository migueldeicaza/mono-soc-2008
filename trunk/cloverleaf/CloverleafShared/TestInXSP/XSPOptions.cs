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
    /// <summary>
    /// A GUI wrapper atop XSP2.
    /// </summary>
    /// <remarks>
    /// This could possibly be extracted and used as nothing more than a GUI
    /// wrapper for XSP2 (which'd be nice on Windows, as the command line on
    /// Windows is teeth-grindingly annoying to use).
    /// </remarks>
    public partial class XSPOptions : Form
    {
        String projectDirectory;

        public XSPOptions(String projDir)
        {

            InitializeComponent();
            projectDirectory = projDir;

            lblProjectName.Text = projectDirectory.Substring(1 +
                    projectDirectory.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            nudXSPPort.Value = CloverleafEnvironment.DefaultLocalXSPPort;
            chkHTTPS.Checked = CloverleafEnvironment.LocalXSPUsesHTTPS;
            chkBrowserAutostart.Checked = CloverleafEnvironment.XSPBrowserAutostart;
        }

        private void cmdGo_Click(object sender, EventArgs e)
        {
            CloverleafEnvironment.DefaultLocalXSPPort = (Int32) nudXSPPort.Value;
            CloverleafEnvironment.LocalXSPUsesHTTPS = chkHTTPS.Checked;
            CloverleafEnvironment.XSPBrowserAutostart = chkBrowserAutostart.Checked;

            ProcessStartInfo ps = new ProcessStartInfo();
            ps.FileName = Path.Combine(CloverleafEnvironment.MonoBinPath, "xsp2.bat");
            ps.Arguments = "--port " + CloverleafEnvironment.DefaultLocalXSPPort.ToString();
            if (CloverleafEnvironment.LocalXSPUsesHTTPS == true)
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
                ps.FileName = "http://127.0.0.1:" + CloverleafEnvironment.DefaultLocalXSPPort.ToString();
                ps.UseShellExecute = true;
                p = new Process();
                p.StartInfo = ps;
                p.Start();
            }

            Application.Exit();
        }
    }
}
