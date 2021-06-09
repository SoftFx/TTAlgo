using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public enum OrderAction
    {
        Open,
        Close,
        Modify,
        Cancel,
    }

    public enum BalanceAction
    {
        Deposite,
        Withdrawal,
        Dividend,
    }

    public enum OrderEntityAction { None, Added, Removed, Updated }

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
        public PluginPermissions Permissions { get; set; }
        public string IsolationTag { get; set; }

        public async Task<OrderCmdResult> OpenOrder(bool isAsync, Api.OpenOrderRequest apiRequest)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;

            var domainRequest = new Domain.OpenOrderRequest
            {
                Symbol = apiRequest.Symbol,
                Type = apiRequest.Type.ToDomainEnum(),
                Side = apiRequest.Side.ToDomainEnum(),
                Amount = apiRequest.Volume,
                MaxVisibleAmount = apiRequest.MaxVisibleVolume,
                Price = apiRequest.Price,
                StopPrice = apiRequest.StopPrice,
                StopLoss = apiRequest.StopLoss,
                TakeProfit = apiRequest.TakeProfit,
                Comment = apiRequest.Comment,
                Slippage = apiRequest.Slippage,
                ExecOptions = apiRequest.Options.ToDomainEnum(),
                Tag = CompositeTag.NewTag(IsolationTag, apiRequest.Tag),
                Expiration = apiRequest.Expiration?.ToUniversalTime().ToTimestamp(),
                OcoRelatedOrderId = apiRequest.OcoRelatedOrderId,
                OcoEqualVolume = apiRequest.OcoEqualVolume,
            };

            PreprocessAndValidateOpenOrderRequest(domainRequest, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderValidationSuccess(domainRequest, OrderAction.Open, smbMetadata.LotSize);
                var orderResp = await _api.OpenOrder(isAsync, domainRequest);
                var resCode = orderResp.ResultCode.ToApiEnum();
                if (resCode != OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(resCode, null, orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(smbMetadata, orderResp.ResultingOrder), orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, null, null) { IsServerResponse = false };
            }

            _logger.LogRequestResults(domainRequest, resultEntity, OrderAction.Open);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CancelOrder(bool isAsync, string orderId)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var request = new Domain.CancelOrderRequest { OrderId = orderId };

            PreprocessAndValidateCancelOrderRequest(request, out var orderToCancel, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderValidationSuccess(request, OrderAction.Cancel);

                var orderResp = await _api.CancelOrder(isAsync, request);

                resultEntity = new OrderResultEntity(orderResp.ResultCode.ToApiEnum(), orderToCancel, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToCancel, null) { IsServerResponse = false };
            }

            _logger.LogRequestResults(request, resultEntity, OrderAction.Cancel);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseOrder(bool isAsync, Api.CloseOrderRequest request)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var requestContext = new Domain.CloseOrderRequest
            {
                OrderId = request.OrderId,
                Amount = request.Volume,
                Slippage = request.Slippage
            };

            PreprocessAndValidateCloseOrderRequest(requestContext, out var orderToClose, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderValidationSuccess(requestContext, OrderAction.Close, smbMetadata.LotSize);

                var orderResp = await _api.CloseOrder(isAsync, requestContext);
                var resCode = orderResp.ResultCode.ToApiEnum();

                if (resCode == OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(smbMetadata, orderResp.ResultingOrder), orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(resCode, orderToClose, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToClose, null) { IsServerResponse = false };
            }

            _logger.LogRequestResults(requestContext, resultEntity, OrderAction.Close);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseNetPosition(bool isAsync, Api.CloseNetPositionRequest request)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var requestContext = new Domain.CloseOrderRequest
            {
                OrderId = request.Symbol,
                Amount = request.Volume,
                Slippage = request.Slippage
            };

            PreprocessAndValidateClosePositionRequest(requestContext, out var position, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                requestContext.OrderId = position.Id;

                _logger.LogOrderValidationSuccess(requestContext, OrderAction.Close, smbMetadata.LotSize);

                var orderResp = await _api.CloseOrder(isAsync, requestContext);
                var resCode = orderResp.ResultCode.ToApiEnum();

                if (resCode == OrderCmdResultCodes.Ok)
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(smbMetadata, orderResp.ResultingOrder), orderResp.TransactionTime);
                else
                    resultEntity = new OrderResultEntity(resCode, Null.Order, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);

                resultEntity = new OrderResultEntity(code, Null.Order, null) { IsServerResponse = false };
            }

            _logger.LogRequestResults(requestContext, resultEntity, OrderAction.Close);
            return resultEntity;
        }

        public async Task<OrderCmdResult> CloseOrderBy(bool isAsync, string orderId, string byOrderId)
        {
            OrderResultEntity resultEntity;
            var code = OrderCmdResultCodes.Ok;
            var request = new Domain.CloseOrderRequest { OrderId = orderId, ByOrderId = byOrderId };

            PreprocessAndValidateCloseOrderByRequest(request, out var orderToClose, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderValidationSuccess(request, OrderAction.Close);

                var orderResp = await _api.CloseOrder(isAsync, request);

                resultEntity = new OrderResultEntity(orderResp.ResultCode.ToApiEnum(), orderToClose, orderResp.TransactionTime);
            }
            else
            {
                if (isAsync)
                    await LeavePluginThread(code == OrderCmdResultCodes.OffQuotes);
                resultEntity = new OrderResultEntity(code, orderToClose, null) { IsServerResponse = false };
            }

            _logger.LogRequestResults(request, resultEntity, OrderAction.Close);
            return resultEntity;
        }

        public async Task<OrderCmdResult> ModifyOrder(bool isAsync, Api.ModifyOrderRequest request)
        {
            OrderResultEntity resultEntity;

            var domainRequest = new Domain.ModifyOrderRequest
            {
                OrderId = request.OrderId,
                CurrentAmount = double.NaN,
                NewAmount = request.Volume,
                MaxVisibleAmount = request.MaxVisibleVolume,
                AmountChange = double.NaN,
                Price = request.Price,
                StopPrice = request.StopPrice,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                Slippage = request.Slippage,
                Comment = request.Comment,
                Expiration = request.Expiration?.ToUniversalTime().ToTimestamp(),
                ExecOptions = request.Options?.ToDomainEnum(),
                OcoRelatedOrderId = request.OcoRelatedOrderId,
                OcoEqualVolume = request.OcoEqualVolume,
            };

            var code = OrderCmdResultCodes.Ok;

            PreprocessAndValidateModifyOrderRequest(domainRequest, out var orderToModify, out var smbMetadata, ref code);

            if (code == OrderCmdResultCodes.Ok)
            {
                _logger.LogOrderValidationSuccess(domainRequest, OrderAction.Modify, smbMetadata.LotSize);

                var result = await _api.ModifyOrder(isAsync, domainRequest);
                var resCode = result.ResultCode.ToApiEnum();

                if (resCode == OrderCmdResultCodes.Ok)
                {
                    resultEntity = new OrderResultEntity(resCode, new OrderAccessor(smbMetadata, result.ResultingOrder), result.TransactionTime);
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

            _logger.LogRequestResults(domainRequest, resultEntity, OrderAction.Modify);
            return resultEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LeavePluginThread(bool delay)
        {
            if (delay)
                await Task.Delay(5); //ugly hack to enable quotes snapshot updates
            else await Task.Yield(); //free plugin thread to enable queue processing
        }

        private void PreprocessAndValidateOpenOrderRequest(Domain.OpenOrderRequest request, out SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
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
            if (!ValidateVolumeLots(request.Amount, smbMetadata, ref code))
                return;
            if (!ValidateMaxVisibleVolumeLots(request.MaxVisibleAmount, smbMetadata, type, request.Amount, ref code))
                return;
            if (!ValidateOCO(request.ExecOptions, request.OcoRelatedOrderId, ref code))
                return;


            request.Amount = RoundVolume(request.Amount, smbMetadata);
            request.MaxVisibleAmount = RoundVolume(request.MaxVisibleAmount, smbMetadata);
            request.Amount = ConvertVolume(request.Amount, smbMetadata);
            request.MaxVisibleAmount = ConvertNullableVolume(request.MaxVisibleAmount, smbMetadata);
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

        private void PreprocessAndValidateCancelOrderRequest(Domain.CancelOrderRequest request, out Order orderToCancel, ref OrderCmdResultCodes code)
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
            if (!ValidateQuotes(smbMetadata, orderToCancel.Side.ToDomainEnum(), ref code))
                return;
        }

        private void PreprocessAndValidateCloseOrderRequest(Domain.CloseOrderRequest request, out Order orderToClose, out SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
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
            if (!ValidateQuotes(smbMetadata, orderToClose.Side.ToDomainEnum().Revert(), ref code))
                return;

            if (!ValidateVolumeLots(request.Amount, smbMetadata, ref code))
                return;
            request.Amount = RoundVolume(request.Amount, smbMetadata);
            request.Amount = ConvertNullableVolume(request.Amount, smbMetadata);

            if (!ValidateVolume(request.Amount, ref code))
                return;
        }

        private void PreprocessAndValidateClosePositionRequest(Domain.CloseOrderRequest request, out NetPosition position, out SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
        {
            smbMetadata = null;
            position = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetNetPosition(request.OrderId, out position, ref code))
                return;
            if (!TryGetSymbol(position.Symbol, out smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, position.Side.ToDomainEnum().Revert(), ref code))
                return;

            if (!ValidateVolumeLots(request.Amount, smbMetadata, ref code))
                return;
            request.Amount = RoundVolume(request.Amount, smbMetadata);
            request.Amount = ConvertNullableVolume(request.Amount, smbMetadata);

            if (!ValidateVolume(request.Amount, ref code))
                return;
        }

        private void PreprocessAndValidateCloseOrderByRequest(Domain.CloseOrderRequest request, out Order orderToClose, ref OrderCmdResultCodes code)
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
            if (!ValidateQuotes(smbMetadata, orderToClose.Side.ToDomainEnum().Revert(), ref code))
                return;
        }

        private void PreprocessAndValidateModifyOrderRequest(Domain.ModifyOrderRequest request, out Order orderToModify, out SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
        {
            orderToModify = null;
            smbMetadata = null;

            if (!ValidateTradePersmission(ref code))
                return;
            if (!TryGetOrder(request.OrderId, out orderToModify, ref code))
                return;

            var side = orderToModify.Side.ToDomainEnum();
            var type = orderToModify.Type.ToDomainEnum();

            request.Symbol = orderToModify.Symbol;
            request.Type = type;
            request.Side = side;
            request.CurrentAmount = orderToModify.RemainingVolume;

            if (!TryGetSymbol(orderToModify.Symbol, out smbMetadata, ref code))
                return;
            if (!ValidateTradeEnabled(smbMetadata, ref code))
                return;
            if (!ValidateQuotes(smbMetadata, side, ref code))
                return;

            if (!ValidateOptions(request.ExecOptions, type, ref code))
                return;
            if (!ValidateVolumeLots(request.NewAmount, smbMetadata, ref code))
                return;
            if (!ValidateMaxVisibleVolumeLots(request.MaxVisibleAmount, smbMetadata, type, request.NewAmount ?? request.CurrentAmount, ref code))
                return;

            request.NewAmount = RoundVolume(request.NewAmount, smbMetadata);
            request.MaxVisibleAmount = RoundVolume(request.MaxVisibleAmount, smbMetadata);
            request.CurrentAmount = ConvertVolume(request.CurrentAmount, smbMetadata);
            request.NewAmount = ConvertNullableVolume(request.NewAmount, smbMetadata);
            request.MaxVisibleAmount = ConvertNullableVolume(request.MaxVisibleAmount, smbMetadata);
            request.AmountChange = request.NewAmount.HasValue ? request.NewAmount.Value - request.CurrentAmount : 0;
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

            //if (!ValidateMargin(request, smbMetadata, ref code)) //incorrect behavior when Margine Call
            //    return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? ConvertNullableVolume(double? volumeInLots, SymbolInfo smbMetadata)
        {
            if (volumeInLots == null)
                return null;
            return ConvertVolume(volumeInLots.Value, smbMetadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double ConvertVolume(double volumeInLots, SymbolInfo smbMetadata)
        {
            var res = smbMetadata.LotSize * volumeInLots;
            var amountDigits = 6; //smbMetadata.AmountDigits;
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
        private double RoundVolume(double volumeInLots, SymbolInfo smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? RoundVolume(double? volumeInLots, SymbolInfo smbMetadata)
        {
            return volumeInLots.Floor(smbMetadata.TradeVolumeStep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double RoundPrice(double price, SymbolInfo smbMetadata, Domain.OrderInfo.Types.Side side)
        {
            return side == Domain.OrderInfo.Types.Side.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double? RoundPrice(double? price, SymbolInfo smbMetadata, Domain.OrderInfo.Types.Side side)
        {
            return side == Domain.OrderInfo.Types.Side.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        #region Validation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetSymbol(string symbolName, out SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
        {
            smbMetadata = _symbols.GetOrNull(symbolName).Info;
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
            order = _account.Orders.GetOrNull(orderId);
            if (order == null)
            {
                code = OrderCmdResultCodes.OrderNotFound;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetNetPosition(string symbol, out NetPosition position, ref OrderCmdResultCodes code)
        {
            position = _account.NetPositions.GetOrNull(symbol);
            if (position == null)
            {
                code = OrderCmdResultCodes.PositionNotFound;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetMarketPrice(SymbolInfo smb, Domain.OrderInfo.Types.Side orderSide, out double price, ref OrderCmdResultCodes code)
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
        private bool ValidateVolumeLots(double volumeLots, SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
        {
            if (volumeLots.Lt(0.0) || volumeLots.Lt(smbMetadata.MinTradeVolume) || volumeLots.Gt(smbMetadata.MaxTradeVolume))
            {
                code = OrderCmdResultCodes.IncorrectVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateVolumeLots(double? volumeLots, SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
        {
            if (!volumeLots.HasValue)
                return true;

            if (volumeLots.Value.Lt(0.0) || volumeLots.Value.Lt(smbMetadata.MinTradeVolume) || volumeLots.Value.Gt(smbMetadata.MaxTradeVolume))
            {
                code = OrderCmdResultCodes.IncorrectVolume;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateMaxVisibleVolumeLots(double? maxVisibleVolumeLots, SymbolInfo smbMetadata, Domain.OrderInfo.Types.Type orderType, double? volumeLots, ref OrderCmdResultCodes code)
        {
            if (!maxVisibleVolumeLots.HasValue)
                return true;

            var isIncorrectMaxVisibleVolume = orderType == Domain.OrderInfo.Types.Type.Stop
                || (maxVisibleVolumeLots.Value.Gt(0.0) && maxVisibleVolumeLots.Value.Lt(smbMetadata.MinTradeVolume))
                || maxVisibleVolumeLots.Value.Gt(smbMetadata.MaxTradeVolume);

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
        private bool ValidateTradeEnabled(SymbolInfo smbMetadata, ref OrderCmdResultCodes code)
        {
            if (!smbMetadata.TradeAllowed)
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

            if (options.Value.HasFlag(Domain.OrderExecOptions.ImmediateOrCancel) && !isOrderTypeCompatibleToIoC)
            {
                code = OrderCmdResultCodes.Unsupported;
                return false;
            }

            var isOrderTypeCompabilityToOCO = orderType == Domain.OrderInfo.Types.Type.Limit || orderType == Domain.OrderInfo.Types.Type.Stop;

            if (options.Value.HasFlag(Domain.OrderExecOptions.OneCancelsTheOther) && !isOrderTypeCompabilityToOCO)
            {
                code = OrderCmdResultCodes.Unsupported;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateOCO(Domain.OrderExecOptions? options, string relatedOrderId, ref OrderCmdResultCodes code)
        {
            if (options == null || !options.Value.HasFlag(Domain.OrderExecOptions.OneCancelsTheOther))
                return true;

            if (string.IsNullOrEmpty(relatedOrderId))
            {
                code = OrderCmdResultCodes.OCORelatedIdNotFound;
                return false;
            }

            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private bool ValidateMargin(Domain.OpenOrderRequest request, SymbolInfo symbol, ref OrderCmdResultCodes code)
        //{
        //    var isHidden = request.MaxVisibleAmount != null && request.MaxVisibleAmount == 0;

        //    if (Calc != null && !Calc.HasEnoughMarginToOpenOrder(symbol, request.Amount, request.Type, request.Side, request.Price, request.StopPrice, isHidden, out CalcErrorCodes error))
        //    {
        //        code = error.ToOrderError();

        //        if (code == OrderCmdResultCodes.Ok)
        //            code = OrderCmdResultCodes.NotEnoughMoney;

        //        return false;
        //    }
        //    return true;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private bool ValidateMargin(Domain.ModifyOrderRequest request, SymbolInfo symbol, ref OrderCmdResultCodes code)
        //{
        //    var oldOrder = _account.Orders.GetOrNull(request.OrderId);

        //    var newIsHidden = request.MaxVisibleAmount != null && request.MaxVisibleAmount == 0;

        //    var newVol = request.NewAmount ?? (double)oldOrder.Info.RemainingAmount;
        //    var newPrice = request.Price ?? oldOrder.Info.Price;
        //    var newStopPrice = request.StopPrice ?? oldOrder.Info.StopPrice;

        //    if (Calc != null && !Calc.HasEnoughMarginToModifyOrder(oldOrder, symbol, newVol, newPrice, newStopPrice, newIsHidden))
        //    {
        //        code = OrderCmdResultCodes.NotEnoughMoney;
        //        return false;
        //    }

        //    return true;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateQuotes(SymbolInfo symbol, Domain.OrderInfo.Types.Side side, ref OrderCmdResultCodes code)
        {
            var quote = symbol.LastQuote;

            if (quote == null)
            {
                code = OrderCmdResultCodes.OffQuotes;
                return false;
            }

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
}
