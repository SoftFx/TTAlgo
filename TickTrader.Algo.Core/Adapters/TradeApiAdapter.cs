using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : TradeCommands
    {
        private ITradeApi api;
        private SymbolProvider symbols;
        private AccountEntity account;
        private PluginLoggerAdapter logger;

        public TradeApiAdapter(ITradeApi api,  SymbolProvider symbols, AccountEntity account, PluginLoggerAdapter logger)
        {
            this.api = api;
            this.symbols = symbols;
            this.account = account;
            this.logger = logger;
        }

        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp, string comment)
        {
            OrderCmdResultCodes code;
            double volume = ConvertVolume(volumeLots, symbol, out code);
            if (code != OrderCmdResultCodes.Ok)
                return new TradeResultEntity(code);

            LogOrderOpening(symbol, type, side, volumeLots, price, sl, tp);

            using (var waitHandler = new TaskProxy<OpenModifyResult>())
            {
                api.OpenOrder(waitHandler, symbol, type, side, price, volume, tp, sl, comment);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                    account.Orders.Add(result.NewOrder);

                var resultEntity = new TradeResultEntity(result.ResultCode, result.NewOrder);

                LogOrderOpenResults(resultEntity);

                return resultEntity;
            }
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            Order orderToCancel = account.Orders.GetOrderOrNull(orderId);
            if (orderToCancel == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            logger.PrintTrade("Canceling order #" + orderId);

            using (var waitHandler = new TaskProxy<CancelResult>())
            {
                api.CancelOrder(waitHandler, orderId, ((OrderEntity)orderToCancel).ClientOrderId, orderToCancel.Side);
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

            Order orderToClose = account.Orders.GetOrderOrNull(orderId);
            if (orderToClose == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            if (closeVolumeLots != null)
            {
                OrderCmdResultCodes code;
                closeVolume = ConvertVolume(closeVolumeLots.Value, orderToClose.Symbol, out code);
                if (code != OrderCmdResultCodes.Ok)
                    return new TradeResultEntity(code);
            }

            logger.PrintTrade("Closing order #" + orderId);

            using (var waitHandler = new TaskProxy<CloseResult>())
            {
                api.CloseOrder(waitHandler, orderId, closeVolume);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    var orderClone = new OrderEntity(orderToClose);
                    orderClone.RemainingAmount -= result.ExecVolume;

                    if (orderClone.RemainingAmount <= 0)
                        account.Orders.Remove(orderId);
                    else
                        account.Orders.Replace(orderClone);

                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed");

                    return new TradeResultEntity(result.ResultCode, orderClone);
                }
                else
                {
                    logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
            }
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price,  double? sl, double? tp, string comment)
        {
            Order orderToModify = account.Orders.GetOrderOrNull(orderId);
            if (orderToModify == null)
                return new TradeResultEntity(OrderCmdResultCodes.OrderNotFound);

            logger.PrintTrade("Modifying order #" + orderId);

            using (var waitHandler = new TaskProxy<OpenModifyResult>())
            {
                api.ModifyOrder(waitHandler, orderId, ((OrderEntity)orderToModify).ClientOrderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                    price, orderToModify.RequestedAmount, tp, sl, comment);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    account.Orders.Replace(result.NewOrder);
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " modified");
                    return new TradeResultEntity(result.ResultCode, result.NewOrder);
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

        private double ConvertVolume(double volumeInLots, string symbolCode, out OrderCmdResultCodes rCode)
        {
            var smbMetatda = symbols.List[symbolCode];
            if (smbMetatda.IsNull)
            {
                rCode = OrderCmdResultCodes.SymbolNotFound;
                return double.NaN;
            }
            else
            {
                rCode = OrderCmdResultCodes.Ok;
                return smbMetatda.ContractSize * volumeInLots;
            }
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
                    logEntry.Append("#").Append(order.Id).Append(" ");
                    AppendOrderParams(logEntry, " ", order.Symbol, order.Type, order.Side,
                        order.RemainingAmount, order.Price, order.StopLoss, order.TakeProfit);
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
                        order.RemainingAmount, order.Price, order.StopLoss, order.TakeProfit);
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
