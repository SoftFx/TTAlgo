using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class VsIntegration
    {
        public static void InstallVsPackage()
        {
            var vsixPath = Path.Combine(EnvService.Instance.RedistFolder, "TickTrader.Algo.VS.Package.vsix");
            Process.Start(vsixPath);
        }
    }
}
