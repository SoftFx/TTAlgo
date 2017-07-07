using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Hosting;
using TickTrader.DedicatedServer.DS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TickTrader.DedicatedServer
{
    public class DSHostService : WebHostService
    {
        private IDedicatedServer _server;
        private ILogger<DSHostService> _logger;
        public DSHostService(IWebHost host) : base(host)
        {
            _server = host.Services.GetRequiredService<IDedicatedServer>();
            _logger = host.Services.GetRequiredService<ILogger<DSHostService>>();
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
