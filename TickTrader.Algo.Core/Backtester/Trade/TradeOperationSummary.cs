using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeChargesInfo
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

    internal struct AssetMoveInfo
    {
    }

    internal struct FillInfo
    {
        public decimal FillAmount { get; set; }
        public decimal FillPrice { get; set; }
        public OrderAccessor Position { get; set; }
        public NetPositionCloseInfo NetClose { get; set; }

        public bool WasNetPositionClosed => NetClose != null && NetClose.CloseAmount > 0;
    }

    internal class TradeOperationSummary
    {
        private StringBuilder _builder = new StringBuilder();

        public TradeOperationSummary()
        {
        }

        public NumberFormatInfo AccountCurrencyFormat { get; set; }
        public bool HasErrors { get; set; }
        public bool IsEmpty => _builder.Length == 0;

        public void Clear()
        {
            _builder.Clear();
            HasErrors = false;
        }

        public string GetJournalRecord()
        {
            return _builder.ToString();
        }

        public void AddOpenAction(OrderAccessor order)
        {
            StartNewAction();

            _builder.Append($"Opened order ");
            PrintOrderDescription(order);
            PrintAmountAndPrice(order);

            //if (order.Commission != 0)
            //    _builder.Append(" commission =").Append(order.Commission);
            //else if (Charges.Commission != 0)
            //    _builder.Append(" commission =").Append(Charges.Commission);

            PrintComment(order);
        }

        public void AddOpenFailAction(OrderType type, string symbol, OrderSide side, double amountLots, OrderCmdResultCodes error, AccountAccessor acc)
        {
            var currFormat = acc.BalanceCurrencyFormat;

            HasErrors = true;

            StartNewAction();

            _builder.Append($"Rejected order {type} {symbol} {side} {amountLots} reason={error}");

            if (acc.IsMarginType)
                _builder.Append(" freeMargin=").Append(acc.Equity - acc.Margin);
        }

        public void AddModificationAction(OrderAccessor oldOrder, OrderAccessor newOrder)
        {
            StartNewAction();
            _builder.Append($"Order is modified ");
            PrintOrderDescription(newOrder);
            PrintAmountAndPrice(newOrder);
        }

        public void AddFillAction(OrderAccessor order, FillInfo info)
        {
            var smbInfo = order.SymbolInfo;
            var priceFormat = smbInfo.PriceFormat;
            var fillAmountLots = info.FillAmount / (decimal)smbInfo.ContractSize;

            StartNewAction();

            _builder.Append("Filled order ");
            PrintOrderDescription(order);
            _builder.Append(" by ").AppendNumber(fillAmountLots);
            _builder.Append(" at price ").AppendNumber(info.FillPrice, priceFormat);
            PrintComment(order);

            var charges = info.NetClose?.Charges;

            if (charges != null)
                _builder.Append(" commission =").Append(charges.Commission);
        }

        public void AddStopLimitActivationAction(OrderAccessor order, decimal price)
        {
            StartNewAction();

            _builder.Append("Activated order ");
            PrintOrderDescription(order);
            PrintComment(order);

            //return _builder.ToString();
        }

        public void AddCloseAction(OrderAccessor pos, decimal profit, decimal price, TradeChargesInfo charges)
        {
            var priceFormat = pos.SymbolInfo.PriceFormat;

            StartNewAction();
            _builder.Append($"Closed position ");
            PrintOrderDescription(pos);

            _builder.Append(" at price ").AppendNumber(price, priceFormat);
            _builder.Append(", profit=").AppendNumber(profit, priceFormat);

            PrintCharges(charges);
        }

        public void AddNetCloseAction(NetPositionCloseInfo closeInfo, Symbol symbol)
        {
            var priceFormat = closeInfo.SymbolInfo.PriceFormat;
            var closeAmountLost = closeInfo.CloseAmount / (decimal)symbol.ContractSize;

            _builder.AppendLine();
            _builder.Append("Closed net position for ").AppendNumber(closeAmountLost);
            _builder.Append(" at price ").AppendNumber(closeInfo.ClosePrice, priceFormat);
            _builder.Append(", profit=").AppendNumber(closeInfo.BalanceMovement, priceFormat);

            PrintCharges(closeInfo.Charges);
        }

        #region print methods

        private void PrintOrderDescription(OrderAccessor order)
        {
            _builder.Append(" #").Append(order.Id)
                .Append(' ').Append(order.Type)
                .Append(' ').Append(order.Symbol)
                .Append(' ').Append(order.Side);

            //_builder.Append(order.RemainingVolume);
        }

        private void PrintComment(OrderAccessor order)
        {
            if (!string.IsNullOrEmpty(order.Comment))
                _builder.Append("  \"").Append(order.Comment).Append('"');
        }

        private void PrintAmountAndPrice(OrderAccessor order)
        {
            var priceFormat = order.SymbolInfo.PriceFormat;

            _builder.Append($", amount=");
            if (order.RequestedVolume == order.RemainingVolume || order.RemainingVolume == 0)
                _builder.Append(order.RequestedVolume);
            else
                _builder.Append(order.RemainingVolume).Append('/').Append(order.RequestedVolume);
            if (order.Entity.Price != null)
                _builder.Append(" price=").AppendNumber(order.Price, priceFormat);
            if (order.Entity.StopPrice != null)
                _builder.Append(" stopPrice=").AppendNumber(order.StopPrice, priceFormat);
        }

        private void StartNewAction()
        {
            if (_builder.Length > 0)
                _builder.AppendLine();
        }

        private void PrintCharges(TradeChargesInfo charges)
        {
            if (charges != null)
                _builder.Append(" commission =").Append(charges.Commission);
        }

        #endregion
    }
}
