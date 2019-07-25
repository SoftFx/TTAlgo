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
        private double _dblMarginAmount;
        private double _dblProfitAmount;

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
        public double WeightedAveragePrice { get; protected set; }
        public decimal TotalWeight { get; private set; }

        public double Margin { get; private set; }
        public double Profit { get; private set; }
        public int ErrorCount { get; private set; }

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
            var oldErros = ErrorCount;

            ErrorCount = 0;

            if (IsEmpty)
            {
                Margin = 0;
                Profit = 0;
            }
            else
            {
                if (_dblMarginAmount > 0)
                {
                    Margin = Calculator.CalculateMargin(_dblMarginAmount, AccountData.Leverage, Type, Side, IsHidden, out var error);
                    if (error != CalcErrorCodes.None)
                        ErrorCount++;
                }
                else
                    Margin = 0;

                if (_dblProfitAmount > 0)
                {
                    Profit = Calculator.CalculateProfit(WeightedAveragePrice, _dblProfitAmount, Side, out var error);
                    if (error != CalcErrorCodes.None)
                        ErrorCount++;
                }
                else
                    Profit = 0;
            }

            return new StatsChange(Margin - oldMargin, Profit - oldProfit, ErrorCount - oldErros);
        }

        public StatsChange AddOrder(decimal remAmount, double? price)
        {
            AddOrderWithoutCalculation(remAmount, price);
            return Recalculate();
        }

        public void AddOrderWithoutCalculation(decimal remAmount, double? price)
        {
            ChangeMarginAmountBy(remAmount);

            if (Type == OrderTypes.Position)
            {
                ChangeProfitAmountBy(remAmount);
                TotalWeight += remAmount * (decimal)price.Value;
                UpdateAveragePrice();
            }
        }

        public void AddPositionWithoutCalculation(decimal posAmount, decimal posPrice)
        {
            ChangeMarginAmountBy(posAmount);
            ChangeProfitAmountBy(posAmount);
            TotalWeight += posAmount * posPrice;
            UpdateAveragePrice();
        }

        public void RemovePositionWithoutCalculation(decimal posAmount, decimal posPrice)
        {
            ChangeMarginAmountBy(-posAmount);
            ChangeProfitAmountBy(-posAmount);
            TotalWeight -= posAmount * posPrice;
            UpdateAveragePrice();
        }

        public StatsChange RemoveOrder(decimal remAmount, double? price)
        {
            //Count--;
            ChangeMarginAmountBy(-remAmount);

            if (Type == OrderTypes.Position)
            {
                ChangeProfitAmountBy(remAmount);
                TotalWeight -= remAmount * (decimal)price.Value;
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
                WeightedAveragePrice = (double)(TotalWeight / ProfitAmount);
            else
                WeightedAveragePrice = 0;
        }

        public void ChangeMarginAmountBy(decimal delta)
        {
            MarginAmount += delta;
            _dblMarginAmount = (double)MarginAmount;

            AmountChanged.Invoke(delta);
        }

        public void ChangeProfitAmountBy(decimal delta)
        {
            ProfitAmount += delta;
            _dblProfitAmount = (double)ProfitAmount;
        }

        //private void Order_TypeAmountChanged(TypeAmountChangeArgs obj)
        //{
        //}

        //private void Order_RemAmountChanged(ParamChangeArgs<decimal> obj)
        //{
        //}
    }
}
