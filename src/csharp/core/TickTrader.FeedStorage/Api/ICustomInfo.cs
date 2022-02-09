namespace TickTrader.FeedStorage.Api
{
    public interface ICustomInfo
    {
        string Name { get; }

        string Description { get; }

        string MarginCurrency { get; } //BaseCurrency

        string ProfitCurrency { get; }

        int Digits { get; }

        double LotSize { get; }

        int Slippage { get; }



        double MaxVolume { get; }

        double MinVolume { get; }

        double VolumeStep { get; }



        bool SwapEnabled { get; }

        Algo.Domain.SwapInfo.Types.Type SwapType { get; }

        double SwapSizeShort { get; }

        double SwapSizeLong { get; }

        int TripleSwapDay { get; }


        Algo.Domain.MarginInfo.Types.CalculationMode ProfitMode { get; }


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
