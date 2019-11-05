using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class IndicatorListenerProxy
    {
        private PluginExecutor _executor;
        private IIndicatorWriter _writer;

        public IndicatorListenerProxy(PluginExecutor executor, IIndicatorWriter writer)
        {
            _executor = executor;
            _writer = writer;

            executor.Config.IsLoggingEnabled = true;
            executor.LogUpdated += Executor_LogUpdated;
        }

        private void Executor_LogUpdated(PluginLogRecord record)
        {
            _writer.LogMessage(record);
        }
    }
}
