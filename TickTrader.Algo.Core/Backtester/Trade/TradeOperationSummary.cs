using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class TradeChargesInfo
    {
        public decimal Swap { get; set; }
        public decimal Commission { get; set; }

        public decimal Total => Swap + Commission;

        public void Clear()
        {
            Swap = 0;
            Commission = 0;
        }
    }

    public struct AssetMoveInfo
    {
    }

    [Flags]
    internal enum TradeActions
    {
        None = 0,
        Fill = 1,
        GrossClose = 2,
        NetClose = 4,
        NewPos = 8,
        NewOrder = 16,
        CashMove = 32,
        Replace = 64
    }

    internal class TradeOperationSummary
    {
        private StringBuilder _builder = new StringBuilder();

        public TradeOperationSummary()
        {
            Charges = new TradeChargesInfo();
        }

        public TradeActions Actions { get; private set; }
        public TradeChargesInfo Charges { get; }
        public OrderAccessor SrcOrder { get; set; }
        public OrderAccessor NewOrder { get; set; }
        public decimal Profit { get; set; }
        public decimal FillAmount { get; set; }
        public decimal FillPrice { get; set; }
        public NetPositionCloseInfo NetCloseInfo { get; set; }
        public OrderAccessor NewPos { get; set; }

        public void Clear()
        {
            Profit = 0;
            FillAmount = 0;
            FillPrice = 0;
            Charges.Clear();
            NewOrder = null;
            NetCloseInfo = new NetPositionCloseInfo();
            NewPos = null;

            Actions = TradeActions.None;
        }

        public bool CheckAction(TradeActions action)
        {
            return (Actions & action) == action;
        }

        public void AddAction(TradeActions action)
        {
            Actions |= action;
        }

        public string PrintOpenInfo()
        {
            var order = NewOrder;

            _builder.Clear();
            _builder.Append($"Opened order ");
            PrintOrderDescription(NewOrder);
            PrintAmountAndPrice(NewOrder.Entity);

            //if (order.Commission != 0)
            //    _builder.Append(" commission =").Append(order.Commission);
            //else if (Charges.Commission != 0)
            //    _builder.Append(" commission =").Append(Charges.Commission);

            PrintComment(order);
            PrintNetClose();

            return _builder.ToString();
        }

        public static string PrintOpenFail(OrderType type, string symbol, OrderSide side, OrderCmdResultCodes error)
        {
            return $"Rejected order {type} {symbol} {side} reason={error}";
        }

        public string PrintModificationInfo()
        {
            _builder.Clear();
            _builder.Append($"Order is modified ");
            PrintOrderDescription(NewOrder);
            PrintAmountAndPrice(NewOrder.Entity);

            return _builder.ToString();
        }

        public string PrintActivationInfo()
        {
            _builder.Clear();

            if (CheckAction(TradeActions.Fill))
            {
                _builder.Append("Filled order");
                PrintOrderDescription(SrcOrder);
                _builder.Append(" by ").Append(FillAmount);
                _builder.Append(" at price ").Append(FillPrice);
                PrintComment(SrcOrder);
                PrintNetClose();

                if (Charges.Commission != 0)
                    _builder.Append(" commission =").Append(Charges.Commission);
            }
            else if (CheckAction(TradeActions.NewOrder))
                _builder.Append("StopLimit Activated");

            return _builder.ToString();
        }

        private void PrintOrderDescription(OrderAccessor order)
        {
            _builder.Append(" #").Append(order.Id)
                .Append(' ').Append(order.Type)
                .Append(' ').Append(order.Symbol)
                .Append(' ').Append(order.Side);

            //_builder.Append(order.RemainingVolume);
        }

        private void PrintNetClose()
        {
            if (CheckAction(TradeActions.NetClose))
            {
                _builder.AppendLine();
                _builder.Append("Closed net position for ").Append(NetCloseInfo.CloseAmount);
                _builder.Append(" at price ").Append(NetCloseInfo.ClosePrice);
                _builder.Append(", profit=").Append(NetCloseInfo.BalanceMovement);
            }
        }

        private void PrintComment(OrderAccessor order)
        {
            if (!string.IsNullOrEmpty(order.Comment))
                _builder.Append("  \"").Append(order.Comment).Append('"');
        }

        private void PrintAmountAndPrice(OrderEntity order)
        {
            _builder.Append($", amount=").Append(order.RemainingVolume);
            if (order.Price != null)
                _builder.Append(" price=").Append(order.Price);
            if (order.StopPrice != null)
                _builder.Append(" stopPrice=").Append(order.StopPrice);
        }
    }
}
