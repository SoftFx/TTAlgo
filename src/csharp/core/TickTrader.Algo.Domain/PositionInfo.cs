using System;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Domain
{
    public partial class PositionInfo : IPositionInfo, IMarginCalculateRequest, IProfitCalculateRequest, ISwapCalculateRequest
    {
        public ISymbolCalculator Calculator { get; set; }


        public OrderInfo.Types.Type Type => OrderInfo.Types.Type.Position;

        public bool IsEmpty => Math.Abs(Volume) < 1e-9;


        bool IMarginProfitCalc.IsHidden => false;

        double IMarginProfitCalc.RemainingAmount => Volume;


        double IMarginCalculateRequest.Volume => Volume;

        bool IMarginCalculateRequest.IsHiddenLimit => false;

        double IProfitCalculateRequest.Price => Price;

        OrderInfo.Types.Side IProfitCalculateRequest.Side => Side;

        double IProfitCalculateRequest.Volume => Volume;

        double? IProfitCalculateRequest.ClosePrice => null;

        OrderInfo.Types.Side ISwapCalculateRequest.Side => Side;

        double ISwapCalculateRequest.Volume => Volume;


        public string GetSnapshotString() => ToString();
    }

    public interface IPositionInfo : IMarginProfitCalc
    {
        double Volume { get; }

        string Symbol { get; }

        double Commission { get; }

        double Swap { get; }

        ISymbolCalculator Calculator { get; set; }

        bool IsEmpty { get; }


        string GetSnapshotString();
    }
}
