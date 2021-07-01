using System;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    internal class OrderNetting
    {
        //private double _dblMarginAmount;
        //private double _dblProfitAmount;

        public OrderNetting(/*IMarginAccountInfo2 accInfo, */OrderInfo.Types.Type type, OrderInfo.Types.Side side, bool isHidden)
        {
            //AccountData = accInfo;
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

        //public IMarginAccountInfo2 AccountData { get; }
        public ISymbolCalculator Calculator { get; internal set; }
        public OrderInfo.Types.Type Type { get; }
        public OrderInfo.Types.Side Side { get; }
        public bool IsHidden { get; }

        public event Action<double> AmountChanged;

        public StatsChange Recalculate()
        {
            UpdateAveragePrice();

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
                    var response = Calculator.Margin.Calculate(new MarginRequest(MarginAmount, Type, IsHidden));
                    Margin = response.Value;
                    if (response.IsFailed)
                        ErrorCount++;
                }
                else
                    Margin = 0;

                if (ProfitAmount > 0)
                {
                    var response = Calculator.Profit.Calculate(new ProfitRequest(WeightedAveragePrice, ProfitAmount, Side));
                    Profit = response.Value;
                    if (response.IsFailed)
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
            ChangeMarginAmountBy(remAmount);

            if (Type == OrderInfo.Types.Type.Position)
            {
                ChangeProfitAmountBy(remAmount);
                TotalWeight += remAmount * price.Value;
                UpdateAveragePrice();
            }
        }

        public void AddPositionWithoutCalculation(double posAmount, double posPrice)
        {
            ChangeMarginAmountBy(posAmount);
            ChangeProfitAmountBy(posAmount);
            TotalWeight += posAmount * posPrice;
            UpdateAveragePrice();
        }

        public void RemovePositionWithoutCalculation(double posAmount, double posPrice)
        {
            ChangeMarginAmountBy(-posAmount);
            ChangeProfitAmountBy(-posAmount);
            TotalWeight -= posAmount * posPrice;
            UpdateAveragePrice();
        }

        public StatsChange RemoveOrder(double remAmount, double? price)
        {
            //Count--;
            ChangeMarginAmountBy(-remAmount);

            if (Type == OrderInfo.Types.Type.Position)
            {
                ChangeProfitAmountBy(-remAmount);
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
            //_dblMarginAmount = MarginAmount;

            AmountChanged.Invoke(delta);
        }

        public void ChangeProfitAmountBy(double delta)
        {
            ProfitAmount += delta;
            //_dblProfitAmount = ProfitAmount;
        }

        //private void Order_TypeAmountChanged(TypeAmountChangeArgs obj)
        //{
        //}

        //private void Order_RemAmountChanged(ParamChangeArgs<decimal> obj)
        //{
        //}
    }
}
