using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.BotTerminal
{
    public class IndicatorListenerProxy
    {
        private ExecutorModel _executor;
        private IIndicatorWriter _writer;

        public IndicatorListenerProxy(ExecutorModel executor, IIndicatorWriter writer)
        {
            _executor = executor;
            _writer = writer;

            executor.LogUpdated += Executor_LogUpdated;
        }

        private void Executor_LogUpdated(PluginLogRecord record)
        {
            _writer.LogMessage(record);
        }
    }
}
