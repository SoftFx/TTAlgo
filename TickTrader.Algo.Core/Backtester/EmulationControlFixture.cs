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
        public DateTime EmulationTimePoint => InvokeEmulator.SafeVirtualTimePoint;
        public InvokeEmulator InvokeEmulator { get; }
        public BacktesterCollector Collector { get; }
        public PluginExecutor Executor { get; }
        public IBacktesterSettings Settings { get; }

        public EmulationControlFixture(IBacktesterSettings settings, PluginExecutor executor, CalculatorFixture calc)
        {
            Settings = settings;
            Collector = new BacktesterCollector(executor);
            InvokeEmulator = new InvokeEmulator(settings, Collector);
            Executor = executor;
        }

        public void Init()
        {
            Collector.Init(Settings);
        }

        public void EmulateExecution()
        {
            try
            {
                InvokeEmulator.EmulateEventsFlow();
            }
            catch (OperationCanceledException)
            {
                Collector.AddEvent(LogSeverities.Error, "Testing canceled!");
                throw;
            }
        }

        public void CollectTestResults()
        {
            var builder = Executor.GetBuilder();
            var acc = builder.Account;

            if (acc.IsMarginType)
            {
                Collector.Stats.InitialBalance = (decimal)Settings.InitialBalance;
                Collector.Stats.FinalBalance = (decimal)acc.Equity;
            }

            if (acc.IsMarginType)
            {
                //Collector.LogTrade("Initial equity: " + Settings.InitialBalance);
                //Collector.LogTrade("Final equity: " + acc.Equity);
                //Collector.LogTrade("Quotes emulated: " + Collector.TicksCount);
                //Collector.LogTrade("Orders opened: " + Collector.OrdersOpened);
                //Collector.LogTrade("Orders rejected: " + Collector.OrdersRejected);
                //Collector.LogTrade("Order modfications: " + Collector.Modifications);
                //Collector.LogTrade("Order modifications rejected: " + Collector.ModificationRejected);
            }
        }

        public void CancelEmulation()
        {
            InvokeEmulator.Cancel();
        }
    }
}
