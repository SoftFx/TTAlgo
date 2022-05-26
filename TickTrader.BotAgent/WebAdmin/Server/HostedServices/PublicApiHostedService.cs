using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TickTrader.Algo.Server.PublicAPI.Adapter;

namespace TickTrader.BotAgent.WebAdmin.Server.HostedServices
{
    public class PublicApiHostedService : IHostedService
    {
        private readonly PublicApiServer _publicApiServer;
        private readonly ILogger<PublicApiHostedService> _logger;


        public PublicApiHostedService(PublicApiServer publicApiServer, ILogger<PublicApiHostedService> logger)
        {
            _publicApiServer = publicApiServer;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_publicApiServer.State != ServerStates.Stopped)
                return;

            _logger.LogInformation("Starting public api server...");
            await _publicApiServer.Start();
            _logger.LogInformation("Started public api server");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_publicApiServer.State != ServerStates.Started)
                return;

            _logger.LogInformation("Stopping public api server...");
            await _publicApiServer.Shutdown();
            _logger.LogInformation("Stopped public api server");
        }
    }
}
