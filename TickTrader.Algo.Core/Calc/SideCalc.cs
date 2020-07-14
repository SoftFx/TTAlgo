using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    internal class SideCalc
    {
        //private readonly PositionContainer netPositions;
        private readonly SymbolCalc _parent;
        private readonly OrderNetting _positions;
        private readonly OrderNetting _limitOrders;
        private readonly OrderNetting _stopOrders;
        private readonly OrderNetting _hiddendOrders;
        //private readonly OrderNetting _marketOrders;
        private decimal _netPosAmount;
        private decimal _netPosPrice;
        //private readonly OrderNetting _hiddenLimitOrders;
        //private readonly SymbolNetting parent;

        //public OrderNetting Orders { get; };

        public SideCalc(SymbolCalc parent, Domain.OrderInfo.Types.Side side)
        {
            _parent = parent;
            _positions = new OrderNetting(parent.AccInfo, Domain.OrderInfo.Types.Type.Position, side, false);
            _limitOrders = new OrderNetting(parent.AccInfo, Domain.OrderInfo.Types.Type.Limit, side, false);
            _stopOrders = new OrderNetting(parent.AccInfo, Domain.OrderInfo.Types.Type.Stop, side, false);
            _hiddendOrders = new OrderNetting(parent.AccInfo, Domain.OrderInfo.Types.Type.Limit, side, true);

            _positions.AmountChanged += OnAmountChanged;
            _limitOrders.AmountChanged += OnAmountChanged;
            _stopOrders.AmountChanged += OnAmountChanged;
            _hiddendOrders.AmountChanged += OnAmountChanged;
        }

        public bool IsEmpty => TotalAmount <= 0;
        public double Margin { get; private set; }
        public decimal TotalAmount { get; private set; }

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
            return result;
        }

        public void AddOrder(IOrderModel2 order)
        {
            //Count++;
            order.EssentialsChanged += Order_EssentialsChanged;
            //order.PriceChanged += Order_PriceChanged;
            var netting = GetNetting(order.Type, order.IsHidden);
            var change = netting.AddOrder(order.RemainingAmount, order.Price);
            UpdateStats(change);
        }

        public void AddOrderWithoutCalculation(IOrderModel2 order)
        {
            //Count++;
            order.EssentialsChanged += Order_EssentialsChanged;
            //order.PriceChanged += Order_PriceChanged;
            var netting = GetNetting(order.Type, order.IsHidden);
            netting.AddOrderWithoutCalculation(order.RemainingAmount, order.Price);
        }

        public void RemoveOrder(IOrderModel2 order)
        {
            //Count--;
            order.EssentialsChanged -= Order_EssentialsChanged;
            //order.PriceChanged -= Order_PriceChanged;
            var netting = GetNetting(order.Type, order.IsHidden);
            var change = netting.RemoveOrder(order.RemainingAmount, order.Price);
            UpdateStats(change);
        }

        public void UpdatePosition(IPositionSide2 pos)
        {
            _positions.RemovePositionWithoutCalculation(_netPosAmount, _netPosPrice);

            _netPosAmount = pos.Amount;
            _netPosPrice = pos.Price;

            _positions.AddPositionWithoutCalculation(_netPosAmount, _netPosPrice);

            var change = _positions.Recalculate();
            UpdateStats(change);
        }

        public void SetCalculator(OrderCalculator calc)
        {
            _positions.Calculator = calc;
            _limitOrders.Calculator = calc;
            _stopOrders.Calculator = calc;
            _hiddendOrders.Calculator = calc;
        }

        private void Order_EssentialsChanged(OrderEssentialsChangeArgs args)
        {
            var c1 = GetNetting(args.OldType, args.OldIsHidden).RemoveOrder(args.OldRemAmount, args.OldPrice);
            var c2 = GetNetting(args.Order.Type, args.Order.IsHidden).AddOrder(args.Order.RemainingAmount, args.Order.Price);
            var cSum = c1 + c2;
            UpdateStats(cSum);
        }

        //private void Order_PriceChanged(OrderPropArgs<decimal> args)
        //{
        //    var netting = GetNetting(args.Order.Type);
        //    var c1 = netting.RemoveOrder(args.Order.RemainingAmount, args.OldVal);
        //    var c2 = netting.AddOrder(args.Order.RemainingAmount, args.Order.Price);
        //    _parent.OnStatsChange(c1 + c2);
        //}

        private OrderNetting GetNetting(Domain.OrderInfo.Types.Type orderType, bool isHidden)
        {
            switch (orderType)
            {
                case Domain.OrderInfo.Types.Type.Limit:
                    {
                        if (isHidden)
                            return _hiddendOrders;
                        else
                            return _limitOrders;
                    }
                case Domain.OrderInfo.Types.Type.Stop: return _stopOrders;
                case Domain.OrderInfo.Types.Type.StopLimit: return _stopOrders; // StopLimit orders have same calculation logic as Stop orders
                case Domain.OrderInfo.Types.Type.Market: return _positions; // Market orders have same calculation logic as positions
                case Domain.OrderInfo.Types.Type.Position: return _positions;
            }

            throw new Exception("Unsupported Order Type: " + orderType);
        }

        private void UpdateStats(StatsChange change)
        {
            Margin += change.MarginDelta;
            _parent.OnStatsChange(change);            
        }

        private void OnAmountChanged(decimal delta)
        {
            TotalAmount += delta;
        }
    }
}
