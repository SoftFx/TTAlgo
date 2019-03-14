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
        public CurrencyEntity CurrencyInfo { get; set; }

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
        public NetPositionOpenInfo NetPos { get; set; }
        public SymbolAccessor SymbolInfo { get; set; }

        public bool WasNetPositionClosed => NetPos?.CloseInfo?.CloseAmount > 0;
    }

    internal class TradeOperationSummary
    {
        private StringBuilder _builder = new StringBuilder();

        public TradeOperationSummary()
        {
        }

        public NumberFormatInfo AccountCurrencyFormat { get; set; }
        public LogSeverities Severity { get; private set; }
        public bool IsEmpty => _builder.Length == 0;

        public void Clear()
        {
            _builder.Clear();
            Severity = LogSeverities.TradeSuccess;
        }

        public string GetJournalRecord()
        {
            return _builder.ToString();
        }

        public void AddOpenAction(OrderAccessor order, TradeChargesInfo charges)
        {
            StartNewAction();

            _builder.Append($"Opened order ");
            PrintOrderDescription(order);
            PrintAmountAndPrice(order);
            PrintAuxFields(order);
            PrintCharges(charges);
            PrintComment(order);
        }

        public void AddOpenFailAction(OrderType type, string symbol, OrderSide side, double amountLots, OrderCmdResultCodes error, AccountAccessor acc)
        {
            var currFormat = acc.BalanceCurrencyFormat;

            SetTradeFaileSeverity();

            StartNewAction();

            _builder.Append($"Rejected order {type} {symbol} {side} {amountLots} reason={error}");

            if (acc.IsMarginType)
                _builder.Append(" freeMargin=").AppendNumber(acc.Equity - acc.Margin, currFormat);
        }

        public void AddModificationAction(OrderAccessor oldOrder, OrderAccessor newOrder)
        {
            StartNewAction();
            _builder.Append($"Order is modified ");
            PrintOrderDescription(newOrder);
            PrintAmountAndPrice(newOrder);
            PrintAuxFields(newOrder);
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

            var charges = info.NetPos?.Charges;

            PrintCharges(charges);
        }

        public void AddCancelAction(OrderAccessor order)
        {
            StartNewAction();

            _builder.Append("Canceled order ");
            PrintOrderDescription(order);
            PrintAmountAndPrice(order);
        }

        public void AddStopLimitActivationAction(OrderAccessor order, decimal price)
        {
            StartNewAction();

            _builder.Append("Activated order ");
            PrintOrderDescription(order);
            PrintComment(order);

            //return _builder.ToString();
        }

        public void AddGrossCloseAction(OrderAccessor pos, decimal profit, decimal price, TradeChargesInfo charges, CurrencyEntity balanceCurrInf)
        {
            var priceFormat = pos.SymbolInfo.PriceFormat;
            var profitFormat = balanceCurrInf.Format;

            StartNewAction();
            _builder.Append($"Closed position ");
            PrintOrderDescription(pos);

            _builder.Append(" at price ").AppendNumber(price, priceFormat);
            _builder.Append(", profit=").AppendNumber(profit, profitFormat);

            PrintCharges(charges);
        }

        public void AddNetCloseAction(NetPositionCloseInfo closeInfo, SymbolAccessor symbol, CurrencyEntity balanceCurrInfo, TradeChargesInfo charges = null)
        {
            if (closeInfo.CloseAmount == 0)
                return;

            var priceFormat = symbol.PriceFormat;
            var closeAmountLost = closeInfo.CloseAmount / (decimal)symbol.ContractSize;
            var profitFormat = balanceCurrInfo.Format;

            StartNewAction();
            _builder.Append("Closed net position for ").AppendNumber(closeAmountLost);
            _builder.Append(" at price ").AppendNumber(closeInfo.ClosePrice, priceFormat);
            _builder.Append(", profit=").AppendNumber(closeInfo.BalanceMovement, profitFormat);

            if (charges != null)
                PrintCharges(charges);
        }

        public void AddNetPositionNotification(PositionAccessor pos, SymbolAccessor smbInfo)
        {
            if (pos.Volume == 0)
                return;

            StartNewAction();

            var priceFormat = smbInfo.PriceFormat;

            _builder.Append("Final position ");
            _builder.Append(pos.Symbol).Append(' ');
            _builder.AppendNumber(pos.Volume).Append(' ');
            _builder.Append(pos.Side);
            _builder.Append(" price=").AppendNumber(pos.Price, priceFormat);
        }

        public void AddStopOutAction(AccountAccessor acc, RateUpdate lastRate, SymbolAccessor rateSymbol)
        {
            SetErrorSeverity();

            StartNewAction();
            _builder.Append("Stop out! margin level: ").AppendNumber(acc.MarginLevel, "N2");
            _builder.Append(" balance: ").AppendNumber(acc.Balance, acc.BalanceCurrencyFormat);
            _builder.Append(" equity: ").AppendNumber(acc.Equity, acc.BalanceCurrencyFormat);
            _builder.Append(" margin: ").AppendNumber(acc.Margin, acc.BalanceCurrencyFormat);
            _builder.Append(" last quote: ");
            PrintQuote(lastRate, rateSymbol);
        }

        private void SetTradeFaileSeverity()
        {
            if (Severity == LogSeverities.Trade || Severity == LogSeverities.TradeSuccess)
                Severity = LogSeverities.TradeFail;
        }

        private void SetErrorSeverity()
        {
            Severity = LogSeverities.Error;
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

        private void PrintAuxFields(OrderAccessor order)
        {
            var priceFormat = order.SymbolInfo.PriceFormat;

            var tp = order.Entity.TakeProfit;
            var sl = order.Entity.StopLoss;

            if (tp != null)
                _builder.Append(", tp=").AppendNumber(tp.Value, priceFormat);
            if (sl != null)
                _builder.Append(", sl=").AppendNumber(sl.Value, priceFormat);
        }

        private void StartNewAction()
        {
            if (_builder.Length > 0)
                _builder.AppendLine();
        }

        private void PrintCharges(TradeChargesInfo charges)
        {
            if (charges != null && charges.Commission != 0)
                _builder.Append(" commission=").AppendNumber(charges.Commission, charges.CurrencyInfo.Format);
        }

        private void PrintQuote(RateUpdate update, SymbolAccessor smbInfo)
        {
            var priceFormat = smbInfo.PriceFormat;

            if (update != null)
            {
                _builder.AppendNumber(update.Bid, priceFormat);
                _builder.Append('/');
                _builder.AppendNumber(update.Ask, priceFormat);
            }
            else
                _builder.Append("N/A");
        }

        #endregion
    }
}
