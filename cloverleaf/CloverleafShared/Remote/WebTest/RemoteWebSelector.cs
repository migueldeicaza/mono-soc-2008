﻿//
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
using System.Net.Sockets;

using Tamir.SharpSsh;

using ICSharpCode.SharpZipLib.Zip;

namespace CloverleafShared.Remote.WebTest
{
    public partial class RemoteWebSelector : Form
    {
        String projectDirectory;
        CloverleafLocator serviceLocator;
        Dictionary<String, String> serviceIPDict;

        public RemoteWebSelector(String projDir)
        {
            InitializeComponent();
            projectDirectory = projDir;

            txtHostName.Text = CloverleafEnvironment.RemoteServerHost;
            lblLocalPath.Text = projectDirectory;

            zeroconfBackgroundWorker.RunWorkerAsync();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            String app = projectDirectory;
            CloverleafEnvironment.RemoteServerHost = txtHostName.Text;
            this.Hide();

            String host = txtHostName.Text;
            String user = txtUsername.Text;
            String password = txtPassword.Text;
			Int32 port = FirstOpenPort(host, 60000, 65000);

            String zipDirectory = CloverleafEnvironment.CloverleafAppDataPath;
            String zipFileName = "web." + host + "." + user + "." + DateTime.Now.Ticks.ToString() + ".zip";
            String zipPath = Path.Combine(zipDirectory, zipFileName);
            String scriptPath = Path.Combine(zipDirectory, "web." + host + "." + user + "." + DateTime.Now.Ticks.ToString() + ".sh");
            String closeScriptPath = Path.Combine(zipDirectory, "web." + host + "." + user + "." + DateTime.Now.Ticks.ToString() + ".close.sh");
            String pidPath = Path.GetFileNameWithoutExtension(zipFileName) + ".pid";
            String remoteDirectory = Path.GetFileNameWithoutExtension(zipFileName);

            FastZip fz = new FastZip();

            fz.CreateEmptyDirectories = true;
            fz.RestoreAttributesOnExtract = true;
            fz.RestoreDateTimeOnExtract = true;
            fz.CreateZip(zipPath, app,
                    true, null);

            Scp scp = new Scp(host, user, password);
            scp.Connect();
            if (scp.Connected == false)
            {
                throw new SshTransferException("Couldn't connect to host with SCP.");
            }
            scp.Mkdir("/home/" + user + "/.cloverleaf");
            scp.Put(zipPath, "/home/" + user + "/.cloverleaf/" + zipFileName);
            File.Delete(zipPath);

            String ssh1ArgumentData = "";
            String ssh2ArgumentData = "";

            if (optXSP.Checked == true)
            {
                ssh1ArgumentData = "#! /bin/bash" + "\n" +
                     "cd /home/" + user + "/.cloverleaf" + "\n" +
                     "mkdir " + remoteDirectory + "\n" +
                     "cp " + zipFileName + " " + remoteDirectory + "\n" +
                     "cd " + remoteDirectory + "\n" +
                     "unzip " + zipFileName + " > /dev/null \n" +
                     "xsp2 --nonstop --port " + port.ToString() + "& \n" +
                     "pgrep -l " + user + " -n mono > /home/" + user + "/.cloverleaf/" + pidPath;
                ssh2ArgumentData = "#! /bin/bash" + "\n" +
                     "cd /home/" + user + "/.cloverleaf" + "\n" +
                     "kill `cat " + pidPath + "`" + "\n" +
                     "rm -rf " + Path.GetFileNameWithoutExtension(pidPath) + "*";
            }
            File.WriteAllText(scriptPath, ssh1ArgumentData);
            File.WriteAllText(closeScriptPath, ssh2ArgumentData);

            if (scp.Connected == false)
            {
                throw new SshTransferException("Couldn't connect to host with SCP.");
            }
            scp.Put(scriptPath, "/home/" + user + "/.cloverleaf/" + Path.GetFileName(scriptPath));
            scp.Put(closeScriptPath, "/home/" + user + "/.cloverleaf/" + Path.GetFileName(closeScriptPath));

            String stdOut = "";
            String stdErr = "";

            SshExec ssh = new SshExec(host, user, password);
            ssh.Connect();
            ssh.RunCommand("/bin/bash /home/" + user + "/.cloverleaf/" + Path.GetFileName(scriptPath),
                    ref stdOut, ref stdErr);

            (new RemoteWebServerCloser(Path.GetFileName(closeScriptPath),
                host, user, password)).Show();

            ProcessStartInfo wwwProcInfo = new ProcessStartInfo();
            wwwProcInfo.FileName = "http://" + host + ":" + port.ToString();
            wwwProcInfo.UseShellExecute = true;
            Process wwwProc = new Process();
            wwwProc.StartInfo = wwwProcInfo;
            wwwProc.Start();
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
            if (txtHostName.Text.Length > 0
                && txtUsername.Text.Length > 0 && txtPassword.Text.Length > 0)
                cmdOK.Enabled = true;
            else
                cmdOK.Enabled = false;
        }

        private void lblLocalPath_Click(object sender, EventArgs e)
        {

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

        private void lstAvailableHosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serviceIPDict.ContainsKey(lstAvailableHosts.SelectedItem.ToString()) == true)
            {
                txtHostName.Text = serviceIPDict[lstAvailableHosts.SelectedItem.ToString()];
            }
        }

		private Int32 FirstOpenPort(String host, Int32 startPort, Int32 endPort)
		{
			TcpClient tcp = new TcpClient();


			for (Int32 currentPort = startPort; currentPort < endPort; currentPort++)
			{
				try
				{
					tcp.Connect(host, currentPort);
				}
				catch (SocketException e)
				{
					// Should be an unused port, so you're off to the races.
					// We could worry about no-host-found issues, but seeing as how
					// Zeroconf just picked up the host, I don't think that's a huge
					// deal.
					return currentPort;
				}
			}

			throw new Exception("Couldn't find an open port on the host '" + host + "'.");
		}
    }
}
