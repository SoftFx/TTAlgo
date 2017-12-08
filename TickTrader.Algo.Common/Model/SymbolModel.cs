﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using ISymbolInfo = TickTrader.BusinessObjects.ISymbolInfo;
using BO = TickTrader.BusinessObjects;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public class SymbolModel : BO.ISymbolInfo, Setup.ISymbolInfo
    {
        private IFeedSubscription subscription;

        public SymbolModel(QuoteDistributor distributor, SymbolEntity info, IReadOnlyDictionary<string, CurrencyEntity> currencies)
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
        public SymbolEntity Descriptor { get; private set; }
        public int PriceDigits { get { return Descriptor.Precision; } }
        public int BaseCurrencyDigits { get; private set; }
        public int QuoteCurrencyDigits { get; private set; }
        public CurrencyEntity BaseCurrency { get; private set; }
        public CurrencyEntity QuoteCurrency { get; private set; }
        public int Depth { get; private set; }
        public int RequestedDepth { get; private set; }
        public QuoteEntity LastQuote { get; private set; }
        public double? CurrentAsk { get; private set; }
        public double? CurrentBid { get; private set; }
        public double LotSize { get { return Descriptor.RoundLot; } }
        public double StopOrderMarginReduction => Descriptor.StopOrderMarginReduction;

        protected QuoteDistributor Distributor { get; private set; }

        #region BO ISymbolInfo

        string BO.ISymbolInfo.Security => Descriptor.SecurityName;
        int BO.ISymbolInfo.SortOrder => Descriptor.SortOrder;
        BO.SwapType BO.ISymbolInfo.SwapType => FdkToAlgo.Convert(Descriptor.SwapType);
        int BO.ISymbolInfo.TripleSwapDay => Descriptor.TripleSwapDay;
        double BO.ISymbolInfo.HiddenLimitOrderMarginReduction => Descriptor.HiddenLimitOrderMarginReduction ?? 1;
        BO.MarginCalculationModes BO.ISymbolInfo.MarginMode => FdkToAlgo.Convert(Descriptor.MarginCalcMode);
        string ISymbolInfo.Symbol { get { return Name; } }
        double ISymbolInfo.ContractSizeFractional { get { return Descriptor.RoundLot; } }
        string ISymbolInfo.MarginCurrency { get { return Descriptor.Currency; } }
        string ISymbolInfo.ProfitCurrency { get { return Descriptor.SettlementCurrency; } }
        double ISymbolInfo.MarginFactorFractional { get { return Descriptor.MarginFactorFractional; } }
        double ISymbolInfo.MarginHedged { get { return Descriptor.MarginHedged; } }
        int ISymbolInfo.Precision { get { return Descriptor.Precision; } }
        bool ISymbolInfo.SwapEnabled { get { return true; } }
        float ISymbolInfo.SwapSizeLong { get { return (float)Descriptor.SwapSizeLong; } }
        float ISymbolInfo.SwapSizeShort { get { return (float)Descriptor.SwapSizeShort; } }
        string ISymbolInfo.Security { get { return ""; } }
        int ISymbolInfo.SortOrder { get { return 0; } }

        TickTrader.BusinessObjects.MarginCalculationModes ISymbolInfo.MarginMode => Descriptor.MarginMode;

        #endregion

        public event Action<SymbolModel> InfoUpdated = delegate { };
        public event Action<SymbolModel> RateUpdated = delegate { };

        public virtual void Close()
        {
            subscription.Dispose();
        }

        public virtual void Update(SymbolEntity newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(this);
        }

        protected virtual void OnNewTick(QuoteEntity tick)
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
            return Descriptor;
        }
    }
}
