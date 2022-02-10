namespace TickTrader.FeedStorage.Api
{
    public interface ICustomInfo
    {
        string Name { get; }

        string Description { get; }

        double LotSize { get; }

        int Digits { get; }

        int Slippage { get; }



        double MaxVolume { get; }

        double MinVolume { get; }

        double VolumeStep { get; }



        bool SwapEnabled { get; }

        Algo.Domain.SwapInfo.Types.Type SwapType { get; }

        double SwapSizeShort { get; }

        double SwapSizeLong { get; }

        int TripleSwapDay { get; }


        string ProfitCurrency { get; }

        Algo.Domain.ProfitInfo.Types.CalculationMode ProfitMode { get; }


        string MarginCurrency { get; } //BaseCurrency

        Algo.Domain.MarginInfo.Types.CalculationMode MarginMode { get; }

        double MarginHedged { get; }

        double MarginFactor { get; }

        double StopOrderMarginReduction { get; }

        double HiddenLimitOrderMarginReduction { get; }



        double Commission { get; }

        Algo.Domain.CommissonInfo.Types.ValueType CommissionType { get; }

        double LimitsCommission { get; }

        double MinCommission { get; }

        string MinCommissionCurr { get; }
    }
}
