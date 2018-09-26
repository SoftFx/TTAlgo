using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class BotsWarden
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TimeSpan _delayPunishment = TimeSpan.FromSeconds(5);
        private Dictionary<TradeBotModel, CancellationTokenSource> _abortTasks;
        private BotManager _botManager;

        public BotsWarden(BotManager botManager)
        {
            _abortTasks = new Dictionary<TradeBotModel, CancellationTokenSource>();
            _botManager = botManager;
            _botManager.StateChanged += BotStateChanged;
        }

        private void BotStateChanged(ITradeBot bot)
        {
            var botModel = (TradeBotModel)bot;
            if (botModel.State == PluginStates.Stopping)
            {
                var tokenSource = new CancellationTokenSource();
                _abortTasks.Add(botModel, tokenSource);

                AbortBotAfter(botModel, _delayPunishment, tokenSource.Token).Forget();
            }
            else if (botModel.State == PluginStates.Stopped)
            {
                СancelAbortTask(botModel);
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

            if (!token.IsCancellationRequested && bot.State == PluginStates.Stopping)
            {
                bot.Abort();
            }
        }
    }
}
