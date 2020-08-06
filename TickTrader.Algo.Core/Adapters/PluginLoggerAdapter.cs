using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using static TickTrader.Algo.Domain.OrderExecReport.Types;

namespace TickTrader.Algo.Core
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

        private static NumberFormatInfo GetFormat(int digits) => _formatStorage.GetOrAdd(digits, u => CreateFormat(u));

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

        //public void LogOrderOpening(Domain.OpenOrderRequest context, SymbolInfo smbInfo)
        //{
        //    var logEntry = new StringBuilder();
        //    logEntry.Append("[Out] Opening ");
        //    AppendOrderParams(logEntry, smbInfo, " Order to ", context);

        //    PrintTrade(logEntry.ToString());
        //}

        //public void LogOrderModifying(Domain.ModifyOrderRequest context, SymbolInfo smbInfo)
        //{
        //    var logEntry = new StringBuilder();
        //    logEntry.Append("[Out] Modifying order #").Append(context.OrderId).Append(" to ");
        //    AppendOrderParams(logEntry, smbInfo, " ", context);

        //    PrintTrade(logEntry.ToString());
        //}

        //public void LogOrderCanceling(Domain.CancelOrderRequest context)
        //{
        //    PrintTrade($"[Out] Canceling order #{context.OrderId}");
        //}

        //public void LogOrderClosing(Domain.CloseOrderRequest context)
        //{
        //    var postfix = (context.Amount.HasValue && context.Amount != 0) ? $", volume={context.Amount}" : "";
        //    PrintTrade($"[Out] Closing order #{context.OrderId}{postfix}");
        //}

        //public void LogOrderClosingBy(Domain.CloseOrderRequest context)
        //{
        //    PrintTrade($"[Out] Closing order #{context.OrderId} by order #{context.ByOrderId}");
        //}

        public void LogOrderValidationSuccess(ITradeRequest request, OrderAction action) => PrintTrade($"[Out] {action} order #{request.OrderId}{request.LogDetails}");

        public void LogOrderOpenResults(OrderResultEntity result, Domain.OpenOrderRequest context)
        {
            var logEntry = new StringBuilder();

            var order = (result.ResultingOrder as OrderAccessor)?.Info;

            (result.IsServerResponse ? logEntry.Append("[In]") : logEntry.Append("[Self]")).Append(" ");
            if (result.IsCompleted)
            {
                logEntry.Append("SUCCESS: Opened ");
                if (order != null)
                {
                    if (order.ImmediateOrCancel)
                    //if (!double.IsNaN(order.LastFillPrice ?? double.NaN) && !double.IsNaN(order.LastFillAmount.Value))
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendIocOrderParams(logEntry, order.SymbolInfo, " ", order);
                    }
                    else
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, order.SymbolInfo, " ", order);
                    }
                }
                else
                    logEntry.Append("Null Order");

                PrintTradeSuccess(logEntry.ToString());
            }
            else
            {
                logEntry.Append("FAILED Opening ");
                AppendOrderParams(logEntry, order?.SymbolInfo, " Order to ", context);
                logEntry.Append(" error=").Append(result.ResultCode);

                PrintTradeFail(logEntry.ToString());
            }
        }

        public void LogOrderModifyResults(Domain.ModifyOrderRequest context, OrderResultEntity result)
        {
            var logEntry = new StringBuilder();

            var order = (result.ResultingOrder as OrderAccessor).Info;

            (result.IsServerResponse ? logEntry.Append("[In]") : logEntry.Append("[Self]")).Append(" ");
            if (result.IsCompleted)
            {
                logEntry.Append("SUCCESS: Modified order #").Append(context.OrderId).Append(" to ");
                if (result.ResultingOrder != null)
                {
                    AppendOrderParams(logEntry, order.SymbolInfo, " ", (result.ResultingOrder as OrderAccessor).Info);
                }
                else
                    logEntry.Append("Null Order");

                PrintTradeSuccess(logEntry.ToString());
            }
            else
            {
                logEntry.Append("FAILED Modifying order #").Append(context.OrderId).Append(" to ");
                AppendOrderParams(logEntry, order.SymbolInfo, " ", context);
                logEntry.Append(" error=").Append(result.ResultCode);

                PrintTradeFail(logEntry.ToString());
            }
        }

        public void LogOrderCancelResults(Domain.CancelOrderRequest context, OrderResultEntity result)
        {
            var suffix = result.IsServerResponse ? "[In]" : "[Self]";
            if (result.IsCompleted)
            {
                PrintTradeSuccess($"{suffix} SUCCESS: Order #{context.OrderId} canceled");
            }
            else
            {
                PrintTradeFail($"{suffix} FAILED Canceling order #{context.OrderId} error={result.ResultCode}");
            }
        }

        public void LogOrderCloseResults(Domain.CloseOrderRequest context, OrderResultEntity result)
        {
            var suffix = result.IsServerResponse ? "[In]" : "[Self]";
            var postfix = result.ResultingOrder.RemainingVolume != 0 ? $", remaining volume={result.ResultingOrder.RemainingVolume}" : $", volume={result.ResultingOrder.LastFillVolume}";
            if (result.IsCompleted)
            {
                PrintTradeSuccess($"{suffix} SUCCESS: Order #{context.OrderId} closed{postfix}");
            }
            else
            {
                PrintTradeFail($"{suffix} FAILED Closing order #{context.OrderId} error={result.ResultCode}");
            }
        }

        public void LogOrderCloseByResults(Domain.CloseOrderRequest context, OrderResultEntity result)
        {
            var suffix = result.IsServerResponse ? "[In]" : "[Self]";
            if (result.IsCompleted)
            {
                PrintTradeSuccess($"{suffix} SUCCESS: Order #{context.OrderId} closed by order #{context.ByOrderId}");
            }
            else
            {
                PrintTradeFail($"{suffix} FAILED Closing order #{context.OrderId} by order #{context.ByOrderId} error={result.ResultCode}");
            }
        }

        public void LogRequestResults(OrderResultEntity result)
        {
            //var message = 
        }

        private void SuccessLogRequestResult(bool isServerResponce)
        {
            //var suffix = 

            PrintTradeSuccess($"{(isServerResponce ? "[In]" : "[Self]")} SUCCESS: ");
        }

        private void FailedLogRequestResult()
        {

        }

        #endregion

        #region Trade Log notifications methods

        public void NotifyOrderEvent(OrderInfo order, ExecAction action)
        {
            var builder = new StringBuilder(1 << 7);

            builder.Append($"Order #{order.Id} ");

            switch (action)
            {
                case ExecAction.Opened:
                    builder.Append($"{action.ToString().ToLowerInvariant()} ");
                    AppendOrderParams(builder, order.SymbolInfo, " order ", order);
                    break;
                case ExecAction.Modified:
                case ExecAction.Splitted:
                    AppendOrderParams(builder, order.SymbolInfo, $" was {action.ToString().ToLowerInvariant()} to ", order);
                    break;
                case ExecAction.Filled:
                case ExecAction.Closed:
                    builder.Append($"{action.ToString().ToLowerInvariant()} by ").AppendNumber(order.LastFillAmount ?? double.NaN);
                    builder.Append(" at price ").AppendNumber(order.LastFillPrice ?? double.NaN, GetFormat(order.SymbolInfo.Digits));
                    goto case ExecAction.Activated;
                case ExecAction.Activated:
                    if (action == ExecAction.Activated)
                        builder.Append($"{action.ToString().ToLowerInvariant()}");
                    PrintCurrentRate(builder, order.SymbolInfo);
                    break;
                default:
                    builder.Append($"{action.ToString().ToLowerInvariant()}");
                    break;
            }

            Logger.OnPrintTrade(builder.ToString());
        }

        private static void PrintCurrentRate(StringBuilder builder, SymbolInfo smbInfo)
        {
            if (smbInfo == null)
                return;

            var priceFomat = FormatExtentions.CreateTradeFormatInfo(5);
            var rate = smbInfo.LastQuote;

            builder.Append(", currentRate={").AppendNumber(rate.Bid, priceFomat);
            builder.Append('/').AppendNumber(rate.Ask, priceFomat).Append('}');
        }

        public void NotifyPositionSplitting(NetPosition position)
        {
            var logEntry = new StringBuilder();
            logEntry.Append($"Position #{position.Id} was splitted to {position.Side} {position.Volume:F3} {position.Symbol} at price {position.Price}");

            Logger.OnPrintTrade(logEntry.ToString());
        }

        public void NotifyDespositWithdrawal(double amount, CurrencyInfo currency)
        {
            string action = amount > 0 ? "Deposit" : "Withdrawal";
            var format = new NumberFormatInfo { NumberDecimalDigits = currency.Digits };
            Logger.OnPrintTrade(action + " " + amount.ToString("N", format) + " " + currency.Name);
        }

        public void NotifyDividend(double amount, string currency, NumberFormatInfo format)
        {
            Logger.OnPrintTrade($"Dividend {amount.ToString("N", format)} {currency}");
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, OrderInfo order)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, order.Type, order.Side, order.RemainingAmount, order.Price, order.StopPrice, order.StopLoss, order.TakeProfit, order.Slippage);
        }

        private void AppendIocOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, OrderInfo order)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, order.Type, order.Side, order.LastFillAmount ?? double.NaN, order.LastFillPrice ?? double.NaN, double.NaN, order.StopLoss, order.TakeProfit, order.Slippage);
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, Domain.OpenOrderRequest request)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, request.Type, request.Side, request.Amount, request.Price ?? double.NaN, request.StopPrice ?? double.NaN, request.StopLoss, request.TakeProfit, request.Slippage);
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, Domain.ModifyOrderRequest request)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, request.Type, request.Side, request.NewAmount ?? request.CurrentAmount, request.Price ?? double.NaN, request.StopPrice ?? double.NaN, request.StopLoss, request.TakeProfit, request.Slippage);
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, double? volumeLots, double? price, double? stopPrice, double? sl, double? tp, double? slippage)
        {
            var priceFormat = /*smbInfo?.PriceFormat ??*/ DefaultPriceFormat;

            logEntry.Append(type).Append(suffix).Append(side).Append(" ").AppendNumber(volumeLots.Value, DefaultVolumeFormat);
            if (smbInfo != null)
                logEntry.Append(" ").Append(smbInfo.Name);

            var extraParams = new StringBuilder();
            AppendExstraParams(extraParams, tp, "TP", priceFormat);
            AppendExstraParams(extraParams, sl, "SL", priceFormat);
            AppendExstraParams(extraParams, slippage, "Slippage");

            if (extraParams.Length != 0)
                extraParams.Append(")");

            logEntry.Append(extraParams);

            if (!double.IsNaN(price.Value))
                logEntry.Append(" at price ").AppendNumber(price.Value, priceFormat);

            if (stopPrice != null && !double.IsNaN(stopPrice.Value))
            {
                if (!double.IsNaN(price.Value))
                    logEntry.Append(", stop price ").AppendNumber(stopPrice.Value, priceFormat);
                else
                    logEntry.Append(" at stop price ").AppendNumber(stopPrice.Value, priceFormat);
            }
        }

        private void AppendExstraParams(StringBuilder str, double? value, string name, NumberFormatInfo format = null)
        {
            if (value == null || double.IsNaN(value.Value))
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
