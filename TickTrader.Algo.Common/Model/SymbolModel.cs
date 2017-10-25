using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public class SymbolModel : BO.ISymbolInfo, Setup.ISymbolInfo
    {
        private IFeedSubscription subscription;

        public SymbolModel(QuoteDistributor distributor, SymbolInfo info, IDictionary<string, CurrencyInfo> currencies)
        {
            Descriptor = info;
            Distributor = distributor;

            subscription = distributor.Subscribe(info.Name);
            subscription.NewQuote += OnNewTick;

            BaseCurrency = currencies.GetOrDefault(info.Currency);
            QuoteCurrency = currencies.GetOrDefault(info.SettlementCurrency);

            BaseCurrencyDigits = BaseCurrency?.Precision ?? 2;
            QuoteCurrencyDigits = QuoteCurrency?.Precision ?? 2;
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
        public double StopOrderMarginReduction => Descriptor.StopOrderMarginReduction ?? 0;

        protected QuoteDistributor Distributor { get; private set; }

        #region BO ISymbolInfo

        string BO.ISymbolInfo.Symbol => Name;
        double BO.ISymbolInfo.ContractSizeFractional => Descriptor.RoundLot;
        string BO.ISymbolInfo.MarginCurrency => Descriptor.Currency;
        string BO.ISymbolInfo.ProfitCurrency => Descriptor.SettlementCurrency;
        double BO.ISymbolInfo.MarginFactorFractional => Descriptor.MarginFactorFractional ?? 1;
        double BO.ISymbolInfo.MarginHedged => Descriptor.MarginHedge;
        int BO.ISymbolInfo.Precision => Descriptor.Precision;
        bool BO.ISymbolInfo.SwapEnabled => true;
        float BO.ISymbolInfo.SwapSizeLong => (float)Descriptor.SwapSizeLong;
        float BO.ISymbolInfo.SwapSizeShort => (float)Descriptor.SwapSizeShort;
        string BO.ISymbolInfo.Security => Descriptor.SecurityName;
        int BO.ISymbolInfo.SortOrder => Descriptor.SortOrder;
        BO.SwapType BO.ISymbolInfo.SwapType => FdkToAlgo.Convert(Descriptor.SwapType);
        int BO.ISymbolInfo.TripleSwapDay => Descriptor.TripleSwapDay;
        double BO.ISymbolInfo.HiddenLimitOrderMarginReduction => Descriptor.HiddenLimitOrderMarginReduction ?? 1;
        BO.MarginCalculationModes BO.ISymbolInfo.MarginMode => FdkToAlgo.Convert(Descriptor.MarginCalcMode);

        #endregion

        public event Action<SymbolInfo> InfoUpdated = delegate { };
        public event Action<SymbolModel> RateUpdated = delegate { };

        public virtual void Close()
        {
            subscription.Dispose();
        }

        public virtual void Update(SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(newInfo);
        }

        protected virtual void OnNewTick(Quote tick)
        {
            LastQuote = tick;

            CurrentBid = tick.GetNullableBid();
            CurrentAsk = tick.GetNullableAsk();

            RateUpdated(this);
        }

        public bool ValidateAmmount(decimal amount, decimal minVolume, decimal maxVolume, decimal step)
        {
            return amount <= maxVolume && amount >= minVolume
                && (amount / step) % 1 == 0;
        }
    }
}
