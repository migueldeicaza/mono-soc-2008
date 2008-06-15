using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace CloverleafService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            CloverleafEnvironment.Initialize();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new AppService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
