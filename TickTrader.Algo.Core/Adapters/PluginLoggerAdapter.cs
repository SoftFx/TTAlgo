using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class PluginLoggerAdapter : IPluginMonitor, IAlertAPI
    {
        private static readonly NumberFormatInfo DefaultPriceFormat = FormatExtentions.CreateTradeFormatInfo(5);
        private static readonly string DefaultVolumeFormat = $"0.{new string('#', 15)}";

        private IPluginLogger logger;

        public PluginLoggerAdapter()
        {
            this.logger = Null.Logger;
        }

        public IPluginLogger Logger
        {
            get => logger;
            set => logger = value ?? throw new InvalidOperationException("Logger cannot be null!");
        }

        #region Logger Methods

        public void UpdateStatus(string status)
        {
            logger.UpdateStatus(status);
        }

        public void Print(string entry)
        {
            logger.OnPrint(entry);
        }

        public void Print(string entry, object[] parameters)
        {
            logger.OnPrint(entry, parameters);
        }

        public void PrintError(string entry)
        {
            logger.OnPrintError(entry);
        }

        public void PrintError(string entry, object[] parameters)
        {
            logger.OnPrintError(entry, parameters);
        }

        public void PrintInfo(string entry)
        {
            logger.OnPrintInfo(entry);
        }

        public void PrintTrade(string entry)
        {
            logger.OnPrintTrade(entry);
        }

        public void PrintTradeSuccess(string entry)
        {
            logger.OnPrintTradeSuccess(entry);
        }

        public void PrintTradeFail(string entry)
        {
            logger.OnPrintTradeFail(entry);
        }

        public IAlertAPI Alert => this;

        #endregion


        #region Alert API

        void IAlertAPI.Print(string message) => logger.OnPrintAlert(message);

        #endregion


        #region Trade Log Builder methods

        public void LogOrderOpening(OpenOrderRequestContext context, SymbolInfo smbInfo)
        {
            var logEntry = new StringBuilder();
            logEntry.Append("[Out] Opening ");
            AppendOrderParams(logEntry, smbInfo, " Order to ", context);

            PrintTrade(logEntry.ToString());
        }

        public void LogOrderOpenResults(OrderResultEntity result, OpenOrderRequestContext context, SymbolInfo smbInfo)
        {
            var logEntry = new StringBuilder();

            (result.IsServerResponse ? logEntry.Append("[In]") : logEntry.Append("[Self]")).Append(" ");
            if (result.IsCompleted)
            {
                var order = result.ResultingOrder;
                logEntry.Append("SUCCESS: Opened ");
                if (order != null)
                {
                    if (!double.IsNaN(order.LastFillPrice) && !double.IsNaN(order.LastFillVolume))
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendIocOrderParams(logEntry, smbInfo, " ", order);
                    }
                    else
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, smbInfo, " ", order);
                    }
                }
                else
                    logEntry.Append("Null Order");

                PrintTradeSuccess(logEntry.ToString());
            }
            else
            {
                logEntry.Append("FAILED Opening ");
                AppendOrderParams(logEntry, smbInfo, " Order to ", context);
                logEntry.Append(" error=").Append(result.ResultCode);

                PrintTradeFail(logEntry.ToString());
            }
        }

        public void LogOrderModifying(ModifyOrderRequestContext context, SymbolInfo smbInfo)
        {
            var logEntry = new StringBuilder();
            logEntry.Append("[Out] Modifying order #").Append(context.OrderId).Append(" to ");
            AppendOrderParams(logEntry, smbInfo, " ", context);

            PrintTrade(logEntry.ToString());
        }

        public void LogOrderModifyResults(ModifyOrderRequestContext context, SymbolInfo smbInfo, OrderResultEntity result)
        {
            var logEntry = new StringBuilder();

            (result.IsServerResponse ? logEntry.Append("[In]") : logEntry.Append("[Self]")).Append(" ");
            if (result.IsCompleted)
            {
                logEntry.Append("SUCCESS: Modified order #").Append(context.OrderId).Append(" to ");
                if (result.ResultingOrder != null)
                {
                    AppendOrderParams(logEntry, smbInfo, " ", result.ResultingOrder);
                }
                else
                    logEntry.Append("Null Order");

                PrintTradeSuccess(logEntry.ToString());
            }
            else
            {
                logEntry.Append("FAILED Modifying order #").Append(context.OrderId).Append(" to ");
                AppendOrderParams(logEntry, smbInfo, " ", context);
                logEntry.Append(" error=").Append(result.ResultCode);

                PrintTradeFail(logEntry.ToString());
            }
        }

        public void LogOrderCanceling(CancelOrderRequestContext context)
        {
            PrintTrade($"[Out] Canceling order #{context.OrderId}");
        }

        public void LogOrderCancelResults(CancelOrderRequestContext context, OrderResultEntity result)
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

        public void LogOrderClosing(CloseOrderRequestContext context)
        {
            var postfix = (context.VolumeLots.HasValue && context.VolumeLots != 0) ? $", volume={context.VolumeLots}" : "";
            PrintTrade($"[Out] Closing order #{context.OrderId}{postfix}");
        }

        public void LogOrderCloseResults(CloseOrderRequestContext context, OrderResultEntity result)
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

        public void LogOrderClosingBy(CloseOrderRequestContext context)
        {
            PrintTrade($"[Out] Closing order #{context.OrderId} by order #{context.ByOrderId}");
        }

        public void LogOrderCloseByResults(CloseOrderRequestContext context, OrderResultEntity result)
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

        #endregion

        #region Trade Log notifications methods

        public void NotifyOrderOpened(OrderAccessor order)
        {
            var logEntry = new StringBuilder();
            logEntry.Append("Opened #").Append(order.Info.Id).Append(' ');
            AppendOrderParams(logEntry, order.SymbolInfo, " order ", order);

            PrintTrade(logEntry.ToString());
        }

        public void NotifyOrderModification(OrderAccessor order)
        {
            var logEntry = new StringBuilder();

            logEntry.Append("Order #").Append(order.Info.Id).Append(" ");
            AppendOrderParams(logEntry, order.SymbolInfo, " was modified to ", order);

            Logger.OnPrintTrade(logEntry.ToString());
        }

        public void NotifyOrderSplitting(OrderAccessor order)
        {
            var logEntry = new StringBuilder();

            logEntry.Append("Order #").Append(order.Info.Id).Append(" ");
            AppendOrderParams(logEntry, order.SymbolInfo, " was splitted to ", order);

            Logger.OnPrintTrade(logEntry.ToString());
        }

        public void NotifyPositionSplitting(NetPosition position)
        {
            var logEntry = new StringBuilder();
            logEntry.Append($"Position #{position.Id} was splitted to {position.Side} {position.Volume:F3} {position.Symbol} at price {position.Price}");

            Logger.OnPrintTrade(logEntry.ToString());
        }

        public void NotifyDespositWithdrawal(double amount, CurrencyEntity currency)
        {
            string action = amount > 0 ? "Deposit" : "Withdrawal";

            Logger.OnPrintTrade(action + " " + amount.ToString("N", currency.Format) + " " + currency.Name);
        }

        public void NotifyDividend(double amount, string currency, NumberFormatInfo format)
        {
            Logger.OnPrintTrade($"Dividend {amount.ToString("N", format)} {currency}");
        }

        public void NotifyOrderFill(OrderAccessor order)
        {
            var smbInfo = order.SymbolInfo;
            var priceFomat = FormatExtentions.CreateTradeFormatInfo(5);

            var builder = new StringBuilder();
            builder.Append("Order #").Append(order.Info.Id);
            builder.Append(" filled by ").AppendNumber(((Order)order).LastFillVolume);
            builder.Append(" at price ").AppendNumber(((Order)order).LastFillPrice, priceFomat);
            PrintCurrentRate(builder, smbInfo);

            Logger.OnPrintTrade(builder.ToString());
        }

        public void NotifyOrderClosed(OrderAccessor order)
        {
            var smbInfo = order.SymbolInfo;
            var priceFomat = FormatExtentions.CreateTradeFormatInfo(5);

            var builder = new StringBuilder();
            builder.Append("Order #").Append(order.Info.Id);
            builder.Append(" closed by ").AppendNumber(((Order)order).LastFillVolume);
            builder.Append(" at price ").AppendNumber(((Order)order).LastFillPrice, priceFomat);
            PrintCurrentRate(builder, smbInfo);

            Logger.OnPrintTrade(builder.ToString());
        }

        public void NotifyOrderExpiration(OrderAccessor order)
        {
            var builder = new StringBuilder();
            builder.Append("Order #").Append(order.Info.Id);
            builder.Append(" expired");

            Logger.OnPrintTrade(builder.ToString());
        }

        public void NotifyOrderActivation(OrderAccessor order)
        {
            var smbInfo = order.SymbolInfo;
            var priceFomat = FormatExtentions.CreateTradeFormatInfo(5);

            var builder = new StringBuilder();
            builder.Append("Order #").Append(order.Info.Id);
            builder.Append(" activated");
            //builder.AppendNumber(order.ExecPrice, priceFomat);
            PrintCurrentRate(builder, smbInfo);

            Logger.OnPrintTrade(builder.ToString());
        }

        public void NotifyOrderCancelation(OrderAccessor order)
        {
            var builder = new StringBuilder();
            builder.Append("Order #").Append(order.Info.Id);
            builder.Append(" canceled");

            Logger.OnPrintTrade(builder.ToString());
        }

        private void PrintCurrentRate(StringBuilder builder, SymbolInfo smbInfo)
        {
            if (smbInfo == null)
                return;

            var priceFomat = FormatExtentions.CreateTradeFormatInfo(5);
            var rate = smbInfo.LastQuote;

            builder.Append(", currentRate={").AppendNumber(rate.Bid, priceFomat);
            builder.Append('/').AppendNumber(rate.Ask, priceFomat).Append('}');
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, Order order)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, order.Type.ToCoreEnum(), order.Side.ToCoreEnum(), order.RemainingVolume, order.Price, order.StopPrice, order.StopLoss, order.TakeProfit, order.Slippage);
        }

        private void AppendIocOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, Order order)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, order.Type.ToCoreEnum(), order.Side.ToCoreEnum(), order.LastFillVolume, order.LastFillPrice, double.NaN, order.StopLoss, order.TakeProfit, order.Slippage);
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, OpenOrderRequestContext request)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, request.Type, request.Side, request.Volume, request.Price ?? double.NaN, request.StopPrice ?? double.NaN, request.StopLoss, request.TakeProfit, request.Slippage);
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, ModifyOrderRequestContext request)
        {
            AppendOrderParams(logEntry, smbInfo, suffix, request.Type, request.Side, request.NewVolume ?? request.CurrentVolume, request.Price ?? double.NaN, request.StopPrice ?? double.NaN, request.StopLoss, request.TakeProfit, request.Slippage);
        }

        private void AppendOrderParams(StringBuilder logEntry, SymbolInfo smbInfo, string suffix, Domain.OrderInfo.Types.Type type, Domain.OrderInfo.Types.Side side, double volumeLots, double price, double stopPrice, double? sl, double? tp, double? slippage)
        {
            var priceFormat = /*smbInfo?.PriceFormat ??*/ DefaultPriceFormat;

            logEntry.Append(type)
                .Append(suffix).Append(side)
                .Append(" ").AppendNumber(volumeLots, DefaultVolumeFormat);
            if (smbInfo != null)
                logEntry.Append(" ").Append(smbInfo.Name);

            var extraParams = new StringBuilder();
            AppendExstraParams(extraParams, tp, "TP", priceFormat);
            AppendExstraParams(extraParams, sl, "SL", priceFormat);
            AppendExstraParams(extraParams, slippage, "Slippage");

            if (extraParams.Length != 0)
                extraParams.Append(")");

            logEntry.Append(extraParams);

            if (!double.IsNaN(price))
                logEntry.Append(" at price ").AppendNumber(price, priceFormat);

            if (!double.IsNaN(stopPrice))
            {
                if (!double.IsNaN(price))
                    logEntry.Append(", stop price ").AppendNumber(stopPrice, priceFormat);
                else
                    logEntry.Append(" at stop price ").AppendNumber(stopPrice, priceFormat);
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
