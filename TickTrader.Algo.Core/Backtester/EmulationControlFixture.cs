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
        public FeedEmulator Feed { get; }
        public IBacktesterSettings Settings { get; }

        public EmulationControlFixture(IBacktesterSettings settings, PluginExecutor executor, CalculatorFixture calc)
        {
            Settings = settings;
            Feed = new FeedEmulator();
            Collector = new BacktesterCollector(executor);
            InvokeEmulator = new InvokeEmulator(settings, Collector, Feed);
            Executor = executor;
        }

        public void OnStart()
        {
            Collector.OnStart(Settings);
        }

        public bool WarmUp(int warmupValue, WarmupUnitTypes warmupUnits)
        {
            try
            {
                if (warmupUnits == WarmupUnitTypes.Days)
                    return InvokeEmulator.WarmupByTimePeriod(TimeSpan.FromDays(warmupValue));
                else if (warmupUnits == WarmupUnitTypes.Hours)
                    return InvokeEmulator.WarmupByTimePeriod(TimeSpan.FromHours(warmupValue));
                else if (warmupUnits == WarmupUnitTypes.Bars)
                    return InvokeEmulator.WarmupByBars(warmupValue);
                else if (warmupUnits == WarmupUnitTypes.Ticks)
                    return InvokeEmulator.WarmupByQuotes(warmupValue);
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw WrapException(ex);
            }
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
            catch (Exception ex)
            {
                throw WrapException(ex);
            }
        }

        public void OnStop()
        {
            var builder = Executor.GetBuilder();

            Collector.OnStop(Settings, builder.Account);
        }

        public void CancelEmulation()
        {
            InvokeEmulator.Cancel();
        }

        public override void Dispose()
        {
            Feed.Dispose();
            Collector.Dispose();

            base.Dispose();
        }

        private Exception WrapException(Exception ex)
        {
            return new AlgoException(ex.GetType().Name + ": " + ex.Message);
        }
    }
}
