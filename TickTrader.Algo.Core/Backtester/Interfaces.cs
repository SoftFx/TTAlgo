using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IQuoteStorage
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
        Dictionary<string, double> InitialAssets { get; }
        Dictionary<string, SymbolEntity> Symbols { get; }
        Dictionary<string, CurrencyEntity> Currencies { get; }
        TimeFrames MainTimeframe { get; }
        DateTime? EmulationPeriodStart { get; }
        DateTime? EmulationPeriodEnd { get; }
    }
}
