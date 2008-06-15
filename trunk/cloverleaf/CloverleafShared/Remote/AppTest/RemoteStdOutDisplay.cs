using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace CloverleafShared.Remote.AppTest
{
    public partial class RemoteStdOutDisplay : Form
    {
        public RemoteStdOutDisplay(String stdOut, String stdErr)
        {
            InitializeComponent();

            txtStdOut.Text = stdOut.Replace("\x0A", Environment.NewLine);
            txtStdErr.Text = stdErr.Replace("\x0A", Environment.NewLine);
        }

        private void cmdSaveStdOut_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.AddExtension = true;
            sf.Title = "Save Standard Output File...";
            sf.InitialDirectory = Environment.CurrentDirectory;
            sf.FileName = "stdOut." + DateTime.Now.Ticks + ".txt";
            sf.ShowDialog();

            if (sf.FileName != "")
                File.WriteAllText(sf.FileName, txtStdOut.Text);
        }

        private void cmdSaveStdErr_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.AddExtension = true;
            sf.Title = "Save Standard Error File...";
            sf.InitialDirectory = Environment.CurrentDirectory;
            sf.FileName = "stdErr." + DateTime.Now.Ticks + ".txt";
            sf.ShowDialog();

            if (sf.FileName != "")
                File.WriteAllText(sf.FileName, txtStdErr.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // kill it with fire!
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
