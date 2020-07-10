using System;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using Machinarium.Qnil;

namespace TickTrader.Algo.Common.Model
{
    public class SymbolModel : Setup.ISymbolInfo
    {
        public SymbolModel(Domain.SymbolInfo info, IVarSet<string, CurrencyEntity> currencies)
        {
            Descriptor = info;

            BaseCurrency = currencies.Snapshot.Read(info.BaseCurrency);
            QuoteCurrency = currencies.Snapshot.Read(info.CounterCurrency);

            BaseCurrencyDigits = BaseCurrency?.Precision ?? 2;
            QuoteCurrencyDigits = QuoteCurrency?.Precision ?? 2;

            BidTracker = new RateDirectionTracker();
            AskTracker = new RateDirectionTracker();

            BidTracker.Precision = info.Digits;
            AskTracker.Precision = info.Digits;
        }

        public string Name => Descriptor.Name;
        public string Description => Descriptor.Description;
        public bool IsUserCreated => false;
        public Domain.SymbolInfo Descriptor { get; private set; }
        public RateDirectionTracker BidTracker { get; private set; }
        public RateDirectionTracker AskTracker { get; private set; }
        public int PriceDigits => Descriptor.Digits;
        public int BaseCurrencyDigits { get; private set; }
        public int QuoteCurrencyDigits { get; private set; }
        public CurrencyEntity BaseCurrency { get; private set; }
        public CurrencyEntity QuoteCurrency { get; private set; }
        public int Depth { get; private set; }
        public int RequestedDepth { get; private set; }
        public QuoteEntity LastQuote { get; private set; }
        public double? CurrentAsk { get; private set; }
        public double? CurrentBid { get; private set; }
        public double LotSize => Descriptor.LotSize;

        public event Action<SymbolModel> InfoUpdated = delegate { };
        public event Action<SymbolModel> RateUpdated = delegate { };

        public virtual void Close()
        {
            //subscription.Dispose();
        }

        public virtual void Update(Domain.SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(this);
        }

        internal virtual void OnNewTick(QuoteEntity tick)
        {
            LastQuote = tick;

            CurrentBid = tick.GetNullableBid();
            CurrentAsk = tick.GetNullableAsk();

            BidTracker.Rate = CurrentBid;
            AskTracker.Rate = CurrentAsk;

            RateUpdated(this);
        }

        public bool ValidateAmmount(decimal amount, decimal minVolume, decimal maxVolume, decimal step)
        {
            return amount <= maxVolume && amount >= minVolume
                && (amount / step) % 1 == 0;
        }

        #region ISymbolInfo

        string Setup.ISymbolInfo.Id => Name;

        Info.SymbolOrigin Setup.ISymbolInfo.Origin => Info.SymbolOrigin.Online;

        #endregion ISymbolInfo
    }
}
