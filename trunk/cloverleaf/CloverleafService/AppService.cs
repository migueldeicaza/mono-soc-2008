using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

/**
using Mono.Zeroconf;
**/
namespace CloverleafService
{
    public partial class AppService : ServiceBase
    {
        public AppService()
        {
            /** Haven't had any luck getting Zeroconf.Providers.Avahi to compile
             *  yet, so this is on the back burner for the moment. They'll need
             *  to type in an IP address. (The horror, the horror!) The client
             *  will save the last used IP address, so that's easy enough.
            
            RegisterService service = new RegisterService();
            service.Name = CloverleafEnvironment.ServiceName;
            service.RegType = "_daap._tcp";
            service.ReplyDomain = "local.";
            service.Port = (short)CloverleafEnvironment.ServicePort;

            // TxtRecords are optional
            TxtRecord txt_record = new TxtRecord();
            txt_record.Add("ApplicationType", "CloverleafServer");
            service.TxtRecord = txt_record;

            service.Register();
            
             */


        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
