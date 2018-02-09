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

            return OpenOrder(isAysnc, symbol, type, side, volumeLots, null, limPrice, stopPrice, sl, tp, comment, options, tag, null);
        }

        public async Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType orderType, OrderSide side, double volumeLots, double? maxVisibleVolumeLots, double? price, double? stopPrice,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag, DateTime? expiration)
        {
            OrderResultEntity resultEntity;
            string isolationTag = CompositeTag.NewTag(_isolationTag, tag);

            var logRequest = new OpenOrderRequest // mock request for logging
            {
                Symbol = symbol,
                Type = orderType,
                Side = side,
                Price = price,
                StopPrice = stopPrice,
                Volume = volumeLots,
                TakeProfit = tp,
                StopLoss = sl,
            };

            var orderToOpen = new MockOrder(symbol, orderType, side)
            {
                RemainingVolume = volumeLots,
                RequestedVolume = volumeLots,
                MaxVisibleVolume = maxVisibleVolumeLots ?? double.NaN,
                Price = price ?? double.NaN,
                StopPrice = stopPrice ?? double.NaN,
                StopLoss = sl ?? double.NaN,
                TakeProfit = tp ?? double.NaN,
                Comment = comment,
                Tag = tag,
                InstanceId = _isolationTag,
            };

            try
            {
                var smbMetadata = GetSymbolOrThrow(symbol);
                ValidateTradePersmission();
                ValidateTradeEnabled(smbMetadata);

                volumeLots = RoundVolume(volumeLots, smbMetadata);
                maxVisibleVolumeLots = RoundVolume(maxVisibleVolumeLots, smbMetadata);
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
                ValidateMaxVisibleVolume(maxVisibleVolume);
                ValidateTp(tp);
                ValidateSl(sl);

                // Update mock request values
                logRequest.Volume = volumeLots;
                logRequest.Price = price;
                logRequest.StopPrice = stopPrice;
                logRequest.StopLoss = sl;
                logRequest.TakeProfit = tp;

                LogOrderOpening(logRequest);

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
                    Tag = isolationTag,
                    Expiration = expiration
                };

                var orderResp = await api.OpenOrder(isAysnc, request);

                if (orderResp.ResultCode != OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, orderToOpen);
                else
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, new OrderAccessor(orderResp.ResultingOrder, smbMetadata));
            }
            catch (OrderValidationError ex)
            {
                resultEntity = new OrderResultEntity(ex.ErrorCode, orderToOpen);
            }

            LogOrderOpenResults(resultEntity, logRequest);
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
                    logger.PrintTradeSuccess("→ SUCCESS: Order #" + orderId + " canceled");
                else
                    logger.PrintTradeFail("→ FAILED Canceling order #" + orderId + " error=" + result.ResultCode);

                return new OrderResultEntity(result.ResultCode, orderToCancel);
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTradeFail("→ FAILED Canceling order #" + orderId + " error=" + ex.ErrorCode);
                return new OrderResultEntity(ex.ErrorCode, orderToCancel);
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
                    logger.PrintTradeSuccess("→ SUCCESS: Order #" + orderId + " closed");
                    return new OrderResultEntity(result.ResultCode, new OrderAccessor(result.ResultingOrder, smbMetadata));
                }
                else
                {
                    logger.PrintTradeFail("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new OrderResultEntity(result.ResultCode, orderToClose);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTradeFail("→ FAILED Closing order #" + orderId + " error=" + ex.ErrorCode);
                return new OrderResultEntity(ex.ErrorCode, orderToClose);
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
                    logger.PrintTradeSuccess("→ SUCCESS: Order #" + orderId + " closed by order #" + byOrderId);
                    return new OrderResultEntity(result.ResultCode, orderToClose);
                }
                else
                {
                    logger.PrintTradeFail("→ FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new OrderResultEntity(result.ResultCode, orderToClose);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTradeFail("→ FAILED Closing order #" + orderId + " by order #" + byOrderId + " error=" + ex.ErrorCode);
                return new OrderResultEntity(ex.ErrorCode, orderToClose);
            }
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            return ModifyOrder(isAysnc, orderId, price, null, null, sl, tp, comment, null, null, null);
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double? price, double? stopPrice, double? maxVisibleVolume, double? sl, double? tp, string comment, DateTime? expiration, double? volume, OrderExecOptions? options)
        {
            OrderResultEntity resultEntity;
            Order orderToModify = null;

            var logRequest = new ReplaceOrderRequest // mock request for logging
            {
                OrderId = orderId,
                Symbol = "AAAAAA",
                Type = OrderType.Market,
                Side = OrderSide.Buy,
                CurrentVolume = double.NaN,
                NewVolume = volume,
                Price = price,
                StopPrice = stopPrice,
                StopLoss = sl,
                TrakeProfit = tp,
            };

            try
            {
                ValidateTradePersmission();

                orderToModify = GetOrderOrThrow(orderId);
                var smbMetadata = GetSymbolOrThrow(orderToModify.Symbol);
                var orderType = orderToModify.Type;

                ValidateOptions(options, orderType);
                ValidateTradeEnabled(smbMetadata);

                //if (orderType == OrderType.Limit || orderType == OrderType.StopLimit)
                //    ValidatePrice(price);

                //if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                //    ValidateStopPrice(stopPrice);

                double orderVolume = ConvertVolume(orderToModify.RemainingVolume, smbMetadata);
                volume = RoundVolume(volume, smbMetadata);
                maxVisibleVolume = RoundVolume(maxVisibleVolume, smbMetadata);
                double? newOrderVolume = ConvertNullableVolume(volume, smbMetadata);
                double? orderMaxVisibleVolume = ConvertNullableVolume(maxVisibleVolume, smbMetadata);
                price = RoundPrice(price, smbMetadata, orderToModify.Side);
                stopPrice = RoundPrice(stopPrice, smbMetadata, orderToModify.Side);
                sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
                tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

                if (orderType == OrderType.Limit || orderType == OrderType.StopLimit)
                    ValidatePrice(price);

                if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    ValidateStopPrice(stopPrice);

                ValidateVolume(newOrderVolume);
                ValidateMaxVisibleVolume(orderMaxVisibleVolume);
                ValidateTp(tp);
                ValidateSl(sl);

                // update mock request values
                logRequest.Symbol = orderToModify.Symbol;
                logRequest.Type = orderToModify.Type;
                logRequest.Side = orderToModify.Side;
                logRequest.CurrentVolume = orderToModify.RemainingVolume;
                logRequest.NewVolume = volume;
                logRequest.Price = price;
                logRequest.StopPrice = stopPrice;
                logRequest.StopLoss = sl;
                logRequest.TrakeProfit = tp;
                LogOrderModifying(logRequest);

                var request = new ReplaceOrderRequest
                {
                    OrderId = orderId,
                    Symbol = orderToModify.Symbol,
                    Type = orderToModify.Type,
                    Side = orderToModify.Side,
                    CurrentVolume = orderVolume,
                    NewVolume = newOrderVolume,
                    Price = price,
                    StopPrice = stopPrice,
                    StopLoss = sl,
                    TrakeProfit = tp,
                    Comment = comment,
                    Expiration = expiration,
                    MaxVisibleVolume = orderMaxVisibleVolume,
                    Options = options
                };

                var result = await api.ModifyOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    resultEntity = new OrderResultEntity(result.ResultCode, new OrderAccessor(result.ResultingOrder, smbMetadata));
                }
                else
                {
                    resultEntity = new OrderResultEntity(result.ResultCode, orderToModify);
                }
            }
            catch (OrderValidationError ex)
            {
                resultEntity = new OrderResultEntity(ex.ErrorCode, orderToModify);
            }

            LogOrderModifyResults(logRequest, resultEntity);
            return resultEntity;
        }

        private Task<OrderCmdResult> CreateResult(OrderCmdResultCodes code)
        {
            return Task.FromResult<OrderCmdResult>(new OrderResultEntity(code));
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

        private void ValidateVolume(double? volume)
        {
            if (!volume.HasValue)
                return;

            if (volume <= 0 || double.IsNaN(volume.Value) || double.IsInfinity(volume.Value))
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

        private void ValidateOptions(OrderExecOptions? options, OrderType orderType)
        {
            if (options == null)
                return;

            if (options.Value.HasFlag(OrderExecOptions.ImmediateOrCancel) && (orderType != OrderType.StopLimit))
                throw new OrderValidationError(OrderCmdResultCodes.Unsupported);
        }

        #endregion

        #region Logging

        private void LogOrderOpening(OpenOrderRequest request)
        {
            var logEntry = new StringBuilder();
            logEntry.Append("Opening ");
            AppendOrderParams(logEntry, " Order to ", request);

            logger.PrintTrade(logEntry.ToString());
        }

        private void LogOrderOpenResults(OrderResultEntity result, OpenOrderRequest request)
        {
            var logEntry = new StringBuilder();

            if (result.IsCompleted)
            {
                var order = result.ResultingOrder;
                logEntry.Append("→ SUCCESS: Opened ");
                if (order != null)
                {
                    if (!double.IsNaN(order.LastFillPrice) && !double.IsNaN(order.LastFillVolume))
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendIocOrderParams(logEntry, " ", order);
                    }
                    else
                    {
                        logEntry.Append("#").Append(order.Id).Append(" ");
                        AppendOrderParams(logEntry, " ", order);
                    }
                }
                else
                    logEntry.Append("Null Order");

                logger.PrintTradeSuccess(logEntry.ToString());
            }
            else
            {
                logEntry.Append("→ FAILED Opening ");
                AppendOrderParams(logEntry, " Order to ", request);
                logEntry.Append(" error=").Append(result.ResultCode);

                logger.PrintTradeFail(logEntry.ToString());
            }
        }

        private void LogOrderModifying(ReplaceOrderRequest request)
        {
            var logEntry = new StringBuilder();
            logEntry.Append("Modifying order #").Append(request.OrderId).Append(" to ");
            AppendOrderParams(logEntry, " ", request);

            logger.PrintTrade(logEntry.ToString());
        }

        private void LogOrderModifyResults(ReplaceOrderRequest request, OrderResultEntity result)
        {
            var logEntry = new StringBuilder();

            if (result.IsCompleted)
            {
                logEntry.Append("→ SUCCESS: Modified order #").Append(request.OrderId).Append(" to ");
                if (result.ResultingOrder != null)
                {
                    AppendOrderParams(logEntry, " ", result.ResultingOrder);
                }
                else
                    logEntry.Append("Null Order");

                logger.PrintTradeSuccess(logEntry.ToString());
            }
            else
            {
                logEntry.Append("→ FAILED Modifying order #").Append(request.OrderId).Append(" to ");
                AppendOrderParams(logEntry, " ", request);
                logEntry.Append(" error=").Append(result.ResultCode);

                logger.PrintTradeFail(logEntry.ToString());
            }
        }

        private void AppendOrderParams(StringBuilder logEntry, string suffix, Order order)
        {
            AppendOrderParams(logEntry, suffix, order.Symbol, order.Type, order.Side, order.RemainingVolume, order.Price, order.StopLoss, order.TakeProfit);
        }

        private void AppendIocOrderParams(StringBuilder logEntry, string suffix, Order order)
        {
            AppendOrderParams(logEntry, suffix, order.Symbol, order.Type, order.Side, order.LastFillVolume, order.LastFillPrice, order.StopLoss, order.TakeProfit);
        }

        private void AppendOrderParams(StringBuilder logEntry, string suffix, OpenOrderRequest request)
        {
            AppendOrderParams(logEntry, suffix, request.Symbol, request.Type, request.Side, request.Volume, request.StopPrice ?? request.Price ?? double.NaN, request.StopLoss, request.TakeProfit);
        }

        private void AppendOrderParams(StringBuilder logEntry, string suffix, ReplaceOrderRequest request)
        {
            AppendOrderParams(logEntry, suffix, request.Symbol, request.Type, request.Side, request.NewVolume ?? request.CurrentVolume, request.StopPrice ?? request.Price ?? double.NaN, request.StopLoss, request.TrakeProfit);
        }

        private void AppendOrderParams(StringBuilder logEntry, string suffix, string symbol, OrderType type, OrderSide side, double volumeLots, double price, double? sl, double? tp)
        {
            logEntry.Append(type)
                .Append(suffix).Append(side)
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
