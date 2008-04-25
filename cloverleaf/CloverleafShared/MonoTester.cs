using System;
using System.Collections.Generic;
using System.Text;

namespace CloverleafShared
{
    public class MonoTester : BaseTester
    {
        public MonoTester(String slnFile, String slnDirectory)
            : base(slnFile, slnDirectory)
        {

        }

        public override void Go()
        {
            List<ProjectInfo> projects = FindProjects(solutionDirectory, true);

            System.Windows.Forms.Application.Run(new frmMonoTestSelector(solutionDirectory, projects));
        }
    }
}
