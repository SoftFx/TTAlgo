using System;
using System.Collections.Generic;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public class CommonTestSettings
    {
        public string MainSymbol { get; set; }
        public AccountInfo.Types.Type AccountType { get; set; }
        public string BalanceCurrency { get; set; }
        public int Leverage { get; set; } = 100;
        public double InitialBalance { get; set; } = 10000;
        public TimeSpan ServerPing { get; set; }
        public Dictionary<string, AssetInfo> InitialAssets { get; } = new Dictionary<string, AssetInfo>();
        public Dictionary<string, SymbolInfo> Symbols { get; } = new Dictionary<string, SymbolInfo>();
        public Dictionary<string, CurrencyInfo> Currencies { get; } = new Dictionary<string, CurrencyInfo>();
        public Feed.Types.Timeframe MainTimeframe { get; set; }
        public Feed.Types.Timeframe ModelTimeframe { get; set; }
        public DateTime? EmulationPeriodStart { get; set; }
        public DateTime? EmulationPeriodEnd { get; set; }
        public int WarmupSize { get; set; } = 10;
        public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MainSymbol))
                throw new AlgoException("MainSymbol must be specified!");

            if (string.IsNullOrWhiteSpace(BalanceCurrency))
                throw new AlgoException("BalanceCurrency must be specified!");

            if (!Symbols.ContainsKey(MainSymbol))
                throw new AlgoException("No metadata for main symbol!");
        }
    }

    internal interface IBacktesterSettings
    {
        CommonTestSettings CommonSettings { get; }
        JournalOptions JournalFlags { get; }
        string JournalPath { get; }

        //TestDataSeriesFlags ChartDataMode { get; }
        Dictionary<string, TestDataSeriesFlags> SymbolDataConfig { get; }
        TestDataSeriesFlags MarginDataMode { get; }
        TestDataSeriesFlags EquityDataMode { get; }
        TestDataSeriesFlags OutputDataMode { get; }
        bool StreamExecReports { get; }
    }

    [Flags]
    public enum TestDataSeriesFlags
    {
        Disabled        = 0,  // no data is collected
        Snapshot        = 1,  // collect data as a snashot, snapshot is available at the end of testing
        Stream          = 2,  // stream data out during the testing process
        Realtime        = 4   // real-time streaming (works only if Stream flag is set)
    }

    public enum EmulatorStates { WarmingUp, Running, Paused, Stopping, Stopped }

    public interface ITestExecController
    {
        EmulatorStates State { get; }
        event Action<EmulatorStates> StateChanged;
        event Action<Exception> ErrorOccurred;
        void Pause();
        void Resume();
        void SetExecDelay(int delayMs);
    }
}
