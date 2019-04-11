using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TickTrader.BotAgent.Hosting
{
    public class WindowsServiceHost : WebHostService
    {
        private ILogger<WindowsServiceHost> _logger;


        public WindowsServiceHost(IWebHost host) : base(host)
        {
            _logger = host.Services.GetRequiredService<ILogger<WindowsServiceHost>>();
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogDebug("Service OnStarting");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            _logger.LogDebug("Service OnStarted");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            _logger.LogDebug("Service OnStopping");
            base.OnStopping();
        }
    }
}
