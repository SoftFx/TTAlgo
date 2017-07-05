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
        private string _isolationTag;

        public TradeApiAdapter(ITradeApi api, SymbolProvider symbols, AccountEntity account, PluginLoggerAdapter logger, string isolationTag)
        {
            this.api = api;
            this.symbols = symbols;
            this.account = account;
            this.logger = logger;
            this._isolationTag = isolationTag;
        }

        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            string isolationTag = CompositeTag.NewTag(_isolationTag, tag);

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
                api.OpenOrder(waitHandler, symbol, type, side, price, volume, tp, sl, comment, options, isolationTag);
                var result = await waitHandler.LocalTask.ConfigureAwait(isAysnc);

                TradeResultEntity resultEntity;
                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    account.Orders.Add(result.NewOrder);
                    resultEntity = new TradeResultEntity(result.ResultCode, result.NewOrder);
                }
                else
                {
                    var orderToOpen = new OrderEntity("-1")
                    {
                        Symbol = symbol,
                        Type = type,
                        Side = side,
                        RemainingVolume = volumeLots,
                        RequestedVolume = volumeLots,
                        Price = price,
                        StopLoss = sl ?? double.NaN,
                        TakeProfit = tp ?? double.NaN,
                        Comment = comment,
                        Tag = isolationTag
                    };
                    resultEntity = new TradeResultEntity(result.ResultCode, orderToOpen);
                }

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
                var smbMetadata = symbols.List[orderToClose.Symbol];
                if (smbMetadata.IsNull)
                    return new TradeResultEntity(OrderCmdResultCodes.SymbolNotFound);

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
                    var orderClone = new OrderEntity(orderToClose);
                    orderClone.RemainingVolume -= result.ExecVolume;

                    if (orderClone.RemainingVolume <= 0)
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

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            Order orderToModify = account.Orders.GetOrderOrNull(orderId);
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
                api.ModifyOrder(waitHandler, orderId, ((OrderEntity)orderToModify).ClientOrderId, orderToModify.Symbol, orderToModify.Type, orderToModify.Side,
                    price, orderVolume, tp, sl, comment);
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

        #region Logging

        private void LogOrderOpening(string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append("Opening ");
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
                logEntry.Append("→ FAILED Opening ");
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
