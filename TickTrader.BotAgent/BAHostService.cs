using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Hosting;
using TickTrader.BotAgent.BA;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TickTrader.BotAgent
{
    public class BAHostService : WebHostService
    {
        private IBotAgent _server;
        private ILogger<BAHostService> _logger;
        public BAHostService(IWebHost host) : base(host)
        {
            _server = host.Services.GetRequiredService<IBotAgent>();
            _logger = host.Services.GetRequiredService<ILogger<BAHostService>>();
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
