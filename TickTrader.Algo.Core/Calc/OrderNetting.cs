using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    internal class OrderNetting
    {
        public OrderNetting(IMarginAccountInfo2 accInfo, OrderTypes type, OrderSides side, bool isHidden)
        {
            AccountData = accInfo;
            Type = type;
            Side = side;
            IsHidden = isHidden;
        }

        public bool IsEmpty => MarginAmount <= 0;
        public double MarginAmount { get; protected set; }
        public double ProfitAmount { get; protected set; }
        public double WeightedAveragePrice { get; protected set; }
        public double TotalWeight { get; private set; }

        public double Margin { get; private set; }
        public double Profit { get; private set; }
        public int ErrorCount { get; private set; }

        public IMarginAccountInfo2 AccountData { get; }
        public OrderCalculator Calculator { get; internal set; }
        public OrderTypes Type { get; }
        public OrderSides Side { get; }
        public bool IsHidden { get; }

        public event Action<double> AmountChanged;

        public StatsChange Recalculate()
        {
            var oldMargin = Margin;
            var oldProfit = Profit;
            var oldErros = ErrorCount;

            ErrorCount = 0;

            if (IsEmpty)
            {
                Margin = 0;
                Profit = 0;
            }
            else
            {
                if (MarginAmount > 0)
                {
                    Margin = Calculator.CalculateMargin(MarginAmount, AccountData.Leverage, Type, Side, IsHidden, out var error);
                    if (error != CalcErrorCodes.None)
                        ErrorCount++;
                }
                else
                    Margin = 0;

                if (ProfitAmount > 0)
                {
                    Profit = Calculator.CalculateProfit(WeightedAveragePrice, ProfitAmount, Side, out var error);
                    if (error != CalcErrorCodes.None)
                        ErrorCount++;
                }
                else
                    Profit = 0;
            }

            return new StatsChange(Margin - oldMargin, Profit - oldProfit, ErrorCount - oldErros);
        }

        public StatsChange AddOrder(double remAmount, double? price)
        {
            AddOrderWithoutCalculation(remAmount, price);
            return Recalculate();
        }

        public void AddOrderWithoutCalculation(double remAmount, double? price)
        {
            //Count++;
            ChangeMarginAmountBy(remAmount);

            if (Type == OrderTypes.Position)
            {
                ProfitAmount += remAmount;
                TotalWeight += remAmount * price.Value;
                UpdateAveragePrice();
            }
        }

        public void AddPositionWithoutCalculation(double posAmount, double posPrice)
        {
            ChangeMarginAmountBy(posAmount);
            ProfitAmount += posAmount;
            TotalWeight += posAmount * posPrice;
            UpdateAveragePrice();
        }

        public void RemovePositionWithoutCalculation(double posAmount, double posPrice)
        {
            ChangeMarginAmountBy(-posAmount);
            ProfitAmount -= posAmount;
            TotalWeight -= posAmount * posPrice;
            UpdateAveragePrice();
        }

        public StatsChange RemoveOrder(double remAmount, double? price)
        {
            //Count--;
            ChangeMarginAmountBy(-remAmount);

            if (Type == OrderTypes.Position)
            {
                ProfitAmount -= remAmount;
                TotalWeight -= remAmount * price.Value;
                UpdateAveragePrice();
            }

            return Recalculate();
        }

        //public StatsChange ChangeOrderAmount(decimal delta, decimal? price)
        //{
        //    MarginAmount += delta;

        //    if (Type == OrderTypes.Position)
        //    {
        //        ProfitAmount += delta;
        //        TotalWeight += delta * price.Value;
        //        UpdateAveragePrice();
        //    }

        //    return Recalculate();
        //}

        private void UpdateAveragePrice()
        {
            if (ProfitAmount > 0)
                WeightedAveragePrice = TotalWeight / ProfitAmount;
            else
                WeightedAveragePrice = 0;
        }

        public void ChangeMarginAmountBy(double delta)
        {
            MarginAmount += delta;

            AmountChanged.Invoke(delta);
        }

        //private void Order_TypeAmountChanged(TypeAmountChangeArgs obj)
        //{
        //}

        //private void Order_RemAmountChanged(ParamChangeArgs<decimal> obj)
        //{
        //}
    }
}
