//
// Program.cs: Main program file for the Cloverleaf External Tool
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

//            try
//            {
                ProcessCmdArgs();
                
//            }
//            catch (Exception e)
//            {
//                MessageBox.Show(e.ToString());
//                Application.Exit();
//            }
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
                        // Might be wanted later for people traversing really
                        // huge solution trees. Really just a stand-in now for
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
                        (new CloverleafShared.TestInMono.MonoTester(cmdArgs[2],
                                Environment.CurrentDirectory)).Go();
                        break;
                    }
                case "--gendarmetest":
                    {
                        (new CloverleafShared.TestInGendarme.GendarmeTester(cmdArgs[2],
                                Environment.CurrentDirectory)).Go();
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
