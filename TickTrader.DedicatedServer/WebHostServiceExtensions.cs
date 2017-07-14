using Microsoft.AspNetCore.Hosting;
using System.ServiceProcess;

namespace TickTrader.DedicatedServer
{
    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var webHostService = new DSHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
}
