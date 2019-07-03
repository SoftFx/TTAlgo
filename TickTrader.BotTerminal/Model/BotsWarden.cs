using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class BotsWarden
    {
        private readonly TimeSpan _delayPunishment = TimeSpan.FromSeconds(5);
        private Dictionary<TradeBotModel, CancellationTokenSource> _abortTasks;
        private LocalAlgoAgent _algoAgent;

        public BotsWarden(LocalAlgoAgent algoAgent)
        {
            _abortTasks = new Dictionary<TradeBotModel, CancellationTokenSource>();
            _algoAgent = algoAgent;
            _algoAgent.BotStateChanged += BotStateChanged;
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
            if (_abortTasks.TryGetValue(tradeBot, out var cancellationTokenSource))
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
