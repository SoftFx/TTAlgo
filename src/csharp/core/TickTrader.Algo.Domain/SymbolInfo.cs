using System;

namespace TickTrader.Algo.Domain
{
    public partial class SymbolInfo : ISymbolInfoWithRate
    {
        public double? Bid { get; private set; }

        public double? Ask { get; private set; }


        double ISymbolInfoWithRate.Bid => Bid ?? double.NaN;

        double ISymbolInfoWithRate.Ask => Ask ?? double.NaN;

        bool ISymbolInfoWithRate.HasBid => LastQuote?.HasBid ?? false;

        bool ISymbolInfoWithRate.HasAsk => LastQuote?.HasAsk ?? false;


        public IQuoteInfo LastQuote { get; private set; }

        public event Action<ISymbolInfo> RateUpdated;


        public SymbolInfo(ISymbolInfo info)
        {
            Name = info.Name;
            BaseCurrency = info.MarginCurrency;
            CounterCurrency = info.ProfitCurrency;
            Digits = info.Digits;
            LotSize = info.LotSize;
            Description = info.Description;
            TradeAllowed = info.TradeAllowed;

            MinTradeVolume = info.MinVolume;
            MaxTradeVolume = info.MaxVolume;
            TradeVolumeStep = info.VolumeStep;

            Slippage = new SlippageInfo
            {
                DefaultValue = info.Slippage,
            };

            Commission = new CommissonInfo
            {
                Commission = info.Commission,
                LimitsCommission = info.LimitsCommission,
                ValueType = info.CommissionType,
                MinCommission = info.MinCommission,
                MinCommissionCurrency = info.MinCommissionCurr,
            };

            Swap = new SwapInfo
            {
                Enabled = info.SwapEnabled,
                Type = info.SwapType,
                SizeLong = info.SwapSizeLong,
                SizeShort = info.SwapSizeShort,
                TripleSwapDay = info.TripleSwapDay,
            };

            Margin = new MarginInfo
            {
                Mode = info.MarginMode,
                Hedged = info.MarginHedged,
                Factor = info.MarginFactor,
                StopOrderReduction = info.StopOrderMarginReduction,
                HiddenLimitOrderReduction = info.HiddenLimitOrderMarginReduction,
            };
        }

        public ISymbolInfoWithRate Update(SymbolInfo info)
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
            Swap = info.Swap;
            Description = info.Description;
            Security = info.Security;
            SortOrder = info.SortOrder;
            GroupSortOrder = info.GroupSortOrder;

            return UpdateRate(info.LastQuote);
        }

        public ISymbolInfoWithRate UpdateRate(IQuoteInfo quote)
        {
            Bid = quote?.Bid;
            Ask = quote?.Ask;

            LastQuote = quote;
            RateUpdated?.Invoke(this);

            return this;
        }


        int ISymbolInfo.Slippage => (int)(Slippage.DefaultValue ?? 0);


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

        int Slippage { get; }

        bool TradeAllowed { get; }


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


        ISymbolInfoWithRate Update(SymbolInfo info);

        ISymbolInfoWithRate UpdateRate(IQuoteInfo quote); //Update Ask, Bid, LastQuote
    }
}
