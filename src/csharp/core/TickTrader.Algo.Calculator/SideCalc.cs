using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator
{
    public sealed class SideCalc
    {
        private enum NettingType { Positions, Limits, Stops, HiddenLimits }

        private readonly Dictionary<NettingType, OrderNetting> _nettings;
        private readonly SymbolCalc _parent;


        public SideCalc(SymbolCalc parent, OrderInfo.Types.Side side, ISymbolCalculator calc)
        {
            _parent = parent;

            _nettings = new Dictionary<NettingType, OrderNetting>
            {
                [NettingType.HiddenLimits] = new OrderNetting(calc, OrderInfo.Types.Type.Limit, side, true),
                [NettingType.Positions] = new OrderNetting(calc, OrderInfo.Types.Type.Position, side, false),
                [NettingType.Limits] = new OrderNetting(calc, OrderInfo.Types.Type.Limit, side, false),
                [NettingType.Stops] = new OrderNetting(calc, OrderInfo.Types.Type.Stop, side, false),
            };
        }

        public bool IsEmpty => _nettings.Values.All(u => u.IsEmpty);

        public double Margin { get; private set; }

        public StatsChangeToken Recalculate()
        {
            var deltaToken = StatsChangeToken.EmptyToken;

            if (!IsEmpty)
                foreach (var netting in _nettings.Values)
                    if (!netting.IsEmpty)
                        deltaToken += netting.Recalculate();

            Margin += deltaToken.MarginDelta;

            return deltaToken;
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

        public void UpdateNetPosition(double amount, double price, PositionChangeTypes type)
        {
            if (type == PositionChangeTypes.Removed)
                _nettings[NettingType.Positions].RemovePositionWithoutCalculation(amount, price);
            else
                _nettings[NettingType.Positions].UpdateNetPositionWithoutCalculation(amount, price);

            var change = _nettings[NettingType.Positions].Recalculate();
            UpdateStats(change);
        }

        private void Order_EssentialsChanged(OrderEssentialsChangeArgs args)
        {
            var c1 = GetNetting(args.OldType, args.OldIsHidden).RemoveOrder(args.OldRemAmount, args.OldPrice);
            var c2 = GetNetting(args.Order.Type, args.Order.IsHidden).AddOrder(args.Order.RemainingAmount, args.Order.Price);
            UpdateStats(c1 + c2);
        }

        private OrderNetting GetNetting(OrderInfo.Types.Type orderType, bool isHidden)
        {
            switch (orderType)
            {
                case OrderInfo.Types.Type.Limit:
                    return isHidden ? _nettings[NettingType.HiddenLimits] : _nettings[NettingType.Limits];
                case OrderInfo.Types.Type.Stop:
                    return _nettings[NettingType.Stops];
                case OrderInfo.Types.Type.StopLimit:
                    return _nettings[NettingType.Stops]; // StopLimit orders have same calculation logic as Stop orders
                case OrderInfo.Types.Type.Market:
                    return _nettings[NettingType.Positions]; // Market orders have same calculation logic as positions
                case OrderInfo.Types.Type.Position:
                    return _nettings[NettingType.Positions];
            }

            throw new ArgumentException($"Unsupported Order Type: {orderType}");
        }

        private void UpdateStats(StatsChangeToken change)
        {
            Margin += change.MarginDelta;
            _parent.OnStatsChange(change);
        }
    }
}
