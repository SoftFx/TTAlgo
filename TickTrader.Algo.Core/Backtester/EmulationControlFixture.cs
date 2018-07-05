using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class EmulationControlFixture : CrossDomainObject
    {
        private CancellationTokenSource _cancelEvent = new CancellationTokenSource();

        public DateTime EmulationTimePoint => InvokeEmulator.SafeVirtualTimePoint;
        public InvokeEmulator InvokeEmulator { get; }
        public BacktesterCollector Collector { get; }
        public PluginExecutor Executor { get; }
        public IBacktesterSettings Settings { get; }

        public EmulationControlFixture(IBacktesterSettings settings, PluginExecutor executor, CalculatorFixture calc)
        {
            Settings = settings;
            InvokeEmulator = new InvokeEmulator(settings);
            Collector = new BacktesterCollector(executor, InvokeEmulator);
            Executor = executor;
        }

        public void EmulateExecution()
        {
            InvokeEmulator.EmulateEventsFlow(_cancelEvent.Token);
        }

        public void CollectTestResults()
        {
            var builder = Executor.GetBuilder();
            var acc = builder.Account;

            if (acc.IsMarginType)
            {
                Collector.LogTrade("Initial equity: " + Settings.InitialBalance);
                Collector.LogTrade("Final equity: " + acc.Equity);
            }
        }

        public void CancelEmulation()
        {
            _cancelEvent.Cancel();
        }
    }
}
