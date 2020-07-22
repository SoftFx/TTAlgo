using System;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using Machinarium.Qnil;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class SymbolModel : Setup.ISymbolInfo, ISymbolInfo2
    {
        public SymbolModel(Domain.SymbolInfo info, IVarSet<string, CurrencyEntity> currencies)
        {
            Descriptor = info;

            BaseCurrency = currencies.Snapshot.Read(info.BaseCurrency);
            QuoteCurrency = currencies.Snapshot.Read(info.CounterCurrency);

            BaseCurrencyDigits = BaseCurrency?.Digits ?? 2;
            QuoteCurrencyDigits = QuoteCurrency?.Digits ?? 2;

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
        public event Action<ISymbolInfo2> RateUpdated = delegate { };

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

        void ISymbolInfo2.UpdateRate(Quote quote) => OnNewTick((QuoteEntity)quote);

        #region ISymbolInfo

        string Setup.ISymbolInfo.Id => Name;

        Info.SymbolOrigin Setup.ISymbolInfo.Origin => Info.SymbolOrigin.Online;

        double ISymbolInfo2.DefaultSlippage => Descriptor.Slippage.DefaultValue ?? 0;

        double ISymbolInfo2.Point => Math.Pow(10, -PriceDigits);

        double ISymbolInfo2.Bid { get => BidTracker.Rate ?? 0; set => BidTracker.Rate = value; }
        double ISymbolInfo2.Ask { get => AskTracker.Rate ?? 0; set => AskTracker.Rate = value; }

        string ISymbolInfo2.MarginCurrency => BaseCurrency.Name;

        string ISymbolInfo2.ProfitCurrency => QuoteCurrency.Name; //??? maybe, QuoteCurrency == CounterCurrency

        double ISymbolInfo2.StopOrderMarginReduction => Descriptor.Margin.StopOrderReduction ?? 1;

        double ISymbolInfo2.HiddenLimitOrderMarginReduction => Descriptor.Margin.HiddenLimitOrderReduction ?? 1;

        double ISymbolInfo2.MarginHedged => Descriptor.Margin.Hedged;

        int ISymbolInfo2.Digits => Descriptor.Digits;

        MarginInfo.Types.CalculationMode ISymbolInfo2.MarginMode => Descriptor.Margin.Mode;

        double ISymbolInfo2.MarginFactorFractional => Descriptor.Margin.Factor;

        double ISymbolInfo2.ContractSizeFractional => Descriptor.LotSize;

        SwapInfo.Types.Type ISymbolInfo2.SwapType => Descriptor.Swap.Type;

        int ISymbolInfo2.TripleSwapDay => Descriptor.Swap.TripleSwapDay;

        bool ISymbolInfo2.SwapEnabled => Descriptor.Swap.Enabled;

        double ISymbolInfo2.SwapSizeLong => Descriptor.Swap.SizeLong ?? 0;

        double ISymbolInfo2.SwapSizeShort => Descriptor.Swap.SizeShort ?? 0;

        #endregion ISymbolInfo
    }
}
