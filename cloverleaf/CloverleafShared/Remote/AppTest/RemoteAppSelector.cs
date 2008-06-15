//
// MonoRunner.cs: Display form for user to select project to test in Mono
//
// Authors:
//  Ed Ropple <ed@edropple.com>
//
// Copyright (C) 2008 Edward Ropple III (http://www.edropple.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;

using ICSharpCode.SharpZipLib.Zip;

namespace CloverleafShared.Remote.AppTest
{
    public partial class RemoteAppSelector : Form
    {
        String solutionDirectory;
        List<ProjectInfo> projectList;

        public RemoteAppSelector(String slnDirectory, List<ProjectInfo> projList)
        {
            InitializeComponent();

            projectList = projList;
            solutionDirectory = slnDirectory;

            txtHostName.Text = CloverleafEnvironment.RemoteServerHost;

            IPAddress[] ipAddresses = Dns.GetHostAddresses(Environment.MachineName);
            foreach (IPAddress ip in ipAddresses)
            {
                cboLocalIPs.Items.Add(ip.ToString());
            }
            cboLocalIPs.SelectedIndex = 0;
            PopulateList();
        }

        void PopulateList()
        {
            lstLaunchItems.Items.Clear();

            foreach (ProjectInfo p in projectList)
            {
                foreach (String s in p.OutputPaths)
                {
                    String foo = Path.Combine(p.Directory, Path.Combine(s, p.AssemblyName + ".exe"));
                    if (File.Exists(foo))
                    {
                        foo = foo.Remove(0, solutionDirectory.Length + 1);
                        lstLaunchItems.Items.Add(foo);
                    }
                }
            }
        }
        private void cmdOK_Click(object sender, EventArgs e)
        {
            String app = Path.Combine(solutionDirectory, (String) lstLaunchItems.SelectedItem);
            CloverleafEnvironment.RemoteServerHost = txtHostName.Text;
            this.Hide();

            String host = txtHostName.Text;
            String user = txtUsername.Text;
            String password = txtPassword.Text;

            String zipDirectory = CloverleafEnvironment.CloverleafAppDataPath;
            String zipFileName = host + "." + user + "." + DateTime.Now.Ticks.ToString() + ".zip";
            String zipPath = Path.Combine(zipDirectory, zipFileName);
            String scriptPath = Path.Combine(zipDirectory, DateTime.Now.Ticks.ToString() + ".sh");
            String remoteExecutable = Path.GetFileName(app);
            String remoteDirectory = Path.GetFileNameWithoutExtension(zipFileName);

            FastZip fz = new FastZip();

            fz.CreateEmptyDirectories = true;
            fz.RestoreAttributesOnExtract = true;
            fz.RestoreDateTimeOnExtract = true;
            fz.CreateZip(zipPath, Path.GetDirectoryName(app),
                    true, null);

            ProcessStartInfo xInfo = new ProcessStartInfo();
            xInfo.FileName = CloverleafEnvironment.XPath;
            xInfo.Arguments = "-ac -internalwm";
            Process xProcess = new Process();
            xProcess.StartInfo = xInfo;
            xProcess.Start();

            ProcessStartInfo scpInfo = new ProcessStartInfo();
            scpInfo.FileName = CloverleafEnvironment.SCPPath;
            scpInfo.CreateNoWindow = false;
            scpInfo.Arguments = "-batch -pw " + password + " \"" +
                        zipPath + "\" " + user + "@" + host + ":/tmp/" + zipFileName;
            Process scpProcess = new Process();
            scpProcess.StartInfo = scpInfo;
            scpProcess.Start();

            while (scpProcess.HasExited == false)
            {
                Application.DoEvents();
            }
            File.Delete(zipPath);

            String ssh1ArgumentData = "#! /bin/bash" + "\n" +
                "export DISPLAY=" + cboLocalIPs.SelectedItem.ToString() + ":0.0" + "\n" +
                "cd /tmp" + "\n" +
                "mkdir " + remoteDirectory + "\n" +
                "cp " + zipFileName + " " + remoteDirectory + "\n" +
                "cd " + remoteDirectory + "\n" +
                "unzip " + zipFileName + "\n" +
                "clear" + "\n" + 
                "echo Starting Mono application..." + "\n" +
                "mono " + remoteExecutable + "\n" +
                "cd /tmp" + "\n" +
                "rm " + zipFileName + "\n" +
                "rm -rf " + remoteDirectory;
            File.WriteAllText(scriptPath, ssh1ArgumentData);

            scpInfo = new ProcessStartInfo();
            scpInfo.FileName = CloverleafEnvironment.SCPPath;
            scpInfo.CreateNoWindow = false;
            scpInfo.Arguments = "-batch -pw " + password + " \"" +
                        scriptPath + "\" " + user + "@" + host + ":/tmp/" + 
                        Path.GetFileName(scriptPath);
            scpProcess = new Process();
            scpProcess.StartInfo = scpInfo;
            scpProcess.Start();

            ProcessStartInfo sshInfo = new ProcessStartInfo();
            sshInfo.FileName = CloverleafEnvironment.SSHPath;
            sshInfo.UseShellExecute = true;
            sshInfo.CreateNoWindow = false;
            sshInfo.Arguments = "-batch -pw " + password + " " + user + "@" +
                        host + " /bin/bash /tmp/" + Path.GetFileName(scriptPath);
            Process ssh1Process = new Process();
            ssh1Process.StartInfo = sshInfo;

            
            ssh1Process.Start();

            while (ssh1Process.HasExited == false)
            {
                Application.DoEvents();
            }
            Application.Exit();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lstLaunchItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (txtHostName.Text.Length > 0)
                cmdOK.Enabled = true;
            else
                cmdOK.Enabled = false;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void lstAvailableHosts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void formtextboxes_TextChanged(object sender, EventArgs e)
        {
            if (lstLaunchItems.SelectedItem != null && txtHostName.Text.Length > 0
                && txtUsername.Text.Length > 0 && txtPassword.Text.Length > 0)
                cmdOK.Enabled = true;
            else
                cmdOK.Enabled = false;
        }
    }
}
