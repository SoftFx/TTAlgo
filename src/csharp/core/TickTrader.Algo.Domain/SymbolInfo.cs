using System;

namespace TickTrader.Algo.Domain
{
    public partial class SymbolInfo : ISymbolInfo
    {
        public double Bid { get; set; }
        public double Ask { get; set; }


        public IQuoteInfo LastQuote { get; private set; }

        public event Action<ISymbolInfo> RateUpdated;

        public void Update(ISymbolInfo newInfo)
        {
        }

        public void UpdateRate(IQuoteInfo quote)
        {
            LastQuote = quote;
            Bid = quote.Bid;
            Ask = quote.Ask;

            RateUpdated?.Invoke(this);
        }

        string ISymbolInfo.MarginCurrency => BaseCurrency;

        string ISymbolInfo.ProfitCurrency => CounterCurrency; // QuoteCurrency.Name; //??? maybe, QuoteCurrency == CounterCurrency

        double ISymbolInfo.Point => Math.Pow(10, -Digits);

        double ISymbolInfo.DefaultSlippage => Slippage.DefaultValue ?? 0;

        double ISymbolInfo.StopOrderMarginReduction => Margin.StopOrderReduction ?? 1;

        double ISymbolInfo.HiddenLimitOrderMarginReduction => Margin.HiddenLimitOrderReduction ?? 1;

        double ISymbolInfo.MarginHedged => Margin.Hedged;

        public int ProfitDigits => 2; //redone

        MarginInfo.Types.CalculationMode ISymbolInfo.MarginMode => Margin.Mode;

        double ISymbolInfo.MarginFactorFractional => Margin.Factor;

        double ISymbolInfo.ContractSizeFractional => LotSize;

        SwapInfo.Types.Type ISymbolInfo.SwapType => Swap.Type;

        int ISymbolInfo.TripleSwapDay => Swap.TripleSwapDay;

        bool ISymbolInfo.SwapEnabled => Swap.Enabled;

        double ISymbolInfo.SwapSizeLong => Swap.SizeLong ?? 0;

        double ISymbolInfo.SwapSizeShort => Swap.SizeShort ?? 0;
    }

    public interface ISymbolInfo : IBaseSymbolInfo
    {
        double DefaultSlippage { get; }
        double Point { get; }
        double Bid { get; set; }
        double Ask { get; set; }
        int Digits { get; }
        double LotSize { get; }

        Domain.MarginInfo.Types.CalculationMode MarginMode { get; }
        double MarginFactorFractional { get; }
        double MarginHedged { get; }
        string MarginCurrency { get; }
        string ProfitCurrency { get; }
        double ContractSizeFractional { get; }

        Domain.SwapInfo.Types.Type SwapType { get; }
        int TripleSwapDay { get; }
        bool SwapEnabled { get; }
        double SwapSizeLong { get; }
        double SwapSizeShort { get; }

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
