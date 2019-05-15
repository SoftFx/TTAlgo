using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    internal class SymbolCalc : IDisposable
    {
        private MarketState _market;
        private OrderCalculator _calc;
        private double _hedgeFormulPart;
        private double _netPosSwap;
        private double _netPosComm;

        public SymbolCalc(string symbol, IMarginAccountInfo2 accInfo, MarketState market)
        {
            Symbol = symbol;
            _market = market;
            AccInfo = accInfo;
            Buy = new SideCalc(this, OrderSides.Buy);
            Sell = new SideCalc(this, OrderSides.Sell);
            CreateCalculator();
        }

        public IMarginAccountInfo2 AccInfo { get; }
        //public int Count { get; private set; }
        public bool IsEmpty => Sell.IsEmpty && Buy.IsEmpty; // Count <= 0;
        public string Symbol { get; }

        public SideCalc Buy { get; }
        public SideCalc Sell { get; }
        public double Margin { get; private set; }
        public OrderCalculator Calc => _calc;

        public event Action<StatsChange> StatsChanged;

        public void Recalculate()
        {
            StatsChange change;

            if (!Buy.IsEmpty)
            {
                if (!Sell.IsEmpty)
                    change = Buy.Recalculate() + Sell.Recalculate();
                else
                    change = Buy.Recalculate();
            }
            else if (!Sell.IsEmpty)
                change = Sell.Recalculate();
            else
                return;

            OnStatsChange(change);
        }

        public void AddOrder(IOrderModel2 order)
        {
            //Count++;
            order.Calculator = _calc;
            GetSideCalc(order).AddOrder(order);
            //AddOrder(order, GetSideCalc(order));
        }

        public void AddOrderWithoutCalculation(IOrderModel2 order)
        {
            //Count++;
            order.Calculator = _calc;
            GetSideCalc(order).AddOrderWithoutCalculation(order);
        }

        public void RemoveOrder(IOrderModel2 order)
        {
            //Count--;
            GetSideCalc(order).RemoveOrder(order);
            //RemoveOrder(order, GetSideCalc(order));
        }

        public void UpdatePosition(IPositionModel2 pos, out double swapDelta, out double commDelta)
        {
            pos.Calculator = Calc;

            swapDelta = pos.Swap - _netPosSwap;
            commDelta = pos.Commission - _netPosComm;

            _netPosSwap = pos.Swap;
            _netPosComm = pos.Commission;

            Buy.UpdatePosition(pos.Long);
            Sell.UpdatePosition(pos.Short);
        }

        public void Dispose()
        {
            _calc?.RemoveUsage();
            _calc = null;
        }

        private SideCalc GetSideCalc(IOrderModel2 order)
        {
            if (order.Side == OrderSides.Buy)
                return Buy;
            else
                return Sell;
        }

        private void UpdateMargin()
        {
            //decimal sellMargin = 0;
            //decimal buyMargin = 0;

            var buyMargin = Buy.Margin;
            var sellMargin = Sell.Margin;

            //if (AccInfo.AccountingType == AccountingTypes.Gross)
            //{
            //    buyMargin = Buy.Margin;
            //    sellMargin = Sell.Margin;
            //}
            //else
            //{
            //    buyMargin = Buy.PendingMargin;
            //    sellMargin = Sell.PendingMargin;

            //    if (Buy.PositionMargin > Sell.PositionMargin)
            //        buyMargin += Buy.PositionMargin - Sell.PositionMargin;
            //    else if (Sell.PositionMargin > Buy.PositionMargin)
            //        sellMargin += Sell.PositionMargin - Buy.PositionMargin;
            //}

            Margin = Math.Max(sellMargin, buyMargin) + _hedgeFormulPart * Math.Min(sellMargin, buyMargin);
        }

        internal void OnStatsChange(StatsChange args)
        {
            var oldMargin = Margin;
            UpdateMargin();
            var delta = Margin - oldMargin;
            StatsChanged.Invoke(new StatsChange(delta, args.ProfitDelta));
        }

        //private void AddOrder(IOrderModel2 order, SideCalc side)
        //{
        //    side.AddOrder(order);
        //}

        //private void RemoveOrder(IOrderModel2 order, SideCalc side)
        //{
        //    side.AddOrder(order);
        //}

        private void CreateCalculator()
        {
            _calc?.RemoveUsage();
            _calc = _market.GetCalculator(Symbol, AccInfo.BalanceCurrency);
            _calc.AddUsage();

            var hedge = _calc.SymbolInfo != null ? _calc.SymbolInfo.MarginHedged : 0.5;
            _hedgeFormulPart = (2 * hedge - 1);

            Buy.SetCalculator(_calc);
            Sell.SetCalculator(_calc);
        }
    }
}
