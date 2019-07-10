using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotAgent.WebAdmin.Server.HostedServices
{
    public class ProtocolHostedService : IHostedService
    {
        private readonly IProtocolServer _protocolServer;
        private readonly ILogger<ProtocolHostedService> _logger;


        public ProtocolHostedService(IProtocolServer protocolServer, ILogger<ProtocolHostedService> logger)
        {
            _protocolServer = protocolServer;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_protocolServer.State != ServerStates.Stopped)
                return;

            _logger.LogInformation("Starting protocol server...");
            await _protocolServer.Start();
            _logger.LogInformation("Started protocol server");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_protocolServer.State != ServerStates.Started)
                return;

            _logger.LogInformation("Stopping protocol server...");
            await _protocolServer.Stop();
            _logger.LogInformation("Stopped protocol server");
        }
    }
}
