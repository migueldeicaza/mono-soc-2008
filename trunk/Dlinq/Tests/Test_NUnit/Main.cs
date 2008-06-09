using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NunitDebugLauncher
{
    public class MainClass
    {
        public static void Main()
        {
            NUnit.Gui.AppEntry.Main(new string[]{"..\\..\\DbLinq.nunit"});
        }
    }
}
