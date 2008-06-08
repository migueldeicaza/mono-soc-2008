using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace CloverleafShared.TestInXSP
{
    public class XSPTester
    {
        private String projectDirectory;

        public XSPTester(String projDir)
        {
            CloverleafEnvironment.Initialize();
            projectDirectory = projDir;

            if (File.Exists(Path.Combine(projectDirectory, "web.config")) == false)
            {
                throw new Exception("The selected project is not an ASP.NET Web Site.");
            }
        }

        public void Go()
        {
            System.Windows.Forms.Application.Run(new XSPOptions(projectDirectory));
        }
    }
}
