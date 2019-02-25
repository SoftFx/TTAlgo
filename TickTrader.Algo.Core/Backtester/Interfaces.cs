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

        BacktesterStreamingModes ChartDataMode { get; }
        BacktesterStreamingModes MarginDataMode { get; }
        BacktesterStreamingModes EquityDataMode { get; }
        BacktesterStreamingModes OutputDataMode { get; }
    }

    public enum BacktesterStreamingModes
    {
        Disabled,       // no data is collected
        Snapshot,       // no updates are sent, data can be loaded as snapshot after completion of backtesting
        BarCompletion,  // backtester sends bar updates upon bar сщьздуешщт, no real-time updates
        Realtime        // backtester sends every bar update (slow, required for visualisation)
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
