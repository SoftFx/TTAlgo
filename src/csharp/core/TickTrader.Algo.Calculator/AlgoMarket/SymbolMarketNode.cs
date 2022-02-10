using TickTrader.Algo.Calculator.Conversions;
using TickTrader.Algo.Calculator.TradeSpecificsCalculators;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.AlgoMarket
{
    public class SymbolMarketNode : ISymbolCalculator, IProfitCalculationInfo, IMarginCalculationInfo, ISwapCalculationInfo, ICommissionCalculationInfo
    {
        private readonly IMarketStateAccountInfo _account;


        public ISideNode Bid { get; }

        public ISideNode Ask { get; }


        public IMarginCalculator Margin { get; private set; }

        public IProfitCalculator Profit { get; private set; }

        public ISwapCalculator Swap { get; private set; }

        public ICommissionCalculator Commission { get; private set; }


        public SymbolMarketNode(IMarketStateAccountInfo acc, ISymbolInfoWithRate smb)
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
            Swap = new SwapCalculator(this, marginFormula, positiveFormula, negativeFormula);
        }

        public ISymbolInfoWithRate SymbolInfo { get; private set; }

        public bool IsShadowCopy { get; private set; }

        public IFeedSubscription UserSubscriptionInfo { get; set; }

        public SubscriptionGroup SubGroup { get; set; }


        SymbolInfo ISymbolCalculator.SymbolInfo => (SymbolInfo)SymbolInfo;

        int IMarginCalculationInfo.Leverage => SymbolInfo.MarginMode.IsLeverageMode() ? _account.Leverage : 1;

        double IMarginCalculationInfo.Factor => SymbolInfo.MarginFactor;

        double? IMarginCalculationInfo.StopOrderReduction => SymbolInfo.StopOrderMarginReduction;

        double? IMarginCalculationInfo.HiddenLimitOrderReduction => SymbolInfo.HiddenLimitOrderMarginReduction;

        bool ISwapCalculationInfo.Enabled => SymbolInfo.SwapEnabled;

        SwapInfo.Types.Type ISwapCalculationInfo.Type => SymbolInfo.SwapType;

        int ISwapCalculationInfo.TripleSwapDay => SymbolInfo.TripleSwapDay;

        double? ISwapCalculationInfo.SwapSizeLong => SymbolInfo.SwapSizeLong;

        double? ISwapCalculationInfo.SwapSizeShort => SymbolInfo.SwapSizeShort;

        int ISwapCalculationInfo.SymbolDigits => SymbolInfo.Digits;

        CommissonInfo.Types.ValueType ICommissionCalculationInfo.Type => SymbolInfo.CommissionType;

        double ICommissionCalculationInfo.LotSize => SymbolInfo.LotSize;

        double ICommissionCalculationInfo.SymbolDigits => SymbolInfo.Digits;

        double ICommissionCalculationInfo.TakerFee => SymbolInfo.Commission;

        double ICommissionCalculationInfo.MakerFee => SymbolInfo.LimitsCommission;

        double ICommissionCalculationInfo.MinCommission => SymbolInfo.MinCommission;

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
