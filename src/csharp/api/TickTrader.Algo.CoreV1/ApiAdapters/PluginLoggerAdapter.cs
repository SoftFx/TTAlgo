using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using static TickTrader.Algo.Domain.OrderExecReport.Types;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class PluginLoggerAdapter : IPluginMonitor, IAlertAPI
    {
        private static readonly Dictionary<int, NumberFormatInfo> _formatStorage = new Dictionary<int, NumberFormatInfo>(Enumerable.Range(1, 10).ToDictionary(u => u, CreateFormat));

        private static readonly NumberFormatInfo DefaultPriceFormat = GetFormat(5);
        private static readonly string DefaultVolumeFormat = $"0.{new string('#', 15)}";


        private IPluginLogger _logger;

        public PluginLoggerAdapter()
        {
            _logger = Null.Logger;
        }

        public IPluginLogger Logger
        {
            get => _logger;
            set => _logger = value ?? throw new InvalidOperationException("Logger cannot be null!");
        }

        private static NumberFormatInfo CreateFormat(int digit) => new NumberFormatInfo() { NumberDecimalDigits = digit };

        private static NumberFormatInfo GetFormat(int digits) => _formatStorage.GetOrAdd(digits, d => CreateFormat(d));

        #region Logger Methods

        public void UpdateStatus(string status) => _logger.UpdateStatus(status);

        public void Print(string entry) => _logger.OnPrint(entry);

        public void Print(string entry, object[] parameters) => _logger.OnPrint(entry, parameters);

        public void PrintError(string entry) => _logger.OnPrintError(entry);

        public void PrintError(string entry, object[] parameters) => _logger.OnPrintError(entry, parameters);

        //public void PrintInfo(string entry) => _logger.OnPrintInfo(entry);

        public void PrintTrade(string entry) => _logger.OnPrintTrade(entry);

        public void PrintTradeSuccess(string entry) => _logger.OnPrintTradeSuccess(entry);

        public void PrintTradeFail(string entry) => _logger.OnPrintTradeFail(entry);

        public IAlertAPI Alert => this;

        #endregion


        #region Alert API

        void IAlertAPI.Print(string message) => _logger.OnPrintAlert(message);

        #endregion


        #region Trade Log Builder methods

        public void LogOrderValidationSuccess(ITradeRequest request, OrderAction action, double lotSize = 1)
        {
            string suffix;

            switch (action)
            {
                case OrderAction.Open:
                    suffix = GetOrderDetails((IOrderLogDetailsInfo)request, lotSize);
                    break;
                case OrderAction.Modify:
                    suffix = GetOrderDetails((IOrderLogDetailsInfo)request, lotSize, "to ");
                    break;
                case OrderAction.Close:
                    suffix = $"#{request.OrderId} position";
                    break;
                default:
                    suffix = $"#{request.OrderId} order";
                    break;
            }

            PrintTrade($"[Out] {ChangeEnding(action, "ing")} {suffix} {request.LogDetails}");
        }

        public void LogRequestResults(ITradeRequest request, OrderResultEntity result, OrderAction action)
        {
            var order = (result.ResultingOrder as OrderAccessor)?.Info;
            string location = result.IsServerResponse ? "[In]" : "[Self]";
            string prefix = string.Empty;
            string suffix = null;

            switch (action)
            {
                case OrderAction.Open:
                    prefix = $"Order #{order?.Id} ";
                    suffix = GetOrderDetails(order, order?.SymbolInfo.LotSize);
                    break;
                case OrderAction.Modify:
                    prefix = $"Order #{order?.Id} ";
                    suffix = GetOrderDetails(order, order?.SymbolInfo.LotSize, "to ");
                    break;
                case OrderAction.Close:
                    prefix = $"Position {order?.Symbol} #{order?.Id} ";
                    if (order?.RemainingAmount.IsZero() == false)
                        suffix = $", remaining volume={result.ResultingOrder.RemainingVolume}";
                    break;
                default:
                    break;
            }

            if (result.IsCompleted)
                PrintTradeSuccess($"{location} SUCCESS: {prefix}{ChangeEnding(action, "ed")} {suffix ?? request.LogDetails}");
            else
            {
                string orderInformation = result.ResultingOrder is NullOrder ? string.Empty : suffix ?? request.LogDetails;

                PrintFailMessage(location, action, orderInformation, result.ResultCode);
            }
        }

        private void PrintFailMessage(string prefix, OrderAction action, string orderInformation, OrderCmdResultCodes errorCode)
        {
            PrintTradeFail($"{prefix} FAILED {ChangeEnding(action, "ing")}{orderInformation} error={errorCode}");
        }

        private static string ChangeEnding(OrderAction action, string newEnd) => $"{action.ToString().TrimEnd('e')}{newEnd}";

        #endregion

        #region Event Notifications

        public void NotifyOrderEvent(OrderInfo order, ExecAction action)
        {
            var builder = new StringBuilder(1 << 7);

            switch (action)
            {
                case ExecAction.Opened:
                    builder.Append($"{action} {GetOrderDetails(order, order.SymbolInfo.LotSize)}");
                    break;
                case ExecAction.Modified:
                case ExecAction.Splitted:
                    builder.Append(GetOrderDetails(order, order.SymbolInfo.LotSize, $"was {action} to "));
                    break;
                case ExecAction.Filled:
                case ExecAction.Closed:
                    AppendFilledCloseOrderInfo(builder, order, action, order.Type.IsStop() ? order.StopPrice : order.Price,
                                               order.IsImmediateOrCancel ? order.LastFillAmount : order.ExecAmount); //Remaning Amount
                    goto case ExecAction.Activated;
                case ExecAction.Activated:
                    if (action == ExecAction.Activated)
                        builder.Append($"Order #{order.Id} {action}");
                    PrintCurrentRate(builder, order.SymbolInfo);
                    break;
                default:
                    builder.Append($"Order #{order.Id} {action}");
                    break;
            }

            Logger.OnPrintTrade(builder.ToString());
        }

        private static void AppendFilledCloseOrderInfo(StringBuilder builder, OrderInfo order, ExecAction action, double? price, double? amount)
        {
            builder.Append($"Order #{order.Id} {action} by ").AppendNumber((amount ?? double.NaN) / order.SymbolInfo.LotSize);
            builder.Append(" at price ").AppendNumber(price ?? double.NaN, GetFormat(order.SymbolInfo.Digits));
        }

        public void NotifyPositionSplitting(NetPosition position) => Logger.OnPrintTrade($"Position #{position.Id} was splitted to {position.Side} {position.Volume:F3} {position.Symbol} at price {position.Price}");

        public void NotifyBalanceEvent(double amount, CurrencyInfo info, BalanceAction action) => Logger.OnPrint($"{action} {amount.ToString("N", GetFormat(info.Digits))} {info.Name}");

        private static void PrintCurrentRate(StringBuilder builder, SymbolInfo smbInfo)
        {
            if (smbInfo == null)
                return;

            var rate = smbInfo.LastQuote;

            builder.Append(", currentRate={").AppendNumber(rate.Bid, DefaultPriceFormat);
            builder.Append('/').AppendNumber(rate.Ask, DefaultPriceFormat).Append('}');
        }

        private static string GetOrderDetails(IOrderLogDetailsInfo info, double? lotSize, string suffix = "")
        {
            if (info == null)
                return "Null order";

            var builder = new StringBuilder(1 << 8);

            if (!string.IsNullOrEmpty(info.OrderId))
                builder.Append($"#{info.OrderId} ");

            builder.Append($"{info.Type} {suffix}{info.Side} ");
            builder.AppendNumber(info.Amount / (lotSize ?? 1) ?? 0);
            builder.Append($" {info.Symbol}");

            var extraParams = new StringBuilder(1 << 6);
            AppendExstraParams(extraParams, info.TakeProfit, "TP", DefaultPriceFormat);
            AppendExstraParams(extraParams, info.StopLoss, "SL", DefaultPriceFormat);
            AppendExstraParams(extraParams, info.Slippage, "Slippage");

            if (extraParams.Length != 0)
                extraParams.Append(')');

            builder.Append(extraParams);

            if (info.Price.HasValue && !info.Price.Value.IsZero())
                builder.Append(" at price ").AppendNumber(info.Price ?? 0, DefaultPriceFormat);

            if (info.StopPrice.HasValue && !info.StopPrice.Value.IsZero())
            {
                if (info.Price.HasValue && !info.Price.Value.IsZero())
                    builder.Append(", stop price ").AppendNumber(info.StopPrice.Value, DefaultPriceFormat);
                else
                    builder.Append(" at stop price ").AppendNumber(info.StopPrice.Value, DefaultPriceFormat);
            }

            return builder.ToString();
        }

        private static void AppendExstraParams(StringBuilder str, double? value, string name, NumberFormatInfo format = null)
        {
            if (value == null || double.IsNaN(value.Value) || value.Value.IsZero())
                return;

            str.Append(str.Length == 0 ? " (" : ", ").Append($"{name}:");

            if (format == null)
                str.Append(value.Value.ToString(DefaultVolumeFormat));
            else
                str.AppendNumber(value.Value, format);
        }

        #endregion
    }
}
