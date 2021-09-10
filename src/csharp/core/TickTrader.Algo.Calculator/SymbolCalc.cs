using System;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    public class SymbolCalc : IDisposable
    {
        private readonly SymbolMarketNode _node;
        private readonly double _hedgeFormulPart;

        private double _netPosSwap;
        private double _netPosComm;

        public SymbolCalc(SymbolMarketNode node)
        {
            _node = node;
            Calculator = node;

            Symbol = node.SymbolInfo.Name;
            Buy = new SideCalc(this, OrderInfo.Types.Side.Buy, Calculator);
            Sell = new SideCalc(this, OrderInfo.Types.Side.Sell, Calculator);

            var hedge = Calculator?.SymbolInfo?.Margin.Hedged ?? 0.5;
            _hedgeFormulPart = 2 * hedge - 1;

            _node.SymbolInfo.RateUpdated += Recalculate;
        }

        public bool IsEmpty => Sell.IsEmpty && Buy.IsEmpty;

        public string Symbol { get; }

        public SideCalc Buy { get; }

        public SideCalc Sell { get; }

        public double Margin { get; private set; }

        public ISymbolCalculator Calculator { get; }

        public event Action<StatsChangeToken> StatsChanged;

        public void Recalculate(ISymbolInfo smb)
        {
            OnStatsChange(Buy.Recalculate() + Sell.Recalculate());
        }

        public void AddOrder(IOrderCalcInfo order)
        {
            order.Calculator = Calculator;
            GetSideCalc(order).AddOrder(order);
        }

        public void AddOrderWithoutCalculation(IOrderCalcInfo order)
        {
            order.Calculator = Calculator;
            GetSideCalc(order).AddOrderWithoutCalculation(order);
        }

        public void RemoveOrder(IOrderCalcInfo order)
        {
            GetSideCalc(order).RemoveOrder(order);
        }

        public void UpdateNetPosition(IPositionInfo pos, PositionChangeTypes type, out double swapDelta, out double commDelta)
        {
            pos.Calculator = Calculator;

            swapDelta = pos.Swap - _netPosSwap;
            commDelta = pos.Commission - _netPosComm;

            _netPosSwap = pos.Swap;
            _netPosComm = pos.Commission;

            double buyVolume = pos.Side.IsBuy() ? pos.Volume : 0.0;
            double sellVolume = pos.Side.IsSell() ? pos.Volume : 0.0;

            double buyPrice = pos.Side.IsBuy() ? pos.Price : 0.0;
            double sellPrice = pos.Side.IsSell() ? pos.Price : 0.0;

            Buy.UpdateNetPosition(buyVolume, buyPrice, type);
            Sell.UpdateNetPosition(sellVolume, sellPrice, type);
        }

        public void Dispose()
        {
            if (_node != null)
                _node.SymbolInfo.RateUpdated -= Recalculate;
        }

        private SideCalc GetSideCalc(IOrderCalcInfo order) => order.Side.IsBuy() ? Buy : Sell;

        private void UpdateMargin()
        {
            var buyMargin = Buy.Margin;
            var sellMargin = Sell.Margin;

            Margin = Math.Max(sellMargin, buyMargin) + _hedgeFormulPart * Math.Min(sellMargin, buyMargin);

            if (IsEmpty)
                Margin = 0;
        }

        internal void OnStatsChange(StatsChangeToken args)
        {
            if (args == StatsChangeToken.EmptyToken)
                return;

            var oldMargin = Margin;
            UpdateMargin();
            var delta = Margin - oldMargin;
            StatsChanged?.Invoke(new StatsChangeToken(delta, args.ProfitDelta, args.ErrorDelta));
        }
    }
}
