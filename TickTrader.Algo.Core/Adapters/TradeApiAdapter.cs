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
        private SymbolsCollection symbols;
        private AccountAccessor account;
        private PluginLoggerAdapter logger;

        public TradeApiAdapter(SymbolsCollection symbols, AccountAccessor account, PluginLoggerAdapter logger)
        {
            this.symbols = symbols;
            this.account = account;
            this.logger = logger;
            api = Null.TradeApi;
        }

        public ITradeApi ExternalApi
        {
            get => api;
            set => api = value ?? throw new InvalidOperationException("TradeApi cannot be null!");
        }

        public ICalculatorApi Calc { get; set; }
        public IPluginPermissions Permissions { get; set; }
        public string IsolationTag { get; set; }

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
            string encodedTag = CompositeTag.NewTag(IsolationTag, tag);
            SymbolAccessor smbMetadata = null;

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
                InstanceId = IsolationTag,
            };

            try
            {
                ValidateTradePersmission();
                smbMetadata = GetSymbolOrThrow(symbol);
                ValidateTradeEnabled(smbMetadata);

                ValidateVolumeLots(volumeLots, smbMetadata);
                ValidateMaxVisibleVolumeLots(maxVisibleVolumeLots, smbMetadata, orderType, volumeLots);
                ValidateOptions(options, orderType);
                volumeLots = RoundVolume(volumeLots, smbMetadata);
                maxVisibleVolumeLots = RoundVolume(maxVisibleVolumeLots, smbMetadata);
                double volume = ConvertVolume(volumeLots, smbMetadata);
                double? maxVisibleVolume = ConvertNullableVolume(maxVisibleVolumeLots, smbMetadata);
                price = RoundPrice(price, smbMetadata, side);
                stopPrice = RoundPrice(stopPrice, smbMetadata, side);
                sl = RoundPrice(sl, smbMetadata, side);
                tp = RoundPrice(tp, smbMetadata, side);


                ValidatePrice(price, orderType == OrderType.Limit || orderType == OrderType.StopLimit);
                ValidateStopPrice(stopPrice, orderType == OrderType.Stop || orderType == OrderType.StopLimit);
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
                    Tag = encodedTag,
                    Expiration = expiration
                };

                ValidateMargin(request, smbMetadata);

                logger.LogOrderOpening(logRequest, smbMetadata);

                var orderResp = await api.OpenOrder(isAysnc, request);

                if (orderResp.ResultCode != OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, orderToOpen, orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, new OrderAccessor(orderResp.ResultingOrder, smbMetadata, account.Leverage), orderResp.TransactionTime);
            }
            catch (OrderValidationError ex)
            {
                resultEntity = new OrderResultEntity(ex.ErrorCode, orderToOpen, null) { IsServerResponse = false };
            }

            logger.LogOrderOpenResults(resultEntity, logRequest, smbMetadata);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            Order orderToCancel = null;

            try
            {
                ValidateTradePersmission();
                orderToCancel = GetOrderOrThrow(orderId);
                var smbMetadata = GetSymbolOrThrow(orderToCancel.Symbol);
                ValidateTradeEnabled(smbMetadata);

                logger.PrintTrade("[Out] Canceling order #" + orderId);

                var request = new CancelOrderRequest { OrderId = orderId, Side = orderToCancel.Side };
                var result = await api.CancelOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                    logger.PrintTradeSuccess("[In] SUCCESS: Order #" + orderId + " canceled");
                else
                    logger.PrintTradeFail("[In] FAILED Canceling order #" + orderId + " error=" + result.ResultCode);

                return new OrderResultEntity(result.ResultCode, orderToCancel, result.TransactionTime);
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTradeFail("[Self] FAILED Canceling order #" + orderId + " error=" + ex.ErrorCode);
                return new OrderResultEntity(ex.ErrorCode, orderToCancel, null) { IsServerResponse = false };
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
                ValidateTradeEnabled(smbMetadata);

                if (closeVolumeLots != null)
                {
                    closeVolumeLots = RoundVolume(closeVolumeLots, smbMetadata);
                    ValidateVolumeLots(closeVolumeLots, smbMetadata);
                    closeVolume = ConvertVolume(closeVolumeLots.Value, smbMetadata);
                }

                ValidateVolume(closeVolume);

                logger.PrintTrade("[Out] Closing order #" + orderId);

                var request = new CloseOrderRequest { OrderId = orderId, Volume = closeVolume };

                var result = await api.CloseOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    logger.PrintTradeSuccess("[In] SUCCESS: Order #" + orderId + " closed");
                    return new OrderResultEntity(result.ResultCode, new OrderAccessor(result.ResultingOrder, smbMetadata, account.Leverage), result.TransactionTime);
                }
                else
                {
                    logger.PrintTradeFail("[Out] FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new OrderResultEntity(result.ResultCode, orderToClose, result.TransactionTime);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTradeFail("[Self] FAILED Closing order #" + orderId + " error=" + ex.ErrorCode);
                return new OrderResultEntity(ex.ErrorCode, orderToClose, null) { IsServerResponse = false };
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

                var smbMetadata = GetSymbolOrThrow(orderToClose.Symbol);
                ValidateTradeEnabled(smbMetadata);

                logger.PrintTrade("[Out] Closing order #" + orderId + " by order #" + byOrderId);

                var request = new CloseOrderRequest { OrderId = orderId, ByOrderId = byOrderId };

                var result = await api.CloseOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    logger.PrintTradeSuccess("[In] SUCCESS: Order #" + orderId + " closed by order #" + byOrderId);
                    return new OrderResultEntity(result.ResultCode, orderToClose, result.TransactionTime);
                }
                else
                {
                    logger.PrintTradeFail("[In] FAILED Closing order #" + orderId + " error=" + result.ResultCode);
                    return new OrderResultEntity(result.ResultCode, orderToClose, result.TransactionTime);
                }
            }
            catch (OrderValidationError ex)
            {
                logger.PrintTradeFail("[Self] FAILED Closing order #" + orderId + " by order #" + byOrderId + " error=" + ex.ErrorCode);
                return new OrderResultEntity(ex.ErrorCode, orderToClose, null) { IsServerResponse = false };
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
            SymbolAccessor smbMetadata = null;

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
                TakeProfit = tp,
            };

            try
            {
                ValidateTradePersmission();

                orderToModify = GetOrderOrThrow(orderId);
                smbMetadata = GetSymbolOrThrow(orderToModify.Symbol);
                var orderType = orderToModify.Type;

                // update mock request values
                logRequest.Symbol = orderToModify.Symbol;
                logRequest.Type = orderToModify.Type;
                logRequest.Side = orderToModify.Side;
                logRequest.CurrentVolume = orderToModify.RemainingVolume;

                ValidateOptions(options, orderType);
                ValidateTradeEnabled(smbMetadata);

                //if (orderType == OrderType.Limit || orderType == OrderType.StopLimit)
                //    ValidatePrice(price);

                //if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                //    ValidateStopPrice(stopPrice);

                double orderVolume = ConvertVolume(orderToModify.RemainingVolume, smbMetadata);
                double orderVolumeInLots = orderVolume / GetSymbolOrThrow(orderToModify.Symbol).ContractSize;

                ValidateVolumeLots(volume, smbMetadata);
                ValidateMaxVisibleVolumeLots(maxVisibleVolume, smbMetadata, orderType, volume ?? orderVolumeInLots);
                volume = RoundVolume(volume, smbMetadata);
                maxVisibleVolume = RoundVolume(maxVisibleVolume, smbMetadata);
                double? newOrderVolume = ConvertNullableVolume(volume, smbMetadata);
                double? orderMaxVisibleVolume = ConvertNullableVolume(maxVisibleVolume, smbMetadata);
                price = RoundPrice(price, smbMetadata, orderToModify.Side);
                stopPrice = RoundPrice(stopPrice, smbMetadata, orderToModify.Side);
                sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
                tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

                ValidatePrice(price, false);
                ValidateStopPrice(stopPrice, false);
                ValidateVolume(newOrderVolume);
                ValidateMaxVisibleVolume(orderMaxVisibleVolume);
                ValidateTp(tp);
                ValidateSl(sl);

                // update mock request values
                logRequest.NewVolume = volume;
                logRequest.Price = price;
                logRequest.StopPrice = stopPrice;
                logRequest.StopLoss = sl;
                logRequest.TakeProfit = tp;

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
                    TakeProfit = tp,
                    Comment = comment,
                    Expiration = expiration,
                    MaxVisibleVolume = orderMaxVisibleVolume,
                    Options = options
                };

                ValidateMargin(request, smbMetadata);

                logger.LogOrderModifying(logRequest, smbMetadata);

                var result = await api.ModifyOrder(isAysnc, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    resultEntity = new OrderResultEntity(result.ResultCode, new OrderAccessor(result.ResultingOrder, smbMetadata, account.Leverage), result.TransactionTime);
                }
                else
                {
                    resultEntity = new OrderResultEntity(result.ResultCode, orderToModify, result.TransactionTime);
                }
            }
            catch (OrderValidationError ex)
            {
                resultEntity = new OrderResultEntity(ex.ErrorCode, orderToModify, null) { IsServerResponse = false };
            }

            logger.LogOrderModifyResults(logRequest, smbMetadata, resultEntity);
            return resultEntity;
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

        private SymbolAccessor GetSymbolOrThrow(string symbolName)
        {
            var smbMetadata = symbols.GetOrDefault(symbolName);
            if (smbMetadata == null)
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

        private bool IsInvalidValue(double val)
        {
            // Because TTS uses decimal for financial calculation
            // We need to validate that out value is inside of decimal range otherwise exception will be thrown
            // Values like 1E-30 which go below decimal precision will be converted to zero normally
            if (val > (double)decimal.MaxValue || val < (double)decimal.MinValue)
                return true;

            return double.IsNaN(val) || double.IsInfinity(val);
        }

        private void ValidateVolume(double volume)
        {
            if (volume <= 0 || IsInvalidValue(volume))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidateVolume(double? volume)
        {
            if (!volume.HasValue)
                return;

            if (volume <= 0 || IsInvalidValue(volume.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidateMaxVisibleVolume(double? volume)
        {
            if (!volume.HasValue)
                return;

            if (volume < 0 || IsInvalidValue(volume.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectMaxVisibleVolume);
        }

        private void ValidateVolumeLots(double volumeLots, Symbol smbMetadata)
        {
            if (volumeLots <= 0 || volumeLots < smbMetadata.MinTradeVolume || volumeLots > smbMetadata.MaxTradeVolume)
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidateVolumeLots(double? volumeLots, Symbol smbMetadata)
        {
            if (!volumeLots.HasValue)
                return;

            if (volumeLots <= 0 || volumeLots < smbMetadata.MinTradeVolume || volumeLots > smbMetadata.MaxTradeVolume)
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        }

        private void ValidateMaxVisibleVolumeLots(double? maxVisibleVolumeLots, Symbol smbMetadata, OrderType orderType, double? volumeLots)
        {
            if (!maxVisibleVolumeLots.HasValue)
                return;

            var isIncorrectMaxVisibleVolume = orderType == OrderType.Stop
                || maxVisibleVolumeLots < 0
                || (maxVisibleVolumeLots > 0 && maxVisibleVolumeLots < smbMetadata.MinTradeVolume)
                || maxVisibleVolumeLots > smbMetadata.MaxTradeVolume;

            if (isIncorrectMaxVisibleVolume)
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectMaxVisibleVolume);
        }

        private void ValidatePrice(double? price, bool required)
        {
            if (required && price == null)
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectPrice);

            if (!price.HasValue)
                return;

            if (price <= 0 || IsInvalidValue(price.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectPrice);
        }

        private void ValidateStopPrice(double? price, bool required)
        {
            if (required && price == null)
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectStopPrice);

            if (!price.HasValue)
                return;

            if (price <= 0 || IsInvalidValue(price.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectStopPrice);
        }

        private void ValidateTp(double? tp)
        {
            if (tp == null)
                return;

            if (tp.Value <= 0 || IsInvalidValue(tp.Value))
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectTp);
        }

        private void ValidateSl(double? sl)
        {
            if (sl == null)
                return;

            if (sl.Value <= 0 || IsInvalidValue(sl.Value))
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
            if (!Permissions.TradeAllowed)
                throw new OrderValidationError(OrderCmdResultCodes.TradeNotAllowed);
        }

        private void ValidateOptions(OrderExecOptions? options, OrderType orderType)
        {
            if (options == null)
                return;

            var isOrderTypeCompatibleToIoC = orderType == OrderType.Limit || orderType == OrderType.StopLimit;

            if (options == OrderExecOptions.ImmediateOrCancel && !isOrderTypeCompatibleToIoC)
                throw new OrderValidationError(OrderCmdResultCodes.Unsupported);
        }

        private void ValidateMargin(OpenOrderRequest request, SymbolAccessor symbol)
        {
            //var orderEntity = new OrderEntity("-1")
            //{
            //    Symbol = request.Symbol,
            //    InitialType = request.Type,
            //    Type = request.Type,
            //    Side = request.Side,
            //    Price = request.Price,
            //    StopPrice = request.StopPrice,
            //    RequestedVolume = request.Volume,
            //    RemainingVolume = request.Volume,
            //    MaxVisibleVolume = request.MaxVisibleVolume,
            //    StopLoss = request.StopLoss,
            //    TakeProfit = request.TakeProfit,
            //    Expiration = request.Expiration,
            //    Options = request.Options,
            //};

            var isHidden = OrderEntity.IsHiddenOrder(request.MaxVisibleVolume);

            if (Calc != null && !Calc.HasEnoughMarginToOpenOrder(symbol.Name, request.Volume, request.Type, request.Side, isHidden))
                throw new OrderValidationError(OrderCmdResultCodes.NotEnoughMoney);
        }

        private void ValidateMargin(ReplaceOrderRequest request, SymbolAccessor symbol)
        {
            var oldOrder = account.Orders.GetOrderOrNull(request.OrderId);

            //var orderEntity = new OrderEntity(request.OrderId)
            //{
            //    Symbol = request.Symbol,
            //    Type = request.Type,
            //    Side = request.Side,
            //    Price = request.Price ?? oldOrder.Price,
            //    StopPrice = request.StopPrice ?? oldOrder.StopPrice,
            //    RequestedVolume = request.NewVolume ?? request.CurrentVolume,
            //    RemainingVolume = request.NewVolume ?? request.CurrentVolume,
            //    MaxVisibleVolume = request.MaxVisibleVolume ?? request.MaxVisibleVolume,
            //    StopLoss = request.StopLoss ?? oldOrder.StopLoss,
            //    TakeProfit = request.TakeProfit ?? oldOrder.TakeProfit,
            //    Expiration = request.Expiration ?? oldOrder.Expiration,
            //    Options = request.Options ?? oldOrder.Entity.Options,
            //};

            var newIsHidden = OrderEntity.IsHiddenOrder(request.MaxVisibleVolume);

            if (Calc != null && !Calc.HasEnoughMarginToModifyOrder(oldOrder.Entity, request.NewVolume.Value, newIsHidden))
                throw new OrderValidationError(OrderCmdResultCodes.NotEnoughMoney);
        }

        private void ApplyHiddenServerLogic(OrderEntity order, SymbolAccessor symbol)
        {
            //add prices for market orders
            if (order.Type == OrderType.Market && order.Price == null)
            {
                order.Price = order.Side == OrderSide.Buy ? symbol.Ask : symbol.Bid;
                if (account.Type == AccountTypes.Cash)
                {
                    order.Price += symbol.Point * symbol.DefaultSlippage * (order.Side == OrderSide.Buy ? 1 : -1);
                }
            }

            //convert order types for cash accounts
            if (account.Type == AccountTypes.Cash)
            {
                switch (order.Type)
                {
                    case OrderType.Market:
                        order.Type = OrderType.Limit;
                        order.Options |= OrderExecOptions.ImmediateOrCancel;
                        break;
                    case OrderType.Stop:
                        order.Type = OrderType.StopLimit;
                        order.Price = order.StopPrice + symbol.Point * symbol.DefaultSlippage * (order.Side == OrderSide.Buy ? -1 : 1);
                        break;
                }
            }
        }

        #endregion

        #region Logging

       

        #endregion
    }

    internal class OrderValidationError : Exception
    {
        public OrderValidationError(OrderCmdResultCodes code)
            : this(null, code)
        {
        }

        public OrderValidationError(string message, OrderCmdResultCodes code)
            : base(message ?? "Validation error: " + code)
        {
            ErrorCode = code;
        }

        public OrderCmdResultCodes ErrorCode { get; }
    }

}
