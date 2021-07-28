﻿using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.AlgoMarket
{
    public class SymbolMarketNode : ISymbolCalculator, IProfitCalculationInfo, IMarginCalculationInfo
    {
        private readonly IMarketStateAccountInfo _account;


        public ISideNode Bid { get; }

        public ISideNode Ask { get; }


        public IMarginCalculator Margin { get; private set; }

        public IProfitCalculator Profit { get; private set; }


        public SymbolMarketNode(IMarketStateAccountInfo acc, ISymbolInfo smb)
        {
            _account = acc;

            SymbolInfo = smb;

            Bid = new BidSideNode(smb);
            Ask = new AskSideNode(smb);
        }

        public void InitCalculators(ConversionManager conversion)
        {
            var marginFormula = conversion.GetMarginFormula(SymbolInfo);
            var positiveFormula = conversion.GetPositiveProfitFormula(SymbolInfo);
            var negativeFormula = conversion.GetNegativeProfitFormula(SymbolInfo);

            Margin = new MarginCalculator(this, marginFormula);
            Profit = new ProfitCalculator(this, positiveFormula, negativeFormula);
        }

        public ISymbolInfo SymbolInfo { get; private set; }

        public bool IsShadowCopy { get; private set; }

        public IFeedSubscription UserSubscriptionInfo { get; set; }

        public SubscriptionGroup SubGroup { get; set; }


        SymbolInfo ISymbolCalculator.SymbolInfo => (SymbolInfo)SymbolInfo;

        int IMarginCalculationInfo.Leverage => SymbolInfo.MarginMode.IsLeverageMode() ? _account.Leverage : 1;

        double IMarginCalculationInfo.Factor => SymbolInfo.MarginFactor;

        double? IMarginCalculationInfo.StopOrderReduction => SymbolInfo.StopOrderMarginReduction;

        double? IMarginCalculationInfo.HiddenLimitOrderReduction => SymbolInfo.HiddenLimitOrderMarginReduction;


        public void Update(SymbolInfo smb)
        {
            IsShadowCopy = smb == null;

            if (smb != null)
                SymbolInfo = smb;

            Ask.Subscribe(smb);
            Bid.Subscribe(smb);
        }
    }
}