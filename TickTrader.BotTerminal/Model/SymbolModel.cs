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
        private SymbolCollectionModel container;
        private List<IRateUpdatesListener> listeners = new List<IRateUpdatesListener>();

        public SymbolModel(SymbolCollectionModel container, SymbolInfo info, IDictionary<string, CurrencyInfo> currencies)
        {
            this.container = container;
            this.Descriptor = info;
            this.Amounts = new OrderAmountModel(info);
            this.PredefinedAmounts = Amounts.GetPredefined();

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
        public OrderAmountModel Amounts { get; private set; }
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

        public void Update(SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(newInfo);
        }

        public void CastNewTick(Quote tick)
        {
            LastQuote = tick;

            CurrentBid = LastQuote.HasBid ? LastQuote.Bid : (double?)null;
            CurrentAsk = LastQuote.HasAsk ? LastQuote.Ask : (double?)null;

            listeners.ForEach(l => l.OnRateUpdate(tick));
        }

        public void Subscribe(IRateUpdatesListener listener)
        {
            this.listeners.Add(listener);
            listener.DepthChanged += listener_DepthChanged;
            UpdateRequestedSubscription();
        }

        public void Unsubscribe(IRateUpdatesListener listener)
        {
            this.listeners.Remove(listener);
            listener.DepthChanged -= listener_DepthChanged;
            UpdateRequestedSubscription();
        }

        public bool ValidateAmmount(decimal amount, decimal minVolume, decimal maxVolume, decimal step)
        {
            return amount <= maxVolume && amount >= minVolume
                && (amount / step) % 1 == 0;
        }

        public void Reset()
        {
            Depth = RequestedDepth;
        }

        private void UpdateRequestedSubscription()
        {
            if (listeners.Count == 0)
                RequestedDepth = 1;
            else
                RequestedDepth = GetMaxDepth();

            if (RequestedDepth != Depth)
            {
                container.EnqueueSubscriptionRequest(RequestedDepth, Name);
                Depth = RequestedDepth;
            }
        }

        private int GetMaxDepth()
        {
            int max = 0;
            foreach (var l in listeners)
            {
                if (l.Depth == 0)
                    return 0;
                if (l.Depth > max)
                    max = l.Depth;
            }
            return max;
        }

        private void listener_DepthChanged()
        {
            UpdateRequestedSubscription();
        }
    }

    internal interface IRateUpdatesListener
    {
        int Depth { get; }
        event Action DepthChanged;
        void OnRateUpdate(Quote tick);
    }
}
