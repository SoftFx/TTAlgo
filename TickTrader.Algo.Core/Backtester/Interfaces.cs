using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface ITickStorage
    {
        IEnumerable<QuoteEntity> GetQuoteStream();
    }

    public interface IBarStorage
    {
        IEnumerable<BarEntity> GrtBarStream();
    }

    internal interface IBacktesterSettings
    {
        string MainSymbol { get; }
        AccountTypes AccountType { get; }
        string BalanceCurrency { get; }
        int Leverage { get; }
        double InitialBalance { get; }
        TimeSpan ServerPing { get; }
        Dictionary<string, double> InitialAssets { get; }
        Dictionary<string, SymbolEntity> Symbols { get; }
        Dictionary<string, CurrencyEntity> Currencies { get; }
        TimeFrames MainTimeframe { get; }
        DateTime? EmulationPeriodStart { get; }
        DateTime? EmulationPeriodEnd { get; }
        JournalOptions JournalFlags { get; }

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

    public enum WarmupUnitTypes { Bars, Ticks, Days, Hours }

    [Flags]
    public enum JournalOptions
    {
        Disabled = 0,
        Enabled = 1,
        WriteInfo = 2,
        WriteCustom = 4,
        WriteTrade = 8,
        WriteOrderModifications = 128
    }

    
}
