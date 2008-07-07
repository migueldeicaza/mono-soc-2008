using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace CloverleafShared.TestInXSP
{
    /// <summary>
    /// The starter class for testing XSP projects locally.
    /// </summary>
    /// <remarks>
    /// There may be a way to streamline the UI of Cloverleaf so
    /// you don't need separate "Test Website" and "Test Application"
    /// commands in the External Tools (or standard) menu. I don't
    /// know it.
    /// </remarks>
    public class XSPTester : BaseWebTester
    {

        public XSPTester(String projDir)
            : base(projDir)
        {

        }
        /// <summary>
        /// Invokes an XSPOptions form using the currently selected project.
        /// </summary>
        public void Go()
        {
            System.Windows.Forms.Application.Run(new XSPOptions(projectDirectory));
        }
    }
}
