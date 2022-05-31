using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TickTrader.BotAgent.BA;

namespace TickTrader.BotAgent.WebAdmin.Server.HostedServices
{
    public class AlgoServerHostedService : IHostedService
    {
        private readonly IBotAgent _botAgent;
        private readonly IConfiguration _config;
        private readonly ILogger<AlgoServerHostedService> _logger;
        private bool _started;


        public AlgoServerHostedService(IBotAgent botAgent, IConfiguration config, ILogger<AlgoServerHostedService> logger)
        {
            _botAgent = botAgent;
            _config = config;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_started)
                return;

            _logger.LogInformation("Starting algo server...");
            await _botAgent.InitAsync(_config);
            _started = true;
            _logger.LogInformation("Started algo server...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_started)
                return;

            _logger.LogInformation("Stopping algo server...");
            await Task.WhenAny(_botAgent.ShutdownAsync(), Task.Delay(TimeSpan.FromMinutes(1)));
            _started = false;
            _logger.LogInformation("Stopped algo server...");
        }
    }
}
