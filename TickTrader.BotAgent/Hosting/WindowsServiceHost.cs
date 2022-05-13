using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.ServiceProcess;

namespace TickTrader.BotAgent.Hosting
{
    public class WindowsServiceHost : ServiceBase
    {
        private ILogger<WindowsServiceHost> _logger;


        //public WindowsServiceHost(IHost host) : base(host)
        //{
        //    _logger = host.Services.GetRequiredService<ILogger<WindowsServiceHost>>();
        //}

        //protected override void OnStarting(string[] args)
        //{
        //    _logger.LogDebug("Service OnStarting");
        //    base.OnStarting(args);
        //}

        //protected override void OnStarted()
        //{
        //    _logger.LogDebug("Service OnStarted");
        //    base.OnStarted();
        //}

        //protected override void OnStopping()
        //{
        //    _logger.LogDebug("Service OnStopping");
        //    base.OnStopping();
        //}
    }
}
