using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using ISymbolInfo = TickTrader.BusinessObjects.ISymbolInfo;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class SymbolModel : ISymbolInfo, TickTrader.Algo.Common.Model.Setup.ISymbolInfo
    {
        private IFeedSubscription subscription;

        public SymbolModel(QuoteDistributor distributor, SymbolInfo info, IReadOnlyDictionary<string, CurrencyInfo> currencies)
        {
            Descriptor = info;
            Distributor = distributor;

            subscription = distributor.Subscribe(info.Name);
            subscription.NewQuote += OnNewTick;

            BaseCurrency = currencies.Read(info.Currency);
            QuoteCurrency = currencies.Read(info.SettlementCurrency);

            BaseCurrencyDigits = BaseCurrency?.Precision ?? 2;
            QuoteCurrencyDigits = QuoteCurrency?.Precision ?? 2;
        }

        public string Name { get { return Descriptor.Name; } }
        public string Description => Descriptor.Description;
        public bool IsUserCreated => false;
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
            get { return FdkToAlgo.Convert(Descriptor.MarginCalcMode); }
        }

        #endregion

        public event Action<SymbolModel> InfoUpdated = delegate { };
        public event Action<SymbolModel> RateUpdated = delegate { };

        public virtual void Close()
        {
            subscription.Dispose();
        }

        public virtual void Update(SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(this);
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

        public SymbolEntity GetAlgoSymbolInfo()
        {
            return FdkToAlgo.Convert(Descriptor);
        }
    }
}
