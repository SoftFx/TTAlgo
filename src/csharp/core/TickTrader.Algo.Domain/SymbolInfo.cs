using System;

namespace TickTrader.Algo.Domain
{
    public partial class SymbolInfo : ISymbolInfo
    {
        public double Bid { get; private set; } = double.NaN;

        public double Ask { get; private set; } = double.NaN;


        public IQuoteInfo LastQuote { get; private set; }

        public event Action<ISymbolInfo> RateUpdated;

        public void Update(ISymbolInfo newInfo)
        {
            if (!(newInfo is SymbolInfo info))
                return;

            //Name = info.Name; //is key in symbol dicts
            TradeAllowed = info.TradeAllowed;
            BaseCurrency = info.BaseCurrency;
            CounterCurrency = info.CounterCurrency;
            Digits = info.Digits;
            LotSize = info.LotSize;
            MinTradeVolume = info.MinTradeVolume;
            MaxTradeVolume = info.MaxTradeVolume;
            TradeVolumeStep = info.TradeVolumeStep;
            Slippage = info.Slippage;
            Commission = info.Commission;
            Margin = info.Margin;
            Swap = info.Swap;
            Description = info.Description;
            Security = info.Security;
            SortOrder = info.SortOrder;
            GroupSortOrder = info.GroupSortOrder;

            UpdateRate(info.LastQuote);
        }

        public void UpdateRate(IQuoteInfo quote)
        {
            LastQuote = quote;
            Bid = quote?.Bid ?? double.NaN;
            Ask = quote?.Ask ?? double.NaN;

            RateUpdated?.Invoke(this);
        }

        bool ISymbolInfo.HasBid => LastQuote?.HasBid ?? false;
        bool ISymbolInfo.HasAsk => LastQuote?.HasAsk ?? false;

        string ISymbolInfo.MarginCurrency => BaseCurrency;

        string ISymbolInfo.ProfitCurrency => CounterCurrency; // QuoteCurrency.Name; //??? maybe, QuoteCurrency == CounterCurrency

        string ISymbolInfo.NodeKey => $"{BaseCurrency}{CounterCurrency}";

        double ISymbolInfo.Point => Math.Pow(10, -Digits);

        double ISymbolInfo.DefaultSlippage => Slippage.DefaultValue ?? 0;

        double ISymbolInfo.StopOrderMarginReduction => Margin.StopOrderReduction ?? 1;

        double ISymbolInfo.HiddenLimitOrderMarginReduction => Margin.HiddenLimitOrderReduction ?? 1;

        double ISymbolInfo.MarginHedged => Margin.Hedged;

        public int ProfitDigits => 2; //redone

        MarginInfo.Types.CalculationMode ISymbolInfo.MarginMode => Margin.Mode;

        double ISymbolInfo.MarginFactor => Margin.Factor;

        double ISymbolInfo.ContractSizeFractional => LotSize;

        SwapInfo.Types.Type ISymbolInfo.SwapType => Swap.Type;

        int ISymbolInfo.TripleSwapDay => Swap.TripleSwapDay;

        bool ISymbolInfo.SwapEnabled => Swap.Enabled;

        double ISymbolInfo.SwapSizeLong => Swap.SizeLong ?? 0;

        double ISymbolInfo.SwapSizeShort => Swap.SizeShort ?? 0;

        CommissonInfo.Types.ValueType ISymbolInfo.CommissionType => Commission.ValueType;

        double ISymbolInfo.Commission => Commission.Commission;

        double ISymbolInfo.LimitsCommission => Commission.LimitsCommission;

        double ISymbolInfo.MinCommission => Commission.MinCommission;

        string ISymbolInfo.MinCommissionCurr => Commission.MinCommissionCurrency;
    }

    public interface ISymbolInfo : IBaseSymbolInfo
    {
        double DefaultSlippage { get; }
        double Point { get; }
        double Bid { get; }
        double Ask { get; }
        bool HasBid { get; }
        bool HasAsk { get; }
        int Digits { get; }
        double LotSize { get; }

        Domain.MarginInfo.Types.CalculationMode MarginMode { get; }
        double MarginFactor { get; }
        double MarginHedged { get; }
        string MarginCurrency { get; }
        string ProfitCurrency { get; }
        string NodeKey { get; }
        double ContractSizeFractional { get; }

        Domain.SwapInfo.Types.Type SwapType { get; }
        int TripleSwapDay { get; }
        bool SwapEnabled { get; }
        double SwapSizeLong { get; }
        double SwapSizeShort { get; }

        CommissonInfo.Types.ValueType CommissionType { get; }
        double Commission { get; }
        double LimitsCommission { get; }
        double MinCommission { get; }
        string MinCommissionCurr { get; }


        double StopOrderMarginReduction { get; }
        double HiddenLimitOrderMarginReduction { get; }

        void UpdateRate(IQuoteInfo quote); //Update Ask, Bid, LastQuote

        event Action<ISymbolInfo> RateUpdated;

        void Update(ISymbolInfo newInfo);
        IQuoteInfo LastQuote { get; }
        int ProfitDigits { get; }
    }

    public interface IBaseSymbolInfo
    {
        string Name { get; }

        int SortOrder { get; }

        int GroupSortOrder { get; }
    }
}
