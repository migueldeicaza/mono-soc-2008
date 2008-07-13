using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Zeroconf;

namespace CloverleafShared.Remote
{
    class CloverleafLocator
    {
        static ServiceBrowser browser;
        static Dictionary<String, String> serviceIPDict;

        public CloverleafLocator()
        {
            Mono.Zeroconf.Providers.Bonjour.ZeroconfProvider p = new
                Mono.Zeroconf.Providers.Bonjour.ZeroconfProvider();
            
            serviceIPDict = new Dictionary<String, String>();
            browser = new ServiceBrowser();

            browser.ServiceAdded += new ServiceBrowseEventHandler(OnServiceAdded);
        }
        public Dictionary<String, String> ServiceIPDictionary
        {
            get { return serviceIPDict; }
        }

        private void OnServiceAdded(object o, ServiceBrowseEventArgs args)
        {
            args.Service.Resolved += new ServiceResolvedEventHandler(OnServiceResolved);
            args.Service.Resolve();
        }

        private void OnServiceResolved(object o, ServiceResolvedEventArgs args)
        {
            if (serviceIPDict.ContainsKey(args.Service.Name) == false)
            {
                Boolean isCloverleafService = false;
                for (int i = 0; i < args.Service.TxtRecord.Count; i++)
                {
                    TxtRecordItem txt = args.Service.TxtRecord.GetItemAt(i);

                    if (txt.Key.ToLower() == "servicetype" && txt.ValueString.ToLower() == "cloverleaf")
                        isCloverleafService = true;
                }

                if (isCloverleafService == true)
                {
                    serviceIPDict.Add(args.Service.Name,
                        args.Service.HostEntry.AddressList[0].ToString());
                }
            }
        }

        public void Go()
        {
            browser.Browse("_http._tcp", "local");
        }

        /// <summary>
        /// Returns the number of similar octets in two IP addresses.
        /// For example, "192.168.0.1" and "192.168.0.255" will return 3.
        /// Used by the remote app selectors to determine the best local
        /// IP address to autoselect.
        /// </summary>
        /// <returns>Number of sismilar octets, starting from the first.</returns>
        public static Int32 NumberOfSimilarOctets(String original, String comparator)
        {
            Int32 similar = 0;

            String[] o = original.Split('.');
            String[] c = comparator.Split('.');

            for (Int32 i = 0; i < 4; i++)
            {
                if (o[i] == c[i])
                    similar++;
                else
                    return similar;
            }

            return similar;
        }
    }
}
