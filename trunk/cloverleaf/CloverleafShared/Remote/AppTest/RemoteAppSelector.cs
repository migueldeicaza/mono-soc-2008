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
using System.Threading;
using System.Windows.Forms;
using System.Net;

using Tamir.SharpSsh;
using ICSharpCode.SharpZipLib.Zip;

namespace CloverleafShared.Remote.AppTest
{
    public partial class RemoteAppSelector : Form
    {
        String solutionDirectory;
        List<ProjectInfo> projectList;
        CloverleafLocator serviceLocator;
        Dictionary<String, String> serviceIPDict;

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

            zeroconfBackgroundWorker.RunWorkerAsync();
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

            Scp scp = new Scp(host, user, password);
            scp.Connect();
            if (scp.Connected == false)
            {
                throw new SshTransferException("Couldn't connect to host with SCP.");
            }
            scp.Mkdir("/home/" + user + "/.cloverleaf");
            scp.Put(zipPath, "/home/" + user + "/.cloverleaf/" + zipFileName);
            File.Delete(zipPath);
            
            String ssh1ArgumentData = "#! /bin/bash" + "\n" +
                "export DISPLAY=" + cboLocalIPs.SelectedItem.ToString() + ":0.0" + "\n" +
                "cd /home/" + user + "/.cloverleaf" + "\n" +
                "mkdir " + remoteDirectory + "\n" +
                "cp " + zipFileName + " " + remoteDirectory + "\n" +
                "cd " + remoteDirectory + "\n" +
                "unzip " + zipFileName + " > /dev/null \n" +
                "mono " + remoteExecutable + "\n" +
                "cd /home/" + user + "/.cloverleaf" + "\n" +
                "rm " + zipFileName + "\n" +
                "rm -rf " + remoteDirectory + "\n" +
                "rm /home/" + user + "/.cloverleaf/" + Path.GetFileName(scriptPath);
            File.WriteAllText(scriptPath, ssh1ArgumentData);

            if (scp.Connected == false)
            {
                throw new SshTransferException("Couldn't connect to host with SCP.");
            }
            scp.Put(scriptPath, "/home/" + user + "/.cloverleaf/" + Path.GetFileName(scriptPath));

            String stdOut = "";
            String stdErr = "";

            SshExec ssh = new SshExec(host, user, password);
            ssh.Connect();
            ssh.RunCommand("/bin/bash /home/" + user + "/.cloverleaf/" + Path.GetFileName(scriptPath),
                    ref stdOut, ref stdErr);

            (new RemoteStdOutDisplay(stdOut, stdErr)).Show();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            // because for some reason the application remains
            // running in the background and shouldn't... so I *KILL IT WITH FIRE!*
            Process.GetCurrentProcess().Kill();
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

        private void formtextboxes_TextChanged(object sender, EventArgs e)
        {
            if (lstLaunchItems.SelectedItem != null && txtHostName.Text.Length > 0
                && txtUsername.Text.Length > 0 && txtPassword.Text.Length > 0)
                cmdOK.Enabled = true;
            else
                cmdOK.Enabled = false;
        }

        private void zeroconfBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            serviceLocator = new CloverleafLocator();

            serviceLocator.Go();
            System.Threading.Thread.Sleep(500);
            serviceIPDict = serviceLocator.ServiceIPDictionary;
        }

        private void zeroconfBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblHostBoxFluff.Text = "Available Hosts:";

            lstAvailableHosts.Items.Clear();

            for (Dictionary<String, String>.Enumerator en =
                    serviceIPDict.GetEnumerator(); en.MoveNext() == true; )
            {
                lstAvailableHosts.Items.Add(en.Current.Key);
            }

            lstAvailableHosts.Enabled = true;
            cmdRecheck.Enabled = true;
        }

        private void cmdRecheck_Click(object sender, EventArgs e)
        {
            lblHostBoxFluff.Text = "Searching for Services...";
            lstAvailableHosts.Items.Clear();
            lstAvailableHosts.Enabled = false;
            cmdRecheck.Enabled = false;
            zeroconfBackgroundWorker.RunWorkerAsync();
        }

        private void lstAvailableHosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serviceIPDict.ContainsKey(lstAvailableHosts.SelectedItem.ToString()) == true)
            {
                txtHostName.Text = serviceIPDict[lstAvailableHosts.SelectedItem.ToString()];
            }

            Int32 octetCount = -1;
            foreach (Object o in cboLocalIPs.Items)
            {
                String foo = o.ToString();

                Int32 bar = CloverleafLocator.NumberOfSimilarOctets(txtHostName.Text, foo);

                if (bar > octetCount)
                {
                    cboLocalIPs.SelectedItem = o;
                    octetCount = bar;
                }
            }
        }
    }
}
