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
        public TradeEmulator TradeEmulator { get; }
        public InvokeEmulator InvokeEmulator { get; }
        public BacktesterCollector Collector { get; }
        public DateTime StartTimePoint { get; }

        public EmulationControlFixture(DateTime startTime, CalculatorFixture calc)
        {
            StartTimePoint = startTime;
            InvokeEmulator = new InvokeEmulator();
            Collector = new BacktesterCollector(InvokeEmulator);
            TradeEmulator = new TradeEmulator(calc, Collector);
        }

        public void EmulateExecution()
        {
            InvokeEmulator.EmulateEventsFlow(StartTimePoint, _cancelEvent.Token);
        }

        public void CancelEmulation()
        {
            _cancelEvent.Cancel();
        }
    }
}
