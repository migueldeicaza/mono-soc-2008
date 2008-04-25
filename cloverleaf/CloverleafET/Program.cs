using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using CloverleafShared;

namespace CloverleafET
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                ProcessCmdArgs();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Application.Exit();
            }
        }

        /// <summary>
        /// Parses the command line arguments passed to the
        /// program by Visual Studio and launches the appropriate
        /// command handler for the various functions.
        /// </summary>
        static void ProcessCmdArgs()
        {
            Int32 folderSearchDepth = -1;
            String[] cmdArgs = Environment.GetCommandLineArgs();

            if (cmdArgs.Length < 3)
                throw new Exception("Invalid number of arguments passed to Cloverleaf. Terminating.");

            if (cmdArgs.Length > 3)
            {
                for (Int32 i = 3; i < cmdArgs.Length; i++)
                {
                    String[] subCmdArgs = cmdArgs[i].Split('=');

                    switch (subCmdArgs[0])
                    {
                        // While I can't imagine the program being TOO slow at
                        // any time, the brute force directory search has its
                        // drawbacks. This option can be implemented in the
                        // future if demanded (but will require a rewrite of
                        // BaseTester.FindProjectFiles to be considerably
                        // longer and uglier). Really just a stand-in now for
                        // optional command arguments so I don't forget to add
                        // support later.
                        case "--folder-search-depth":
                            {
                                folderSearchDepth = Int32.Parse(subCmdArgs[1]);
                                break;
                            }
                    }
                }
            }

            switch (cmdArgs[1])
            {
                case "--monotest":
                    {
                        (new MonoTester(cmdArgs[2], Environment.CurrentDirectory)).Go();
                        break;
                    }
                case "--gendarmetest":
                    {
                        break;
                    }
                case "--remotetest":
                    {
                        break;
                    }
            }
        }
    }
}
