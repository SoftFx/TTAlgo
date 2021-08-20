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


        public PositionInfo BuildPositionSides()
        {
            if (Long == null && Side.IsBuy())
            {
                Long = new PositionSide(Volume, Price);
                Short = new PositionSide(0, 0);
            }
            else
            if (Short == null && Side.IsSell())
            {
                Long = new PositionSide(0, 0);
                Short = new PositionSide(Volume, Price);
            }

            return this;
        }
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

    public class PositionSide : IPositionSide
    {
        private double margin;
        private double profit;

        public double Amount { get; set; }
        public double Price { get; set; }

        public PositionSide(double amount, double price)
        {
            Amount = amount;
            Price = price;
        }

        public double Margin
        {
            get { return margin; }
            set
            {
                if (margin != value)
                {
                    margin = value;
                    MarginUpdated?.Invoke();
                }
            }
        }

        public double Profit
        {
            get { return profit; }
            set
            {
                if (profit != value)
                {
                    profit = value;
                    ProfitUpdated?.Invoke();
                }
            }
        }

        public System.Action ProfitUpdated;
        public System.Action MarginUpdated;
    }

    public interface IPositionSide
    {
        double Amount { get; }
        double Price { get; }
        double Margin { get; set; }
        double Profit { get; set; }
    }
}
