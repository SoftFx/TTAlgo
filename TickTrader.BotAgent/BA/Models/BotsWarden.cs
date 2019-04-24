using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.BA.Models
{
    public class BotsWarden
    {
        private readonly TimeSpan _delayPunishment = TimeSpan.FromSeconds(5);
        private Dictionary<string, CancellationTokenSource> _abortTasks;
        private IBotAgent _botAgent;

        public BotsWarden(IBotAgent botAgent)
        {
            _abortTasks = new Dictionary<string, CancellationTokenSource>();
            _botAgent = botAgent;
            _botAgent.BotStateChanged += BotStateChanged;
        }

        private void BotStateChanged(BotModelInfo bot)
        {
            if (bot.State == PluginStates.Stopping)
            {
                var tokenSource = new CancellationTokenSource();
                _abortTasks.Add(bot.InstanceId, tokenSource);

                AbortBotAfter(bot.InstanceId, _delayPunishment, tokenSource.Token).Forget();
            }
            else if (bot.State == PluginStates.Stopped)
            {
                СancelAbortTask(bot.InstanceId);
            }
        }

        private void СancelAbortTask(string botId)
        {
            if (_abortTasks.TryGetValue(botId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                _abortTasks.Remove(botId);
            }
        }

        private async Task AbortBotAfter(string botId, TimeSpan delay, CancellationToken token)
        {
            // BotStateChanged event violates actor rules. We have to continue on different thread so we don't get deadlock
            await Task.Delay(delay, token).ConfigureAwait(false);

            if (!token.IsCancellationRequested)
            {
                _botAgent.AbortBot(botId);
            }
        }
    }
}
