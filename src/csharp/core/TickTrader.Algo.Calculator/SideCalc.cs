using System;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    public class SideCalc
    {
        private readonly SymbolCalc _parent;
        private readonly OrderNetting _positions;
        private readonly OrderNetting _limitOrders;
        private readonly OrderNetting _stopOrders;
        private readonly OrderNetting _hiddendOrders;
        private double _netPosAmount;
        private double _netPosPrice;

        public SideCalc(SymbolCalc parent, OrderInfo.Types.Side side, ISymbolCalculator calc)
        {
            _parent = parent;
            _positions = new OrderNetting(calc, OrderInfo.Types.Type.Position, side, false);
            _limitOrders = new OrderNetting(calc, OrderInfo.Types.Type.Limit, side, false);
            _stopOrders = new OrderNetting(calc, OrderInfo.Types.Type.Stop, side, false);
            _hiddendOrders = new OrderNetting(calc, OrderInfo.Types.Type.Limit, side, true);
        }

        public bool IsEmpty => _positions.IsEmpty && _limitOrders.IsEmpty && _stopOrders.IsEmpty && _hiddendOrders.IsEmpty;

        public double Margin { get; private set; }

        public StatsChange Recalculate()
        {
            var result = new StatsChange(0, 0, 0);

            if (!_positions.IsEmpty)
                result += _positions.Recalculate();

            if (!_limitOrders.IsEmpty)
                result += _limitOrders.Recalculate();

            if (!_stopOrders.IsEmpty)
                result += _stopOrders.Recalculate();

            if (!_hiddendOrders.IsEmpty)
                result += _hiddendOrders.Recalculate();

            Margin += result.MarginDelta;

            if (IsEmpty)
                Margin = 0;

            return result;
        }

        public void AddOrder(IOrderCalcInfo order)
        {
            order.EssentialsChanged += Order_EssentialsChanged;
            var netting = GetNetting(order.Type, order.IsHidden);
            var change = netting.AddOrder(order.RemainingAmount, order.Price);
            UpdateStats(change);
        }

        public void AddOrderWithoutCalculation(IOrderCalcInfo order)
        {
            order.EssentialsChanged += Order_EssentialsChanged;
            var netting = GetNetting(order.Type, order.IsHidden);
            netting.AddOrderWithoutCalculation(order.RemainingAmount, order.Price);
        }

        public void RemoveOrder(IOrderCalcInfo order)
        {
            order.EssentialsChanged -= Order_EssentialsChanged;
            var netting = GetNetting(order.Type, order.IsHidden);
            var change = netting.RemoveOrder(order.RemainingAmount, order.Price);
            UpdateStats(change);
        }

        public void UpdateNetPosition(IPositionSide pos, PositionChangeTypes type)
        {
            if (type == PositionChangeTypes.Removed)
                _positions.RemovePositionWithoutCalculation(pos.Amount, pos.Price);
            else
                _positions.UpdateNetPositionWithoutCalculation(pos.Amount, pos.Price);

            var change = _positions.Recalculate();
            UpdateStats(change);
        }

        private void Order_EssentialsChanged(OrderEssentialsChangeArgs args)
        {
            var c1 = GetNetting(args.OldType, args.OldIsHidden).RemoveOrder(args.OldRemAmount, args.OldPrice);
            var c2 = GetNetting(args.Order.Type, args.Order.IsHidden).AddOrder(args.Order.RemainingAmount, args.Order.Price);
            var cSum = c1 + c2;
            UpdateStats(cSum);
        }

        private OrderNetting GetNetting(OrderInfo.Types.Type orderType, bool isHidden)
        {
            switch (orderType)
            {
                case OrderInfo.Types.Type.Limit:
                    return isHidden ? _hiddendOrders : _limitOrders;

                case OrderInfo.Types.Type.Stop: return _stopOrders;
                case OrderInfo.Types.Type.StopLimit: return _stopOrders; // StopLimit orders have same calculation logic as Stop orders
                case OrderInfo.Types.Type.Market: return _positions; // Market orders have same calculation logic as positions
                case OrderInfo.Types.Type.Position: return _positions;
            }

            throw new ArgumentException($"Unsupported Order Type: {orderType}");
        }

        private void UpdateStats(StatsChange change)
        {
            Margin += change.MarginDelta;
            _parent.OnStatsChange(change);
        }
    }
}
