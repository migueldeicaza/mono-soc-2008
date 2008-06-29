using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Tamir.SharpSsh;

namespace CloverleafShared.Remote.WebTest
{
    public partial class RemoteWebServerCloser : Form
    {
        String closeScriptPath;
        String host;
        String user;
        String password;
        public RemoteWebServerCloser(String scriptPath, String sshHost, String sshUser, String sshPass)
        {
            InitializeComponent();

            closeScriptPath = scriptPath;
            host = sshHost;
            user = sshUser;
            password = sshPass;
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            SshExec ssh = new SshExec(host, user, password);
            ssh.Connect();
            ssh.RunCommand("/bin/bash /home/" + user + "/.cloverleaf/" + Path.GetFileName(closeScriptPath));

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
