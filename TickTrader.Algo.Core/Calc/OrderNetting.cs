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
        public decimal MarginAmount { get; protected set; }
        public decimal ProfitAmount { get; protected set; }
        public decimal WeightedAveragePrice { get; protected set; }
        public decimal TotalWeight { get; private set; }

        public decimal Margin { get; private set; }
        public decimal Profit { get; private set; }

        public IMarginAccountInfo2 AccountData { get; }
        public OrderCalculator Calculator { get; internal set; }
        public OrderTypes Type { get; }
        public OrderSides Side { get; }
        public bool IsHidden { get; }

        public event Action<decimal> AmountChanged;

        public StatsChange Recalculate()
        {
            var oldMargin = Margin;
            var oldProfit = Profit;

            if (IsEmpty)
            {
                Margin = 0;
                Profit = 0;
            }
            else
            {
                if (MarginAmount > 0)
                    Margin = Calculator.CalculateMargin(MarginAmount, AccountData.Leverage, Type, Side, IsHidden);
                else
                    Margin = 0;

                if (ProfitAmount > 0)
                    Profit = Calculator.CalculateProfit(WeightedAveragePrice, ProfitAmount, Side);
                else
                    Profit = 0;
            }

            return new StatsChange(Margin - oldMargin, Profit - oldProfit);
        }

        public StatsChange AddOrder(decimal remAmount, decimal? price)
        {
            AddOrderWithoutCalculation(remAmount, price);
            return Recalculate();
        }

        public void AddOrderWithoutCalculation(decimal remAmount, decimal? price)
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

        public void AddPositionWithoutCalculation(decimal posAmount, decimal posPrice)
        {
            ChangeMarginAmountBy(posAmount);
            ProfitAmount += posAmount;
            TotalWeight += posAmount * posPrice;
            UpdateAveragePrice();
        }

        public void RemovePositionWithoutCalculation(decimal posAmount, decimal posPrice)
        {
            ChangeMarginAmountBy(-posAmount);
            ProfitAmount -= posAmount;
            TotalWeight -= posAmount * posPrice;
            UpdateAveragePrice();
        }

        public StatsChange RemoveOrder(decimal remAmount, decimal? price)
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

        public void ChangeMarginAmountBy(decimal delta)
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
