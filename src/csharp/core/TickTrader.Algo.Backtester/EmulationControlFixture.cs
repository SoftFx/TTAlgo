using System;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
{
    internal class EmulationControlFixture
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
            Executor = executor;

            Feed = fEmulator ?? new FeedEmulator();
            Collector = new BacktesterCollector(executor);
            InvokeEmulator = new InvokeEmulator(settings, Collector, Feed, Executor);
            TradeHistory = new TradeHistoryEmulator();
        }

        public bool OnStart()
        {
            Feed.InitStorages();

            Collector.OnStart(Settings, Feed);

            return InvokeEmulator.StartFeedRead();
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
            InvokeEmulator.EmulateExecution(warmupValue, warmupUnits);
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

        public void Dispose()
        {
            Feed.Dispose();
            Collector.Dispose();
        }

        //private void TradeHistory_OnReportAdded(TradeReportAdapter rep)
        //{
        //    Executor.OnUpdate(rep.Info);
        //}
    }
}
