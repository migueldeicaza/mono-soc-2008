using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloverleafShared.Remote.AppTest
{
    public class RemoteAppTester : BaseAppTester
    {
        public RemoteAppTester(String slnFile, String slnDirectory)
            : base(slnFile, slnDirectory)
        {
            CloverleafEnvironment.Initialize();
        }

        public override void Go()
        {
            List<ProjectInfo> projects = FindProjects(solutionDirectory, true);

            System.Windows.Forms.Application.Run(new RemoteAppSelector(solutionDirectory, projects));
        }
    }
}
