using System;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : TradeCommands
    {
        private ITradeApi api;
        private SymbolProvider symbols;
        private AccountAccessor account;
        private PluginLoggerAdapter logger;
        private string _isolationTag;
        private ITradePermissions _permissions;

        public TradeApiAdapter(ITradeApi api, SymbolProvider symbols, AccountAccessor account, PluginLoggerAdapter logger, ITradePermissions tradePermissions, string isolationTag)
        {
            this.api = api;
            this.symbols = symbols;
            this.account = account;
            this.logger = logger;
            this._isolationTag = isolationTag;
            this._permissions = tradePermissions;
        }

        public Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            var limPrice = type != OrderType.Stop ? price : (double?)null;
            var stopPrice = type == OrderType.Stop ? price : (double?)null;

            return OpenOrder(isAysnc, symbol, type, side, volumeLots, null, limPrice, stopPrice, sl, tp, comment, options, tag);
        }

        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType orderType, OrderSide side, double volumeLots, double? maxVisibleVolumeLots, double? price, double? stopPrice,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag, DateTime? expiration)
        {
            OrderCmdResult resultEntity;
            string isolationTag = CompositeTag.NewTag(_isolationTag, tag);

            var orderToOpen = new OrderEntity("-1")
            {
                Symbol = symbol,
                Type = orderType,
                Side = side,
                RemainingVolume = volumeLots,
                RequestedVolume = volumeLots,
                MaxVisibleVolume = maxVisibleVolumeLots ?? double.NaN,
                Price = price ?? double.NaN,
                StopPrice = stopPrice ?? double.NaN,
                StopLoss = sl ?? double.NaN,
                TakeProfit = tp ?? double.NaN,
                Comment = comment,
                UserTag = tag,
                InstanceId = _isolationTag,
            };

            try
            {
                var smbMetadata = GetSymbolOrThrow(symbol);
                ValidateTradePersmission();
                ValidateTradeEnabled(smbMetadata);

                volumeLots = RoundVolume(volumeLots, smbMetadata);
                double volume = ConvertVolume(volumeLots, smbMetadata);
                double? maxVisibleVolume = ConvertNullableVolume(maxVisibleVolumeLots, smbMetadata);
                price = RoundPrice(price, smbMetadata, side);
                stopPrice = RoundPrice(stopPrice, smbMetadata, side);
                sl = RoundPrice(sl, smbMetadata, side);
                tp = RoundPrice(tp, smbMetadata, side);

                if (orderType == OrderType.Limit || orderType == OrderType.StopLimit)
                    ValidatePrice(price);

                if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    ValidateStopPrice(stopPrice);

                ValidateVolume(volume);
                ValidateTp(tp);
                ValidateSl(sl);

                LogOrderOpening(symbol, orderType, side, volumeLots, (stopPrice ?? price) ?? double.NaN, sl, tp);

                var request = new OpenOrderRequest
                {
                    Symbol = symbol,
                    Type = orderType,
                    Side = side,
                    Price = price,
                    StopPrice = stopPrice,
                    Volume = volume,
                    MaxVisibleVolume = maxVisibleVolume,
                    TakeProfit = tp,
                    StopLoss = sl,
                    Comment = comment,
                    Options = options,
                    Tag = isolationTag
                };

                resultEntity = await api.OpenOrder(isAysnc, symbol, orderType, side, price, stopPrice, volume, maxVisibleVolume, tp, sl, comment, options, isolationTag, expiration);

                if (resultEntity.ResultCode != OrderCmdResultCodes.Ok)
                    resultEntity = new TradeResultEntity(resultEntity.ResultCode, new OrderAccessor(orderToOpen));
            }
            catch (OrderValidationError ex)
            {
                resultEntity = new TradeResultEntity(ex.ErrorCode, new OrderAccessor(orderToOpen));
            }

            LogOrderOpenResults(resultEntity);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            Order orderToCancel = null;

            try
            {
                ValidateTradePersmission();
                orderToCancel = GetOrderOrThrow(orderId);

                logger.PrintTrade("Canceling order #" + orderId);

                var request = new CancelOrderRequest { OrderId = orderId, Side = orderToCancel.Side };
                var result = await api.CancelOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " canceled");
                else
                    logger.PrintTrade("→ FAILED Canceling order #" + orderId + " error=" + result.ResultCode);

                return new TradeResultEntity(result.ResultCode, orderToCancel);
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTrade("→ FAILED Canceling order #" + orderId + " error=" + ex.ErrorCode);
                return new TradeResultEntity(ex.ErrorCode, orderToCancel);
            }
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAysnc, string orderId, double? closeVolumeLots)
        {
            Order orderToClose = null;

            try
            {
                ValidateTradePersmission();
                if (closeVolumeLots != null)
                    ValidateVolume(closeVolumeLots.Value);

                double? closeVolume = null;
                orderToClose = GetOrderOrThrow(orderId);
                var smbMetadata = GetSymbolOrThrow(orderToClose.Symbol);

                if (closeVolumeLots != null)
                {
                    closeVolumeLots = RoundVolume(closeVolumeLots, smbMetadata);
                    closeVolume = ConvertVolume(closeVolumeLots.Value, smbMetadata);
                }

                logger.PrintTrade("Closing order #" + orderId);

                var request = new CloseOrderRequest { OrderId = orderId, Volume = closeVolume };

                var result = await api.CloseOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed");
                    return new TradeResultEntity(result.ResultCode, result.ResultingOrder);
                }
                else
                {
                    logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + ex.ErrorCode);
                return new TradeResultEntity(ex.ErrorCode, orderToClose);
            }
        }

        public async Task<OrderCmdResult> CloseOrderBy(bool isAysnc, string orderId, string byOrderId)
        {
            Order orderToClose = null;

            try
            {
                ValidateTradePersmission();

                orderToClose = GetOrderOrThrow(orderId);
                Order orderByClose = GetOrderOrThrow(byOrderId);
                var request = new CloseOrderRequest { OrderId = orderId, ByOrderId = byOrderId };

                var result = await api.CloseOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " closed by order #" + byOrderId);
                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
                else
                {
                    logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToClose);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                return new TradeResultEntity(result.ResultCode, orderToClose);
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " by order #" + byOrderId + " error=" + result.ResultCode);
                return new TradeResultEntity(result.ResultCode, orderToClose);
                logger.PrintTrade("→ FAILED Closing order #" + orderId + " error=" + ex.ErrorCode);
                return new TradeResultEntity(ex.ErrorCode, orderToClose);
            }
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            return ModifyOrder(isAysnc, orderId, price, null, null, sl, tp, comment);
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double? price, double? stopPrice, double? maxVisibleVolume, double? sl, double? tp, string comment)
        {
            Order orderToModify = null;

            try
            {
                ValidateTradePersmission();

                orderToModify = GetOrderOrThrow(orderId);
                var smbMetadata = GetSymbolOrThrow(orderToModify.Symbol);
                var orderType = orderToModify.Type;

                ValidateTradeEnabled(smbMetadata);

                if (orderType == OrderType.Limit || orderType == OrderType.StopLimit)
                    ValidatePrice(price);

                if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    ValidateStopPrice(stopPrice);

                ValidateSl(sl);
                ValidateTp(tp);

                double orderVolume = ConvertVolume(orderToModify.RequestedVolume, smbMetadata);
                double? orderMaxVisibleVolume = maxVisibleVolume.HasValue ? ConvertVolume(maxVisibleVolume.Value, smbMetadata) : maxVisibleVolume;
                price = RoundPrice(price, smbMetadata, orderToModify.Side);
                stopPrice = RoundPrice(stopPrice, smbMetadata, orderToModify.Side);
                sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
                tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

                logger.PrintTrade("Modifying order #" + orderId);

                var request = new ReplaceOrderRequest
                {
                    OrderId = orderId,
                    Symbol = orderToModify.Symbol,
                    Type = orderToModify.Type,
                    Side = orderToModify.Side,
                    CurrentVolume = orderVolume,
                    Price = price,
                    StopPrice = stopPrice,
                    StopLoss = sl,
                    TrakeProfit = tp,
                    Comment = comment
                };

                var result = await api.ModifyOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    logger.PrintTrade("→ SUCCESS: Order #" + orderId + " modified");
                    return new TradeResultEntity(result.ResultCode, result.ResultingOrder);
                }
                else
                {
                    logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + result.ResultCode);
                    return new TradeResultEntity(result.ResultCode, orderToModify);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTrade("→ FAILED Modifying order #" + orderId + " error=" + ex.ErrorCode);
                return new TradeResultEntity(ex.ErrorCode, orderToModify);
            }
        }

        private Task<OrderCmdResult> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<OrderCmdResult>(new TradeResultEntity(code));
        }

        private double? ConvertNullableVolume(double? volumeInLots, Symbol smbMetadata)
        {
            if (volumeInLots == null)
                return null;
            return ConvertVolume(volumeInLots.Value, smbMetadata);
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

        #region Validation

        private Symbol GetSymbolOrThrow(string symbolName)
        {
            var smbMetadata = symbols.List[symbolName];
            if (smbMetadata.IsNull)
                throw new OrderValidationError(OrderCmdResultCodes.SymbolNotFound);
            return smbMetadata;
        }

        private Order GetOrderOrThrow(string orderId)
        {
            var order = account.Orders.GetOrderOrNull(orderId);
            if (order == null)
                throw new OrderValidationError(OrderCmdResultCodes.OrderNotFound);
            return order;
        }

        private void ValidateVolume(double volume)
        {
            if (volume <= 0 || double.IsNaN(volume) || double.IsInfinity(volume))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidateMaxVisibleVolume(double? volume)
        {
            if (!volume.HasValue)
                return;

            if (volume < 0 || double.IsNaN(volume.Value) || double.IsInfinity(volume.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectMaxVisibleVolume);
        }

        private void ValidatePrice(double? price)
        {
            if (price == null || price <= 0 || double.IsNaN(price.Value) || double.IsInfinity(price.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectPrice);
        }

        private void ValidateStopPrice(double? price)
        {
            if (price == null || price <= 0 || double.IsNaN(price.Value) || double.IsInfinity(price.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectStopPrice);
        }

        private void ValidateTp(double? tp)
        {
            if (tp == null)
                return;

            if (tp.Value <= 0 || double.IsNaN(tp.Value) || double.IsInfinity(tp.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectTp);
        }

        private void ValidateSl(double? sl)
        {
            if (sl == null)
                return;

            if (sl.Value <= 0 || double.IsNaN(sl.Value) || double.IsInfinity(sl.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectSl);
        }

        private void ValidateOrderId(string orderId)
        {
            long parsedId;
            if (!long.TryParse(orderId, out parsedId))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectOrderId);
        }

        private void ValidateTradeEnabled(Symbol smbMetadata)
        {
            if (!smbMetadata.IsTradeAllowed)
                throw new OrderValidationError(OrderCmdResultCodes.TradeNotAllowed);
        }

        private void ValidateTradePersmission()
        {
            if (!_permissions.TradeAllowed)
                throw new OrderValidationError(OrderCmdResultCodes.TradeNotAllowed);
        }

        #endregion

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

            if ((tp != null && !double.IsNaN(tp.Value)) || (sl != null && !double.IsNaN(sl.Value)))
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

    internal class OrderValidationError : Exception
    {
        public OrderValidationError(OrderCmdResultCodes code)
        {
            ErrorCode = code;
        }

        public OrderCmdResultCodes ErrorCode { get; }
    }

}
