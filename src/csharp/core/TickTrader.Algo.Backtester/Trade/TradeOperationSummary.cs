using System.Globalization;
using System.Text;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class TradeChargesInfo
    {
        public double Swap { get; set; }
        public double Commission { get; set; }

        public double Total => Swap + Commission;
        public CurrencyInfo CurrencyInfo { get; set; }

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
        public double FillAmount { get; set; }
        public double FillPrice { get; set; }
        public OrderAccessor Position { get; set; }
        public NetPositionOpenInfo NetPos { get; set; }
        public SymbolInfo SymbolInfo { get; set; }

        public bool WasNetPositionClosed => NetPos?.CloseInfo?.CloseAmount > 0;
    }

    internal class TradeOperationSummary
    {
        private StringBuilder _builder = new StringBuilder();

        public TradeOperationSummary()
        {
        }

        public NumberFormatInfo BalanceCurrencyFormat { get; set; }
        public NumberFormatInfo PriceFormat { get; } = FormatExtentions.CreateTradeFormatInfo(5);
        public PluginLogRecord.Types.LogSeverity Severity { get; private set; }
        public bool IsEmpty => _builder.Length == 0;

        public void Clear()
        {
            _builder.Clear();
            Severity = PluginLogRecord.Types.LogSeverity.TradeSuccess;
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
            //PrintComment(order);
        }

        public void AddOpenFailAction(OrderType type, string symbol, OrderSide side, double amountLots, OrderCmdResultCodes error, AccountAccessor acc)
        {
            SetTradeFaileSeverity();

            StartNewAction();

            _builder.Append($"Rejected order {type} {symbol} {side} {amountLots} reason={error}");

            if (acc.IsMarginType)
                _builder.Append(" freeMargin=").AppendNumber(acc.Equity - acc.Margin, BalanceCurrencyFormat);
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
            var priceFormat = PriceFormat;
            var fillAmountLots = info.FillAmount / smbInfo.LotSize;

            StartNewAction();

            _builder.Append("Filled order ");
            PrintOrderDescription(order);
            _builder.Append(" by ").AppendNumber(fillAmountLots);
            _builder.Append(" at price ").AppendNumber(info.FillPrice, priceFormat);
            PrintComment(order);

            var charges = info.NetPos?.Charges;

            PrintCharges(charges);
        }

        public void AddCancelAction(OrderAccessor order, TradeReportInfo.Types.Reason reason)
        {
            StartNewAction();

            _builder.Append($"{reason} order ");
            PrintOrderDescription(order);
            PrintAmountAndPrice(order);
        }

        public void AddStopLimitActivationAction(OrderAccessor order, double price)
        {
            StartNewAction();

            _builder.Append("Activated order ");
            PrintOrderDescription(order);
            PrintComment(order);

            //return _builder.ToString();
        }

        public void AddGrossCloseAction(OrderAccessor pos, double profit, double price, TradeChargesInfo charges)
        {
            var priceFormat = PriceFormat;
            var profitFormat = BalanceCurrencyFormat;

            StartNewAction();
            _builder.Append($"Closed position ");
            PrintOrderDescription(pos);

            _builder.Append(" at price ").AppendNumber(price, priceFormat);
            _builder.Append(", profit=").AppendNumber(profit, profitFormat);
            _builder.Append(", swap=").AppendNumber(charges.Swap, profitFormat);

            PrintCharges(charges);
        }

        public void AddNetCloseAction(NetPositionCloseInfo closeInfo, SymbolInfo symbol, TradeChargesInfo charges = null)
        {
            if (closeInfo.CloseAmount.E(0))
                return;

            var priceFormat = PriceFormat;
            var closeAmountLost = closeInfo.CloseAmount / symbol.LotSize;
            var profitFormat = BalanceCurrencyFormat;

            StartNewAction();
            _builder.Append("Closed net position for ").AppendNumber(closeAmountLost);
            _builder.Append(" at price ").AppendNumber(closeInfo.ClosePrice, priceFormat);
            _builder.Append(", profit=").AppendNumber(closeInfo.Profit, profitFormat);
            _builder.Append(", swap=").AppendNumber(closeInfo.Swap, profitFormat);

            if (charges != null)
                PrintCharges(charges);
        }

        public void AddNetPositionNotification(NetPosition pos, SymbolInfo smbInfo)
        {
            if (pos.Volume.E(0))
                return;

            StartNewAction();

            var priceFormat = PriceFormat;

            _builder.Append("Final position ");
            _builder.Append(pos.Symbol).Append(' ');
            _builder.AppendNumber(pos.Volume).Append(' ');
            _builder.Append(pos.Side);
            _builder.Append(" price=").AppendNumber(pos.Price, priceFormat);
        }

        public void AddStopOutAction(AccountAccessor acc, IRateInfo lastRate, SymbolAccessor rateSymbol)
        {
            SetErrorSeverity();

            StartNewAction();
            _builder.Append("Stop out! margin level: ").AppendNumber(acc.MarginLevel, "N2");
            _builder.Append(" balance: ").AppendNumber(acc.Balance, BalanceCurrencyFormat);
            _builder.Append(" equity: ").AppendNumber(acc.Equity, BalanceCurrencyFormat);
            _builder.Append(" margin: ").AppendNumber(acc.Margin, BalanceCurrencyFormat);
            _builder.Append(" last quote: ");
            PrintQuote(lastRate, rateSymbol);
        }

        private void SetTradeFaileSeverity()
        {
            if (Severity == PluginLogRecord.Types.LogSeverity.Trade || Severity == PluginLogRecord.Types.LogSeverity.TradeSuccess)
                Severity = PluginLogRecord.Types.LogSeverity.TradeFail;
        }

        private void SetErrorSeverity()
        {
            Severity = PluginLogRecord.Types.LogSeverity.Error;
        }

        #region print methods

        private void PrintOrderDescription(OrderAccessor order)
        {
            _builder.Append(" #").Append(order.Info.Id)
                .Append(' ').Append(order.Info.Type);
            if (order.Info.IsImmediateOrCancel)
                _builder.Append(" IoC");
            _builder.Append(' ').Append(order.Info.Symbol)
                .Append(' ').Append(order.Info.Side);
        }

        private void PrintComment(OrderAccessor order)
        {
            if (!string.IsNullOrEmpty(order.Info.Comment))
                _builder.Append("  \"").Append(order.Info.Comment).Append('"');
        }

        private void PrintAmountAndPrice(OrderAccessor order)
        {
            var priceFormat = PriceFormat;

            _builder.Append($", amount=");
            if (order.Info.RequestedAmount == order.Info.RequestedAmount || order.Info.RequestedAmount == 0)
                _builder.Append(order.Info.RequestedAmount);
            else
                _builder.Append(order.Info.RequestedAmount).Append('/').Append(order.Info.RequestedAmount);
            if (order.Info.Price != null)
                _builder.Append(" price=").AppendNumber(order.Info.Price ?? 0, priceFormat);
            if (order.Info.StopPrice != null)
                _builder.Append(" stopPrice=").AppendNumber(order.Info.StopPrice ?? 0, priceFormat);
        }

        private void PrintAuxFields(OrderAccessor order)
        {
            var priceFormat = PriceFormat;

            var tp = order.Info.TakeProfit;
            var sl = order.Info.StopLoss;
            var comment = order.Info.Comment;

            if (tp != null)
                _builder.Append(", tp=").AppendNumber(tp.Value, priceFormat);
            if (sl != null)
                _builder.Append(", sl=").AppendNumber(sl.Value, priceFormat);
            if (!string.IsNullOrEmpty(comment))
                _builder.Append($", comment={comment}");
        }

        private void StartNewAction()
        {
            if (_builder.Length > 0)
                _builder.AppendLine();
        }

        private void PrintCharges(TradeChargesInfo charges)
        {
            if (charges != null && charges.Commission != 0)
            {
                var profitFormat = BalanceCurrencyFormat;
                _builder.Append(", commission=").AppendNumber(charges.Commission, profitFormat);
            }
        }

        private void PrintQuote(IRateInfo update, SymbolAccessor smbInfo)
        {
            var priceFormat = PriceFormat;

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
