using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

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
            if (botModel.State == PluginModelInfo.Types.PluginState.Stopping)
            {
                var tokenSource = new CancellationTokenSource();
                _abortTasks.Add(botModel, tokenSource);

                AbortBotAfter(botModel, _delayPunishment, tokenSource.Token).Forget();
            }
            else if (botModel.State == PluginModelInfo.Types.PluginState.Stopped)
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
            await Task.Delay(delay, token).ConfigureAwait(false);

            if (!token.IsCancellationRequested && bot.State == PluginModelInfo.Types.PluginState.Stopping)
            {
                bot.Abort();
            }
        }
    }
}
