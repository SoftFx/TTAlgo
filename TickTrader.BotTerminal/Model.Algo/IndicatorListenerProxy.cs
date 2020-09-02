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
        private RuntimeModel _runtime;
        private IIndicatorWriter _writer;

        public IndicatorListenerProxy(RuntimeModel runtime, IIndicatorWriter writer)
        {
            _runtime = runtime;
            _writer = writer;

            runtime.LogUpdated += Executor_LogUpdated;
        }

        private void Executor_LogUpdated(UnitLogRecord record)
        {
            _writer.LogMessage(record);
        }
    }
}
