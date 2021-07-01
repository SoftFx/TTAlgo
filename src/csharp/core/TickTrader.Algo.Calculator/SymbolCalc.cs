using System;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    public class SymbolCalc : IDisposable
    {
        //private AlgoMarketState _market;
        private ISymbolCalculator _calc;
        private double _hedgeFormulPart;
        private double _netPosSwap;
        private double _netPosComm;

        public SymbolCalc(string symbol, AlgoMarketState market)
        {
            Symbol = symbol;
            //_market = market;
            Buy = new SideCalc(this, OrderInfo.Types.Side.Buy);
            Sell = new SideCalc(this, Domain.OrderInfo.Types.Side.Sell);

            CreateCalculator();

            Tracker = market.GetSymbolNodeOrNull(Symbol);
            Tracker.SymbolInfo.RateUpdated += Recalculate;
        }

        internal SymbolMarketNode Tracker { get; }
        public bool IsEmpty => Sell.IsEmpty && Buy.IsEmpty; // Count <= 0;
        public string Symbol { get; }

        public SideCalc Buy { get; }
        public SideCalc Sell { get; }
        public double Margin { get; private set; }
        public ISymbolCalculator Calc => _calc;

        public event Action<StatsChange> StatsChanged;

        public void Recalculate(ISymbolInfo smb)
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

        public void AddOrder(IOrderCalcInfo order)
        {
            //Count++;
            //order.Calculator = _calc;
            GetSideCalc(order).AddOrder(order);
            //AddOrder(order, GetSideCalc(order));
        }

        public void AddOrderWithoutCalculation(IOrderCalcInfo order)
        {
            //Count++;
            //order.Calculator = _calc;
            GetSideCalc(order).AddOrderWithoutCalculation(order);
        }

        public void RemoveOrder(IOrderCalcInfo order)
        {
            //Count--;
            GetSideCalc(order).RemoveOrder(order);
            //RemoveOrder(order, GetSideCalc(order));
        }

        public void UpdatePosition(IPositionInfo pos, PositionChangeTypes type, out double swapDelta, out double commDelta)
        {
            //pos.Calculator = Calc;

            swapDelta = pos.Swap - _netPosSwap;
            commDelta = pos.Commission - _netPosComm;

            _netPosSwap = pos.Swap;
            _netPosComm = pos.Commission;

            Buy.UpdatePosition(pos.Long, type);
            Sell.UpdatePosition(pos.Short, type);
        }

        public void Dispose()
        {
            //_calc?.RemoveUsage();
            //_calc?.Dispose();
            _calc = null;

            if (Tracker != null)
                Tracker.SymbolInfo.RateUpdated -= Recalculate;
        }

        private SideCalc GetSideCalc(IOrderCalcInfo order) => order.Side.IsBuy() ? Buy : Sell;

        private void UpdateMargin()
        {
            var buyMargin = Buy.Margin;
            var sellMargin = Sell.Margin;

            Margin = Math.Max(sellMargin, buyMargin) + _hedgeFormulPart * Math.Min(sellMargin, buyMargin);

            if (Buy.IsEmpty && Sell.IsEmpty)
                Margin = 0;
        }

        internal void OnStatsChange(StatsChange args)
        {
            var oldMargin = Margin;
            UpdateMargin();
            var delta = Margin - oldMargin;
            StatsChanged?.Invoke(new StatsChange(delta, args.ProfitDelta, args.ErrorDelta));
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
            //_calc?.RemoveUsage();
            //_calc = _market.GetCalculator(Symbol);
            //_calc.AddUsage();

            //var hedge = _calc.SymbolInfo != null ? _calc.SymbolInfo.Margin.Hedged : 0.5;
            //_hedgeFormulPart = (2 * hedge - 1);

            //Buy.SetCalculators(_calc);
            //Sell.SetCalculators(_calc);

            //_calc.Recalculate += Recalculate;
        }

        //private void Tracker_Changed()
        //{
        //    Recalculate(null);
        //}
    }
}
