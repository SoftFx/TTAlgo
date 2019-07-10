using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ServiceProcess;

namespace TickTrader.BotAgent.Hosting
{
    public static class WebHostExtensions
    {
        public static void Launch(this IWebHost host)
        {
            var launchSettings = host.Services.GetRequiredService<IOptions<LaunchSettings>>().Value;

            switch (launchSettings.Mode)
            {
                case LaunchMode.Console:
                    host.Run();
                    break;
                case LaunchMode.WindowsService:
                    ServiceBase.Run(new WindowsServiceHost(host));
                    break;
            }
        }
    }
}
