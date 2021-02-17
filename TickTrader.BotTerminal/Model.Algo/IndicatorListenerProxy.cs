using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

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

        private void Executor_LogUpdated(UnitLogRecord record)
        {
            _writer.LogMessage(record);
        }
    }
}
