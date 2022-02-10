using System;

namespace TickTrader.Algo.Domain
{
    public partial class SymbolInfo : ISymbolInfoWithRate
    {
        public double Bid { get; private set; } = double.NaN;

        public double Ask { get; private set; } = double.NaN;

        bool ISymbolInfoWithRate.HasBid => LastQuote?.HasBid ?? false;

        bool ISymbolInfoWithRate.HasAsk => LastQuote?.HasAsk ?? false;


        public IQuoteInfo LastQuote { get; private set; }

        public event Action<ISymbolInfo> RateUpdated;


        public void Update(SymbolInfo info)
        {
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
            Profit = info.Profit;
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

        double ISymbolInfo.MaxVolume => MaxTradeVolume;

        double ISymbolInfo.MinVolume => MinTradeVolume;

        double ISymbolInfo.VolumeStep => TradeVolumeStep;


        string ISymbolInfo.MarginCurrency => BaseCurrency;

        MarginInfo.Types.CalculationMode ISymbolInfo.MarginMode => Margin.Mode;

        double ISymbolInfo.StopOrderMarginReduction => Margin.StopOrderReduction ?? 1;

        double ISymbolInfo.HiddenLimitOrderMarginReduction => Margin.HiddenLimitOrderReduction ?? 1;

        double ISymbolInfo.MarginHedged => Margin.Hedged;

        double ISymbolInfo.MarginFactor => Margin.Factor;


        string ISymbolInfo.ProfitCurrency => CounterCurrency;

        ProfitInfo.Types.CalculationMode ISymbolInfo.ProfitMode => Profit.Mode;


        bool ISymbolInfo.SwapEnabled => Swap.Enabled;

        SwapInfo.Types.Type ISymbolInfo.SwapType => Swap.Type;

        int ISymbolInfo.TripleSwapDay => Swap.TripleSwapDay;

        double ISymbolInfo.SwapSizeLong => Swap.SizeLong ?? 0;

        double ISymbolInfo.SwapSizeShort => Swap.SizeShort ?? 0;


        CommissonInfo.Types.ValueType ISymbolInfo.CommissionType => Commission.ValueType;

        double ISymbolInfo.Commission => Commission.Commission;

        double ISymbolInfo.LimitsCommission => Commission.LimitsCommission;

        double ISymbolInfo.MinCommission => Commission.MinCommission;

        string ISymbolInfo.MinCommissionCurr => Commission.MinCommissionCurrency;
    }


    public interface IBaseSymbolInfo
    {
        string Name { get; }

        int SortOrder { get; }

        int GroupSortOrder { get; }
    }


    public interface ISymbolInfo : IBaseSymbolInfo
    {
        string Description { get; }

        string Security { get; }

        double LotSize { get; }

        int Digits { get; }


        double MaxVolume { get; }

        double MinVolume { get; }

        double VolumeStep { get; }


        string MarginCurrency { get; }

        MarginInfo.Types.CalculationMode MarginMode { get; }

        double MarginFactor { get; }

        double MarginHedged { get; }

        double StopOrderMarginReduction { get; }

        double HiddenLimitOrderMarginReduction { get; }


        string ProfitCurrency { get; }

        ProfitInfo.Types.CalculationMode ProfitMode { get; }


        bool SwapEnabled { get; }

        SwapInfo.Types.Type SwapType { get; }

        int TripleSwapDay { get; }

        double SwapSizeLong { get; }

        double SwapSizeShort { get; }


        double Commission { get; }

        CommissonInfo.Types.ValueType CommissionType { get; }

        double LimitsCommission { get; }

        double MinCommission { get; }

        string MinCommissionCurr { get; }
    }

    public interface ISymbolInfoWithRate : ISymbolInfo
    {
        double Bid { get; }

        double Ask { get; }

        bool HasBid { get; }

        bool HasAsk { get; }

        IQuoteInfo LastQuote { get; }


        event Action<ISymbolInfo> RateUpdated;


        void Update(SymbolInfo newInfo);

        void UpdateRate(IQuoteInfo quote); //Update Ask, Bid, LastQuote
    }
}
