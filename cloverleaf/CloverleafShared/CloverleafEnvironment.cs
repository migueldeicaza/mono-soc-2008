using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Nini.Config;

namespace CloverleafShared
{
    public static class CloverleafEnvironment
    {
        private static String cloverleafAppDataDirectory;

        private static String monoRootPath;
        private static String monoBinPath;

        private static String iniFileName;
        private static IniConfigSource iniFile;

        public static void Initialize()
        {
            if (monoRootPath != null) return;

            cloverleafAppDataDirectory =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Cloverleaf");
            if (Directory.Exists(cloverleafAppDataDirectory) == false)
                Directory.CreateDirectory(cloverleafAppDataDirectory);

            iniFileName = Path.Combine(cloverleafAppDataDirectory, "config.ini");
            if (File.Exists(iniFileName))
            {
                iniFile = new IniConfigSource(iniFileName);
            }
            else
            {
                CreateConfiguration();
                iniFile.Save(iniFileName);
            }

            String clrKey =
                    (String)Microsoft.Win32.Registry.GetValue(iniFile.Configs["Default"].GetString("MonoRegKey"),
                    "DefaultCLR", "NONE");
            if (clrKey == "NONE")
                throw new Exception("No Mono CLR found in the registry.");

            monoRootPath =
                    (String)Microsoft.Win32.Registry.GetValue(iniFile.Configs["Default"].GetString("MonoRegKey") +
                    "\\" + clrKey, "SdkInstallRoot", "NONE");
            if (monoRootPath == "NONE")
                throw new Exception("No default path for the Mono SDK found.");

            monoBinPath = Path.Combine(MonoRootPath, "bin");

        }
        public static String CloverleafAppDataPath
        {
            get { return cloverleafAppDataDirectory; }
        }
        public static String CloverleafPath
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); }
        }
        public static String CloverleafToolsPath
        {
            get { return Path.Combine(CloverleafPath, "tools"); }
        }
        public static String XPath
        {
            get
            {
                return Path.Combine(Path.Combine(CloverleafEnvironment.CloverleafToolsPath, "xming"),
                  "xming.exe");
            }
        }
        public static String MonoRootPath
        {
            get { return monoRootPath; }
        }
        public static String MonoBinPath
        {
            get { return monoBinPath; }
        }
        public static Int32 DefaultLocalXSPPort
        {
            get { return iniFile.Configs["XSPTester"].GetInt("port", 12021); }
            set
            {
                iniFile.Configs["XSPTester"].Set("port", value);
                iniFile.Save(iniFileName);
            }
        }
        public static Boolean LocalXSPUsesHTTPS
        {
            get { return iniFile.Configs["XSPTester"].GetBoolean("https", false); }
            set
            {
                iniFile.Configs["XSPTester"].Set("https", value);
                iniFile.Save(iniFileName);
            }
        }
        public static Boolean XSPBrowserAutostart
        {
            get { return iniFile.Configs["XSPTester"].GetBoolean("browser_autostart", true); }
            set
            {
                iniFile.Configs["XSPTester"].Set("browser_autostart", value);
                iniFile.Save(iniFileName);
            }
        }

        public static String RemoteServerHost
        {
            get
            {
                return iniFile.Configs["RemoteTests"].GetString("ip", "127.0.0.1");
            }
            set
            {
                iniFile.Configs["RemoteTests"].Set("ip", value);
                iniFile.Save(iniFileName);
            }
        }
        public static Int32 RemoteServerPort
        {
            get
            {
                return iniFile.Configs["RemoteTests"].GetInt("port", 15951);
            }
            set
            {
                iniFile.Configs["RemoteTests"].Set("port", value);
                iniFile.Save(iniFileName);
            }
        }
        private static void CreateConfiguration()
        {
            iniFile = new IniConfigSource();

            iniFile.Configs.Add(new IniConfig("Default", iniFile));
            iniFile.Configs.Add(new IniConfig("XSPTester", iniFile));
            iniFile.Configs.Add(new IniConfig("RemoteTests", iniFile));

            iniFile.Configs["Default"].Set("MonoRegKey", @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Mono");

            iniFile.Configs["XSPTester"].Set("port", 12021);
            iniFile.Configs["XSPTester"].Set("https", false);
            iniFile.Configs["XSPTester"].Set("browser_autostart", true);

            iniFile.Configs["RemoteTests"].Set("ip", "127.0.0.1");
            iniFile.Configs["RemoteTests"].Set("port", 15951);
        }
    }
}
