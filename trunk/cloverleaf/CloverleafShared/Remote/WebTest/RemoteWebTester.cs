using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloverleafShared.Remote.WebTest
{
    public class RemoteWebTester : BaseWebTester
    {
        public RemoteWebTester(String projDir) : base(projDir)
        {
            CloverleafEnvironment.Initialize();
        }

        public void Go()
        {
            System.Windows.Forms.Application.Run(new RemoteWebSelector(projectDirectory));
        }
    }
}
