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
        public DateTime EmulationTimePoint => InvokeEmulator.SlimUpdateVirtualTimePoint;
        public InvokeEmulator InvokeEmulator { get; }
        public BacktesterCollector Collector { get; }
        public PluginExecutorCore Executor { get; }
        public FeedEmulator Feed { get; }
        public TradeHistoryEmulator TradeHistory { get; }
        public IBacktesterSettings Settings { get; }

        public event Action<EmulatorStates> StateUpdated
        {
            add => InvokeEmulator.StateUpdated += value;
            remove => InvokeEmulator.StateUpdated -= value;
        }

        public EmulationControlFixture(IBacktesterSettings settings, PluginExecutorCore executor, CalculatorFixture calc, FeedEmulator fEmulator)
        {
            Settings = settings;
            Feed = fEmulator ?? new FeedEmulator();
            Collector = new BacktesterCollector(executor);
            InvokeEmulator = new InvokeEmulator(settings, Collector, Feed, executor.Start, executor.EmulateStop);
            TradeHistory = new TradeHistoryEmulator();
            TradeHistory.OnReportAdded += TradeHistory_OnReportAdded;
            Executor = executor;
        }

        public bool OnStart()
        {
            try
            {
                Feed.InitStorages();

                Collector.OnStart(Settings, Feed);

                return InvokeEmulator.StartFeedRead();
            }
            catch (Exception ex)
            {
                throw WrapException(ex);
            }
        }

        public void OnStop()
        {
            Executor.WaitStop();

            Feed.DeinitStorages();

            var builder = Executor.GetBuilder();
            //var tradeEmulator = (TradeEmulator)Executor.GetTradeFixute();

            //tradeEmulator.Stop();

            Collector.OnStop(Settings, builder?.Account, Feed);
        }

        public void EmulateExecution(int warmupValue, WarmupUnitTypes warmupUnits)
        {
            try
            {
                InvokeEmulator.EmulateExecution(warmupValue, warmupUnits);
            }
            catch (Exception ex)
            {
                throw WrapException(ex);
            }
        }

        public void CancelEmulation()
        {
            InvokeEmulator.Cancel();
        }

        public void Pause()
        {
            InvokeEmulator.Pause();
        }

        public void Resume()
        {
            InvokeEmulator.Resume();
        }

        public void SetExecDelay(int delayMs)
        {
            InvokeEmulator.SetExecDelay(delayMs);
        }

        public override void Dispose()
        {
            Feed.Dispose();
            Collector.Dispose();

            base.Dispose();
        }

        private void TradeHistory_OnReportAdded(TradeReportAdapter rep)
        {
            Executor.OnUpdate(rep.Entity);
        }

        private Exception WrapException(Exception ex)
        {
            if (ex is AlgoException)
                return ex;

            if (ex is OperationCanceledException || ex is TaskCanceledException)
                return new AlgoOperationCanceledException(ex.Message);

            return new AlgoException(ex.GetType().Name + ": " + ex.Message);
        }
    }
}
