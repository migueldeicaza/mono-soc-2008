using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Nini.Config;

namespace CloverleafService
{
    public static class CloverleafEnvironment
    {
        private static String iniFileName;
        private static IniConfigSource iniFile;

        public static void Initialize()
        {
            iniFileName = 
                Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
                "cloverleafservice.config.ini");
            if (File.Exists(iniFileName))
            {
                iniFile = new IniConfigSource(iniFileName);
            }
            else
            {
                CreateConfiguration();
                iniFile.Save(iniFileName);
            }
        }

        public static String ServiceName
        {
            get
            {
                return iniFile.Configs["Default"].GetString("ServiceName",
                        "Cloverleaf Service on " + Environment.MachineName.ToUpper());
            }
        }
        public static Int32 ServicePort
        {
            get
            {
                return iniFile.Configs["Default"].GetInt("ServicePort", 15951);
            }
        }

        private static void CreateConfiguration()
        {
            iniFile = new IniConfigSource();

            iniFile.Configs.Add(new IniConfig("Default", iniFile));
            iniFile.Configs.Add(new IniConfig("XSPTester", iniFile));

            iniFile.Configs["Default"].Set("ServiceName",
                   "Cloverleaf Service on " + Environment.MachineName.ToUpper());
            iniFile.Configs["Default"].Set("ServicePort", 15951);
        }
    }
}
