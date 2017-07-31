using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotsWarden
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TimeSpan _delayPunishment = TimeSpan.FromSeconds(5);
        private IBotAggregator _botAggregator;
        public BotsWarden(IBotAggregator aggregator)
        {
            _botAggregator = aggregator;
            _botAggregator.StateChanged += BotStateChanged;
        }

        private async void BotStateChanged(TradeBotModel tradeBot)
        {
            if (tradeBot.State == BotModelStates.Stopping)
            {
                await Task.Delay(_delayPunishment);

                if (tradeBot.State == BotModelStates.Stopping)
                {
                    tradeBot.Abort();
                    _logger.Info($"Bot '{tradeBot.InstanceId}' was aborted");
                }
            }
        }
    }
}
