using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;
using TickTrader.BusinessObjects.Messaging;

namespace TickTrader.Algo.Core
{
    internal class TradeEmulator : TradeCommands, IExecutorFixture
    {
        private ActivationEmulator _activator = new ActivationEmulator();
        private CalculatorFixture _calcFixture;
        private AccountAccessor _acc;
        private IFixtureContext _context;
        private DealerEmulator _dealer = new DefaultDealer();
        private InvokeEmulator _scheduler;
        private BacktesterCollector _collector;
        private IBacktesterSettings _settings;
        private long _orderIdSeed;
        private double _stopOutLevel = 30;
        private RateUpdate _lastRate;

        public TradeEmulator(IFixtureContext context, IBacktesterSettings settings, CalculatorFixture calc, InvokeEmulator scheduler, BacktesterCollector collector)
        {
            _context = context;
            _calcFixture = calc;
            _scheduler = scheduler;
            _collector = collector;
            _settings = settings;

            VirtualServerPing = settings.ServerPing;
            _scheduler.RateUpdated += r =>
            {
                _lastRate = r;
                CheckActivation(r);
                RecalculateAccount();
            };
        }

        public TimeSpan VirtualServerPing { get; set; }

        public void Start()
        {
            _context.Builder.SetCustomTradeAdapter(this);

            _acc = _context.Builder.Account;

            _acc.Orders.Clear();
            _acc.NetPositions.Clear();
            _acc.Assets.Clear();

            _acc.Balance = 0;
            _acc.ResetCurrency();
            _acc.Leverage = 0;
            _acc.Profit = 0;
            _acc.MarginLevel = 0;
            _acc.Margin = 0;

            _acc.Type = _settings.AccountType;

            if (_acc.IsMarginType)
            {
                _acc.Balance = (decimal)_settings.InitialBalance;
                _acc.Leverage = _settings.Leverage;
                _acc.UpdateCurrency(_settings.Currencies.GetOrStub(_settings.BalanceCurrency));
                _opSummary.AccountCurrencyFormat = _acc.BalanceCurrencyFormat;
            }
            else if (_acc.IsCashType)
            {
                var currencies = _context.Builder.Currencies.CurrencyListImp.ToDictionary(c => c.Name);

                foreach (var asset in _settings.InitialAssets)
                    _acc.Assets.Update(new AssetEntity(asset.Value, asset.Key), currencies);
            }
        }

        public void Stop()
        {
            CloseAllPositions(TradeTransReasons.Rollover);
        }

        public void Restart()
        {
        }

        public void Dispose()
        {
        }

        #region TradeCommands

        public Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType type, OrderSide side, double volumeLots, double price,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag)
        {
            var limPrice = type != OrderType.Stop ? price : (double?)null;
            var stopPrice = type == OrderType.Stop ? price : (double?)null;

            return OpenOrder(isAysnc, symbol, type, side, volumeLots, null, limPrice, stopPrice, sl, tp, comment, options, tag, null);
        }

        public Task<OrderCmdResult> OpenOrder(bool isAysnc, string symbol, OrderType orderType, OrderSide side, double volumeLots, double? maxVisibleVolumeLots, double? price, double? stopPrice,
            double? sl, double? tp, string comment, OrderExecOptions options, string tag, DateTime? expiration)
        {
            return ExecTradeRequest(isAysnc, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                var calc = _calcFixture.GetCalculator(symbol, _calcFixture.Acc.BalanceCurrency);
                var smbMetadata = (SymbolAccessor)calc.SymbolInfo;

                try
                {
                    volumeLots = RoundVolume(volumeLots, smbMetadata);
                    maxVisibleVolumeLots = RoundVolume(maxVisibleVolumeLots, smbMetadata);
                    double volume = ConvertVolume(volumeLots, smbMetadata);
                    double? maxVisibleVolume = ConvertNullableVolume(maxVisibleVolumeLots, smbMetadata);
                    price = RoundPrice(price, smbMetadata, side);
                    stopPrice = RoundPrice(stopPrice, smbMetadata, side);
                    sl = RoundPrice(sl, smbMetadata, side);
                    tp = RoundPrice(tp, smbMetadata, side);

                    // emulate server ping
                    await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                    using (JournalScope())
                    {
                        ValidateOrderTypeForAccount(orderType, calc.SymbolInfo);
                        ValidateTypeAndPrice(orderType, price, stopPrice, sl, tp, maxVisibleVolume, options, calc.SymbolInfo);

                        //Facade.ValidateExpirationTime(Request.Expiration, _acc);

                        var order = OpenOrder(calc, orderType, side, volume, maxVisibleVolume, price, stopPrice, sl, tp, comment, options, tag, expiration, OpenOrderOptions.None);

                        _collector.OnOrderOpened();

                        // set result
                        return new OrderResultEntity(OrderCmdResultCodes.Ok, order.Clone());
                    }
                }
                catch (OrderValidationError ex)
                {
                    error = ex.ErrorCode;
                }
                catch (MisconfigException ex)
                {
                    error = OrderCmdResultCodes.Misconfiguration;
                    _scheduler.SetFatalError(ex);
                }

                _collector.OnOrderRejected();

                using (JournalScope())
                    _opSummary.AddOpenFailAction(orderType, symbol, side, volumeLots, error, _acc);

                return new OrderResultEntity(error);
            });
        }

        public Task<OrderCmdResult> CancelOrder(bool isAysnc, string orderId)
        {
            return ExecTradeRequest(isAysnc, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                try
                {
                    // emulate server ping
                    await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                    //Logger.Info(() => LogPrefix + "Processing cancel order request " + Request);

                    // Check schedule for the symbol
                    var order = _acc.Orders.GetOrderOrThrow(orderId);
                    //var symbol = node.GetSymbolEntity(order.Symbol);

                    //Facade.Infrustructure.LogTransactionDetails(() => "Processing cancel order request " + Request, JournalEntrySeverities.Info, Token, TransactDetails.Create(order.OrderId, symbol.Name));

                    //node.CheckTradeTime(Request, symbol);

                    TradeTransReasons trReason = TradeTransReasons.ClientRequest;
                    //if (Request.ExpirationFlag)
                    //    trReason = TradeTransReasons.Expired;
                    //if (Request.StopoutFlag)
                    //    trReason = TradeTransReasons.StopOut;

                    CancelOrder(order, trReason);

                    _collector.LogTrade($"Canceled order #{orderId} {order.Type} {order.Symbol} {order.Side} amount={order.RequestedVolume}");

                    // set result
                    return new OrderResultEntity(OrderCmdResultCodes.Ok, order);
                }
                catch (OrderValidationError ex)
                {
                    error = ex.ErrorCode;
                }
                catch (MisconfigException ex)
                {
                    error = OrderCmdResultCodes.Misconfiguration;
                    _scheduler.SetFatalError(ex);
                }

                return new OrderResultEntity(error);
            });
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double price, double? sl, double? tp, string comment)
        {
            return ModifyOrder(isAysnc, orderId, price, null, null, sl, tp, comment, null, null, null);
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, string orderId, double? price, double? stopPrice, double? maxVisibleVolume, double? sl, double? tp, string comment, DateTime? expiration, double? volume, OrderExecOptions? options)
        {
            return ExecTradeRequest(isAysnc, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                try
                {
                    var orderToModify = _acc.GetOrderOrThrow(orderId);
                    var smbMetadata = orderToModify.SymbolInfo;

                    double orderVolume = ConvertVolume(orderToModify.RemainingVolume, smbMetadata);
                    double orderVolumeInLots = orderVolume / smbMetadata.ContractSize;

                    volume = RoundVolume(volume, smbMetadata);
                    maxVisibleVolume = RoundVolume(maxVisibleVolume, smbMetadata);

                    double? newOrderVolume = ConvertNullableVolume(volume, smbMetadata);
                    double? orderMaxVisibleVolume = ConvertNullableVolume(maxVisibleVolume, smbMetadata);
                    price = RoundPrice(price, smbMetadata, orderToModify.Side);
                    stopPrice = RoundPrice(stopPrice, smbMetadata, orderToModify.Side);
                    sl = RoundPrice(sl, smbMetadata, orderToModify.Side);
                    tp = RoundPrice(tp, smbMetadata, orderToModify.Side);

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


                    // emulate server ping
                    await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                    using (JournalScope())
                    {
                        var order = ReplaceOrder(request);

                        _collector.OnOrderModified();

                        // set result
                        return new OrderResultEntity(OrderCmdResultCodes.Ok, order.Clone());
                    }
                }
                catch (OrderValidationError ex)
                {
                    error = ex.ErrorCode;
                }
                catch (MisconfigException ex)
                {
                    error = OrderCmdResultCodes.Misconfiguration;
                    _scheduler.SetFatalError(ex);
                }

                if (_collector.WriteOrderModifications)
                    _collector.LogTradeFail($"Rejected modify #{orderId} reason={error}");

                _collector.OnOrderModificatinRejected();

                return new OrderResultEntity(error);
            });
        }

        Task<OrderCmdResult> TradeCommands.CloseOrder(bool isAysnc, string orderId, double? volume)
        {
            var req = new CloseOrderRequest();
            req.OrderId = orderId;
            req.Volume = volume;
            return CloseOrder(isAysnc, req);
        }

        Task<OrderCmdResult> TradeCommands.CloseOrderBy(bool isAysnc, string orderId, string byOrderId)
        {
            var req = new CloseOrderRequest();
            req.OrderId = orderId;
            req.ByOrderId = byOrderId;
            return CloseOrder(isAysnc, req);
        }

        private Task<OrderCmdResult> CloseOrder(bool isAysnc, CloseOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                try
                {
                    if (_acc.Type == AccountTypes.Gross)
                    {
                        var order = _acc.GetOrderOrThrow(request.OrderId);
                        var smbMetadata = order.SymbolInfo;
                        var closeVolume = (double?)null;

                        if (request.Volume != null)
                        {
                            var closeVolumeLots = RoundVolume(request.Volume, smbMetadata);
                            ValidateVolumeLots(closeVolumeLots, smbMetadata);
                            closeVolume = ConvertVolume(closeVolumeLots.Value, smbMetadata);
                        }

                        if (request.ByOrderId == null)
                        {
                            // emulate server ping
                            await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                            using (JournalScope())
                            {
                                EnsureOrderIsPosition(order);

                                var currentRate = smbMetadata.LastQuote;
                                var dealerRequest = new ClosePositionDealerRequest(order, currentRate);
                                dealerRequest.CloseAmount = request.Volume;

                                _dealer.ConfirmPositionClose(dealerRequest);

                                if (!dealerRequest.Confirmed || dealerRequest.DealerPrice <= 0)
                                    throw new OrderValidationError("Order is rejected by dealer", OrderCmdResultCodes.DealerReject);

                                ClosePosition(order, TradeTransReasons.ClientRequest, closeVolume.NanAwareToDecimal(), dealerRequest.DealerPrice, smbMetadata,
                                    ClosePositionOptions.None, request.ByOrderId);
                            }
                        }
                        else
                        {
                            var byOrder = _acc.GetOrderOrThrow(request.OrderId);

                            // emulate server ping
                            await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                            using (JournalScope())
                            {
                                EnsureOrderIsPosition(order);
                                EnsureOrderIsPosition(byOrder);

                                ConfirmPositionCloseBy(order, byOrder, TradeTransReasons.ClientRequest, true);
                            }
                        }

                        return new OrderResultEntity(OrderCmdResultCodes.Ok, order.Clone());
                    }
                    else
                        throw new OrderValidationError(OrderCmdResultCodes.Unsupported);
                }
                catch (OrderValidationError ex)
                {
                    error = ex.ErrorCode;
                }
                catch (MisconfigException ex)
                {
                    error = OrderCmdResultCodes.Misconfiguration;
                    _scheduler.SetFatalError(ex);
                }

                _collector.LogTradeFail($"Rejected close #{request.OrderId} reason={error}");
                return new OrderResultEntity(OrderCmdResultCodes.Misconfiguration);
            });
        }

        private static int requestSeed;

        private Task<OrderCmdResult> ExecTradeRequest(bool isAsync, Func<Task<OrderCmdResult>> executorInvoke)
        {
            var task = executorInvoke();

            var id = requestSeed++;

            if (!isAsync)
            {
                while (!task.IsCompleted)
                    _scheduler.ProcessNextTrade();
            }

            return task;
        }

        #endregion

        #region Trade Facade copy

        private string NewOrderId()
        {
            return (++_orderIdSeed).ToString();
        }

        private OrderAccessor OpenOrder(OrderCalculator orderCalc, OrderType orderType, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice,
            double? sl, double? tp, string comment, OrderExecOptions execOptions, string tag, DateTime? expiration, OpenOrderOptions options)
        {
            var symbolInfo = orderCalc.SymbolInfo;

            var orderEntity = new OrderEntity(NewOrderId());
            //order.SymbolPrecision = symbolInfo.Digits;
            orderEntity.RequestedVolume = volume;
            orderEntity.RemainingVolume = volume;
            orderEntity.MaxVisibleVolume = maxVisibleVolume;

            orderEntity.Side = side;
            orderEntity.Type = orderType;
            orderEntity.Symbol = symbolInfo.Symbol;
            orderEntity.Created = _scheduler.VirtualTimePoint;
            orderEntity.Comment = comment;

            //order.ClientOrderId = request.ClientOrderId;
            //order.Status = OrderStatuses.New;
            //order.InitialType = request.InitialType ?? request.Type;
            //order.ParentOrderId = request.ParentOrderId;

            //decimal? price = (decimal?)request.Price;
            //decimal? stopPrice = (decimal?)request.StopPrice;

            // Slippage calculation
            //if ((_acc.AccountingType == AccountingTypes.Cash) && ((request.InitialType == OrderTypes.Market) || (request.InitialType == OrderTypes.Stop)))
            //{
            //    decimal initialPrice = price ?? 0;
            //    decimal freeMarginPrice = acc.CalculateFreeMarginPrice(order, symbolInfo.Digits);
            //    decimal slippage = TradeLogic.GetSlippagePips(request.Side, symbolInfo);

            //    if (order.Side == OrderSide.Buy)
            //    {
            //        // Buy order price with slippage is limited by the upper free margin price or initial price
            //        price =  Math.Min(initialPrice + slippage, Math.Max(initialPrice, freeMarginPrice));
            //        price = ObjectCaches.RoundingTools.WithPrecision(symbolInfo.Digits).Floor(price.Value);
            //    }
            //    else
            //    {
            //        // Sell order price with slippage is limited by the minimal avaliable price for the symbol
            //        price = Math.Max(initialPrice + slippage, freeMarginPrice);
            //        price = ObjectCaches.RoundingTools.WithPrecision(symbolInfo.Digits).Ceil(price.Value);
            //    }

            //    var slippageCalculationMessage = $"Slippage calculation: InitialPrice={initialPrice} Slippage={slippage} FreeMarginPrice={freeMarginPrice} Price={price}";
            //    //LogTransactionDetails(() => slippageCalculationMessage, JournalEntrySeverities.Info);
            //}

            orderEntity.StopLoss = sl;
            orderEntity.TakeProfit = tp;
            //order.TransferringCoefficient = request.TransferringCoefficient;
            orderEntity.UserTag = tag;
            orderEntity.InstanceId = _acc.InstanceId;
            orderEntity.Expiration = expiration;
            orderEntity.Options = execOptions;
            //order.ReqOpenPrice = clientPrice;
            //order.ReqOpenAmount = clientAmount;

            if (orderType != OrderType.Stop)
                orderEntity.Price = price;
            if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                orderEntity.StopPrice = stopPrice;

            var order = new OrderAccessor(orderEntity, (SymbolAccessor)symbolInfo);

            _calcFixture.ValidateNewOrder(order, orderCalc);

            //string comment = null;

            // add new order
            //acc.AddTemporaryNewOrder(order);

            RateUpdate currentRate = _calcFixture.GetCurrentRateOrThrow(symbolInfo.Symbol);

            TradeTransReasons trReason = options.HasFlag(OpenOrderOptions.Stopout) ? TradeTransReasons.StopOut : TradeTransReasons.DealerDecision;

            if (!options.HasFlag(OpenOrderOptions.SkipDealing))
            {
                // Dealer request
                var dealerRequest = new OpenOrderDealerRequest(order, currentRate.LastQuote);
                _dealer.ConfirmOrderOpen(dealerRequest);

                if (!dealerRequest.Confirmed || dealerRequest.DealerAmount < 0 || dealerRequest.DealerPrice <= 0)
                    throw new OrderValidationError("Order is rejected by dealer", OrderCmdResultCodes.DealerReject);

                return ConfirmOrderOpening(order, trReason, dealerRequest.DealerPrice, dealerRequest.DealerAmount);
            }

            return ConfirmOrderOpening(order, trReason, null, null);
        }

        private OrderAccessor ConfirmOrderOpening(OrderAccessor order, TradeTransReasons trReason, decimal? execPrice, decimal? execAmount)
        {
            RateUpdate currentRate = _calcFixture.GetCurrentRateOrNull(order.Symbol);

            _acc.Orders.Add(order);

            //CommissionStrategy.OnOrderOpened(order, null);

            //var orderCopy = order.Clone();
            var fillInfo = new FillInfo();

            if (order.Type == OrderType.Market)
            {
                // fill order
                fillInfo = FillOrder(order, execPrice, execAmount, trReason);
            }
            else if (order.Type == OrderType.Limit && order.HasOption(OrderExecOptions.ImmediateOrCancel))
            {
                // fill order
                if (execPrice != null && execAmount != null)
                    fillInfo = FillOrder(order, execPrice, execAmount, trReason);
                //else
                //    orderCopy = order.Clone();

                if (order.RemainingAmount > 0) // partial fill
                {
                    //// cancel remaining part
                    //LogTransactionDetails(() => "Cancelling IoC Order #" + order.OrderId + ", RemainingAmount=" + orderCopy.RemainingAmount + ", Reason=" + TradeTransReasons.DealerDecision,
                    //    JournalEntrySeverities.Info, order.Clone());

                    //ConfirmOrderCancelation(acc, TradeTransReasons.DealerDecision, order.OrderId, null, clientRequestId, false);
                }
            }
            else if (order.Type == OrderType.Limit || order.Type == OrderType.Stop || order.Type == OrderType.StopLimit)
            {
                RegisterOrder(order, currentRate);
                //FinalizeOrderOperation(order, null, order.SymbolRef, acc, OrderStatuses.Calculated, OrderExecutionEvents.Allocated);
            }
            else if (order.Type == OrderType.Position)
                throw new OrderValidationError("Invalid order type", OrderCmdResultCodes.InternalError);
            else
                throw new OrderValidationError("Unknown order type", OrderCmdResultCodes.InternalError);

            RecalculateAccount();

            // fire API event
            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(order)));

            _opSummary.AddOpenAction(order);
            if (fillInfo.WasNetPositionClosed)
                _opSummary.AddNetCloseAction(fillInfo.NetClose, order.SymbolInfo);

            return order;
        }

        private OrderAccessor ReplaceOrder(ReplaceOrderRequest request)
        {
            // Check schedule for the symbol
            var order = _acc.Orders.GetOrderOrThrow(request.OrderId);
            var symbol = _context.Builder.Symbols.GetOrDefault(order.Symbol);

            //Facade.Infrustructure.LogTransactionDetails(() => "Processing modify order request " + Request, JournalEntrySeverities.Info, Token, TransactDetails.Create(order.OrderId, symbol.Name));

            //node.CheckTradeTime(Request, symbol);

            //Acc.ThrowIfInoperable(Request.IsClientRequest);
            //Acc.ThrowIfReadonly(Request.IsClientRequest);

            //OrderModel replaceOrder = Facade.InitOrderReplace(Acc, Request);
            //Request.OrderId = replaceOrder.OrderId; // ensure orderId
            //Request.Type = replaceOrder.Type;
            //Request.Side = replaceOrder.Side;
            //Request.Symbol = replaceOrder.Symbol;

            //ValidateType(replaceOrder);

            if (!order.IsPending) // forbid to change price and volume for positions (server style!)
            {
                request.Price = null;
                request.NewVolume = null;
            }

            //SymbolEntity symbolInfo = node.ServerConfig.GetSymbolByNameOrThrow(Request.Symbol);
            //GroupSecurityCfg securityCfg = Acc.GetSecurityCfgAndThrowIfInoperable(symbolInfo, Request.IsClientRequest);

            //if (Request.IsClientRequest)
            //if (request.Price != null)
            //    ValidatePrice(request.Price.Value, symbol);

            // Optimistic check the previous order remaining amount
            //if (request.PrevRemainingAmount.HasValue)
            //{
            //    if (replaceOrder.RemainingAmount != Request.PrevRemainingAmount.Value)
            //    {
            //        throw ServerFaultException.Create(new OrderModificationFault("Order amount was changed in-flight before the request is processed!", FaultCodes.OrderModificationFault));
            //    }
            //}

            //if ((request.Amount.HasValue || request.RemainingAmount.HasValue) && replaceOrder.IsPending)
            //{
            //    Request.RemainingAmount = Request.Amount;
            //    VerifyAmout(Request.Amount.Value, securityCfg, symbolInfo;
            //}

            // In-Flight Mitigation pending order modification
            //bool cancelOrder = false;
            //if (Request.Amount.HasValue && Request.InFlightMitigationFlag.HasValue && Request.InFlightMitigationFlag.Value && replaceOrder.IsPending)
            //{
            //    decimal executed = replaceOrder.Amount - replaceOrder.RemainingAmount;

            //    // This calculation has the goal of preventing orders from being overfilled
            //    if (Request.Amount.Value > executed)
            //        Request.RemainingAmount = Request.Amount.Value - executed;
            //    else
            //        cancelOrder = true;
            //}

            //if (Request.MaxVisibleAmount.HasValue && (Request.MaxVisibleAmount.Value >= 0))
                //Facade.VerifyMaxVisibleAmout(Request.MaxVisibleAmount, securityCfg, symbolInfo, Request.IsClientRequest);

            var newVolume = request.NewVolume.NanAwareToDecimal() ?? order.Amount;
            var newPrice = request.Price ?? order.Price;
            var newStopPrice = request.StopPrice ?? order.StopPrice;

            bool volumeChanged = newVolume != order.Amount;

            // Check margin of the modified order
            if (volumeChanged)
                _calcFixture.ValidateModifyOrder(order, newVolume, newPrice.NanAwareToDecimal(), newStopPrice.NanAwareToDecimal());

            // dealer request
            var dealerRequest = new ModifyOrderDealerRequest(order, symbol.LastQuote);
            dealerRequest.NewComment = request.Comment;
            dealerRequest.NewPrice = request.Price;
            dealerRequest.NewVolume = request.NewVolume;
            dealerRequest.NewStopPrice = request.StopPrice;
            _dealer.ConfirmOrderReplace(dealerRequest);

            if (!dealerRequest.Confirmed)
                throw new OrderValidationError("Rejected By Dealer", OrderCmdResultCodes.DealerReject);

            return ConfirmOrderReplace(order, request);

            //if (cancelOrder)
            //{
            //    var cancelRequest = new CancelOrderRequest();
            //    cancelRequest.AccountId = Request.AccountId;
            //    cancelRequest.ManagerId = Request.ManagerId;
            //    cancelRequest.AccountManagerTag = Request.AccountManagerTag;
            //    cancelRequest.ManagerOptions = Request.ManagerOptions;
            //    cancelRequest.RequestClientId = Request.RequestClientId;
            //    cancelRequest.OrderId = Request.OrderId;
            //    cancelRequest.ClientOrderId = Request.ClientOrderId;

            //    TradeTransReasons trReason = Request.IsClientRequest ? TradeTransReasons.ClientRequest : TradeTransReasons.DealerDecision;

            //    RefOrder = await Facade.CancelOrderAsync(cancelRequest, Acc, trReason);

            //    // Create report
            //    var report = new CancelOrderReport();
            //    report.RequestClientId = Request.RequestClientId;
            //    report.OrderCopy = RefOrder;
            //    report.Level = Request.Level;
            //    SetResponse(report);
            //}
            //else
            //{
            //    try
            //    {
            //        // Dealer request.
            //        DealerResponseParams dResp = await Facade.ExecuteDealing(Acc, replaceOrder, Request.IsClientRequest, Request.SkipDealing,
            //            () =>
            //            {
            //                DealerRequest request = node.CreateDealerRequest(DealerReqTypes.ModifyOrder, Acc, replaceOrder, Facade.GetCurrentOpenPrice(replaceOrder));
            //                request.ModifyInfo = Request;
            //                return request;
            //            },
            //            (pr, force) => true);

            //        if (dResp?.DealerLogin != null)
            //        {
            //            StringBuilder what = new StringBuilder();
            //            what.AppendFormat("Replace Order #{0}", order.OrderId);
            //            if (dResp.Amount.HasValue || dResp.Price.HasValue) what.Append(" with");
            //            if (dResp.Amount.HasValue) what.AppendFormat(" Amount={0}", dResp.Amount);
            //            if (dResp.Price.HasValue) what.AppendFormat(" Price={0}", dResp.Price);

            //            Facade.LogTransactionDetails(() => $"Dealer '{dResp.DealerLogin}' confirmed {what}", JournalEntrySeverities.Info, TransactDetails.Create(order.OrderId, symbol.Name));
            //        }

            //        RefOrder = ConfirmOrderReplace(Acc, Request.OrderId.Value, Request, dResp?.Comment);
            //    }
            //    catch (ServerFaultException<DealerRejectFault> ex)
            //    {
            //        Facade.RejectOrderReplace(Acc, Request.OrderId.Value, Request.RequestClientId,
            //            new TradeRejectInfo(TradeRejectReasons.RejectedByDealer, ex.Details.Message));
            //        throw;
            //    }
            //    catch (ServerFaultException<TimeoutFault> ex)
            //    {
            //        Facade.RejectOrderReplace(Acc, Request.OrderId.Value, Request.RequestClientId,
            //            new TradeRejectInfo(TradeRejectReasons.DealerTimeout, ex.Details.Message));
            //        throw;
            //    }
            //}
        }

        private OrderAccessor ConfirmOrderReplace(OrderAccessor order, ReplaceOrderRequest request)
        {
            //OrderModel order = (!request.IsClientRequest && request.UpdateNewOrdersInDealing)
            //    ? acc.GetNewOrder(orderId)
            //    : acc.GetOrder(orderId);

            RateUpdate currentRate = _calcFixture.GetCurrentRateOrThrow(request.Symbol);

            var oldOrderCopy = order.Clone();

            if (order.Type != OrderType.Market)
                UnregisterOrder(order);

            if (order.IsPending && request.NewVolume.HasValue && (decimal)request.NewVolume != order.Amount)
            {
                decimal newVolume = (decimal)request.NewVolume.Value;
                decimal filledVolume = order.Amount - order.RemainingAmount;

                order.Amount = newVolume;
                order.RemainingAmount = newVolume - filledVolume;

                // Recalculate commission if necessary.
                //var mAcc = acc as MarginAccountModel;
                //CommissionStrategy.OnOrderModified(order, null, mAcc);
            }

            // Update or reset max visible amount value
            if (order.IsPending && request.MaxVisibleVolume.HasValue)
            {
                if (request.MaxVisibleVolume.Value < 0)
                {
                    order.Entity.MaxVisibleVolume = null;
                }
                else
                {
                    order.Entity.MaxVisibleVolume = request.MaxVisibleVolume;
                    //order.Options = order.Options.SetFlag(OrderExecutionOptions.HiddenIceberg);
                }
            }

            if (request.Price.HasValue)
            {
                if (order.Type == OrderType.Limit || order.Type == OrderType.StopLimit)
                {
                    order.Entity.Price = request.Price.Value;
                    //order.ReqOpenPrice = request.Price.Value;
                }
            }

            if (request.StopPrice.HasValue)
            {
                if (order.Type == OrderType.Stop || order.Type == OrderType.StopLimit)
                {
                    order.Entity.StopPrice = request.StopPrice ?? order.StopPrice;
                    //order.ReqOpenPrice = request.StopPrice.Value;
                }
            }

            // Change IOC option for stop-limit orders
            //if ((request.ImmediateOrCancelFlag.HasValue) && (order.InitialType == OrderTypes.StopLimit) && (order.Type == OrderTypes.StopLimit))
            //{
            //    order.Options = request.ImmediateOrCancelFlag.Value ? order.Options.SetFlag(OrderExecutionOptions.ImmediateOrCancel) : order.Options.ClearFlag(OrderExecutionOptions.ImmediateOrCancel);
            //}

            if (order.IsPending && request.Expiration.HasValue)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                if (request.Expiration.Value > epoch)
                    order.Entity.Expiration = request.Expiration.Value;
                else
                    order.Entity.Expiration = null;
            }

            if (_acc.AccountingType == AccountingTypes.Gross)
            {
                if (request.StopLoss.HasValue)
                    order.Entity.StopLoss = (request.StopLoss.Value != 0) ? request.StopLoss : null;
                if (request.TakeProfit.HasValue)
                    order.Entity.TakeProfit = (request.TakeProfit.Value != 0) ? request.TakeProfit : null;
            }

            order.Entity.Comment = request.Comment ?? order.Comment;
            order.Entity.UserTag = request.Tag == null ? order.Entity.UserTag : CompositeTag.ExtarctUserTarg(request.Tag);
            //order.Magic = request.Magic ?? order.Magic;

            // calculate reduced commission options
            //ISymbolRate currentRate = infrustructure.GetCurrentRateOrNull(order.Symbol);
            //if (order.IsHidden || order.IsIceberg)
            //{
            //    order.IsReducedOpenCommission = false;
            //    order.IsReducedCloseCommission = false;
            //}
            //else
            //{
            //    if (order.Type == OrderTypes.Limit)
            //    {
            //        if ((currentRate != null) &&
            //            ((order.Side == OrderSides.Buy && currentRate.NullableAsk.HasValue) ||
            //             (order.Side == OrderSides.Sell && currentRate.NullableBid.HasValue)))
            //        {
            //            order.IsReducedOpenCommission = !order.HasOption(OrderExecutionOptions.ImmediateOrCancel) &&
            //                                            ((order.Side == OrderSides.Buy && order.Price < currentRate.Ask) ||
            //                                             (order.Side == OrderSides.Sell && order.Price > currentRate.Bid));
            //            if (acc.AccountingType == AccountingTypes.Cash)
            //                order.IsReducedCloseCommission = order.IsReducedOpenCommission;
            //        }
            //    }
            //    else if (order.Type == OrderTypes.Position && acc.AccountingType == AccountingTypes.Gross)
            //    {
            //        if ((currentRate != null) &&
            //            ((order.Side == OrderSides.Buy && currentRate.NullableBid.HasValue) ||
            //             (order.Side == OrderSides.Sell && currentRate.NullableAsk.HasValue)))
            //        {
            //            order.IsReducedCloseCommission = order.TakeProfit.HasValue &&
            //                                             ((order.Side == OrderSides.Buy &&
            //                                               order.TakeProfit.Value > currentRate.Bid) ||
            //                                              (order.Side == OrderSides.Sell &&
            //                                               order.TakeProfit.Value < currentRate.Ask));
            //        }
            //    }
            //}

            if (order.Type != OrderType.Market)
                RegisterOrder(order, currentRate);

            //_acc.CalculateOrder(order, infrustructure);
            order.FireChanged();

            // fire API event
            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrderCopy, order)));

            RecalculateAccount();

            if (_collector.WriteOrderModifications)
                _opSummary.AddModificationAction(oldOrderCopy, order);

            //Order orderCopy = FinalizeOrderOperation(order, orderPrev, order.SymbolRef, acc, OrderStatuses.Calculated, OrderExecutionEvents.Modified, request.RequestClientId);
            return order;
        }

        private FillInfo FillOrder(OrderAccessor order, decimal? fillPrice, decimal? fillAmount, TradeTransReasons reason)
        {
            decimal actualPrice;
            decimal actualAmount;

            if (fillPrice == null)
            {
                var quote = _calcFixture.GetCurrentRateOrNull(order.Symbol);
                actualPrice = GetCurrentOpenPrice(order) ?? 0;
            }
            else
                actualPrice = fillPrice.Value;

            if (fillAmount != null)
                actualAmount = fillAmount.Value;
            else
                actualAmount = order.RemainingAmount;

            return FillOrder2(order, actualPrice, actualAmount, reason);
        }

        // can lead to: 1) new gross position 2) net position settlement 3) asset movement
        private FillInfo FillOrder2(OrderAccessor order, decimal fillPrice, decimal fillAmount, TradeTransReasons reason)
        {
            if (order.Type == OrderType.Position)
                throw new Exception("Order already filled #" + order.OrderId);

            var copy = order.Clone();

            if (fillAmount >= order.RemainingAmount)
                fillAmount = order.RemainingAmount;
            bool partialFill = order.RemainingAmount > fillAmount;

            //Logger.Info(() => OperationContext.LogPrefix + "Fill order " + order + ", FillAmount=" + fillAmount);

            if (partialFill)
            {
                //order.Status = OrderStatuses.Calculated;
                order.RemainingAmount -= fillAmount;
                _calcFixture.CalculateOrder(order);

                if (order.IsPending)
                    ResetOrderActivation(order);
            }
            else
            {
                //order.Status = OrderStatuses.Filled;
                order.RemainingAmount = 0;

                if (order.IsPending)
                    UnregisterOrder(order);
            }

            //order.AggrFillPrice += fillAmount * fillPrice;
            //order.AverageFillPrice = order.AggrFillPrice / (order.Amount - order.RemainingAmount);
            //order.Entity.Filled = OperationContext.ExecutionTime;
            order.Entity.Modified = _scheduler.VirtualTimePoint;

            if ((_acc.AccountingType == AccountingTypes.Net) || (_acc.AccountingType == AccountingTypes.Cash))
            {
                // increase reported action number
                //order.ActionNo++;
            }

            // Create reports
            //TradeReportModel tradeReport = null;
            //if (acc.AccountingType != AccountingTypes.Gross)
            //    tradeReport = TradeReportModel.Create(acc, TradeTransTypes.OrderFilled, reason);

            // do account-specific fill
            if (_acc.Type == AccountTypes.Cash)
                UpdateAssetsOnFill(order, fillPrice, fillAmount);
            //acc.FillOrder(order, this, fillPrice, fillAmount, partialFill, isExecutedAsClient, execReport, fillReport);

            // update/charge commission
            //TradeChargesInfo charges = new TradeChargesInfo();
            //CommissionStrategy.OnOrderFilled(order, fillAmount, fillPrice, charges, tradeReport, execReport);

            // Update an execution report
            //execReport.Order = order.Clone();
            //execReport.Order.Status = partialFill && order.IsPending ? OrderStatuses.Calculated : OrderStatuses.Filled;

            //Order pumpingOrderCopy = order.Clone();
            //fillReport.BaseOrderCopy = pumpingOrderCopy;

            // update trade history
            //if (tradeReport != null)
            //{
            //    tradeReport.FillGenericOrderData(order);
            //    tradeReport.OrderLastFillAmount = fillAmount;
            //    tradeReport.OrderFillPrice = fillPrice;

            //    if (acc is MarginAccountModel)
            //    {
            //        MarginAccountModel mAcc = (MarginAccountModel)acc;
            //        tradeReport.FillAccountBalanceConversionRates(mAcc.BalanceCurrency, mAcc.Balance);
            //    }
            //}

            // journal
            //string chargesInfo = (acc is CashAccountModel)
            //    ? chargesInfo = $" Commission={charges.Commission} AgentCommission={charges.AgentCommission}"
            //    : null;

            //LogTransactionDetails(() => "Final order " + pumpingOrderCopy, JournalEntrySeverities.Info, pumpingOrderCopy);

            decimal profit = 0;
            OrderAccessor newPos = null;
            NetPositionCloseInfo closeInfo = null;

            if (_acc.Type == AccountTypes.Gross)
                newPos = CreatePositionFromOrder(TradeTransReasons.PndOrdAct, order, fillPrice, fillAmount, !partialFill);
            else if (_acc.Type == AccountTypes.Net)
            {
                closeInfo = OpenNetPositionFromOrder(order, fillAmount, fillPrice);
                profit = closeInfo.BalanceMovement;
            }

            //_collector.LogTrade($"Filled Order #{copy.Id} {order.Type} {order.Symbol} Price={fillPrice} Amount={fillAmount} RemainingAmount={copy.RemainingAmount} Profit={profit} Comment=\"{copy.Comment}\"");

            //_acc.AfterFillOrder(order, this, fillPrice, fillAmount, partialFill, tradeReport, execReport, fillReport);

            order.FireChanged();

            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderFilled(new OrderFilledEventArgsImpl(copy, order)));

            return new FillInfo() { FillAmount = fillAmount, FillPrice = fillPrice, Position = newPos, NetClose = closeInfo };
        }

        private OrderAccessor CancelOrder(OrderAccessor order, TradeTransReasons trReason)
        {
            if (order.Type != OrderType.Limit && order.Type != OrderType.Stop && order.Type != OrderType.StopLimit)
                throw new OrderValidationError("Only Limit, Stop and StopLimit orders can be canceled. Please check the type of the order #" + order.OrderId, OrderCmdResultCodes.OrderNotFound);

            var dealerReq = new CancelOrderDealerRequest(order, order.SymbolInfo.LastQuote);
            _dealer.ConfirmOrderCancelation(dealerReq);

            if (!dealerReq.Confirmed)
                throw new OrderValidationError("Cancellation is rejected by dealer", OrderCmdResultCodes.DealerReject);

            return ConfirmOrderCancelation(trReason, order);
        }

        private OrderAccessor ConfirmOrderCancelation(TradeTransReasons trReason, OrderAccessor order, OrderAccessor originalOrder = null)
        {
            //var order = _acc.GetOrderOrThrow(orderId);

            // to prevent cancelling orders that was already filled
            if (originalOrder != null && !originalOrder.IsSameOrder(order))
                throw new OrderValidationError($"Type of order #{order.Id} was changed.", OrderCmdResultCodes.DealerReject);

            UnregisterOrder(order);

            if (order.Type != OrderType.Position)
            {
                // increase reported action number
                order.ActionNo++;
            }

            // remove order
            _acc.Orders.Remove(order.Id);

            // journal
            //LogTransactionDetails(() => $"Confirmed Order Cancellation #{orderId}, reason={trReason}", JournalEntrySeverities.Info, order.Clone());

            //Order orderCopy = FinalizeOrderOperation(order, null, order.SymbolRef, acc,
            //    trReason == TradeTransReasons.Expired ? OrderStatuses.Expired : OrderStatuses.Canceled,
            //    trReason == TradeTransReasons.Expired ? OrderExecutionEvents.Expired : OrderExecutionEvents.Canceled,
            //    clientRequestId);

            if (trReason == TradeTransReasons.StopOut && _acc.IsMarginType)
            {
                //MarginAccountModel mAcc = (MarginAccountModel)acc;
                //order.UserComment = "Stopout: MarginLevel = " + decimal.Round(mAcc.MarginLevel, 2) + ", Margin = " + decimal.Round(mAcc.Margin, mAcc.RoundingDigits) + ", Equity = " + decimal.Round(mAcc.Equity, mAcc.RoundingDigits);
            }

            // fire API event
            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderCanceled(new OrderCanceledEventArgsImpl(order)));

            if (trReason == TradeTransReasons.Expired)
                RecalculateAccount();

            // update trade history
            //TradeReportModel report = TradeReportModel.Create(acc, trReason == TradeTransReasons.Expired ? TradeTransTypes.OrderExpired : TradeTransTypes.OrderCanceled, trReason);
            //report.FillGenericOrderData(order);
            //report.FillAccountSpecificFields();
            //report.OrderRemainingAmount = order.RemainingAmount >= 0 ? order.RemainingAmount : default(decimal?);
            //report.OrderMaxVisibleAmount = order.MaxVisibleAmount;

            //if (acc is MarginAccountModel)
            //{
            //    MarginAccountModel mAcc = (MarginAccountModel)acc;
            //    report.FillAccountBalanceConversionRates(mAcc.BalanceCurrency, mAcc.Balance);
            //}

            return order;
        }

        private OrderAccessor CreatePositionFromOrder(TradeTransReasons trReason, OrderAccessor parentOrder,
           decimal openPrice, decimal posAmount, bool transformOrder)
        {
            return CreatePosition(trReason, parentOrder, parentOrder.Side, parentOrder.SymbolInfo, parentOrder.Calculator, openPrice, posAmount, transformOrder);
        }

        private OrderAccessor CreatePosition(TradeTransReasons trReason, OrderAccessor parentOrder, OrderSide side, SymbolAccessor smb, OrderCalculator fCalc, decimal openPrice, decimal posAmount, bool transformOrder)
        {
            OrderAccessor position;
            //TradeChargesInfo charges = new TradeChargesInfo();
            var currentRate = _calcFixture.GetCurrentRateOrNull(smb.Name);

            //if (parentOrder != null)
            //{
            //    remove pending order from account
            //    _acc.Orders.Remove(parentOrder.Id);

            //    unregister transformed position
            //    UnregisterOrder(parentOrder);
            //}

            if (transformOrder)
            {
                position = parentOrder;
                //position.PositionCreated = OperationContext.ExecutionTime;
            }
            else
            {
                position = new OrderAccessor(new OrderEntity(NewOrderId()), smb);
                //position.ClientOrderId = Guid.NewGuid().ToString("D");
                position.Entity.Side = side;
                position.Entity.Created = _scheduler.VirtualTimePoint;
                //position.PositionCreated = OperationContext.ExecutionTime;
                //position.SymbolPrecision = smb.Digits;

                if (parentOrder != null)
                {
                    position.Entity.MaxVisibleVolume = parentOrder.MaxVisibleVolume;
                    position.Entity.StopLoss = parentOrder.StopLoss;
                    position.Entity.TakeProfit = parentOrder.TakeProfit;
                    position.Entity.Comment = parentOrder.Comment;
                    position.Entity.UserTag = parentOrder.Entity.UserTag;
                    //position.ManagerComment = parentOrder.ManagerComment;
                    //position.ManagerTag = parentOrder.ManagerTag;
                    //position.Magic = parentOrder.Magic;
                    //position.TransferringCoefficient = parentOrder.TransferringCoefficient;
                    //position.IsReducedOpenCommission = parentOrder.IsReducedOpenCommission;
                    //position.ReqOpenPrice = parentOrder.ReqOpenPrice;
                    //position.ReqOpenAmount = parentOrder.ReqOpenAmount;
                    //position.Options = parentOrder.Options;
                    //position.ClientApp = parentOrder.ClientApp;

                    // add parent pending order back to account
                    //acc.AddOrderNotify(parentOrder);
                    // register only pending orders and positions
                    if (parentOrder.IsPending || (parentOrder.Type == OrderType.Position))
                        RegisterOrder(parentOrder, currentRate);
                }
            }

            //position.ParentOrderId = (parentOrder != null) ? parentOrder.OrderId : position.OrderId;
            //position.InitialType = (parentOrder != null) ? parentOrder.InitialType : OrderTypes.Market;

            position.Entity.Type = OrderType.Position;
            //position.Status = OrderStatuses.Calculated;
            position.Entity.Price = (double)openPrice; // position open price

            // stop price for stops
            //if ((parentOrder != null) && (parentOrder.InitialType == OrderTypes.StopLimit || parentOrder.InitialType == OrderTypes.Stop))
            //    position.Entity.StopPrice = parentOrder.StopPrice;
            //else
            //    position.Entity.StopPrice = null;

            position.Amount = posAmount;
            position.RemainingAmount = posAmount;
            position.Entity.Modified = _scheduler.VirtualTimePoint;
            //position.Expired = null;

            if (_acc.AccountingType == AccountingTypes.Gross && position.Entity.TakeProfit.HasValue)
            {
                decimal? currentRateBid = currentRate?.NullableBid();
                decimal? currentRateAsk = currentRate?.NullableAsk();
                //position.IsReducedCloseCommission = ((position.Side == OrderSide.Buy && currentRateBid.HasValue &&
                //                                      position.TakeProfit > currentRateBid) ||
                //                                     (position.Side == OrderSide.Sell && currentRateAsk.HasValue &&
                //                                      position.TakeProfit < currentRateAsk));
            }

            // increase reported action number
            position.ActionNo++;

            // add position to account
            if (!transformOrder)
                _acc.Orders.Add(position);

            // calculate margin & profit
            fCalc.UpdateMargin(position, _acc);
            fCalc.UpdateProfit(position);

            // Update order initial margin rate.
            position.OpenConversionRate = position.MarginRateCurrent;

            // calculate commission
            //CommissionStrategy.OnPositionOpened(position, charges, acc);

            //// log
            //if (transformOrder)
            //    Logger.Info(() => OperationContext.LogPrefix + "Replace order with position " + position);
            //else
            //    Logger.Info(() => OperationContext.LogPrefix + "Create new position: " + position);

            // register order
            RegisterOrder(position, (RateUpdate)fCalc.CurrentRate);

            position.FireChanged();

            // journal
            //LogTransactionDetails(() => "Created Position #" + position.OrderId + " price=" + position.Price + " amount=" + position.Amount
            //                            + " {" + FormatRate(fCalc.CurrentRate) + "}", JournalEntrySeverities.Info, position.Clone());

            //Order posCopy = position.Clone();
            //LogTransactionDetails(() => "Final position " + posCopy, JournalEntrySeverities.Info, position.Clone());

            return position;
        }

        //private void FinalizeOrderOperation(OrderAccessor order, OrderAccessor orderPrev, SymbolEntity symbol)
        //{
        //    order.Entity.Modified = _scheduler.VirtualTimePoint;

        //    Order orderClone = order.Clone();

        //    LogTransactionDetails(() => "Final order " + orderClone, JournalEntrySeverities.Info, orderClone);

        //     send notification
        //    SendExecutionReport(orderClone, symbol, operation, acc, clientRequestId, null, null, null, orderPrev);

        //    return orderClone;
        //}

        private void RegisterOrder(OrderAccessor order, RateUpdate currentRate)
        {
            ActivationRecord activationInfo = _activator.AddOrder(order, currentRate);
            // Check if order must be activated immediately
            if (activationInfo != null)
            {
                //var checkActivationTask = new CheckActivationTask(activationInfo.Account.Id, activationInfo.OrderId, currentRate.Symbol);
                //OperationContext.Current.Infrustrucure.EnqeueTask(checkActivationTask);
            }

            //ExpirationManager.AddOrder(order);
        }

        private void ResetOrderActivation(OrderAccessor order)
        {
            _activator.ResetOrderActivation(order);
        }

        private void UnregisterOrder(OrderAccessor order)
        {
            _activator.RemoveOrder(order);
            //ExpirationManager.RemoveOrder(order);
        }

        private void RecalculateAccount()
        {
            if (_acc.IsMarginType)
            {
                if (_calcFixture.IsCalculated)
                {
                    if (_acc.Margin > 0 && _acc.MarginLevel < _stopOutLevel)
                        OnStopOut();
                }
            }
        }

        private void OnStopOut()
        {
            var msgBuilder = new StringBuilder();
            msgBuilder.Append("Stop out! margin level: ").AppendNumber(_acc.MarginLevel, _acc.BalanceCurrencyFormat);
            msgBuilder.Append(" equity: ").AppendNumber(_acc.Equity, _acc.BalanceCurrencyFormat);
            msgBuilder.Append(" margin: ").AppendNumber(_acc.Margin, _acc.BalanceCurrencyFormat);
            msgBuilder.Append(" last quote: ").Append(_lastRate);
            _collector.AddEvent(LogSeverities.Error, msgBuilder.ToString());

            _scheduler.SetFatalError(new StopOutException("Stop out!"));
        }

        internal void UpdateAssetsOnFill(OrderAccessor order, decimal fillPrice, decimal fillAmount)
        {
            var smb = order.SymbolInfo;
            var roundDigits = _context.Builder.Currencies.GetOrNull(smb.ProfitCurrency)?.Digits ?? 2;

            //var mrgAsset = _acc.Assets.GetOrCreateAsset(smb.MarginCurrency);
            //var prfAsset = _acc.Assets.GetOrCreateAsset(smb.ProfitCurrency);

            //var marginReport = CreateChangeReport(mrgAsset, 0);
            //var profitReport = CreateChangeReport(prfAsset, 0);

            decimal mChange = 0;
            decimal pChange = 0;

            if (order.Side == OrderSide.Buy)
            {
                mChange = fillAmount;
                pChange = -(fillAmount * fillPrice).CeilBy(roundDigits);
            }
            else if (order.Side == OrderSide.Sell)
            {
                mChange = -fillAmount;
                pChange = (fillAmount * fillPrice).FloorBy(roundDigits);
            }

            // Update asset amount
            _acc.IncreaseAsset(smb.MarginCurrency, mChange);
            _acc.IncreaseAsset(smb.ProfitCurrency, pChange);

            // Update asset report amount
            //marginReport.Balance += marginReport.ChangeAmount;
            //profitReport.Balance += profitReport.ChangeAmount;

            // Update locked amount
            //if (order.Side == OrderSide.Buy)
            //    profitReport.LockedAmount -= order.Margin ?? 0;
            //else
            //    marginReport.LockedAmount -= order.Margin ?? 0;

            //var moveReport = new List<AssetChangeReport>
            //{
            //    marginReport,
            //    profitReport
            //};

            //execReport.AssetMovement = moveReport;
        }

        internal NetPositionCloseInfo OpenNetPositionFromOrder(OrderAccessor fromOrder, decimal fillAmount, decimal fillPrice)
        {
            var smb = fromOrder.SymbolInfo;
            var position = _acc.NetPositions.GetOrCreatePosition(smb.Name);
            //tradeReport.FillPosData(position);
            position.Increase(fillAmount, fillPrice, fromOrder.Side);
            position.Modified = _scheduler.VirtualTimePoint;

            var charges = new TradeChargesInfo();

            // commission
            CommisionEmulator.OnNetPositionOpened(fromOrder, position, fillAmount, smb, charges, _calcFixture);

            //tradeReport.Commission = charges.Commission;
            //tradeReport.AgentCommission = charges.AgentCommission;
            //tradeReport.MinCommissionCurrency = charges.MinCommissionCurrency;
            //tradeReport.MinCommissionConversionRate = charges.MinCommissionConversionRate;

            decimal balanceMovement = charges.Total;
            //tradeReport.BalanceMovement = balanceMovement;

            if (fromOrder.Type == OrderType.Market || fromOrder.RemainingAmount == 0)
                _acc.Orders.Remove(fromOrder.Id);

            // journal;
            //LogTransactionDetails(() => "Position opened: symbol=" + smb.Name + " price=" + fillPrice + " amount=" + fillAmount + " commision=" + charges.Commission + " reason=" + tradeReport.TrReason,
            //JournalEntrySeverities.Info, TransactDetails.Create(position.Id, position.Symbol));

            var settlementInfo = DoNetSettlement(position, fromOrder.Side);
            settlementInfo.Charges = charges;
            //tradeReport.FillAccountSpecificFields();
            //tradeReport.FillPosData(position);
            //tradeReport.OpenConversionRate = fromOrder.MarginRateCurrent;

            //LogTransactionDetails(() => "Final position: " + position.GetBriefInfo(), JournalEntrySeverities.Info, TransactDetails.Create(position.Id, position.Symbol));

            balanceMovement += settlementInfo.BalanceMovement;
            //execReport.Profit = new ExecProfitInfo(balanceMovement, acc.Balance, acc.BalanceCurrency);
            //SendExecutionReport(execReport, acc);
            //SendPositionReport(acc, CreatePositionReport(acc, PositionReportType.CreatePosition, position.SymbolRef, balanceMovement));

            _acc.Balance += balanceMovement;

            _collector.OnCommisionCharged(charges.Commission);

            return settlementInfo;
        }

        public NetPositionCloseInfo DoNetSettlement(PositionAccessor position, OrderSide fillSide = OrderSide.Buy)
        {
            decimal oneSideClosingAmount = Math.Min(position.Short.Amount, position.Long.Amount);
            decimal oneSideClosableAmount = Math.Max(position.Short.Amount, position.Long.Amount);
            decimal balanceMovement = 0;
            //NetAccountModel acc = position.Acc;

            if (oneSideClosingAmount > 0)
            {
                decimal k = oneSideClosingAmount / oneSideClosableAmount;
                decimal closeSwap = RoundMoney(k * position.Swap, _calcFixture.RoundingDigits);
                decimal openPrice = fillSide == OrderSide.Buy ? position.Long.Price : position.Short.Price;
                decimal closePrice = fillSide == OrderSide.Buy ? position.Short.Price : position.Long.Price;
                decimal profitRate;
                decimal profit = RoundMoney(position.Calculator.CalculateProfitFixedPrice(openPrice, oneSideClosingAmount, closePrice, TickTraderToAlgo.Convert(fillSide), out profitRate), _calcFixture.RoundingDigits);

                var copy = position.Clone();

                position.DecreaseBothSides(oneSideClosingAmount);

                position.Swap -= closeSwap;
                balanceMovement = profit + closeSwap;

                var isClosed = position.IsEmpty;

                if (position.IsEmpty)
                    position.Remove();

                //report.BalanceMovement += balanceMovement;
                //report.PosClosed = OperationContext.ExecutionTime;
                //report.PosOpenPrice = openPrice;
                //report.PosClosePrice = closePrice;
                //report.PosLastAmount = oneSideClosingAmount;
                //report.Swap = report.Swap ?? 0 + closeSwap;
                //report.ProfitLoss = report.ProfitLoss ?? 0 + profit;
                //report.CloseConversionRate = profitRate;

                //LogTransactionDetails(() => "Position closed: symbol=" + position.Symbol + " amount=" + oneSideClosingAmount + " open=" + openPrice + " close=" + closePrice
                //                            + " profit=" + profit + " swap=" + closeSwap,
                //    JournalEntrySeverities.Info, TransactDetails.Create(position.Id, position.Symbol));

                _collector.OnPositionClosed(_scheduler.VirtualTimePoint, profit, 0, closeSwap);
                _scheduler.EnqueueEvent(b => b.Account.NetPositions.FirePositionUpdated(new PositionModifiedEventArgsImpl(copy, position, isClosed)));
            }

            var info = new NetPositionCloseInfo();
            info.CloseAmount = oneSideClosingAmount;
            info.ClosePrice = position.Long.Price;
            info.BalanceMovement = balanceMovement;
            info.SymbolInfo = (SymbolAccessor)position.Calculator.SymbolInfo;

            return info;
        }

        internal void CheckActivation(RateUpdate quote)
        {
            decimal lockedActivateMargin = 0;

            IEnumerable<ActivationRecord> records = _activator.CheckPendingOrders(quote);
            foreach (ActivationRecord record in records)
            {
                using (JournalScope()) ActivateOrder(record, ref lockedActivateMargin);
            }
        }

        // can lead to: 1) new gross position 2) close gross position 3) close net position 4) asset movement
        private void ActivateOrder(ActivationRecord record, ref decimal lockedActivateMargin)
        {
            // Perform automatic order activation.
            //AccountModel account = record.Account;

            // Skip orders activation for blocked accounts
            //if (account.IsBlocked)
            //    return;

            //GroupSecurityCfg securityCfg = account.GetSecurityCfg(smbInfo);
            if ((record.ActivationType == ActivationTypes.Pending) && (record.Order.Type == OrderType.Stop))
            {
                //bool needCancelation = false;

                // Check margin of the activated pending order
                try
                {
                    //if (record.Order.Type != OrderType.StopLimit)
                    //    account.ValidateOrderActivation(record.Order, record.ActivationPrice, record.Order.RemainingAmount, ref lockedActivateMargin);
                }
                catch (ServerFaultException<NotEnoughMoneyFault>)
                {
                    //needCancelation = true;
                }
                catch (ServerFaultException<OffQuotesFault>)
                {
                    //needCancelation = true;
                }

                // Insufficient margin. Cancel pending order
                //if (needCancelation)
                //{
                //    var order = record.Order;
                //    LocalAccountTransaction.Start(account.Id, "Pending order " + record.OrderId + " was canceled during activation because of insufficient margin to activate!", JournalTransactionTypes.CancelOrder,
                //        tr =>
                //        {
                //            tr.TradeInfrastructure.LogTransactionDetails(() => $"Account state: {account}", JournalEntrySeverities.Info, tr.Token, TransactDetails.Create(order.OrderId, order.Symbol));

                //            CancelOrderRequest request = new CancelOrderRequest();
                //            request.Level = TradeMessageLevels.Manager;
                //            request.AccountId = order.Account.Id;
                //            request.OrderId = order.OrderId;
                //            request.StopoutFlag = false;
                //            request.ExpirationFlag = false;
                //            request.ManagerOptions = TradeRequestOptions.DealerRequest;

                //            return CancelOrderAsync(request, order.Account, TradeTransReasons.DealerDecision);
                //        });

                //    Logger.Info(() => OperationContext.LogPrefix + "Pending order " + record.OrderId + " was canceled during activation because of insufficient margin to activate!");

                //    return;
                //}
            }

            var fillInfo = new FillInfo();

            if (record.ActivationType == ActivationTypes.Pending)
            {
                if (record.Order.Type == OrderType.StopLimit)
                {
                    ActivateStopLimitOrder(record.Order, TradeTransReasons.PndOrdAct);
                    _opSummary.AddStopLimitActivationAction(record.Order, record.ActivationPrice);
                }
                else
                {
                    fillInfo = FillOrder(record.Order, record.ActivationPrice, record.Order.RemainingAmount, TradeTransReasons.PndOrdAct);
                    _opSummary.AddFillAction(record.Order, fillInfo);
                    if (fillInfo.WasNetPositionClosed)
                        _opSummary.AddNetCloseAction(fillInfo.NetClose, record.Order.SymbolInfo);
                }
            }
            else if ((_acc.AccountingType == AccountingTypes.Gross) && (record.ActivationType == ActivationTypes.StopLoss || record.ActivationType == ActivationTypes.TakeProfit))
            {
                TradeTransReasons trReason = TradeTransReasons.DealerDecision;
                if (record.ActivationType == ActivationTypes.StopLoss)
                    trReason = TradeTransReasons.StopLossAct;
                else if (record.ActivationType == ActivationTypes.TakeProfit)
                    trReason = TradeTransReasons.TakeProfitAct;

                var smb = _context.Builder.Symbols.GetOrDefault(record.Order.Symbol);
                ClosePosition(record.Order, trReason, record.Order.RemainingAmount, record.Price, smb, 0, null);
            }
        }

        private void ActivateStopLimitOrder(OrderAccessor order, TradeTransReasons reason)
        {
            UnregisterOrder(order);

            // Increase reported action number
            order.ActionNo++;

            // remove order
            _acc.Orders.Remove(order.Id);

            // journal
            //LogTransactionDetails(() => $"Activate StopLimit Order #{order.OrderId}, reason={reason}", JournalEntrySeverities.Info, order.Clone());

            //Order orderCopy = FinalizeOrderOperation(order, null, order.SymbolRef, acc, OrderStatuses.Activated, OrderExecutionEvents.Activated, null);

            // Update trade history
            //TradeReportModel report = TradeReportModel.Create(acc, TradeTransTypes.OrderActivated, TradeTransReasons.DealerDecision);
            //report.FillGenericOrderData(order);
            //report.FillAccountSpecificFields();
            //report.OrderRemainingAmount = order.RemainingAmount >= 0 ? order.RemainingAmount : default(decimal?);
            //report.OrderMaxVisibleAmount = order.MaxVisibleAmount;

            //if (acc is MarginAccountModel)
            //{
            //    MarginAccountModel mAcc = (MarginAccountModel)acc;
            //    report.FillAccountBalanceConversionRates(mAcc.BalanceCurrency, mAcc.Balance);
            //}

            OpenOrder(order.Calculator, OrderType.Limit, order.Side, order.RemainingVolume, null, order.Price,
                order.StopPrice, order.StopLoss, order.TakeProfit, order.Comment, order.Entity.Options, order.Entity.UserTag, order.Expiration, OpenOrderOptions.SkipDealing);
        }

        private void ClosePosition(OrderAccessor position, TradeTransReasons trReason, 
            decimal? amount, decimal? price, SymbolAccessor smb, ClosePositionOptions options, string posById = null)
        {
            OrderCalculator fCalc = position.Calculator;

            // normalize amount
            decimal actualCloseAmount = NormalizeAmount(amount, position.RemainingAmount);

            bool partialClose = actualCloseAmount < position.RemainingAmount;
            bool nullify = (options & ClosePositionOptions.Nullify) != 0;
            bool reopenRemaining = (options & ClosePositionOptions.ReopenRemaining) != 0;
            bool dropCommission = (options & ClosePositionOptions.DropCommision) != 0;

            // profit & closePrice
            decimal closePrice;
            decimal profit;

            if (nullify)
            {
                closePrice = price.Value;
                profit = 0;
            }
            else if ((price != null) && (price.Value > 0))
            {
                closePrice = price.Value;
                profit = RoundMoney(fCalc.CalculateProfitFixedPrice(position, actualCloseAmount, closePrice), _calcFixture.RoundingDigits);
            }
            else
            {
                profit = RoundMoney(fCalc.CalculateProfit(position, actualCloseAmount, out closePrice), _calcFixture.RoundingDigits);
            }

            //position.CloseConversionRate = profit >= 0 ? fCalc.PositiveProfitConversionRate.Value : fCalc.NegativeProfitConversionRate.Value;

            position.ClosePrice = closePrice;

            //if (managerComment != null)
            //    position.ManagerComment = managerComment;

            // Calculate commission & swap.

            //position.IsReducedCloseCommission = position.IsReducedCloseCommission && trReason == TradeTransReasons.TakeProfitAct;

            var charges = new TradeChargesInfo();

            if (partialClose)
            {
                decimal newRemainingAmount = position.RemainingAmount - actualCloseAmount;
                decimal k = newRemainingAmount / position.RemainingAmount;

                position.RemainingAmount = newRemainingAmount;
                //position.Status = OrderStatuses.Calculated;

                if (position.Swap != null)
                {
                    decimal partialSwap = CommisionEmulator.GetPartialSwap(position.Swap.Value, k, _calcFixture.RoundingDigits);

                    charges.Swap = position.Swap.Value - partialSwap;
                    position.Entity.Swap = (double)partialSwap;
                }
            }
            else
            {
                charges.Swap = position.Swap ?? 0;
                position.RemainingAmount = 0;
            }

            //if (trReason == TradeTransReasons.Rollover)
            //    CommissionStrategy.OnRollover(position, actualCloseAmount, charges, acc);
            //else
            //    CommissionStrategy.OnPositionClosed(position, actualCloseAmount, charges, acc);)

            CommisionEmulator.OnGrossPositionClosed(position, actualCloseAmount, smb, charges, _calcFixture);

            if (dropCommission)
            {
                charges.Commission = 0;
                //charges.AgentCommission = 0;
                //charges.MinCommissionCurrency = null;
                //charges.MinCommissionConversionRate = null;
                position.Entity.Commission = 0;
                //position.AgentCommision = null;
            }

            bool remove = (!partialClose || reopenRemaining);

            // Remove remaining order / reset order activation.
            if (remove)
            {
                //position.Status = OrderStatuses.Filled;
                _acc.Orders.Remove(position.Id);
                UnregisterOrder(position);
            }
            else
                ResetOrderActivation(position);

            // Reopen position with remaining amount.
            if (partialClose && reopenRemaining)
                CreatePosition(trReason, position, position.Side, smb, fCalc, (decimal)position.Price, position.RemainingAmount, false);

            // change balance
            decimal totalProfit = charges.Total + profit;
            _acc.Balance += totalProfit;

            // Update modify timestamp.
            position.Entity.Modified = _scheduler.VirtualTimePoint;

            decimal historyAmount = nullify ? 0 : actualCloseAmount;

            // Update comment for trade history entry.
            //switch (trReason)
            //{
            //    case TradeTransReasons.StopOut:
            //        position.UserComment = "[Stopout: MarginLevel = " + decimal.Round(acc.MarginLevel, 2) + ", Margin = " + decimal.Round(acc.Margin, acc.RoundingDigits) + ", Equity = " + decimal.Round(acc.Equity, acc.RoundingDigits) + "] " + position.UserComment;
            //        break;
            //    case TradeTransReasons.TakeProfitAct:
            //        reqAmount = actualCloseAmount;
            //        reqPrice = position.TakeProfit;
            //        position.UserComment = "[TP] " + position.UserComment;
            //        break;
            //    case TradeTransReasons.StopLossAct:
            //        reqAmount = actualCloseAmount;
            //        reqPrice = position.StopLoss;
            //        position.UserComment = "[SL] " + position.UserComment;
            //        break;
            //}

            // update trade history
            //TradeReportModel.Create(acc, TradeTransTypes.PositionClosed, trReason)
            //    .FillGenericOrderData(position)
            //    .FillClosePosData(position, actualCloseAmount, closePrice, reqAmount, reqPrice, posById)
            //    .FillCharges(charges, profit, totalProfit)
            //    .FillProfitConversionRates(acc.BalanceCurrency, profit)
            //    .FillAccountBalanceConversionRates(acc.BalanceCurrency, acc.Balance)
            //    .FillAccountSpecificFields();

            var orderCopy = position.Clone();

            //// journal
            //string jPrefix = remove ? "Closed " : "Partially Closed";
            //LogTransactionDetails(() => jPrefix + " #" + position.OrderId + ", symbol=" + smb.Name + " price=" + closePrice + " amount=" + historyAmount
            //                            + " remaining=" + position.RemainingAmount + " profit=" + profit + " charges=" + charges.Total + " totalProfit=" + totalProfit + " reason=" + trReason,
            //    JournalEntrySeverities.Info, orderCopy);
            //LogTransactionDetails(() => "Final position " + orderCopy, JournalEntrySeverities.Info, orderCopy);

            if (!remove)
                position.FireChanged();

            // Recalculate account if it is not disabled.
            if ((options & ClosePositionOptions.NoRecalculate) == 0)
                RecalculateAccount();

            //ExecProfitInfo profitInfo = new ExecProfitInfo(totalProfit, acc.Balance, acc.BalanceCurrency);

            //if (sendNotifications)
            //{
            //    // send an execution report of filled part
            //    orderCopy.Status = remove ? OrderStatuses.Filled : OrderStatuses.PartiallyFilled;
            //    orderCopy.Commission = charges.Commission;
            //    orderCopy.AgentCommision = charges.AgentCommission;
            //    SendExecutionReport(orderCopy, smb, OrderExecutionEvents.Filled, acc, clientRequestId, new ExecFillInfo(actualCloseAmount, closePrice), profitInfo);

            //    if (!remove)
            //    {
            //        // send an execution report of remaining position
            //        Order remOrder = position.Clone();
            //        remOrder.Status = OrderStatuses.Calculated;
            //        SendExecutionReport(remOrder, smb, OrderExecutionEvents.Filled, acc, clientRequestId);
            //    }
            //}

            // increase reported action number
            position.ActionNo++;

            _opSummary.AddCloseAction(position, profit, closePrice, charges);
            _collector.OnPositionClosed(_scheduler.VirtualTimePoint, profit, charges.Commission, charges.Swap);

            //return profitInfo;
        }

        public void ConfirmPositionCloseBy(OrderAccessor position1, OrderAccessor position2, TradeTransReasons trReason, bool usePartialClosing)
        {
            var smb = position1.SymbolInfo;
            OrderCalculator fCalc = position1.Calculator;

            decimal closeAmount = Math.Min(position1.RemainingAmount, position2.RemainingAmount);

            if (position1.RemainingAmount < position2.RemainingAmount)
                Ref.Swap(ref position1, ref position2);

            // journal
            //LogTransactionDetails(() => "Confirmed Close #" + position1.OrderId + " By #" + position2.OrderId + " amount=" + closeAmount + ", reason=" + trReason, JournalEntrySeverities.Info);

            ClosePositionOptions pos1options = 0;
            ClosePositionOptions pos2options = 0;

            if (!usePartialClosing)
                pos1options |= ClosePositionOptions.ReopenRemaining;

            //if (grSecurity.CloseByMod == CloseByModifications.AllByCurrentPrice || acc.AccountingType == AccountingTypes.Net)
            //{
            //    decimal closeByPrice = fCalc.CurrentRate.Ask;
            //    ClosePosition(isExecutedAsClient, position1, acc, trReason, null, null, closeAmount, closeByPrice, smb, pos1options, clientRequestId, managerComment, true, position2.OrderId);
            //    ClosePosition(isExecutedAsClient, position2, acc, trReason, null, null, closeAmount, closeByPrice, smb, pos2options, clientRequestId, managerComment, true, position1.OrderId);
            //}
            //else
            //{
                pos2options |= ClosePositionOptions.Nullify;
                pos2options |= ClosePositionOptions.DropCommision;
                ClosePosition(position1, trReason, closeAmount, (decimal)position2.Price, smb, pos1options, position2.Id);
                ClosePosition(position2, trReason, closeAmount, (decimal)position2.Price, smb, pos2options, position1.Id);
            //}
        }

        private void CloseAllPositions(TradeTransReasons reason)
        {
            var toClose = _acc.Orders.Where(o => o.Type == OrderType.Position).ToList();

            if (toClose.Count > 0)
            {
                _collector.LogTrade($"Closing {toClose.Count} positions remaining after startegy stopped.");

                foreach (var order in _acc.Orders)
                {
                    if (order.Type == OrderType.Position)
                    {
                        using (JournalScope())
                            ClosePosition(order, reason, null, null, order.SymbolInfo, ClosePositionOptions.None);
                    }
                }
            }

            var netPosToClose = _acc.NetPositions.ToList();

            if (netPosToClose.Count > 0)
            {
                _collector.LogTrade($"Closing {netPosToClose.Count} positions remaining after startegy stopped.");

                foreach (var pos in netPosToClose)
                {
                    using (JournalScope())
                    {
                        OpenOrder(pos.Calculator, OrderType.Market, pos.Side.Revert(), pos.VolumeUnits, null, null, null, null, null, "",
                            OrderExecOptions.None, null, null, OpenOrderOptions.SkipDealing);
                    }
                }
            }
        }

        #endregion

        #region Price Logic

        private decimal? GetCurrentOpenPrice(OrderAccessor order, RateUpdate currentRate = null)
        {
            if (currentRate == null)
                currentRate = _calcFixture.GetCurrentRateOrNull(order.Symbol);

            return GetCurrentOpenPrice(order.Side, currentRate);
        }

        private decimal? GetCurrentOpenPrice(OrderSide side, string smb)
        {
            RateUpdate currentRate = _calcFixture.GetCurrentRateOrNull(smb);
            return GetCurrentOpenPrice(side, currentRate);
        }

        private decimal? GetCurrentOpenPrice(OrderSide side, RateUpdate currentRate)
        {
            return GetOpenOrderPrice(currentRate, side);
        }

        private decimal? GetCurrentClosePrice(IOrder order, RateUpdate currentRate = null)
        {
            if (currentRate == null)
                currentRate = _calcFixture.GetCurrentRateOrNull(order.Symbol);

            return GetPositionClosePrice(currentRate, order.Side);
        }

        private static decimal? GetPositionClosePrice(RateUpdate tick, OrderSides positionSide)
        {
            if (tick == null)
                return null;

            if (positionSide == OrderSides.Buy)
                return tick.NullableBid();
            else if (positionSide == OrderSides.Sell)
                return tick.NullableAsk();

            throw new Exception("Unknown order side: " + positionSide);
        }

        private static decimal? GetOpenOrderPrice(RateUpdate tick, OrderSide orderSide)
        {
            try
            {
                if (tick == null)
                    return null;

                if (orderSide == OrderSide.Buy)
                    return tick.NullableAsk();
                if (orderSide == OrderSide.Sell)
                    return tick.NullableBid();
            }
            catch (Exception)
            {
                throw new OrderValidationError("Can not get open price for " + orderSide + " " + tick.Symbol + " order!", OrderCmdResultCodes.OffQuotes);
            }

            throw new Exception("Unknown order side: " + orderSide);
        }

        #endregion Price Logic

        #region Amount logic

        private static decimal NormalizeAmount(decimal? requestedAmount, decimal remainingAmount)
        {
            if (requestedAmount == null || requestedAmount.Value > remainingAmount)
                return remainingAmount;

            return requestedAmount.Value;
        }

        #endregion

        #region Commission & Swap

        #endregion 

        #region Rounding

        public decimal RoundMoney(decimal rawValue, int? roundDigits)
        {
            if (roundDigits == null)
                return rawValue;
            else
                return rawValue.FloorBy(roundDigits.Value);
        }

        public decimal? RoundMoney(decimal? rawValue, int? roundDigits)
        {
            if (rawValue == null || roundDigits == null)
                return rawValue;
            else
                return rawValue.Value.FloorBy(roundDigits.Value);
        }

        #endregion

        #region Validation

        //private void ValidatePrice(ISymbolInfo symbol, OrderType type, double? limitPrice, double? stopPrice)
        //{
        //    if (((type == OrderType.Market) || (type == OrderType.Limit) || (type == OrderType.StopLimit)) && limitPrice != null)
        //        ValidatePrice((decimal)limitPrice, symbol);
        //    else if (((type == OrderType.Stop) || (type == OrderType.StopLimit)) && stopPrice != null)
        //        ValidatePrice((decimal)stopPrice, symbol);
        //}

        private void ValidateLimitPrice(double? price, ISymbolInfo smbInfo)
        {
            if (price == null || price <= 0.0)
                throw new OrderValidationError("Price not specified.", OrderCmdResultCodes.IncorrectPrice);

            ValidatePrice((decimal)price, smbInfo);
        }

        private void ValidateStopPrice(double? stopPrice, ISymbolInfo smbInfo)
        {
            if (stopPrice == null || stopPrice <= 0.0)
                throw new OrderValidationError("Stop price not specified.", OrderCmdResultCodes.IncorrectStopPrice);

            ValidatePrice((decimal)stopPrice, smbInfo);
        }

        private void ValidatePrice(decimal price, ISymbolInfo smbInfo)
        {
            if (price.IsPrecisionGreater(smbInfo.Precision))
                throw new OrderValidationError("Price precision is more than symbol digits.", OrderCmdResultCodes.IncorrectPrice);
        }

        private void ValidateOrderTypeForAccount(OrderType orderType, ISymbolInfo symbolInfo)
        {
            var currentQuote = _calcFixture.GetCurrentRateOrNull(symbolInfo.Symbol);
            if (currentQuote == null)
            {
                if ((_acc.AccountingType != AccountingTypes.Cash) || (orderType == OrderType.Market))
                    throw new OrderValidationError("No quote for symbol " + symbolInfo.Symbol, OrderCmdResultCodes.OffQuotes);
            }

            //Request.InitialType = Request.Type;

            //if ((Request.Type == OrderType.Market) || (Request.Price == null) || (Request.Price == 0.0M)))
            //    Request.Price = TradeLogic.GetOpenOrderPrice(currentQuote, Request.Side);

            //if (Request.Type == OrderType.Market)
            //{
            //    if ((_acc.AccountingType == AccountingTypes.Gross) || (_acc.AccountingType == AccountingTypes.Net))
            //    {
            //        if (Request.IsClientRequest || (Request.Price == null) || (Request.Price == 0.0M))
            //            Request.Price = TradeLogic.GetOpenOrderPrice(currentQuote, Request.Side);
            //    }
            //    else if (_acc.AccountingType == AccountingTypes.Cash)
            //    {
            //        // Cash accounts: Emulate market orders with Limit+IOC+Slippage
            //        Request.Type = OrderTypes.Limit;
            //        Request.SetOption(OrderExecutionOptions.ImmediateOrCancel);
            //        Request.SetOption(OrderExecutionOptions.MarketWithSlippage);
            //        if (Request.IsClientRequest || (Request.Price == null) || (Request.Price == 0.0M))
            //            Request.Price = TradeLogic.GetOpenOrderPrice(currentQuote, Request.Side);
            //        if (!Request.Price.HasValue)
            //            throw new ServerFaultException<OffQuotesFault>("No quote for symbol " + symbolInfo.Name);
            //    }
            //}
            //else if (Request.Type == OrderTypes.Limit)
            //{
            //    // Set IOC flag for market with slippage in any case
            //    if (Request.IsOptionSet(OrderExecutionOptions.MarketWithSlippage))
            //        Request.SetOption(OrderExecutionOptions.ImmediateOrCancel);
            //}
            //else if (Request.Type == OrderTypes.Stop)
            //{
            //    if (_acc.AccountingType == AccountingTypes.Cash)
            //    {
            //        // Cash accounts: Emulate stop orders with StopLimit+IOC+Slippage
            //        Request.Type = OrderTypes.StopLimit;
            //        Request.SetOption(OrderExecutionOptions.ImmediateOrCancel);
            //        if (Request.StopPrice.HasValue)
            //            Request.Price = Request.StopPrice;
            //    }
            //}
        }

        private void ValidateTypeAndPrice(OrderType orderType, double? price, double? stopPrice, double? sl, double? tp, double? maxVisibleVolume, OrderExecOptions options, ISymbolInfo symbol)
        {
            if ((orderType != OrderType.Limit) && (orderType != OrderType.Market) && (orderType != OrderType.Stop) && (orderType != OrderType.StopLimit))
                throw new OrderValidationError("Invalid order type.", OrderCmdResultCodes.Unsupported);

            if ((_acc.AccountingType == AccountingTypes.Cash) &&
                ((orderType == OrderType.Limit) || (orderType == OrderType.Stop) || (orderType == OrderType.StopLimit)) &&
                (sl.HasValue || tp.HasValue))
                throw new OrderValidationError("SL/TP is not supported by pending order for cash account!", OrderCmdResultCodes.Unsupported);

            if (orderType == OrderType.Market)
            {
                //if (Request.IsOptionSet(OrderExecutionOptions.MarketWithSlippage))
                //    throw new OrderValidationError("'MarketWithSlippage' flag is not supported for market orders", FaultCodes.InvalidOption);
                if (options.HasFlag(OrderExecOptions.ImmediateOrCancel))
                    throw new OrderValidationError("'ImmediateOrCancel' flag is not supported for market orders", OrderCmdResultCodes.Unsupported);
                //if (Request.IsOptionSet(OrderExecutionOptions.HiddenIceberg))
                //    throw new OrderValidationError("'HiddenIceberg' flag is not supported for market orders", FaultCodes.InvalidOption);
                //if (Request.IsOptionSet(OrderExecutionOptions.FillOrKill))
                //    throw new OrderValidationError("'FillOrKill' flag is not supported for market orders", FaultCodes.InvalidOption);

                if (maxVisibleVolume.HasValue)
                    throw new OrderValidationError("Max visible amount is not valid for market orders", OrderCmdResultCodes.IncorrectMaxVisibleVolume);

                if (price != null)
                    ValidateLimitPrice(price.Value, symbol);
            }
            else if (orderType == OrderType.Limit)
            {
                if (maxVisibleVolume.HasValue && maxVisibleVolume.Value < 0)
                    throw new OrderValidationError("Max visible amount must be positive", OrderCmdResultCodes.IncorrectMaxVisibleVolume);

                if (price == null || price <= 0.0)
                    throw new OrderValidationError("Price not specified.", OrderCmdResultCodes.IncorrectPrice);

                //if (Request.MaxVisibleVolume.HasValue && Request.MaxVisibleVolume.Value >= 0)
                //    Request.SetOption(OrderExecutionOptions.HiddenIceberg);

                ValidateLimitPrice(price.Value, symbol);
            }
            else if (orderType == OrderType.Stop)
            {
                //if (Request.IsOptionSet(OrderExecOptions.MarketWithSlippage))
                //    throw new OrderValidationError("'MarketWithSlippage' flag is not supported for stop orders", FaultCodes.InvalidOption);
                if (options.HasFlag(OrderExecOptions.ImmediateOrCancel))
                    throw new OrderValidationError("'ImmediateOrCancel' flag is not supported for stop orders", OrderCmdResultCodes.Unsupported);
                //if (Request.IsOptionSet(OrderExecOptions.HiddenIceberg))
                //    throw new OrderValidationError("'HiddenIceberg' flag is not supported for stop orders", FaultCodes.InvalidOption);
                //if (Request.IsOptionSet(OrderExecOptions.FillOrKill))
                //    throw new OrderValidationError("'FillOrKill' flag is not supported for stop orders", FaultCodes.InvalidOption);

                if (maxVisibleVolume.HasValue)
                    throw new OrderValidationError("Max visible amount is not valid for stop orders", OrderCmdResultCodes.IncorrectMaxVisibleVolume);

                ValidateStopPrice(stopPrice, symbol);
            }
            else if (orderType == OrderType.StopLimit)
            {
                //if (Request.IsOptionSet(OrderExecOptions.MarketWithSlippage))
                //    throw new OrderValidationError("'MarketWithSlippage' flag is not supported for stop limit orders", FaultCodes.InvalidOption);
                if (maxVisibleVolume.HasValue && maxVisibleVolume.Value < 0)
                    throw new OrderValidationError("Max visible amount must be positive", OrderCmdResultCodes.IncorrectVolume);

                ValidateLimitPrice(price, symbol);
                ValidateStopPrice(stopPrice, symbol);

                //if (Request.MaxVisibleAmount.HasValue && Request.MaxVisibleAmount.Value >= 0)
                //    Request.SetOption(OrderExecutionOptions.HiddenIceberg);
            }
        }

        private void ValidateVolumeLots(double? volumeLots, Symbol smbMetadata)
        {
            if (!volumeLots.HasValue)
                return;

            if (volumeLots <= 0 || volumeLots < smbMetadata.MinTradeVolume || volumeLots > smbMetadata.MaxTradeVolume)
                throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        }

        private void EnsureOrderIsPosition(OrderAccessor order)
        {
            if (order.Type != OrderType.Position)
                throw new OrderValidationError("Position #" + order.OrderId + " was not found.", OrderCmdResultCodes.OrderNotFound);
        }

        #endregion

        #region Volume Logic

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

        #endregion

        #region Journal

        private TradeOperationSummary _opSummary = new TradeOperationSummary();

        private JournalToken JournalScope()
        {
            _opSummary.Clear();
            return new JournalToken(this);
        }

        private void PrintJournalTransaction()
        {
            if (!_opSummary.IsEmpty)
            {
                var record = _opSummary.GetJournalRecord();
                var severity = _opSummary.HasErrors ? LogSeverities.TradeFail : LogSeverities.Trade;

                _collector.AddEvent(severity, record);
            }
        }

        private struct JournalToken : IDisposable
        {
            private TradeEmulator _emulator;

            public JournalToken(TradeEmulator emulator)
            {
                _emulator = emulator;
            }

            public void Dispose()
            {
                _emulator.PrintJournalTransaction();
            }
        }

        #endregion

        //#region DealerContext

        //AccountDataProvider DealerContext.AccountData => _acc;

        //RateUpdate DealerContext.GetRate(string symbol)
        //{
        //    return _calcFixture.GetCurrentRateOrNull(symbol);
        //}

        //Task DealerContext.Delay(TimeSpan delay)
        //{
        //    return _scheduler.EmulateAsyncDelay(delay);
        //}

        //#endregion
    }

    [Flags]
    public enum ClosePositionOptions
    {
        None = 0,
        Nullify = 0x01,
        DropCommision = 0x02,
        ReopenRemaining = 0x04,
        NoRecalculate = 0x08,
        NoPositionReport = 0x10
    }

    [Flags]
    internal enum OpenOrderOptions
    {
        None = 0,
        SkipDealing = 0x01,
        Stopout = 0x02
    }
}
