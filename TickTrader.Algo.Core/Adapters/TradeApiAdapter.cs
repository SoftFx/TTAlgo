using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : TradeCommands
    {
        private ITradeApi api;
        private SymbolProvider symbols;
        private AccountEntity account;
        private PluginLoggerAdapter logger;

        public TradeApiAdapter(ITradeApi api, SymbolProvider symbols, AccountEntity account, PluginLoggerAdapter logger)
        {
            this.api = api;
            this.symbols = symbols;
            this.account = account;
            this.logger = logger;
        }

        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            var smbMetadata = symbols.List[symbol];
            if (smbMetadata.IsNull)
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

            volumeLots = RoundVolume(volumeLots, smbMetadata);
            double volume = ConvertVolume(volumeLots, smbMetadata);
            price = RoundPrice(price, smbMetadata, side);
            sl = RoundPrice(sl, smbMetadata, side);
            tp = RoundPrice(tp, smbMetadata, side);

            LogOrderOpening(symbol, type, side, volumeLots, price, sl, tp);

            using (var waitHandler = new TaskProxy<OpenModifyResult>())
            {
                api.OpenOrder(waitHandler, symbol, type, side, price, volume, tp, sl, comment, options, tag);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                TradeResultEntity resultEntity;
                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    var orderModel = account.Orders.Add(result.NewOrder);
                    resultEntity = new TradeResultEntity(result.ResultCode, orderModel);
                }
                else
                {
                    var orderToOpen = new OrderEntity("-1")
                    {
                        Symbol = symbol,
                        Type = type,
                        Side = side,
                        RemainingVolume = new TradeVolume(volume, volumeLots),
                        RequestedVolume = new TradeVolume(volume, volumeLots),
                        Price = price,
                        StopLoss = sl ?? double.NaN,
                        TakeProfit = tp ?? double.NaN,
                        Comment = comment,
                        Tag = tag
                    };
                    var orderModel = new OrderAccessor(orderToOpen);
                    resultEntity = new TradeResultEntity(result.ResultCode, orderModel);
                }

                LogOrderOpenResults(resultEntity);

                return resultEntity;
            }
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            var orderToCancel = account.Orders.GetOrderOrNull(orderId);
            if (orderToCancel == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            logger.PrintTrade("Canceling order #" + orderId);

            using (var waitHandler = new TaskProxy<CancelResult>())
            {
                api.CancelOrder(waitHandler, orderId, orderToCancel.Entity.ClientOrderId, orderToCancel.Side);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    account.Orders.Remove(orderId);
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " canceled");
                }
                else
                    logger.PrintTrade("→ FAILED Canceling order #" + orderId + " error=" + result.ResultCode);

                return new TradeResultEntity(result.ResultCode, orderToCancel);
            }
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? closeVolumeLots)
        {
            double? closeVolume = null;

            var orderToClose = account.Orders.GetOrderOrNull(orderId);
            if (orderToClose == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var smbMetadata = symbols.List[orderToClose.Symbol];
            if (smbMetadata.IsNull)
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

            if (closeVolumeLots != null)
            {
                closeVolumeLots = RoundVolume(closeVolumeLots, smbMetadata);
                closeVolume = ConvertVolume(closeVolumeLots.Value, smbMetadata);
            }

            logger.PrintTrade("Closing order #" + orderId);

            using (var waitHandler = new TaskProxy<CloseResult>())
            {
                api.CloseOrder(waitHandler, orderId, closeVolume);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    orderToClose.Entity.RemainingVolume = ModifyVolume(orderToClose.Entity.RemainingVolume, -result.ExecVolume, smbMetadata);

                    if (orderToClose.RemainingVolume <= 0)
                        account.Orders.Remove(orderId);

                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed");

                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
                else
                {
                    logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
            }
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            var orderToModify = account.Orders.GetOrderOrNull(orderId);
            if (orderToModify == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            var smbMetadata = symbols.List[orderToModify.Symbol];
            if (smbMetadata.IsNull)
                return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

            double orderVolume = ConvertVolume(orderToModify.RequestedVolume, smbMetadata);
            price = RoundPrice(price, smbMetadata, orderToModify.Side);
            sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
            tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

            logger.PrintTrade("Modifying order #" + orderId);

            using (var waitHandler = new TaskProxy<OpenModifyResult>())
            {
                api.ModifyOrder(waitHandler, orderId, orderToModify.Entity.ClientOrderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                    price, orderVolume, tp, sl, comment);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    account.Orders.Replace(result.NewOrder);
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " modified");
                    return new TradeResultEntity(result.ResultCode, orderToModify);
                }
                else
                {
                    logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToModify);
                }
            }
        }

        private Task<OrderCmdResult> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<OrderCmdResult>(new TradeResultEntity(code));
        }

        private double ConvertVolume(double volumeInLots, Symbol smbMetadata)
        {
            return smbMetadata.ContractSize * volumeInLots;
        }

        private double RoundVolume(double volumeInLots, Symbol smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        private double? RoundVolume(double? volumeInLots, Symbol smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        private double RoundPrice(double price, Symbol smbMetadata, OrderSide side)
        {
            return side == OrderSide.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        private double? RoundPrice(double? price, Symbol smbMetadata, OrderSide side)
        {
            return side == OrderSide.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        private TradeVolume ModifyVolume(TradeVolume oldVol, double byLots, Symbol smbInfo)
        {
            var lotSize = smbInfo.ContractSize;
            var byUnits = lotSize * byLots;
            return new TradeVolume(oldVol.Units - byUnits, oldVol.Lots - byLots);
        }

        #region Logging

        private void LogOrderOpening(string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Executing ");
            AppendOrderParams(logEntry, " Order to ", symbol, type, side, volumeLots, price, sl, tp);
            logger.PrintTrade(logEntry.ToString());
        }

        private void LogOrderOpenResults(OrderCmdResult result)
        {
            var order = result.ResultingOrder;
            StringBuilder logEntry = new StringBuilder();

            if (result.IsCompleted)
            {
                logEntry.Append("→ SUCCESS: Opened ");
                if (order != null)
                {
                    if (!double.IsNaN(order.LastFillPrice))
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, " ", order.Symbol, order.Type, order.Side,
                            order.LastFillVolume, order.LastFillPrice, order.StopLoss, order.TakeProfit);
                    }
                    else
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, " ", order.Symbol, order.Type, order.Side,
                            order.RemainingVolume, order.Price, order.StopLoss, order.TakeProfit);
                    }

                }
                else
                    logEntry.Append("Null Order");
            }
            else
            {
                logEntry.Append("→ FAILED Executing ");
                if (order != null)
                {
                    AppendOrderParams(logEntry, " Order to ", order.Symbol, order.Type, order.Side,
                        order.RemainingVolume, order.Price, order.StopLoss, order.TakeProfit);
                    logEntry.Append(" error=").Append(result.ResultCode);
                }
                else
                    logEntry.Append("Null Order");
            }

            logger.PrintTrade(logEntry.ToString());
        }

        private void AppendOrderParams(StringBuilder logEntry, string sufix, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            logEntry.Append(type)
                .Append(sufix).Append(side)
                .Append(" ").Append(volumeLots)
                .Append(" ").Append(symbol);

            if (tp != null || sl != null)
            {
                logEntry.Append(" (");
                if (sl != null)
                    logEntry.Append("SL:").Append(sl.Value);
                if (sl != null && tp != null)
                    logEntry.Append(", ");
                if (tp != null)
                    logEntry.Append("TP:").Append(tp.Value);

                logEntry.Append(")");
            }

            if (!double.IsNaN(price) && price != 0)
                logEntry.Append(" at price ").Append(price);
        }

        #endregion
    }
}
