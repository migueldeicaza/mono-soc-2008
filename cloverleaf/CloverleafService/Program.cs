using System;
using System.Collections.Generic;
using System.Text;

using Mono.Zeroconf;

namespace CloverleafService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <remarks>
        /// TODO: Need to provide a way to configure this for a given
        /// machine (so the end user can name it, etc.). Eventually it
        /// might be useful for cleaning up zombied xsp2/apache2 processes,
        /// etc.
        /// </remarks>
        static void Main()
        {
			Configuration.Initialize();

			RegisterService service = new RegisterService();
			service.Name = Configuration.ServiceName;
			service.RegType = "_http._tcp";
			service.Port = 12321;
			service.TxtRecord = new TxtRecord();
			service.TxtRecord.Add(new TxtRecordItem("servicetype", "cloverleaf"));
			service.TxtRecord.Add(new TxtRecordItem("systemtype", Environment.MachineName + ", " +
					Environment.OSVersion.ToString()));

			service.Register();

			while (true) System.Threading.Thread.Sleep(5000);
        }
    }
}
