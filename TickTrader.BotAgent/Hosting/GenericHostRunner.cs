using Microsoft.Extensions.Hosting;
using System;

namespace TickTrader.BotAgent.Hosting
{
    public static class GenericHostRunner
    {
        public static void Run(IHost host, LaunchMode mode)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && mode == LaunchMode.WindowsService)
                GenericHostService.Run(host);
            else
                host.Run();
        }
    }
}
