using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Nini.Config;

namespace CloverleafShared
{
    /// <summary>
    /// Basic configuration details and a wrapper for the INI file in
    /// which they are stored.
    /// </summary>
    /// <remarks>
    /// Yeah, I know, I know, XML is the future and all that--but I
    /// like INI and nobody should be editing this file by hand anyway,
    /// I would think. ;-)
    /// </remarks>
    public static class CloverleafEnvironment
    {
        private static String cloverleafAppDataDirectory;

        private static String monoRootPath;
        private static String monoBinPath;

        private static String iniFileName;
        private static IniConfigSource iniFile;

		public static Boolean IsAddin { get; private set; }

        /// <summary>
        /// Prepares the CloverleafEnvironment values for application use.
        /// </summary>
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

		public static void InitializeAddin()
		{
			CloverleafEnvironment.IsAddin = true;
			Initialize();
		}

        /// <summary>
        /// The user-specific application data directory for Cloverleaf.
        /// </summary>
        public static String CloverleafAppDataPath
        {
            get { return cloverleafAppDataDirectory; }
        }
        /// <summary>
        /// The Cloverleaf executable's root path.
        /// </summary>
        public static String CloverleafPath
        {
            get { return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); }
        }
        /// <summary>
        /// General path for any tools that Cloverleaf needs.
        /// </summary>
        public static String CloverleafToolsPath
        {
            get { return Path.Combine(CloverleafPath, "tools"); }
        }
        /// <summary>
        /// Path to the Xming X server.
        /// </summary>
        public static String XPath
        {
            get
            {
                return Path.Combine(Path.Combine(CloverleafEnvironment.CloverleafToolsPath, "xming"),
                  "xming.exe");
            }
        }
        /// <summary>
        /// The root path of the established default Mono installation,
        /// as defined in the registry.
        /// </summary>
        public static String MonoRootPath
        {
            get { return monoRootPath; }
        }
        /// <summary>
        /// The path to Mono's binaries.
        /// </summary>
        public static String MonoBinPath
        {
            get { return monoBinPath; }
        }
        /// <summary>
        /// The port on which XSP2 should run.
        /// </summary>
        public static Int32 DefaultLocalXSPPort
        {
            get { return iniFile.Configs["XSPTester"].GetInt("port", 12021); }
            set
            {
                iniFile.Configs["XSPTester"].Set("port", value);
                iniFile.Save(iniFileName);
            }
        }
        /// <summary>
        /// Should XSP2 run with HTTPS support?
        /// </summary>
        public static Boolean LocalXSPUsesHTTPS
        {
            get { return iniFile.Configs["XSPTester"].GetBoolean("https", false); }
            set
            {
                iniFile.Configs["XSPTester"].Set("https", value);
                iniFile.Save(iniFileName);
            }
        }
        /// <summary>
        /// Should the browser automatically start when XSP2 is invoked locally?
        /// </summary>
        public static Boolean XSPBrowserAutostart
        {
            get { return iniFile.Configs["XSPTester"].GetBoolean("browser_autostart", true); }
            set
            {
                iniFile.Configs["XSPTester"].Set("browser_autostart", value);
                iniFile.Save(iniFileName);
            }
        }
        /// <summary>
        /// Last entered remote server IP/hostname. Used by --appremote and --wwwremote.
        /// </summary>
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

        /// <summary>
        /// The number of seconds to wait before killing a thread that's doing
        /// a Zeroconf poll, for remote web and app development.
        /// </summary>
        public static Int32 ZeroconfPollTimeout
        {
            get
            {
                return iniFile.Configs["RemoteTests"].GetInt("timeout", 15);
            }
        }

        /// <summary>
        /// Create a new configuration.
        /// </summary>
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
