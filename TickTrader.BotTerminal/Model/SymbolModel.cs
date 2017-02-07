using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;
using ISymbolInfo = TickTrader.BusinessObjects.ISymbolInfo;

namespace TickTrader.BotTerminal
{
    internal class SymbolModel : ISymbolInfo
    {
        private object lockObj = new object();
        private QuoteDistributor distributor;
        private IFeedSubscription subscription;

        public SymbolModel(QuoteDistributor distributor, SymbolInfo info, IDictionary<string, CurrencyInfo> currencies)
        {
            this.distributor = distributor;
            this.Descriptor = info;
            this.Amounts = new OrderAmountModel(info);
            this.PredefinedAmounts = Amounts.GetPredefined();

            subscription = distributor.Subscribe(info.Name);
            subscription.NewQuote += OnNewTick;

            BaseCurrency = currencies.GetOrDefault(info.Currency);
            QuoteCurrency = currencies.GetOrDefault(info.SettlementCurrency);

            BaseCurrencyDigits = BaseCurrency?.Precision ?? 2;
            QuoteCurrencyDigits = QuoteCurrency?.Precision ?? 2;

            BidTracker = new RateDirectionTracker();
            AskTracker = new RateDirectionTracker();

            BidTracker.Precision = info.Precision;
            AskTracker.Precision = info.Precision;
        }

        public string Name { get { return Descriptor.Name; } }
        public SymbolInfo Descriptor { get; private set; }
        public int PriceDigits { get { return Descriptor.Precision; } }
        public int BaseCurrencyDigits { get; private set; }
        public int QuoteCurrencyDigits { get; private set; }
        public CurrencyInfo BaseCurrency { get; private set; }
        public CurrencyInfo QuoteCurrency { get; private set; }
        public int Depth { get; private set; }
        public int RequestedDepth { get; private set; }
        public Quote LastQuote { get; private set; }
        public double? CurrentAsk { get; private set; }
        public double? CurrentBid { get; private set; }
        public double LotSize { get { return Descriptor.RoundLot; } }
        public OrderAmountModel Amounts { get; private set; }
        public RateDirectionTracker BidTracker { get; private set; }
        public RateDirectionTracker AskTracker { get; private set; }
        public List<decimal> PredefinedAmounts { get; private set; }

        #region ISymbolInfo

        string ISymbolInfo.Symbol { get { return Name; } }
        double ISymbolInfo.ContractSizeFractional { get { return Descriptor.RoundLot; } }
        string ISymbolInfo.MarginCurrency { get { return Descriptor.Currency; } }
        string ISymbolInfo.ProfitCurrency { get { return Descriptor.SettlementCurrency; } }
        double ISymbolInfo.MarginFactorFractional { get { return Descriptor.MarginFactorFractional ?? 1; } }
        double ISymbolInfo.MarginHedged { get { return Descriptor.MarginHedge; } }
        int ISymbolInfo.Precision { get { return Descriptor.Precision; } }
        bool ISymbolInfo.SwapEnabled { get { return true; } }
        float ISymbolInfo.SwapSizeLong { get { return (float)Descriptor.SwapSizeLong; } }
        float ISymbolInfo.SwapSizeShort { get { return (float)Descriptor.SwapSizeShort; } }
        string ISymbolInfo.Security { get { return ""; } }
        int ISymbolInfo.SortOrder { get { return 0; } }

        TickTrader.BusinessObjects.MarginCalculationModes ISymbolInfo.MarginMode
        {
            get
            {
                switch (Descriptor.MarginCalcMode)
                {
                    case MarginCalcMode.Cfd: return BusinessObjects.MarginCalculationModes.CFD;
                    case MarginCalcMode.CfdIndex: return BusinessObjects.MarginCalculationModes.CFD_Index;
                    case MarginCalcMode.CfdLeverage: return BusinessObjects.MarginCalculationModes.CFD_Leverage;
                    case MarginCalcMode.Forex: return BusinessObjects.MarginCalculationModes.Forex;
                    case MarginCalcMode.Futures: return BusinessObjects.MarginCalculationModes.Futures;
                    default: throw new NotImplementedException();
                }
            }
        }

        #endregion

        public event Action<SymbolInfo> InfoUpdated = delegate { };
        public event Action<SymbolModel> RateUpdated = delegate { };

        public void Close()
        {
            subscription.Dispose();
        }

        public IFeedSubscription Subscribe(int depth = 1)
        {
            return distributor.Subscribe(Name, depth);
        }

        public void Update(SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(newInfo);

            BidTracker.Precision = newInfo.Precision;
            AskTracker.Precision = newInfo.Precision;
        }

        private void OnNewTick(Quote tick)
        {
            LastQuote = tick;

            CurrentBid = LastQuote.HasBid ? LastQuote.Bid : (double?)null;
            CurrentAsk = LastQuote.HasAsk ? LastQuote.Ask : (double?)null;

            if (tick.HasBid)
                BidTracker.Rate = tick.Bid;

            if (tick.HasAsk)
                AskTracker.Rate = tick.Ask;

            RateUpdated(this);
        }

        public bool ValidateAmmount(decimal amount, decimal minVolume, decimal maxVolume, decimal step)
        {
            return amount <= maxVolume && amount >= minVolume
                && (amount / step) % 1 == 0;
        }
    }
}
