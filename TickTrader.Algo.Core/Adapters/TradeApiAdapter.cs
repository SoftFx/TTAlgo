using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

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

        public async Task<OrderCmdResult> OpenOrder(bool isAsync, Api.OpenOrderRequest apiRequest)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;

            var requestContext = new OpenOrderRequestContext
            {
                Symbol = apiRequest.Symbol,
                Type = apiRequest.Type.ToCoreEnum(),
                Side = apiRequest.Side.ToCoreEnum(),
                Volume = apiRequest.Volume,
                MaxVisibleVolume = apiRequest.MaxVisibleVolume,
                Price = apiRequest.Price,
                StopPrice = apiRequest.StopPrice,
                StopLoss = apiRequest.StopLoss,
                TakeProfit = apiRequest.TakeProfit,
                Comment = apiRequest.Comment,
                Slippage = apiRequest.Slippage,
                ExecOptions = apiRequest.Options.ToDomainEnum(),
                Tag = CompositeTag.NewTag(IsolationTag, apiRequest.Tag),
                Expiration = apiRequest.Expiration,
            };

            PreprocessAndValidateOpenOrderRequest(requestContext, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderOpening(requestContext, smbMetadata);
                var orderResp = await _api.OpenOrder(isAsync, requestContext.Request);
                var resCode = orderResp.ResultCode.ToApiEnum();
                if (resCode != OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(resCode, null, orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(orderResp.ResultingOrder, smbMetadata).ApiOrder, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, null, null) { IsServerResponse = false };
            }

            _logger.LogOrderOpenResults(resultEntity, requestContext, smbMetadata);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAsync, string orderId)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var requestContext = new CancelOrderRequestContext { OrderId = orderId };

            PreprocessAndValidateCancelOrderRequest(requestContext, out var orderToCancel, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderCanceling(requestContext);

                var orderResp = await _api.CancelOrder(isAsync, requestContext.Request);

                resultEntity = new OrderResultEntity(orderResp.ResultCode.ToApiEnum(), orderToCancel, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToCancel, null) { IsServerResponse = false };
            }

            _logger.LogOrderCancelResults(requestContext, resultEntity);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAsync, Api.CloseOrderRequest request)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var requestContext = new CloseOrderRequestContext
            {
                OrderId = request.OrderId,
                VolumeLots = request.Volume,
                Slippage = request.Slippage
            };

            PreprocessAndValidateCloseOrderRequest(requestContext, out var orderToClose, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderClosing(requestContext);

                var orderResp = await _api.CloseOrder(isAsync, requestContext.Request);
                var resCode = orderResp.ResultCode.ToApiEnum();

                if (resCode == OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(orderResp.ResultingOrder, smbMetadata).ApiOrder, orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(resCode, orderToClose, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToClose, null) { IsServerResponse = false };
            }

            _logger.LogOrderCloseResults(requestContext, resultEntity);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseOrderBy(bool isAsync, string orderId, string byOrderId)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var requestContext = new CloseOrderRequestContext { OrderId = orderId, ByOrderId = byOrderId };

            PreprocessAndValidateCloseOrderByRequest(requestContext, out var orderToClose, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderClosingBy(requestContext);

                var orderResp = await _api.CloseOrder(isAsync, requestContext.Request);

                resultEntity = new OrderResultEntity(orderResp.ResultCode.ToApiEnum(), orderToClose, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToClose, null) { IsServerResponse = false };
            }

            _logger.LogOrderCloseByResults(requestContext, resultEntity);
            return resultEntity;
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAsync, Api.ModifyOrderRequest request)
        {
            OrderResultEntity resultEntity;

            var requestContext = new ModifyOrderRequestContext
            {
                OrderId = request.OrderId,
                CurrentVolume = double.NaN,
                NewVolume = request.Volume,
                MaxVisibleVolume = request.MaxVisibleVolume,
                AmountChange = double.NaN,
                Price = request.Price,
                StopPrice = request.StopPrice,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                Slippage = request.Slippage,
                Comment = request.Comment,
                Expiration = request.Expiration,
                ExecOptions = request.Options?.ToDomainEnum(),
            };

            var code = OrderCmdResultCodes.Ok;

            PreprocessAndValidateModifyOrderRequest(requestContext, out var orderToModify, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderModifying(requestContext, smbMetadata);

                var result = await _api.ModifyOrder(isAsync, requestContext.Request);
                var resCode = result.ResultCode.ToApiEnum();

                if (resCode == OrderCmdResultCodes.Ok)
                {
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(result.ResultingOrder, smbMetadata).ApiOrder, result.TransactionTime);
                }
                else
                {
                    resultEntity = new OrderResultEntity(resCode, orderToModify, result.TransactionTime);
                }
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToModify, null) { IsServerResponse = false };
            }

            _logger.LogOrderModifyResults(requestContext, smbMetadata, resultEntity);
            return resultEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LeavePluginThread(bool delay)
        {
            if (delay)
                await Task.Delay(5); //ugly hack to enable quotes snapshot updates
            else await Task.Yield(); //free plugin thread to enable queue processing
        }

        private void PreprocessAndValidateOpenOrderRequest(OpenOrderRequestContext request, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
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

            if (!ValidateOptions(request.ExecOptions, type, ref code))
                return;
            if (!ValidateVolumeLots(request.Volume, smbMetadata, ref code))
                return;
            if (!ValidateMaxVisibleVolumeLots(request.MaxVisibleVolume, smbMetadata, type, request.Volume, ref code))
                return;

            request.Volume = RoundVolume(request.Volume, smbMetadata);
            request.MaxVisibleVolume = RoundVolume(request.MaxVisibleVolume, smbMetadata);
            request.Amount = ConvertVolume(request.Volume, smbMetadata);
            request.MaxVisibleAmount = ConvertNullableVolume(request.MaxVisibleVolume, smbMetadata);
            request.Price = RoundPrice(request.Price, smbMetadata, side);
            request.StopPrice = RoundPrice(request.StopPrice, smbMetadata, side);
            request.StopLoss = RoundPrice(request.StopLoss, smbMetadata, side);
            request.TakeProfit = RoundPrice(request.TakeProfit, smbMetadata, side);

            if (type == Domain.OrderInfo.Types.Type.Market && request.Price == null)
            {
                if (!TryGetMarketPrice(smbMetadata, side, out var marketPrice, ref code))
                    return;
                request.Price = marketPrice;
            }

            if (!ValidatePrice(request.Price, type == Domain.OrderInfo.Types.Type.Limit || type == Domain.OrderInfo.Types.Type.StopLimit, ref code))
                return;
            if (!ValidateStopPrice(request.StopPrice, type == Domain.OrderInfo.Types.Type.Stop || type == Domain.OrderInfo.Types.Type.StopLimit, ref code))
                return;
            if (!ValidateVolume(request.Amount, ref code))
                return;
            if (!ValidateMaxVisibleVolume(request.MaxVisibleAmount, type, ref code))
                return;
            if (!ValidateTp(request.TakeProfit, ref code))
                return;
            if (!ValidateSl(request.StopLoss, ref code))
                return;
            if (!ValidateSlippage(request.Slippage, ref code))
                return;

            //if (!ValidateMargin(request, smbMetadata, ref code)) //incorrect behavior when Margine Call
            //    return;
        }

        private void PreprocessAndValidateCancelOrderRequest(CancelOrderRequestContext request, out Order orderToCancel, ref OrderCmdResultCodes code)
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
            if (!ValidateQuotes(smbMetadata, orderToCancel.Side.ToCoreEnum(), ref code))
                return;
        }

        private void PreprocessAndValidateCloseOrderRequest(CloseOrderRequestContext request, out Order orderToClose, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
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
            if (!ValidateQuotes(smbMetadata, orderToClose.Side.ToCoreEnum().Revert(), ref code))
                return;

            if (!ValidateVolumeLots(request.VolumeLots, smbMetadata, ref code))
                return;
            request.VolumeLots = RoundVolume(request.VolumeLots, smbMetadata);
            request.Amount = ConvertNullableVolume(request.VolumeLots, smbMetadata);

            if (!ValidateVolume(request.Amount, ref code))
                return;
        }

        private void PreprocessAndValidateCloseOrderByRequest(CloseOrderRequestContext request, out Order orderToClose, ref OrderCmdResultCodes code)
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
            if (!ValidateQuotes(smbMetadata, orderToClose.Side.ToCoreEnum().Revert(), ref code))
                return;
        }

        private void PreprocessAndValidateModifyOrderRequest(ModifyOrderRequestContext request, out Order orderToModify, out SymbolAccessor smbMetadata, ref OrderCmdResultCodes code)
        {
            orderToModify = null;
            smbMetadata = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetOrder(request.OrderId, out orderToModify, ref code))
                return;

            var side = orderToModify.Side.ToCoreEnum();
            var type = orderToModify.Type.ToCoreEnum();

            request.Symbol = orderToModify.Symbol;
            request.Type = type;
            request.Side = side;
            request.CurrentVolume = orderToModify.RemainingVolume;

            if (!TryGetSymbol(orderToModify.Symbol, out smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, side, ref code))
                return;

            if (!ValidateOptions(request.ExecOptions, type, ref code))
                return;
            if (!ValidateVolumeLots(request.NewVolume, smbMetadata, ref code))
                return;
            if (!ValidateMaxVisibleVolumeLots(request.MaxVisibleVolume, smbMetadata, type, request.NewVolume ?? request.CurrentVolume, ref code))
                return;

            request.NewVolume = RoundVolume(request.NewVolume, smbMetadata);
            request.MaxVisibleVolume = RoundVolume(request.MaxVisibleVolume, smbMetadata);
            request.CurrentAmount = ConvertVolume(request.CurrentVolume, smbMetadata);
            request.NewAmount = ConvertNullableVolume(request.NewVolume, smbMetadata);
            request.MaxVisibleAmount = ConvertNullableVolume(request.MaxVisibleVolume, smbMetadata);
            request.AmountChange = request.NewVolume.HasValue ? ConvertVolume(request.NewVolume.Value - request.CurrentVolume, smbMetadata) : 0;
            request.Price = RoundPrice(request.Price, smbMetadata, side);
            request.StopPrice = RoundPrice(request.StopPrice, smbMetadata, side);
            request.StopLoss = RoundPrice(request.StopLoss, smbMetadata, side);
            request.TakeProfit = RoundPrice(request.TakeProfit, smbMetadata, side);

            if (!ValidatePrice(request.Price, false, ref code))
                return;
            if (!ValidateStopPrice(request.StopPrice, false, ref code))
                return;
            if (!ValidateVolume(request.NewAmount, ref code))
                return;
            if (!ValidateMaxVisibleVolume(request.MaxVisibleAmount, type, ref code))
                return;
            if (!ValidateTp(request.TakeProfit, ref code))
                return;
            if (!ValidateSl(request.StopLoss, ref code))
                return;
            if (!ValidateSlippage(request.Slippage, ref code))
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
        private double RoundPrice(double price, Symbol smbMetadata, Domain.OrderInfo.Types.Side side)
        {
            return side == Domain.OrderInfo.Types.Side.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? RoundPrice(double? price, Symbol smbMetadata, Domain.OrderInfo.Types.Side side)
        {
            return side == Domain.OrderInfo.Types.Side.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
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
            order = _account.Orders.GetOrderOrNull(orderId)?.ApiOrder;
            if (order == null)
            {
                code = OrderCmdResultCodes.OrderNotFound;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetMarketPrice(SymbolAccessor smb, Domain.OrderInfo.Types.Side orderSide, out double price, ref OrderCmdResultCodes code)
        {
            price = double.NaN;
            var rate = smb.LastQuote;

            if (rate == null)
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            if (orderSide == Domain.OrderInfo.Types.Side.Buy)
            {
                if (!rate.HasAsk)
                {
                    code = OrderCmdResultCodes.OffQuotes;
                    return false;
                }
                price = rate.Ask;
                return true;
            }
            else if (orderSide == Domain.OrderInfo.Types.Side.Sell)
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
        private bool ValidateMaxVisibleVolume(double? volume, Domain.OrderInfo.Types.Type orderType, ref OrderCmdResultCodes code)
        {
            if (!volume.HasValue)
                return true;

            if (IsInvalidValue(volume.Value))
            {
                code = OrderCmdResultCodes.IncorrectMaxVisibleVolume;
                return false;
            }

            if (orderType == Domain.OrderInfo.Types.Type.Market)
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
        private bool ValidateMaxVisibleVolumeLots(double? maxVisibleVolumeLots, Symbol smbMetadata, Domain.OrderInfo.Types.Type orderType, double? volumeLots, ref OrderCmdResultCodes code)
        {
            if (!maxVisibleVolumeLots.HasValue)
                return true;

            var isIncorrectMaxVisibleVolume = orderType == Domain.OrderInfo.Types.Type.Stop
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

            if (tp.Value < 0 || IsInvalidValue(tp.Value))
            {
                code = OrderCmdResultCodes.IncorrectTp;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateSlippage(double? slippage, ref OrderCmdResultCodes code)
        {
            if (slippage == null)
                return true;

            if (slippage.Value < 0 || IsInvalidValue(slippage.Value))
            {
                code = OrderCmdResultCodes.IncorrectSlippage;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateSl(double? sl, ref OrderCmdResultCodes code)
        {
            if (sl == null)
                return true;

            if (sl.Value < 0 || IsInvalidValue(sl.Value))
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
        private bool ValidateOptions(Domain.OrderExecOptions? options, Domain.OrderInfo.Types.Type orderType, ref OrderCmdResultCodes code)
        {
            if (options == null)
                return true;

            var isOrderTypeCompatibleToIoC = orderType == Domain.OrderInfo.Types.Type.Limit || orderType == Domain.OrderInfo.Types.Type.StopLimit;

            if (options == Domain.OrderExecOptions.ImmediateOrCancel && !isOrderTypeCompatibleToIoC)
            {
                code = OrderCmdResultCodes.Unsupported;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMargin(OpenOrderRequestContext request, SymbolAccessor symbol, ref OrderCmdResultCodes code)
        {
            var isHidden = OrderEntity.IsHiddenOrder((decimal?)request.MaxVisibleAmount);

            if (Calc != null && !Calc.HasEnoughMarginToOpenOrder(symbol, request.Amount, request.Type, request.Side, request.Price, request.StopPrice, isHidden, out CalcErrorCodes error))
            {
                code = error.ToOrderError();

                if (code == OrderCmdResultCodes.Ok)
                    code = OrderCmdResultCodes.NotEnoughMoney;

                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMargin(ModifyOrderRequestContext request, SymbolAccessor symbol, ref OrderCmdResultCodes code)
        {
            var oldOrder = _account.Orders.GetOrderOrNull(request.OrderId);

            var newIsHidden = OrderEntity.IsHiddenOrder((decimal?)request.MaxVisibleAmount);

            var newVol = request.NewAmount ?? (double)oldOrder.RemainingAmount;
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
        private bool ValidateQuotes(SymbolAccessor symbol, Domain.OrderInfo.Types.Side side, ref OrderCmdResultCodes code)
        {
            var quote = symbol.LastQuote;

            if (_account.Type != AccountInfo.Types.Type.Cash && (!quote.HasBid || !quote.HasAsk))
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            if (side == Domain.OrderInfo.Types.Side.Sell && quote.IsBidIndicative)
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

            if (side == Domain.OrderInfo.Types.Side.Buy && quote.IsAskIndicative)
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
