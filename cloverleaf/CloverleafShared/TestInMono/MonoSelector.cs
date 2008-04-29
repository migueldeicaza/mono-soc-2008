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

namespace CloverleafShared.TestInMono
{
    public partial class MonoSelector : Form
    {
        String solutionDirectory;
        List<ProjectInfo> projectList;

        public MonoSelector(String slnDirectory, List<ProjectInfo> projList)
        {
            InitializeComponent();

            projectList = projList;
            solutionDirectory = slnDirectory;

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

            this.Hide();
            (new MonoRunner(app)).Show();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lstLaunchItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdOK.Enabled = true;
        }
    }
}
