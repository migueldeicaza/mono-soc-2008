using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CloverleafShared
{
    /// <summary>
    /// The base testing class for handling web projects (--xsptest and --wwwremote).
    /// </summary>
    public abstract class BaseWebTester
    {
        protected readonly String projectDirectory;

        public BaseWebTester(String projDir)
        {
            CloverleafEnvironment.Initialize();
            projectDirectory = projDir;

            if (File.Exists(Path.Combine(projectDirectory, "web.config")) == false)
            {
                throw new Exception("The selected project is not an ASP.NET Web Site.");
            }
        }
    }
}
