using System;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Domain
{
    public partial class PositionInfo : IPositionInfo, IMarginCalculateRequest, IProfitCalculateRequest
    {
        public bool IsEmpty => Math.Abs(Volume) < 1e-9;

        public IPositionSide Long { get; set; }

        public IPositionSide Short { get; set; }

        public ISymbolCalculator Calculator { get; set; }

        double IPositionInfo.Amount => Math.Max(Long.Amount, Short.Amount);

        DateTime? IPositionInfo.Modified => Modified?.ToDateTime();

        double IMarginProfitCalc.Price => (double)(Long.Amount > Short.Amount ? Long.Price : Short.Price);
        double IMarginProfitCalc.RemainingAmount => Math.Max(Long.Amount, Short.Amount);
        OrderInfo.Types.Type IMarginProfitCalc.Type => OrderInfo.Types.Type.Position;

        

        bool IMarginProfitCalc.IsHidden => false;

        OrderInfo.Types.Type IMarginCalculateRequest.Type => OrderInfo.Types.Type.Position;

        double IMarginCalculateRequest.Volume => Math.Max(Long.Amount, Short.Amount);

        bool IMarginCalculateRequest.IsHiddenLimit => false;


        public string GetSnapshotString() => ToString();
    }

    public interface IPositionInfo : IMarginProfitCalc
    {
        double Amount { get; }
        string Symbol { get; }
        double Commission { get; }
        double Swap { get; }
        IPositionSide Long { get; } // buy
        IPositionSide Short { get; } //sell
        DateTime? Modified { get; }
        ISymbolCalculator Calculator { get; set; }
        bool IsEmpty { get; }


        string GetSnapshotString();
    }

    public interface IPositionSide
    {
        double Amount { get; }
        double Price { get; }
        double Margin { get; set; }
        double Profit { get; set; }
    }
}
