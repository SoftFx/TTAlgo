using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TickTrader.BotAgent.BA;

namespace TickTrader.BotAgent.WebAdmin.Server.HostedServices
{
    public class BotAgentHostedService : IHostedService
    {
        private readonly IBotAgent _botAgent;
        private readonly IFdkOptionsProvider _fdkOptionsProvider;
        private readonly ILogger<BotAgentHostedService> _logger;
        private bool _started;


        public BotAgentHostedService(IBotAgent botAgent, IFdkOptionsProvider fdkOptionsProvider, ILogger<BotAgentHostedService> logger)
        {
            _botAgent = botAgent;
            _fdkOptionsProvider = fdkOptionsProvider;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_started)
                return;

            _logger.LogInformation("Starting bot agent...");
            await _botAgent.InitAsync(_fdkOptionsProvider);
            _started = true;
            _logger.LogInformation("Started bot agent...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_started)
                return;

            _logger.LogInformation("Stopping bot agent...");
            await Task.WhenAny(_botAgent.ShutdownAsync(), Task.Delay(TimeSpan.FromMinutes(1)));
            _started = false;
            _logger.LogInformation("Stopped bot agent...");
        }
    }
}
