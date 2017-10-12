using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    internal class BotsWarden
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TimeSpan _delayPunishment = TimeSpan.FromSeconds(5);
        private Dictionary<TradeBotModel, CancellationTokenSource> _abortTasks;
        private IBotAggregator _botAggregator;

        public BotsWarden(IBotAggregator aggregator)
        {
            _abortTasks = new Dictionary<TradeBotModel, CancellationTokenSource>();
            _botAggregator = aggregator;
            _botAggregator.StateChanged += BotStateChanged;
        }

        private void BotStateChanged(TradeBotModel bot)
        {
            if (bot.State == BotModelStates.Stopping)
            {
                var tokenSource = new CancellationTokenSource();
                _abortTasks.Add(bot, tokenSource);

                AbortBotAfter(bot, _delayPunishment, tokenSource.Token).Forget();
            }
            else if(bot.State == BotModelStates.Stopped)
            {
                СancelAbortTask(bot);
            }
        }

        private void СancelAbortTask(TradeBotModel tradeBot)
        {
            if (_abortTasks.TryGetValue(tradeBot, out CancellationTokenSource cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                _abortTasks.Remove(tradeBot);
            }
        }

        private async Task AbortBotAfter(TradeBotModel bot, TimeSpan delay, CancellationToken token)
        {
            await Task.Delay(delay, token);

            if (!token.IsCancellationRequested && bot.State == BotModelStates.Stopping)
            {
                bot.Abort();
                _logger.Info($"Bot '{bot.InstanceId}' was aborted");
            }
        }
    }
}
