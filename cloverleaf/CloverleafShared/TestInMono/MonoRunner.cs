//
// MonoRunner.cs: Display form while Cloverleaf runs an app in Mono
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CloverleafShared.TestInMono
{
    public partial class MonoRunner : Form
    {

        StreamReader stdErrReader;
        Process monoProc = null;
        
        public MonoRunner(String appToRun)
        {
            InitializeComponent();

            monoProc = new Process();
            monoProc.StartInfo.FileName = Path.Combine(Path.Combine(CloverleafEnvironment.MonoRootPath,
                                "bin"), "mono.exe");
            monoProc.StartInfo.Arguments = "\"" + appToRun + "\"";
            monoProc.StartInfo.UseShellExecute = true;

            // for some reason, redirecting the standard error
            // raises InvalidOperationException
//          stdErrReader = monoProc.StandardError;
            monoProc.Start();

            lblProcessStatus.Text = "Running...";
            lblProcessStatus.ForeColor = Color.Green;
            cmdKill.Enabled = true;
            
            timProcessMonitor.Enabled = true;
        }

        private void timProcessMonitor_Tick(object sender, EventArgs e)
        {
            if (monoProc.HasExited == true)
            {
                timProcessMonitor.Enabled = false;
                cmdClose.Enabled = true;
                cmdKill.Enabled = false;
                lblProcessStatus.Text = "Terminated";
                lblProcessStatus.ForeColor = Color.Red;
            }
        }

        private void cmdKill_Click(object sender, EventArgs e)
        {
            if (monoProc != null) monoProc.Kill();
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
