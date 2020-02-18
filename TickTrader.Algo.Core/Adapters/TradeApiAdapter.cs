using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Calc;

namespace TickTrader.Algo.Core
{
    internal class TradeApiAdapter : TradeCommands
    {
        private ITradeApi _api;
        private SymbolsCollection _symbols;
        private AccountAccessor _account;
        private PluginLoggerAdapter _logger;

        public TradeApiAdapter(SymbolsCollection symbols, AccountAccessor account, PluginLoggerAdapter logger)
        {
            _symbols = symbols;
            _account = account;
            _logger = logger;
            _api = Null.TradeApi;
        }

        public ITradeApi ExternalApi
        {
            get => _api;
            set => _api = value ?? throw new InvalidOperationException("TradeApi cannot be null!");
        }

        public ICalculatorApi Calc { get; set; }
        public IPluginPermissions Permissions { get; set; }
        public string IsolationTag { get; set; }

        public Task<OrderCmdResult> OpenOrder(bool isAsync, string symbol, OrderType type, OrderSide side, double volumeLots, double price,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            var limPrice = type != OrderType.Stop ? price : (double?)null;
            var stopPrice = type == OrderType.Stop ? price : (double?)null;

            return OpenOrder(isAsync, symbol, type, side, volumeLots, null, limPrice, stopPrice, sl, tp, comment, options, tag, null);
        }

        public async Task<OrderCmdResult> OpenOrder(bool isAsync, string symbol, OrderType orderType, OrderSide side, double volumeLots, double? maxVisibleVolumeLots, double? price, double? stopPrice,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag, DateTime? expiration)
        {
            OrderResultEntity resultEntity;
            string encodedTag = CompositeTag.NewTag(IsolationTag, tag);

            var code = OrderCmdResultCodes.Ok;

            var request = new OpenOrderRequest
            {
                Symbol = symbol,
                Type = orderType,
                Side = side,
                VolumeLots = volumeLots,
                MaxVisibleVolumeLots = maxVisibleVolumeLots,
                Price = price,
                StopPrice = stopPrice,
                StopLoss = sl,
                TakeProfit = tp,
                Comment = comment,
                Options = options,
                Tag = encodedTag,
                Expiration = expiration,
            };

            PreprocessAndValidateOpenOrderRequest(request, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderOpening(request, smbMetadata);
                var orderResp = await _api.OpenOrder(isAsync, request);
                if (orderResp.ResultCode != OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, null, orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, new OrderAccessor(orderResp.ResultingOrder, smbMetadata, _account.Leverage), orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, null, null) { IsServerResponse = false };
            }

            _logger.LogOrderOpenResults(resultEntity, request, smbMetadata);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAsync, string orderId)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var request = new CancelOrderRequest { OrderId = orderId };

            PreprocessAndValidateCancelOrderRequest(request, out var orderToCancel, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderCanceling(request);

                var orderResp = await _api.CancelOrder(isAsync, request);

                resultEntity = new OrderResultEntity(orderResp.ResultCode, orderToCancel, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToCancel, null) { IsServerResponse = false };
            }

            _logger.LogOrderCancelResults(request, resultEntity);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAsync, string orderId, double? closeVolumeLots)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var request = new CloseOrderRequest { OrderId = orderId, VolumeLots = closeVolumeLots };

            PreprocessAndValidateCloseOrderRequest(request, out var orderToClose, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderClosing(request);

                var orderResp = await _api.CloseOrder(isAsync, request);

                if (orderResp.ResultCode == OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, new OrderAccessor(orderResp.ResultingOrder, smbMetadata, _account.Leverage), orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(orderResp.ResultCode, orderToClose, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToClose, null) { IsServerResponse = false };
            }

            _logger.LogOrderCloseResults(request, resultEntity);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseOrderBy(bool isAsync, string orderId, string byOrderId)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var request = new CloseOrderRequest { OrderId = orderId, ByOrderId = byOrderId };

            PreprocessAndValidateCloseOrderByRequest(request, out var orderToClose, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderClosingBy(request);

                var orderResp = await _api.CloseOrder(isAsync, request);

                resultEntity = new OrderResultEntity(orderResp.ResultCode, orderToClose, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToClose, null) { IsServerResponse = false };
            }

            _logger.LogOrderCloseByResults(request, resultEntity);
            return resultEntity;
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAsync, string orderId, double price, double? sl, double? tp, string comment)
        {
            return ModifyOrder(isAsync, orderId, price, null, null, sl, tp, comment, null, null, null);
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAsync, string orderId, double? price, double? stopPrice, double? maxVisibleVolume, double? sl, double? tp,
            string comment, DateTime? expiration, double? volume, OrderExecOptions? options)
        {
            OrderResultEntity resultEntity;

            var request = new ReplaceOrderRequest
            {
                OrderId = orderId,
                CurrentVolumeLots = double.NaN,
                NewVolumeLots = volume,
                MaxVisibleVolumeLots = maxVisibleVolume,
                VolumeChange = double.NaN,
                Price = price,
                StopPrice = stopPrice,
                StopLoss = sl,
                TakeProfit = tp,
                Comment = comment,
                Expiration = expiration,
                Options = options,
            };

            var code = OrderCmdResultCodes.Ok;

            PreprocessAndValidateModifyOrderRequest(request, out var orderToModify, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderModifying(request, smbMetadata);

                var result = await _api.ModifyOrder(isAsync, request);

                if (result.ResultCode == OrderCmdResultCodes.Ok)
                {
                    resultEntity = new OrderResultEntity(result.ResultCode, new OrderAccessor(result.ResultingOrder, smbMetadata, _account.Leverage), result.TransactionTime);
                }
                else
                {
                    resultEntity = new OrderResultEntity(result.ResultCode, orderToModify, result.TransactionTime);
                }
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToModify, null) { IsServerResponse = false };
            }

            _logger.LogOrderModifyResults(request, smbMetadata, resultEntity);
            return resultEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LeavePluginThread(bool delay)
        {
            if (delay)
                await Task.Delay(5); //ugly hack to enable quotes snapshot updates
            else await Task.Yield(); //free plugin thread to enable queue processing
        }

        private void PreprocessAndValidateOpenOrderRequest(OpenOrderRequest request, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
        {
            smbMetadata = null;

            var side = request.Side;
            var type = request.Type;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetSymbol(request.Symbol, out smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, side, ref code))
                return;

            if (!ValidateOptions(request.Options, type, ref code))
                return;
            if (!ValidateVolumeLots(request.VolumeLots, smbMetadata, ref code))
                return;
            if (!ValidateMaxVisibleVolumeLots(request.MaxVisibleVolumeLots, smbMetadata, type, request.VolumeLots, ref code))
                return;

            request.VolumeLots = RoundVolume(request.VolumeLots, smbMetadata);
            request.MaxVisibleVolumeLots = RoundVolume(request.MaxVisibleVolumeLots, smbMetadata);
            request.Volume = ConvertVolume(request.VolumeLots, smbMetadata);
            request.MaxVisibleVolume = ConvertNullableVolume(request.MaxVisibleVolumeLots, smbMetadata);
            request.Price = RoundPrice(request.Price, smbMetadata, side);
            request.StopPrice = RoundPrice(request.StopPrice, smbMetadata, side);
            request.StopLoss = RoundPrice(request.StopLoss, smbMetadata, side);
            request.TakeProfit = RoundPrice(request.TakeProfit, smbMetadata, side);

            if (type == OrderType.Market && request.Price == null)
            {
                if (!TryGetMarketPrice(smbMetadata, side, out var marketPrice, ref code))
                    return;
                request.Price = marketPrice;
            }

            if (!ValidatePrice(request.Price, type == OrderType.Limit || type == OrderType.StopLimit, ref code))
                return;
            if (!ValidateStopPrice(request.StopPrice, type == OrderType.Stop || type == OrderType.StopLimit, ref code))
                return;
            if (!ValidateVolume(request.Volume, ref code))
                return;
            if (!ValidateMaxVisibleVolume(request.MaxVisibleVolume, type, ref code))
                return;
            if (!ValidateTp(request.TakeProfit, ref code))
                return;
            if (!ValidateSl(request.StopLoss, ref code))
                return;

            if (!ValidateMargin(request, smbMetadata, ref code))
                return;
        }

        private void PreprocessAndValidateCancelOrderRequest(CancelOrderRequest request, out Order orderToCancel, ref OrderCmdResultCodes code)
        {
            orderToCancel = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetOrder(request.OrderId, out orderToCancel, ref code))
                return;
            if (!TryGetSymbol(orderToCancel.Symbol, out var smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, orderToCancel.Side, ref code))
                return;
        }

        private void PreprocessAndValidateCloseOrderRequest(CloseOrderRequest request, out Order orderToClose, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
        {
            orderToClose = null;
            smbMetadata = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetOrder(request.OrderId, out orderToClose, ref code))
                return;
            if (!TryGetSymbol(orderToClose.Symbol, out smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, orderToClose.Side.Revert(), ref code))
                return;

            if (!ValidateVolumeLots(request.VolumeLots, smbMetadata, ref code))
                return;
            request.VolumeLots = RoundVolume(request.VolumeLots, smbMetadata);
            request.Volume = ConvertNullableVolume(request.VolumeLots, smbMetadata);

            if (!ValidateVolume(request.Volume, ref code))
                return;
        }

        private void PreprocessAndValidateCloseOrderByRequest(CloseOrderRequest request, out Order orderToClose, ref OrderCmdResultCodes code)
        {
            orderToClose = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetOrder(request.OrderId, out orderToClose, ref code))
                return;
            if (!TryGetOrder(request.ByOrderId, out var orderByClose, ref code))
                return;
            if (!TryGetSymbol(orderToClose.Symbol, out var smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, orderToClose.Side.Revert(), ref code))
                return;
        }

        private void PreprocessAndValidateModifyOrderRequest(ReplaceOrderRequest request, out Order orderToModify, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
        {
            orderToModify = null;
            smbMetadata = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetOrder(request.OrderId, out orderToModify, ref code))
                return;

            var side = orderToModify.Side;
            var type = orderToModify.Type;

            request.Symbol = orderToModify.Symbol;
            request.Type = type;
            request.Side = side;
            request.CurrentVolumeLots = orderToModify.RemainingVolume;

            if (!TryGetSymbol(orderToModify.Symbol, out smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, side, ref code))
                return;

            if (!ValidateOptions(request.Options, type, ref code))
                return;
            if (!ValidateVolumeLots(request.NewVolumeLots, smbMetadata, ref code))
                return;
            if (!ValidateMaxVisibleVolumeLots(request.MaxVisibleVolumeLots, smbMetadata, type, request.NewVolumeLots ?? request.CurrentVolumeLots, ref code))
                return;

            request.NewVolumeLots = RoundVolume(request.NewVolumeLots, smbMetadata);
            request.MaxVisibleVolumeLots = RoundVolume(request.MaxVisibleVolumeLots, smbMetadata);
            request.CurrentVolume = ConvertVolume(request.CurrentVolumeLots, smbMetadata);
            request.NewVolume = ConvertNullableVolume(request.NewVolumeLots, smbMetadata);
            request.MaxVisibleVolume = ConvertNullableVolume(request.MaxVisibleVolumeLots, smbMetadata);
            request.VolumeChange = request.NewVolumeLots.HasValue ? ConvertVolume(request.NewVolumeLots.Value - request.CurrentVolumeLots, smbMetadata) : 0;
            request.Price = RoundPrice(request.Price, smbMetadata, side);
            request.StopPrice = RoundPrice(request.StopPrice, smbMetadata, side);
            request.StopLoss = RoundPrice(request.StopLoss, smbMetadata, side);
            request.TakeProfit = RoundPrice(request.TakeProfit, smbMetadata, side);

            if (!ValidatePrice(request.Price, false, ref code))
                return;
            if (!ValidateStopPrice(request.StopPrice, false, ref code))
                return;
            if (!ValidateVolume(request.NewVolume, ref code))
                return;
            if (!ValidateMaxVisibleVolume(request.MaxVisibleVolume, type, ref code))
                return;
            if (!ValidateTp(request.TakeProfit, ref code))
                return;
            if (!ValidateSl(request.StopLoss, ref code))
                return;

            if (!ValidateMargin(request, smbMetadata, ref code))
                return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? ConvertNullableVolume(double? volumeInLots, SymbolAccessor smbMetadata)
        {
            if (volumeInLots == null)
                return null;
            return ConvertVolume(volumeInLots.Value, smbMetadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double ConvertVolume(double volumeInLots, SymbolAccessor smbMetadata)
        {
            var res = smbMetadata.ContractSize * volumeInLots;
            var amountDigits = smbMetadata.AmountDigits;
            if (amountDigits == 0)
            {
                res = Math.Round(res);
            }
            else if (amountDigits > 0)
            {
                res = res.Round(amountDigits);
            }
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double RoundVolume(double volumeInLots, Symbol smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? RoundVolume(double? volumeInLots, Symbol smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double RoundPrice(double price, Symbol smbMetadata, OrderSide side)
        {
            return side == OrderSide.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? RoundPrice(double? price, Symbol smbMetadata, OrderSide side)
        {
            return side == OrderSide.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        #region Validation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetSymbol(string symbolName, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
        {
            smbMetadata = _symbols.GetOrDefault(symbolName);
            if (smbMetadata == null)
            {
                code = OrderCmdResultCodes.SymbolNotFound;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetOrder(string orderId, out Order order, ref OrderCmdResultCodes code)
        {
            order = _account.Orders.GetOrderOrNull(orderId);
            if (order == null)
            {
                code = OrderCmdResultCodes.OrderNotFound;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetMarketPrice(SymbolAccessor smb, OrderSide orderSide, out double price, ref OrderCmdResultCodes code)
        {
            price = double.NaN;
            var rate = smb.LastQuote;

            if (rate == null)
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            if (orderSide == OrderSide.Buy)
            {
                if (!rate.HasAsk)
                {
                    code = OrderCmdResultCodes.OffQuotes;
                    return false;
                }
                price = rate.Ask;
                return true;
            }
            else if (orderSide == OrderSide.Sell)
            {
                if (!rate.HasBid)
                {
                    code = OrderCmdResultCodes.OffQuotes;
                    return false;
                }
                price = rate.Bid;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsInvalidValue(double val)
        {
            // Because TTS uses decimal for financial calculation
            // We need to validate that out value is inside of decimal range otherwise exception will be thrown
            // Values like 1E-30 which go below decimal precision will be converted to zero normally
            if (val > (double)decimal.MaxValue || val < (double)decimal.MinValue)
                return true;

            return double.IsNaN(val) || double.IsInfinity(val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateVolume(double volume, ref OrderCmdResultCodes code)
        {
            if (volume <= 0 || IsInvalidValue(volume))
            {
                code = OrderCmdResultCodes.IncorrectVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateVolume(double? volume, ref OrderCmdResultCodes code)
        {
            if (!volume.HasValue)
                return true;

            if (volume <= 0 || IsInvalidValue(volume.Value))
            {
                code = OrderCmdResultCodes.IncorrectVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMaxVisibleVolume(double? volume, OrderType orderType, ref OrderCmdResultCodes code)
        {
            if (!volume.HasValue)
                return true;

            if (IsInvalidValue(volume.Value))
            {
                code = OrderCmdResultCodes.IncorrectMaxVisibleVolume;
                return false;
            }

            if (orderType == OrderType.Market)
            {
                code = OrderCmdResultCodes.MarketWithMaxVisibleVolume;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateVolumeLots(double volumeLots, Symbol smbMetadata, ref OrderCmdResultCodes code)
        {
            if (volumeLots <= 0 || volumeLots < smbMetadata.MinTradeVolume || volumeLots > smbMetadata.MaxTradeVolume)
            {
                code = OrderCmdResultCodes.IncorrectVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateVolumeLots(double? volumeLots, Symbol smbMetadata, ref OrderCmdResultCodes code)
        {
            if (!volumeLots.HasValue)
                return true;

            if (volumeLots <= 0 || volumeLots < smbMetadata.MinTradeVolume || volumeLots > smbMetadata.MaxTradeVolume)
            {
                code = OrderCmdResultCodes.IncorrectVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMaxVisibleVolumeLots(double? maxVisibleVolumeLots, Symbol smbMetadata, OrderType orderType, double? volumeLots, ref OrderCmdResultCodes code)
        {
            if (!maxVisibleVolumeLots.HasValue)
                return true;

            var isIncorrectMaxVisibleVolume = orderType == OrderType.Stop
                || (maxVisibleVolumeLots > 0 && maxVisibleVolumeLots < smbMetadata.MinTradeVolume)
                || maxVisibleVolumeLots > smbMetadata.MaxTradeVolume;

            if (isIncorrectMaxVisibleVolume)
            {
                code = OrderCmdResultCodes.IncorrectMaxVisibleVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidatePrice(double? price, bool required, ref OrderCmdResultCodes code)
        {
            if (required && price == null)
            {
                code = OrderCmdResultCodes.IncorrectPrice;
                return false;
            }

            if (!price.HasValue)
                return true;

            if (price <= 0 || IsInvalidValue(price.Value))
            {
                code = OrderCmdResultCodes.IncorrectPrice;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateStopPrice(double? price, bool required, ref OrderCmdResultCodes code)
        {
            if (required && price == null)
            {
                code = OrderCmdResultCodes.IncorrectStopPrice;
                return false;
            }

            if (!price.HasValue)
                return true;

            if (price <= 0 || IsInvalidValue(price.Value))
            {
                code = OrderCmdResultCodes.IncorrectStopPrice;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateTp(double? tp, ref OrderCmdResultCodes code)
        {
            if (tp == null)
                return true;

            if (tp.Value <= 0 || IsInvalidValue(tp.Value))
            {
                code = OrderCmdResultCodes.IncorrectTp;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateSl(double? sl, ref OrderCmdResultCodes code)
        {
            if (sl == null)
                return true;

            if (sl.Value <= 0 || IsInvalidValue(sl.Value))
            {
                code = OrderCmdResultCodes.IncorrectSl;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateTradeEnabled(Symbol smbMetadata, ref OrderCmdResultCodes code)
        {
            if (!smbMetadata.IsTradeAllowed)
            {
                code = OrderCmdResultCodes.TradeNotAllowed;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateTradePersmission(ref OrderCmdResultCodes code)
        {
            if (!Permissions.TradeAllowed)
            {
                code = OrderCmdResultCodes.TradeNotAllowed;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateOptions(OrderExecOptions? options, OrderType orderType, ref OrderCmdResultCodes code)
        {
            if (options == null)
                return true;

            var isOrderTypeCompatibleToIoC = orderType == OrderType.Limit || orderType == OrderType.StopLimit;

            if (options == OrderExecOptions.ImmediateOrCancel && !isOrderTypeCompatibleToIoC)
            {
                code = OrderCmdResultCodes.Unsupported;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMargin(OpenOrderRequest request, SymbolAccessor symbol, ref OrderCmdResultCodes code)
        {
            var isHidden = OrderEntity.IsHiddenOrder((decimal?)request.MaxVisibleVolume);

            if (Calc != null && !Calc.HasEnoughMarginToOpenOrder(symbol, request.Volume, request.Type, request.Side, request.Price, request.StopPrice, isHidden, out CalcErrorCodes error))
            {
                code = error.ToOrderError();

                if (code == OrderCmdResultCodes.Ok)
                    code = OrderCmdResultCodes.NotEnoughMoney;

                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMargin(ReplaceOrderRequest request, SymbolAccessor symbol, ref OrderCmdResultCodes code)
        {
            var oldOrder = _account.Orders.GetOrderOrNull(request.OrderId);

            var newIsHidden = OrderEntity.IsHiddenOrder((decimal?)request.MaxVisibleVolume);

            var newVol = request.NewVolume ?? oldOrder.RemainingVolume;
            var newPrice = request.Price ?? oldOrder.Price;
            var newStopPrice = request.StopPrice ?? oldOrder.StopPrice;

            if (Calc != null && !Calc.HasEnoughMarginToModifyOrder(oldOrder, symbol, newVol, newPrice, newStopPrice, newIsHidden))
            {
                code = OrderCmdResultCodes.NotEnoughMoney;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateQuotes(SymbolAccessor symbol, OrderSide side, ref OrderCmdResultCodes code)
        {
            var quote = symbol.LastQuote;

            if (_account.Type != AccountTypes.Cash && (!quote.HasBid || !quote.HasAsk))
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            if (side == OrderSide.Sell && quote.IsBidIndicative)
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            if (side == OrderSide.Buy && quote.IsAskIndicative)
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            return true;
        }

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

    internal static class TradeApiExtentions
    {
        public static OrderCmdResultCodes ToOrderError(this CalcErrorCodes error)
        {
            switch (error)
            {
                case CalcErrorCodes.None: return OrderCmdResultCodes.Ok;
                case CalcErrorCodes.NoCrossSymbol: return OrderCmdResultCodes.Misconfiguration;
                case CalcErrorCodes.OffCrossQuote: return OrderCmdResultCodes.OffQuotes;
                case CalcErrorCodes.OffQuote: return OrderCmdResultCodes.OffQuotes;
            }

            throw new Exception("Unknown code: " + error);
        }
    }

}
