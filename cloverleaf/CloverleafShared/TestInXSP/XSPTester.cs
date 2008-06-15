using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace CloverleafShared.TestInXSP
{
    public class XSPTester : BaseWebTester
    {

        public XSPTester(String projDir)
            : base(projDir)
        {

        }

        public void Go()
        {
            System.Windows.Forms.Application.Run(new XSPOptions(projectDirectory));
        }
    }
}
