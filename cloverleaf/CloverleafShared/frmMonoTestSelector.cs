using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace CloverleafShared
{
    public partial class frmMonoTestSelector : Form
    {
        String solutionDirectory;
        List<ProjectInfo> projectList;

        public frmMonoTestSelector(String slnDirectory, List<ProjectInfo> projList)
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
            (new frmMonoTestRunning(app)).Show();
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
