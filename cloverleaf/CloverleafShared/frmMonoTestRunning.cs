using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CloverleafShared
{
    public partial class frmMonoTestRunning : Form
    {
        // stderr redirection causes crashes. wtf?
        StreamReader stdErrReader;

        ProcessStartInfo ps;
        Process p = null;

        public frmMonoTestRunning(String appToRun)
        {
            InitializeComponent();

            ps = new ProcessStartInfo();
            ps.FileName = Path.Combine(
                Path.Combine(Environment.GetEnvironmentVariable("MonoDir", EnvironmentVariableTarget.User),
                                "bin"), "mono.exe");
            ps.Arguments = "\"" + appToRun + "\"";

            timProcessMonitor.Enabled = true;
        }

        private void timProcessMonitor_Tick(object sender, EventArgs e)
        {
            if (p == null) // process hasn't started yet
            {
                p = Process.Start(ps);
//              stdErrReader = p.StandardError;

                lblProcessStatus.Text = "Running...";
                lblProcessStatus.ForeColor = Color.Green;
                cmdKill.Enabled = true;
            }
            else
            {
//              txtStdErr.Text += stdErrReader.ReadToEnd();
                if (p.HasExited == true)
                {
                    timProcessMonitor.Enabled = false;
                    cmdClose.Enabled = true;
                    cmdKill.Enabled = false;
                    lblProcessStatus.Text = "Terminated";
                    lblProcessStatus.ForeColor = Color.Red;
                }
            }
        }

        private void cmdKill_Click(object sender, EventArgs e)
        {
            if (p != null) p.Kill();
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
