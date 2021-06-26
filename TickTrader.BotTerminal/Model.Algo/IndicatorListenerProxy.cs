using System;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.BotTerminal
{
    public class IndicatorListenerProxy
    {
        private readonly IIndicatorWriter _writer;
        private readonly IDisposable _logSub;

        public IndicatorListenerProxy(ExecutorModel executor, IIndicatorWriter writer)
        {
            _writer = writer;

            _logSub = executor.LogUpdated.Subscribe(Executor_LogUpdated);
        }

        public void Dispose()
        {
            _logSub.Dispose();
        }

        private void Executor_LogUpdated(PluginLogRecord record)
        {
            _writer.LogMessage(record);
        }
    }
}
