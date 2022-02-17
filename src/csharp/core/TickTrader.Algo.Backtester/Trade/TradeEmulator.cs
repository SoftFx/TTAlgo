using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Calculator;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Calculator.TradeSpecificsCalculators;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Backtester
{
    internal class TradeEmulator : TradeCommands, IExecutorFixture
    {
        private ActivationEmulator _activator;
        private OrderExpirationEmulator _expirationManager = new OrderExpirationEmulator();
        private CalculatorFixture _calcFixture;
        private AccountAccessor _acc;
        private IFixtureContext _context;
        private DealerEmulator _dealer = new DefaultDealer();
        private InvokeEmulator _scheduler;
        private BacktesterCollector _collector;
        private IBacktesterSettings _settings;
        private int _leverage;
        private long _orderIdSeed;
        private double _stopOutLevel = 30;
        //private RateUpdate _lastRate;
        private TradeHistoryEmulator _history;
        private bool _sendReports;

        public TradeEmulator(IFixtureContext context, IBacktesterSettings settings, CalculatorFixture calc, InvokeEmulator scheduler, BacktesterCollector collector,
            TradeHistoryEmulator history, Metadata.Types.PluginType pluginType)
        {
            _context = context;
            _activator = new ActivationEmulator(_context.MarketData);
            _calcFixture = calc;
            _scheduler = scheduler;
            _collector = collector;
            _settings = settings;
            _leverage = settings.CommonSettings.Leverage;
            _history = history;

            _sendReports = settings.StreamExecReports;

            if (pluginType == Metadata.Types.PluginType.TradeBot)
            {
                if (_settings.CommonSettings.AccountType == AccountInfo.Types.Type.Net || _settings.CommonSettings.AccountType == AccountInfo.Types.Type.Gross)
                    _scheduler.Scheduler.AddDailyJob(b => Rollover(), 0, 0);
            }

            VirtualServerPing = settings.CommonSettings.ServerPing;
            _scheduler.RateUpdated += r =>
            {
                //_lastRate = r;
                CheckActivation(r);
                RecalculateAccount();
            };
        }

        public TimeSpan VirtualServerPing { get; set; }

        private DateTime ExecutionTime => _scheduler.UnsafeVirtualTimePoint;

        public void Start()
        {
            _context.Builder.SetCustomTradeAdapter(this);
            _context.Builder.TradeHistoryProvider = _history;

            _acc = _context.Builder.Account;

            _acc.Orders.Clear();
            _acc.NetPositions.Clear();
            _acc.Assets.Clear();

            _acc.Balance = 0;
            _acc.ResetCurrency();
            _acc.Leverage = 0;

            _acc.Type = _settings.CommonSettings.AccountType;

            if (_acc.IsMarginType)
            {
                _acc.Balance = _settings.CommonSettings.InitialBalance;
                _acc.Leverage = _settings.CommonSettings.Leverage;
                _acc.UpdateCurrency(_settings.CommonSettings.Currencies.GetOrDefault(_settings.CommonSettings.BalanceCurrency));
                _opSummary.AccountCurrencyFormat = _acc.BalanceCurrencyFormat;
            }
            else if (_acc.IsCashType)
            {
                //var currencies = _context.Builder.Currencies.ToDictionary(c => c.Name);

                foreach (var asset in _settings.CommonSettings.InitialAssets.Values)
                    _acc.Assets.Update(asset, out _);
            }
        }

        public void Stop()
        {
            CloseAllPositions(TradeReportInfo.Types.Reason.Rollover);
            CancelAllPendings(TradeReportInfo.Types.Reason.Rollover);
        }

        public void PreRestart()
        {
        }

        public void PostRestart()
        {
        }

        public void Dispose()
        {
        }

        #region TradeCommands

        public Task<OrderCmdResult> OpenOrder(bool isAsync, Api.OpenOrderRequest request)
        {
            return ExecTradeRequest(isAsync, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                var smbMetadata = _context.Builder.Symbols.GetOrNull(request.Symbol).Info;

                var roundedVolumeLots = RoundVolume(request.Volume, smbMetadata);

                try
                {
                    //using (calc.UsageScope())
                    //{
                    //maxVisibleVolumeLots = RoundVolume(maxVisibleVolumeLots, smbMetadata);
                    double volume = ToUnits(roundedVolumeLots, smbMetadata);
                    double? maxVisibleVolume = null; //ConvertNullableVolume(maxVisibleVolumeLots, smbMetadata);
                    var price = RoundPrice(request.Price, smbMetadata, request.Side.ToDomainEnum());
                    var stopPrice = RoundPrice(request.StopPrice, smbMetadata, request.Side.ToDomainEnum());
                    var sl = RoundPrice(request.StopLoss, smbMetadata, request.Side.ToDomainEnum());
                    var tp = RoundPrice(request.TakeProfit, smbMetadata, request.Side.ToDomainEnum());

                    // emulate server ping
                    await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                    using (JournalScope())
                    {
                        VerifyAmout(volume, smbMetadata);
                        ValidateOrderTypeForAccount(request.Type, smbMetadata);
                        ValidateTypeAndPrice(request.Type, price, stopPrice, sl, tp, maxVisibleVolume, request.Options, smbMetadata);

                        //Facade.ValidateExpirationTime(Request.Expiration, _acc);

                        var order = OpenOrder(smbMetadata, request.Type.ToDomainEnum(), request.Side.ToDomainEnum(), volume, maxVisibleVolume, price, stopPrice, sl, tp, request.Comment, request.Options.ToDomainEnum(), request.Tag, request.Expiration, OpenOrderOptions.None);

                        _collector.OnOrderOpened();

                        // set result
                        return new OrderResultEntity(OrderCmdResultCodes.Ok, order.Clone(), ExecutionTime);
                    }
                    //}
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
                    _opSummary.AddOpenFailAction(request.Type, request.Symbol, request.Side, roundedVolumeLots, error, _acc);

                return new OrderResultEntity(error, null, ExecutionTime);
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
                    var order = _acc.Orders.GetOrNull(orderId) ?? throw new OrderValidationError($"Order Not Found {orderId}", OrderCmdResultCodes.OrderNotFound);
                    //var symbol = node.GetSymbolEntity(order.Symbol);

                    //Facade.Infrustructure.LogTransactionDetails(() => "Processing cancel order request " + Request, JournalEntrySeverities.Info, Token, TransactDetails.Create(order.OrderId, symbol.Name));

                    //node.CheckTradeTime(Request, symbol);

                    TradeReportInfo.Types.Reason trReason = TradeReportInfo.Types.Reason.ClientRequest;
                    //if (Request.ExpirationFlag)
                    //    trReason = TradeReportInfo.Types.Reason.Expired;
                    //if (Request.StopoutFlag)
                    //    trReason = TradeReportInfo.Types.Reason.StopOut;

                    using (JournalScope())
                        CancelOrder(order, trReason);

                    // set result
                    return new OrderResultEntity(OrderCmdResultCodes.Ok, order, ExecutionTime);
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

                return new OrderResultEntity(error, null, ExecutionTime);
            });
        }

        public Task<OrderCmdResult> ModifyOrder(bool isAysnc, Api.ModifyOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                try
                {
                    var orderToModify = _acc.GetOrderOrThrow(request.OrderId);
                    var smbMetadata = orderToModify.SymbolInfo;

                    var orderVolume = orderToModify.Info.RemainingAmount;

                    var roundedNewVolumeLots = RoundVolume(request.Volume, smbMetadata);
                    var roundedMaxVisibleVolume = RoundVolume(request.MaxVisibleVolume, smbMetadata);

                    var newOrderVolume = ToUnits(roundedNewVolumeLots, smbMetadata);
                    var orderMaxVisibleVolume = ToUnits(roundedMaxVisibleVolume, smbMetadata);

                    var price = RoundPrice(request.Price, smbMetadata, orderToModify.Info.Side);
                    var stopPrice = RoundPrice(request.StopPrice, smbMetadata, orderToModify.Info.Side);
                    var sl = RoundPrice(request.StopLoss, smbMetadata, orderToModify.Info.Side);
                    var tp = RoundPrice(request.TakeProfit, smbMetadata, orderToModify.Info.Side);
                    var comment = string.IsNullOrEmpty(request.Comment) ? orderToModify.Info.Comment : request.Comment;

                    var coreRequest = new Domain.ModifyOrderRequest
                    {
                        OrderId = request.OrderId,
                        Symbol = orderToModify.Info.Symbol,
                        Type = orderToModify.Info.Type,
                        Side = orderToModify.Info.Side,
                        CurrentAmount = (double)orderVolume,
                        NewAmount = (double?)newOrderVolume,
                        Price = price,
                        StopPrice = stopPrice,
                        StopLoss = sl,
                        TakeProfit = tp,
                        Comment = comment,
                        Expiration = request.Expiration?.ToUniversalTime().ToTimestamp(),
                        MaxVisibleAmount = (double?)orderMaxVisibleVolume,
                        ExecOptions = request.Options?.ToDomainEnum(),
                    };

                    // emulate server ping
                    await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                    using (JournalScope())
                    {
                        var order = ReplaceOrder(coreRequest);

                        _collector.OnOrderModified();

                        // set result
                        return new OrderResultEntity(OrderCmdResultCodes.Ok, order.Clone(), ExecutionTime);
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
                    _collector.LogTradeFail($"Rejected modify #{request.OrderId} reason={error}");

                _collector.OnOrderModificatinRejected();

                return new OrderResultEntity(error, null, ExecutionTime);
            });
        }

        Task<OrderCmdResult> TradeCommands.CloseOrder(bool isAysnc, Api.CloseOrderRequest request)
        {
            var req = new Domain.CloseOrderRequest
            {
                OrderId = request.OrderId,
                Amount = request.Volume,
                Slippage = request.Slippage,
            };

            return CloseOrder(isAysnc, req);
        }

        Task<OrderCmdResult> TradeCommands.CloseNetPosition(bool isAsync, Api.CloseNetPositionRequest request)
        {
            throw new NotImplementedException();
        }

        Task<OrderCmdResult> TradeCommands.CloseOrderBy(bool isAysnc, string orderId, string byOrderId)
        {
            var req = new Domain.CloseOrderRequest
            {
                OrderId = orderId,
                ByOrderId = byOrderId
            };
            return CloseOrder(isAysnc, req);
        }

        private Task<OrderCmdResult> CloseOrder(bool isAysnc, Domain.CloseOrderRequest request)
        {
            return ExecTradeRequest(isAysnc, async () =>
            {
                OrderCmdResultCodes error = OrderCmdResultCodes.UnknownError;

                try
                {
                    if (_acc.Type == AccountInfo.Types.Type.Gross)
                    {
                        var order = _acc.GetOrderOrThrow(request.OrderId);
                        var smbMetadata = order.SymbolInfo;
                        double? closeVolume = null;

                        if (request.Amount != null)
                        {
                            var closeVolumeLots = RoundVolume(request.Amount, smbMetadata);
                            closeVolume = ToUnits(closeVolumeLots.Value, smbMetadata);
                            VerifyCloseAmout(closeVolume, smbMetadata);
                            request.Amount = (double?)closeVolume;
                        }

                        if (request.ByOrderId == null)
                        {
                            // emulate server ping
                            await _scheduler.EmulateAsyncDelay(VirtualServerPing, true);

                            using (JournalScope())
                            {
                                EnsureOrderIsPosition(order);

                                var currentRate = smbMetadata.LastQuote;
                                var dealerRequest = new ClosePositionDealerRequest(order, null/*currentRate*/);
                                dealerRequest.CloseAmount = request.Amount;

                                _dealer.ConfirmPositionClose(dealerRequest);

                                if (!dealerRequest.Confirmed || dealerRequest.DealerPrice <= 0)
                                    throw new OrderValidationError("Order is rejected by dealer", OrderCmdResultCodes.DealerReject);

                                ClosePosition(order, TradeReportInfo.Types.Reason.ClientRequest, closeVolume, null, closeVolume, dealerRequest.DealerPrice, smbMetadata,
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

                                ConfirmPositionCloseBy(order, byOrder, TradeReportInfo.Types.Reason.ClientRequest, true);
                            }
                        }

                        return new OrderResultEntity(OrderCmdResultCodes.Ok, order.Clone(), ExecutionTime);
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
                return new OrderResultEntity(OrderCmdResultCodes.Misconfiguration, null, ExecutionTime);
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

        private OrderAccessor OpenOrder(SymbolInfo symbolInfo, OrderInfo.Types.Type orderType, OrderInfo.Types.Side side, double volume, double? maxVisibleVolume, double? price, double? stopPrice,
            double? sl, double? tp, string comment, Domain.OrderExecOptions execOptions, string tag, DateTime? expiration, OpenOrderOptions options)
        {
            var order = new OrderAccessor(symbolInfo);

            order.Info.Id = NewOrderId();
            //order.SymbolPrecision = symbolInfo.Digits;
            order.Info.RequestedAmount = volume;
            order.Info.RemainingAmount = volume;
            order.Info.MaxVisibleAmount = maxVisibleVolume;

            order.Info.Side = side;
            order.Info.Type = orderType;
            order.Info.Symbol = symbolInfo.Name;
            order.Info.Comment = comment;

            var timestamp = _scheduler.UnsafeVirtualTimestamp;
            order.Info.Created = timestamp;
            order.Info.Modified = timestamp;

            //order.ClientOrderId = request.ClientOrderId;
            //order.Status = OrderStatuses.New;
            order.Info.InitialType = orderType;
            //order.ParentOrderId = request.ParentOrderId;

            //double? price = (double?)request.Price;
            //double? stopPrice = (double?)request.StopPrice;

            // Slippage calculation
            //if ((_acc.AccountingType == AccountingTypes.Cash) && ((request.InitialType == OrderTypes.Market) || (request.InitialType == OrderTypes.Stop)))
            //{
            //    double initialPrice = price ?? 0;
            //    double freeMarginPrice = acc.CalculateFreeMarginPrice(order, symbolInfo.Digits);
            //    double slippage = TradeLogic.GetSlippagePips(request.Side, symbolInfo);

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

            order.Info.StopLoss = sl;
            order.Info.TakeProfit = tp;
            //order.TransferringCoefficient = request.TransferringCoefficient;
            order.Info.UserTag = tag;
            order.Info.InstanceId = _acc.InstanceId;
            order.Info.Expiration = expiration.HasValue ? expiration.Value.ToUniversalTime().ToTimestamp() : null;
            order.Info.Options = execOptions.ToOrderOptions();
            //order.ReqOpenPrice = clientPrice;
            //order.ReqOpenAmount = clientAmount;

            if (orderType != OrderInfo.Types.Type.Stop)
                order.Info.Price = price;
            if (orderType == OrderInfo.Types.Type.Stop || orderType == OrderInfo.Types.Type.StopLimit)
                order.Info.StopPrice = stopPrice;

            _calcFixture.ValidateNewOrder(order);

            //string comment = null;

            // add new order
            //acc.AddTemporaryNewOrder(order);

            var trReason = options.HasFlag(OpenOrderOptions.Stopout) ? TradeReportInfo.Types.Reason.StopOut : TradeReportInfo.Types.Reason.DealerDecision;

            if (!options.HasFlag(OpenOrderOptions.SkipDealing))
            {
                // Dealer request
                var dealerRequest = new OpenOrderDealerRequest(order, new QuoteEntity((QuoteInfo)symbolInfo.LastQuote));
                _dealer.ConfirmOrderOpen(dealerRequest);

                if (!dealerRequest.Confirmed || dealerRequest.DealerAmount < 0 || dealerRequest.DealerPrice <= 0)
                    throw new OrderValidationError("Order is rejected by dealer", OrderCmdResultCodes.DealerReject);

                var dealerAmount = ToUnits(dealerRequest.DealerAmount, symbolInfo);

                return ConfirmOrderOpening(order, trReason, dealerRequest.DealerPrice, dealerAmount, options);
            }

            return ConfirmOrderOpening(order, trReason, null, null, options);
        }

        private OrderAccessor ConfirmOrderOpening(OrderAccessor order, TradeReportInfo.Types.Reason trReason, double? execPrice, double? execAmount, OpenOrderOptions options)
        {
            var currentRate = _calcFixture.GetCurrentRateOrNull(order.Info.Symbol);

            bool isInstantOrder = false;

            //CommissionStrategy.OnOrderOpened(order, null);

            //var orderCopy = order.Clone();
            var fillInfo = new FillInfo();

            // fire API event
            if (order.Info.Type != OrderInfo.Types.Type.Position)
                _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderOpened(new OrderOpenedEventArgsImpl(order)));

            if (order.Info.Type == OrderInfo.Types.Type.Market)
            {
                // fill order
                fillInfo = FillOrder(order, execPrice, execAmount, trReason);
                isInstantOrder = _acc.Type != AccountInfo.Types.Type.Gross;
            }
            else if (order.Info.Type == OrderInfo.Types.Type.Limit && order.Info.IsImmediateOrCancel)
            {
                // fill order
                fillInfo = FillOrder(order, execPrice, execAmount, trReason);
                //else
                //    orderCopy = order.Clone();

                if (order.Info.RemainingAmount > 0) // partial fill
                {
                    //// cancel remaining part
                    //LogTransactionDetails(() => "Cancelling IoC Order #" + order.OrderId + ", RemainingAmount=" + orderCopy.RemainingAmount + ", Reason=" + Domain.TradeReportInfo.Types.Reason.DealerDecision,
                    //    JournalEntrySeverities.Info, order.Clone());

                    //ConfirmOrderCancelation(acc, Domain.TradeReportInfo.Types.Reason.DealerDecision, order.OrderId, null, clientRequestId, false);
                }

                isInstantOrder = _acc.Type != AccountInfo.Types.Type.Gross;
            }
            else if (order.Info.Type == OrderInfo.Types.Type.Limit || order.Info.Type == OrderInfo.Types.Type.Stop || order.Info.Type == OrderInfo.Types.Type.StopLimit)
            {
                _acc.Orders.Add(order);
                RegisterOrder(order, currentRate);

                //FinalizeOrderOperation(order, null, order.SymbolRef, acc, OrderStatuses.Calculated, OrderExecutionEvents.Allocated);
            }
            else if (order.Info.Type == OrderInfo.Types.Type.Position)
                throw new OrderValidationError("Invalid order type", OrderCmdResultCodes.InternalError);
            else
                throw new OrderValidationError("Unknown order type", OrderCmdResultCodes.InternalError);

            RecalculateAccount();

            // execution report
            if (_sendReports)
                _context.SendExtUpdate(TesterTradeTransaction.OnOpenOrder(order, isInstantOrder, fillInfo, (double)_acc.Balance));

            // summary

            bool isFakeOrder = options.HasFlag(OpenOrderOptions.FakeOrder);
            if (!isFakeOrder)
                _opSummary.AddOpenAction(order, fillInfo.NetPos?.Charges);
            if (fillInfo.NetPos != null)
            {
                var closeActionCharges = isFakeOrder ? fillInfo.NetPos.Charges : null;
                _opSummary.AddNetCloseAction(fillInfo.NetPos.CloseInfo, order.SymbolInfo, _acc.BalanceCurrencyInfo, closeActionCharges);
                _opSummary.AddNetPositionNotification(fillInfo.NetPos.ResultingPosition, order.SymbolInfo);
            }

            return order;
        }

        private OrderAccessor ReplaceOrder(Domain.ModifyOrderRequest request)
        {
            // Check schedule for the symbol
            var order = _acc.Orders.GetOrNull(request.OrderId) ?? throw new OrderValidationError($"Order Not Found {request.OrderId}", OrderCmdResultCodes.OrderNotFound);
            var symbol = _context.Builder.Symbols.GetOrNull(order.Info.Symbol);

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

            if (!order.Info.Type.IsPending()) // forbid to change price and volume for positions (server style!)
            {
                request.Price = null;
                request.NewAmount = null;
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
            //    double executed = replaceOrder.Amount - replaceOrder.RemainingAmount;

            //    // This calculation has the goal of preventing orders from being overfilled
            //    if (Request.Amount.Value > executed)
            //        Request.RemainingAmount = Request.Amount.Value - executed;
            //    else
            //        cancelOrder = true;
            //}

            //if (Request.MaxVisibleAmount.HasValue && (Request.MaxVisibleAmount.Value >= 0))
            //Facade.VerifyMaxVisibleAmout(Request.MaxVisibleAmount, securityCfg, symbolInfo, Request.IsClientRequest);

            var newVolume = request.NewAmount ?? order.Info.RequestedAmount;
            var newPrice = request.Price ?? order.Info.Price;
            var newStopPrice = request.StopPrice ?? order.Info.StopPrice;

            bool volumeChanged = newVolume != order.Info.RequestedAmount;

            // Check margin of the modified order
            if (volumeChanged)
                _calcFixture.ValidateModifyOrder(order, newVolume, newPrice, newStopPrice);

            // dealer request
            var dealerRequest = new ModifyOrderDealerRequest(order, null/*symbol.LastQuote*/);
            dealerRequest.NewComment = request.Comment;
            dealerRequest.NewPrice = request.Price;
            dealerRequest.NewVolume = request.NewAmount;
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

            //    Domain.TradeReportInfo.Types.Reason trReason = Request.IsClientRequest ? Domain.TradeReportInfo.Types.Reason.ClientRequest : Domain.TradeReportInfo.Types.Reason.DealerDecision;

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

        private OrderAccessor ConfirmOrderReplace(OrderAccessor order, Domain.ModifyOrderRequest request)
        {
            //OrderModel order = (!request.IsClientRequest && request.UpdateNewOrdersInDealing)
            //    ? acc.GetNewOrder(orderId)
            //    : acc.GetOrder(orderId);

            var currentRate = _calcFixture.GetCurrentRateOrThrow(request.Symbol);

            var oldOrderCopy = order.Clone();
            var orderType = order.Info.Type;

            if (!orderType.IsMarket())
                UnregisterOrder(order);

            var newVol = request.NewAmount;

            if (orderType.IsPending() && newVol.HasValue && newVol != order.Info.RequestedAmount)
            {
                var filledVolume = order.Info.RequestedAmount - order.Info.RemainingAmount;

                order.Info.RequestedAmount = newVol.Value;
                order.Info.ChangeRemaningAmount(newVol.Value - filledVolume);

                // Recalculate commission if necessary.
                //var mAcc = acc as MarginAccountModel;
                //CommissionStrategy.OnOrderModified(order, null, mAcc);
            }

            // Update or reset max visible amount value
            if (orderType.IsPending() && request.MaxVisibleAmount.HasValue)
            {
                if (request.MaxVisibleAmount.Value < 0)
                {
                    order.Info.MaxVisibleAmount = null;
                }
                else
                {
                    order.Info.MaxVisibleAmount = request.MaxVisibleAmount;
                    //order.Options = order.Options.SetFlag(OrderExecutionOptions.HiddenIceberg);
                }
            }

            if (request.Price.HasValue && orderType.IsLimit())
            {
                order.Info.Price = request.Price.Value;
                //order.ReqOpenPrice = request.Price.Value;
            }

            if (request.StopPrice.HasValue && orderType.IsStop())
            {
                order.Info.StopPrice = request.StopPrice.Value;
                //order.ReqOpenPrice = request.StopPrice.Value;
            }

            // Change IOC option for stop-limit orders
            //if ((request.ImmediateOrCancelFlag.HasValue) && (order.InitialType == OrderTypes.StopLimit) && (order.Type == OrderTypes.StopLimit))
            //{
            //    order.Options = request.ImmediateOrCancelFlag.Value ? order.Options.SetFlag(OrderExecutionOptions.ImmediateOrCancel) : order.Options.ClearFlag(OrderExecutionOptions.ImmediateOrCancel);
            //}

            if (request.Expiration != null && orderType.IsPending())
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                if (request.Expiration.ToDateTime() > epoch)
                    order.Info.Expiration = request.Expiration;
                else
                    order.Info.Expiration = null;
            }

            if (_acc.Type == AccountInfo.Types.Type.Gross)
            {
                if (request.StopLoss.HasValue)
                    order.Info.StopLoss = (request.StopLoss.Value != 0) ? request.StopLoss : null;
                if (request.TakeProfit.HasValue)
                    order.Info.TakeProfit = (request.TakeProfit.Value != 0) ? request.TakeProfit : null;
            }

            order.Info.Comment = request.Comment ?? order.Info.Comment;
            order.Info.UserTag = request.Tag == null ? order.Info.UserTag : CompositeTag.ExtarctUserTarg(request.Tag);
            order.Info.Modified = _scheduler.UnsafeVirtualTimestamp;
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

            if (!orderType.IsMarket())
                RegisterOrder(order, currentRate);

            //_acc.CalculateOrder(order, infrustructure);
            //order.FireChanged();

            // fire API event
            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrderCopy, order)));

            RecalculateAccount();

            // execution report
            if (_sendReports)
                _context.SendExtUpdate(TesterTradeTransaction.OnReplaceOrder(order));

            // summary
            if (_collector.WriteOrderModifications)
                _opSummary.AddModificationAction(oldOrderCopy, order);

            //Order orderCopy = FinalizeOrderOperation(order, orderPrev, order.SymbolRef, acc, OrderStatuses.Calculated, OrderExecutionEvents.Modified, request.RequestClientId);
            return order;
        }

        private FillInfo FillOrder(OrderAccessor order, double? fillPrice, double? fillAmount, TradeReportInfo.Types.Reason reason)
        {
            double actualPrice;
            double actualAmount;

            if (fillPrice == null)
            {
                var quote = _calcFixture.GetCurrentRateOrNull(order.Info.Symbol);
                actualPrice = GetCurrentOpenPrice(order) ?? 0;
            }
            else
                actualPrice = fillPrice.Value;

            if (fillAmount != null)
                actualAmount = fillAmount.Value;
            else
                actualAmount = order.Info.RemainingAmount;

            return FillOrder2(order, actualPrice, actualAmount, reason);
        }

        // can lead to: 1) new gross position 2) net position settlement 3) asset movement
        private FillInfo FillOrder2(OrderAccessor order, double fillPrice, double fillAmount, TradeReportInfo.Types.Reason reason)
        {
            var orderType = order.Info.Type;

            if (orderType.IsPosition())
                throw new Exception("Order already filled #" + order.Info.Id);

            var copy = order.Clone();

            if (fillAmount >= order.Info.RemainingAmount)
                fillAmount = order.Info.RemainingAmount;
            bool partialFill = order.Info.RemainingAmount > fillAmount;

            //Logger.Info(() => OperationContext.LogPrefix + "Fill order " + order + ", FillAmount=" + fillAmount);

            if (partialFill)
            {
                //order.Status = OrderStatuses.Calculated;
                order.Info.ChangeRemaningAmount(order.Info.RemainingAmount - fillAmount);
                //_calcFixture.CalculateOrder(order);

                //if (order.IsPending)
                //    ResetOrderActivation(order);
            }
            else
            {
                //order.Status = OrderStatuses.Filled;
                order.Info.ChangeRemaningAmount(0);

                if (orderType.IsPending())
                    UnregisterOrder(order);
            }

            //order.AggrFillPrice += fillAmount * fillPrice;
            //order.AverageFillPrice = order.AggrFillPrice / (order.Amount - order.RemainingAmount);
            //order.Entity.Filled = OperationContext.ExecutionTime;
            order.Info.Modified = _scheduler.UnsafeVirtualTimestamp;
            order.Info.LastFillPrice = fillPrice;
            order.Info.LastFillAmount = fillAmount;

            if ((_acc.Type == AccountInfo.Types.Type.Net) || (_acc.Type == AccountInfo.Types.Type.Cash))
            {
                // increase reported action number
                //order.ActionNo++;
            }

            // Create reports
            TradeReportAdapter tradeReport = null;
            if (_acc.Type != AccountInfo.Types.Type.Gross)
                tradeReport = _history.Create(ExecutionTime, order.SymbolInfo, TradeReportInfo.Types.ReportType.OrderFilled, reason);

            // do account-specific fill
            if (_acc.Type == AccountInfo.Types.Type.Cash)
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
            if (tradeReport != null)
            {
                tradeReport.FillGenericOrderData(_calcFixture, order);
                tradeReport.Info.OrderLastFillAmount = (double?)fillAmount;
                tradeReport.Info.OrderFillPrice = (double?)fillPrice;

                if (_acc.IsMarginType)
                {
                    //MarginAccountModel mAcc = (MarginAccountModel)acc;
                    tradeReport.FillAccountBalanceConversionRates(_calcFixture, _acc.BalanceCurrency, _acc.Balance);
                }
            }

            // journal
            //string chargesInfo = (acc is CashAccountModel)
            //    ? chargesInfo = $" Commission={charges.Commission} AgentCommission={charges.AgentCommission}"
            //    : null;

            //LogTransactionDetails(() => "Final order " + pumpingOrderCopy, JournalEntrySeverities.Info, pumpingOrderCopy);

            //double profit = 0;
            OrderAccessor newPos = null;
            NetPositionOpenInfo netInfo = null;

            if (_acc.Type == AccountInfo.Types.Type.Gross)
                newPos = CreatePositionFromOrder(TradeReportInfo.Types.Reason.PendingOrderActivation, order, fillPrice, fillAmount, !partialFill);
            else if (_acc.Type == AccountInfo.Types.Type.Net)
            {
                netInfo = OpenNetPositionFromOrder(order, fillAmount, fillPrice, tradeReport);
                //profit = netInfo.CloseInfo.BalanceMovement;
            }

            //_collector.LogTrade($"Filled Order #{copy.Id} {order.Type} {order.Symbol} Price={fillPrice} Amount={fillAmount} RemainingAmount={copy.RemainingAmount} Profit={profit} Comment=\"{copy.Comment}\"");

            //_acc.AfterFillOrder(order, this, fillPrice, fillAmount, partialFill, tradeReport, execReport, fillReport);

            //order.FireChanged();

            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderFilled(new OrderFilledEventArgsImpl(copy, order)));

            if (tradeReport != null)
                _history.Add(tradeReport);

            return new FillInfo() { FillAmount = fillAmount, FillPrice = fillPrice, Position = newPos, NetPos = netInfo, SymbolInfo = order.SymbolInfo };
        }

        private OrderAccessor CancelOrder(OrderAccessor order, TradeReportInfo.Types.Reason trReason)
        {
            if (!order.Info.Type.IsPending())
                throw new OrderValidationError("Only Limit, Stop and StopLimit orders can be canceled. Please check the type of the order #" + order.Info.Id, OrderCmdResultCodes.OrderNotFound);

            var dealerReq = new CancelOrderDealerRequest(order, null /*order.SymbolInfo.Info.LastQuote*/);
            _dealer.ConfirmOrderCancelation(dealerReq);

            if (!dealerReq.Confirmed)
                throw new OrderValidationError("Cancellation is rejected by dealer", OrderCmdResultCodes.DealerReject);

            return ConfirmOrderCancelation(trReason, order);
        }

        private OrderAccessor ConfirmOrderCancelation(TradeReportInfo.Types.Reason trReason, OrderAccessor order, OrderAccessor originalOrder = null)
        {
            //var order = _acc.GetOrderOrThrow(orderId);

            // to prevent cancelling orders that was already filled
            if (originalOrder != null && !originalOrder.IsSameOrder(order))
                throw new OrderValidationError($"Type of order #{order.Info.Id} was changed.", OrderCmdResultCodes.DealerReject);

            UnregisterOrder(order);

            if (order.Info.Type != OrderInfo.Types.Type.Position)
            {
                // increase reported action number
                order.Entity.ActionNo++;
            }

            // remove order
            _acc.Orders.Remove(order.Info);

            // journal
            //LogTransactionDetails(() => $"Confirmed Order Cancellation #{orderId}, reason={trReason}", JournalEntrySeverities.Info, order.Clone());

            //Order orderCopy = FinalizeOrderOperation(order, null, order.SymbolRef, acc,
            //    trReason == Domain.TradeReportInfo.Types.Reason.Expired ? OrderStatuses.Expired : OrderStatuses.Canceled,
            //    trReason == Domain.TradeReportInfo.Types.Reason.Expired ? OrderExecutionEvents.Expired : OrderExecutionEvents.Canceled,
            //    clientRequestId);

            if (trReason == TradeReportInfo.Types.Reason.StopOut && _acc.IsMarginType)
            {
                //MarginAccountModel mAcc = (MarginAccountModel)acc;
                //order.UserComment = "Stopout: MarginLevel = " + double.Round(mAcc.MarginLevel, 2) + ", Margin = " + double.Round(mAcc.Margin, mAcc.RoundingDigits) + ", Equity = " + double.Round(mAcc.Equity, mAcc.RoundingDigits);
            }

            // fire API event
            _scheduler.EnqueueEvent(b => b.Account.Orders.FireOrderCanceled(new OrderCanceledEventArgsImpl(order)));

            if (trReason == TradeReportInfo.Types.Reason.Expired)
                RecalculateAccount();

            // update trade history
            var report = _history.Create(_scheduler.UnsafeVirtualTimePoint, order.SymbolInfo, trReason == TradeReportInfo.Types.Reason.Expired ? TradeReportInfo.Types.ReportType.OrderExpired : TradeReportInfo.Types.ReportType.OrderCanceled, trReason);
            report.FillGenericOrderData(_calcFixture, order);
            report.FillAccountSpecificFields(_calcFixture);
            report.Info.RemainingQuantity = 0;
            report.Info.MaxVisibleQuantity = order.Info.MaxVisibleAmount;

            if (_acc.IsMarginType)
            {
                report.FillAccountBalanceConversionRates(_calcFixture, _acc.BalanceCurrency, _acc.Balance);
            }

            _history.Add(report);

            // execution report
            if (_sendReports)
                _context.SendExtUpdate(TesterTradeTransaction.OnCancelOrder(order));

            // summary
            _opSummary.AddCancelAction(order);

            return order;
        }

        private OrderAccessor CreatePositionFromOrder(TradeReportInfo.Types.Reason trReason, OrderAccessor parentOrder,
           double openPrice, double posAmount, bool transformOrder)
        {
            return CreatePosition(trReason, parentOrder, parentOrder.Info.Side, parentOrder.SymbolInfo, openPrice, posAmount, transformOrder);
        }

        private OrderAccessor CreatePosition(TradeReportInfo.Types.Reason trReason, OrderAccessor parentOrder, OrderInfo.Types.Side side, SymbolInfo smb, double openPrice, double posAmount, bool transformOrder)
        {
            OrderAccessor position;
            //TradeChargesInfo charges = new TradeChargesInfo();
            var currentRate = _calcFixture.GetCurrentRateOrNull(smb.Name);
            var symbolCalc = _context.MarketData.GetCalculator(smb);
            var wasTmpOrder = parentOrder.Info.Type == OrderInfo.Types.Type.Market
                || (parentOrder.Info.Type == OrderInfo.Types.Type.Limit && parentOrder.Info.IsImmediateOrCancel);

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
                position.Entity.PositionCreated = ExecutionTime;
            }
            else
            {
                position = new OrderAccessor(smb);
                position.Info.Id = NewOrderId();
                position.Info.Symbol = smb.Name;
                //position.ClientOrderId = Guid.NewGuid().ToString("D");
                position.Info.Side = side;
                position.Entity.PositionCreated = ExecutionTime;
                //position.SymbolPrecision = smb.Digits;

                var timestamp = _scheduler.UnsafeVirtualTimestamp;
                position.Info.Created = timestamp;
                position.Info.Modified = timestamp;

                if (parentOrder != null)
                {
                    position.Info.MaxVisibleAmount = parentOrder.Info.MaxVisibleAmount;
                    position.Info.StopLoss = parentOrder.Info.StopLoss;
                    position.Info.TakeProfit = parentOrder.Info.TakeProfit;
                    position.Info.Comment = parentOrder.Info.Comment;
                    position.Info.UserTag = parentOrder.Info.UserTag;
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
                }
            }

            //position.ParentOrderId = (parentOrder != null) ? parentOrder.OrderId : position.OrderId;
            position.Info.InitialType = (parentOrder != null) ? parentOrder.Info.InitialType : OrderInfo.Types.Type.Market;

            //position.Entity.Type = OrderType.Position;
            //position.Status = OrderStatuses.Calculated;
            //position.Entity.Price = (double)openPrice; // position open price

            position.Info.ChangeEssentials(OrderInfo.Types.Type.Position, posAmount, openPrice, null);

            // stop price for stops
            //if ((parentOrder != null) && (parentOrder.InitialType == OrderTypes.StopLimit || parentOrder.InitialType == OrderTypes.Stop))
            //    position.Entity.StopPrice = parentOrder.StopPrice;
            //else
            //    position.Entity.StopPrice = null;

            position.Info.RequestedAmount = posAmount;
            //position.RemainingAmount = posAmount;
            position.Info.Expiration = null;

            //if (_acc.Type == AccountInfo.Types.Type.Gross && position.Info.TakeProfit.HasValue)
            //{
            //    double? currentRateBid = ((QuoteInfo)currentRate)?.NullableBid();
            //    double? currentRateAsk = ((QuoteInfo)currentRate)?.NullableAsk();
            //    //position.IsReducedCloseCommission = ((position.Side == OrderSide.Buy && currentRateBid.HasValue &&
            //    //                                      position.TakeProfit > currentRateBid) ||
            //    //                                     (position.Side == OrderSide.Sell && currentRateAsk.HasValue &&
            //    //                                      position.TakeProfit < currentRateAsk));
            //}

            // increase reported action number
            position.Entity.ActionNo++;

            // add position to accoun
            if (wasTmpOrder || !transformOrder)
                _acc.Orders.Add(position);

            // calculate margin & profit
            //fCalc.UpdateMargin(position, _acc);
            //fCalc.UpdateProfit(position);

            // Update order initial margin rate.
            //position.Entity.OpenConversionRate = position.MarginRateCurrent;

            // calculate commission
            //CommisionEmulator.OnGrossPositionOpened(position, position.SymbolInfo, _calcFixture);
            var commissionRes = symbolCalc.Commission.Calculate(new CommissionRequest(position.Info.Side, position.Info.RemainingAmount, false, false));
            position.Info.ChangeCommission(RoundMoney(2 * commissionRes.Value, _calcFixture.RoundingDigits)); // one-way commission mode

            //// log
            //if (transformOrder)
            //    Logger.Info(() => OperationContext.LogPrefix + "Replace order with position " + position);
            //else
            //    Logger.Info(() => OperationContext.LogPrefix + "Create new position: " + position);

            // register order
            RegisterOrder(position, currentRate);

            //position.FireChanged();

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

        private void RegisterOrder(OrderAccessor order, IQuoteInfo currentRate)
        {
            ActivationRecord activationInfo = _activator.AddOrder(order, (QuoteInfo)currentRate);
            // Check if order must be activated immediately
            if (activationInfo != null)
            {
                // TO DO : enqueue activation task
                Action<PluginBuilder> checkActivationTask = (b) => ActivateOrderTransaction(activationInfo);
                _scheduler.EnqueueTradeUpdate(checkActivationTask);
            }

            RegisterForExpirationCheck(order);
        }

        //private void ResetOrderActivation(OrderAccessor order)
        //{
        //    _activator.ResetOrderActivation(order);
        //}

        private void UnregisterOrder(OrderAccessor order)
        {
            _activator.RemoveOrder(order);
            _expirationManager.RemoveOrder(order);
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
            if (_scheduler.IsStopPhase)
                return;

            var lastRate = _calcFixture.GetCurrentRateOrNull(_settings.CommonSettings.MainSymbol);
            var mainSymbol = _context.Builder.Symbols.GetOrNull(_settings.CommonSettings.MainSymbol);

            using (JournalScope())
                _opSummary.AddStopOutAction(_acc, (QuoteInfo)lastRate, mainSymbol);

            _scheduler.SetFatalError(new StopOutException("Stop out!"));
        }

        internal void UpdateAssetsOnFill(OrderAccessor order, double fillPrice, double fillAmount)
        {
            var smb = order.SymbolInfo;
            var roundDigits = _context.Builder.Currencies.GetOrNull(smb.CounterCurrency)?.Info?.Digits ?? 2;

            //var mrgAsset = _acc.Assets.GetOrCreateAsset(smb.MarginCurrency);
            //var prfAsset = _acc.Assets.GetOrCreateAsset(smb.ProfitCurrency);

            //var marginReport = CreateChangeReport(mrgAsset, 0);
            //var profitReport = CreateChangeReport(prfAsset, 0);

            var mChange = 0.0;
            var pChange = 0.0;

            if (order.Info.Side == OrderInfo.Types.Side.Buy)
            {
                mChange = fillAmount;
                pChange = -(fillAmount * fillPrice).CeilBy(roundDigits);
            }
            else if (order.Info.Side == OrderInfo.Types.Side.Sell)
            {
                mChange = -fillAmount;
                pChange = (fillAmount * fillPrice).FloorBy(roundDigits);
            }

            // Update asset amount
            _acc.IncreaseAsset(smb.BaseCurrency, mChange);
            _acc.IncreaseAsset(smb.CounterCurrency, pChange);

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

        internal NetPositionOpenInfo OpenNetPositionFromOrder(OrderAccessor fromOrder, double fillAmount, double fillPrice, TradeReportAdapter tradeReport)
        {
            var smb = fromOrder.SymbolInfo;
            var symbolCalc = _context.MarketData.GetCalculator(smb);
            var fillSide = fromOrder.Info.Side;
            var position = _acc.NetPositions.GetOrCreatePosition(smb.Name, NewOrderId());
            var posInfo = position.Info;
            var timestamp = _scheduler.UnsafeVirtualTimestamp;
            posInfo.Modified = timestamp;

            var charges = new TradeChargesInfo();
            charges.CurrencyInfo = _acc.BalanceCurrencyInfo;

            // commission
            var commissionRes = symbolCalc.Commission.Calculate(new CommissionRequest(fromOrder.Info.Side, fillAmount, false, fromOrder.Info.MaxVisibleAmount.HasValue));
            var commission = RoundMoney(commissionRes.Value, _calcFixture.RoundingDigits);
            //CommisionEmulator.OnNetPositionOpened(fromOrder, position, fillAmount, smb, charges, _calcFixture);
            charges.Commission = commission;

            tradeReport.Info.Commission = commission;
            //tradeReport.Entity.AgentCommission = (double)charges.AgentCommission;
            //tradeReport.Entity.MinCommissionCurrency = (double)charges.MinCommissionCurrency;
            //tradeReport.Entity.MinCommissionConversionRate =  (double)charges.MinCommissionConversionRate;

            var balanceMovement = commission;

            if (fromOrder.Info.Type.IsMarket() || fromOrder.Info.RemainingAmount.E(0))
                _acc.Orders.Remove(fromOrder.Info);

            // journal;
            //LogTransactionDetails(() => "Position opened: symbol=" + smb.Name + " price=" + fillPrice + " amount=" + fillAmount + " commision=" + charges.Commission + " reason=" + tradeReport.TrReason,
            //JournalEntrySeverities.Info, TransactDetails.Create(position.Id, position.Symbol));

            var openInfo = new NetPositionOpenInfo();
            openInfo.Charges = charges;
            openInfo.ResultingPosition = position;
            openInfo.CloseInfo = new NetPositionCloseInfo();
            if (posInfo.IsEmpty)
            {
                posInfo.Side = fillSide;
                posInfo.Price = fillPrice;
                posInfo.Volume = fillAmount;
            }
            else if (posInfo.Side == fillSide)
            {
                var posPrice = posInfo.Price;
                var posAmount = posInfo.Volume;
                posInfo.Price = (posPrice * posAmount + fillPrice * fillAmount) / (posAmount + fillAmount);
                posInfo.Volume = posAmount + fillAmount;
            }
            else
            {
                var posPrice = posInfo.Price;
                var posSide = posInfo.Side;
                var posAmount = posInfo.Volume;
                var closingAmount = Math.Min(fillAmount, posAmount);
                double closeSwap;

                if (posAmount.Lt(fillAmount))
                {
                    posInfo.Side = fillSide;
                    posInfo.Price = fillPrice;
                    posInfo.Volume = fillAmount - posAmount;

                    // all existing swap goes away, since position changes side
                    closeSwap = posInfo.Swap;
                    posInfo.Swap = 0.0;
                }
                else
                {
                    posInfo.Volume = posAmount - fillAmount;

                    // partial close, removing part of swap
                    var k = fillAmount / posAmount;
                    closeSwap = RoundMoney(k * position.Info.Swap, _calcFixture.RoundingDigits);
                    posInfo.Swap -= closeSwap;
                }

                if (position.Info.IsEmpty)
                    _acc.NetPositions.RemovePosition(smb.Name);

                var profitRes = symbolCalc.Profit.Calculate(new ProfitRequest(posPrice, closingAmount, posSide, fillPrice));
                if (profitRes.Error != CalculationError.None)
                    throw new Exception();
                var profit = RoundMoney(profitRes.Value, _calcFixture.RoundingDigits);

                balanceMovement += profit + closeSwap;

                tradeReport.Info.PositionClosed = ExecutionTime.ToUniversalTime().ToTimestamp();
                tradeReport.Info.PositionOpenPrice = posPrice;
                tradeReport.Info.PositionClosePrice = fillPrice;
                tradeReport.Info.PositionCloseQuantity = closingAmount;
                tradeReport.Info.Swap += closeSwap;
                //report.Entity.CloseConversionRate = (double)profitRate;

                openInfo.CloseInfo.CloseAmount = closingAmount;
                openInfo.CloseInfo.ClosePrice = posPrice;
                openInfo.CloseInfo.BalanceMovement = profit + closeSwap;

                _collector.OnPositionClosed(ExecutionTime, profit, 0, closeSwap);

                //LogTransactionDetails(() => "Position closed: symbol=" + position.Symbol + " amount=" + oneSideClosingAmount + " open=" + openPrice + " close=" + closePrice
                //                            + " profit=" + profit + " swap=" + closeSwap,
                //    JournalEntrySeverities.Info, TransactDetails.Create(position.Id, position.Symbol));
            }

            tradeReport.Info.TransactionAmount = balanceMovement;
            tradeReport.FillAccountSpecificFields(_calcFixture);
            tradeReport.FillPosData(position, fillPrice, 0/*fromOrder.MarginRateCurrent*/);
            tradeReport.Info.PositionOpened = timestamp;
            //tradeReport.Entity.OpenConversionRate = (double?)fromOrder.MarginRateCurrent;

            //LogTransactionDetails(() => "Final position: " + position.GetBriefInfo(), JournalEntrySeverities.Info, TransactDetails.Create(position.Id, position.Symbol));

            //execReport.Profit = new ExecProfitInfo(balanceMovement, acc.Balance, acc.BalanceCurrency);
            //SendExecutionReport(execReport, acc);
            //SendPositionReport(acc, CreatePositionReport(acc, PositionReportType.CreatePosition, position.SymbolRef, balanceMovement));

            _acc.Balance += balanceMovement;

            _collector.OnCommisionCharged(commission);

            return openInfo;
        }

        internal void CheckActivation(IRateInfo rate)
        {
            var records = _activator.CheckPendingOrders(rate);
            for (int i = 0; i < records.Count; i++)
                ActivateOrderTransaction(records[i]);
        }

        private void ActivateOrderTransaction(ActivationRecord record)
        {
            double lockedActivateMargin = 0;

            using (JournalScope())
                ActivateOrder(record, ref lockedActivateMargin);
        }

        // can lead to: 1) new gross position 2) close gross position 3) close net position 4) asset movement
        private void ActivateOrder(ActivationRecord record, ref double lockedActivateMargin)
        {
            // Perform automatic order activation.
            //AccountModel account = record.Account;

            // Skip orders activation for blocked accounts
            //if (account.IsBlocked)
            //    return;

            if (record.Order.Info.RemainingAmount == 0)
                return; // already activated

            //GroupSecurityCfg securityCfg = account.GetSecurityCfg(smbInfo);
            //if ((record.ActivationType == ActivationType.Pending) && (record.Order.Type == Domain.OrderInfo.Types.Type.Stop))
            //{
            //    //bool needCancelation = false;

            //    // Check margin of the activated pending order
            //    try
            //    {
            //        //    account.ValidateOrderActivation(record.Order, record.ActivationPrice, record.Order.RemainingAmount, ref lockedActivateMargin);
            //    }
            //    catch (ServerFaultException<NotEnoughMoneyFault>)
            //    {
            //        //needCancelation = true;
            //    }
            //    catch (ServerFaultException<OffQuotesFault>)
            //    {
            //        //needCancelation = true;
            //    }

            //    // Insufficient margin. Cancel pending order
            //    //if (needCancelation)
            //    //{
            //    //    var order = record.Order;
            //    //    LocalAccountTransaction.Start(account.Id, "Pending order " + record.OrderId + " was canceled during activation because of insufficient margin to activate!", JournalTransactionTypes.CancelOrder,
            //    //        tr =>
            //    //        {
            //    //            tr.TradeInfrastructure.LogTransactionDetails(() => $"Account state: {account}", JournalEntrySeverities.Info, tr.Token, TransactDetails.Create(order.OrderId, order.Symbol));

            //    //            CancelOrderRequest request = new CancelOrderRequest();
            //    //            request.Level = TradeMessageLevels.Manager;
            //    //            request.AccountId = order.Account.Id;
            //    //            request.OrderId = order.OrderId;
            //    //            request.StopoutFlag = false;
            //    //            request.ExpirationFlag = false;
            //    //            request.ManagerOptions = TradeRequestOptions.DealerRequest;

            //    //            return CancelOrderAsync(request, order.Account, Domain.TradeReportInfo.Types.Reason.DealerDecision);
            //    //        });

            //    //    Logger.Info(() => OperationContext.LogPrefix + "Pending order " + record.OrderId + " was canceled during activation because of insufficient margin to activate!");

            //    //    return;
            //    //}
            //}

            var fillInfo = new FillInfo();

            if (record.ActivationType == ActivationType.Pending)
            {
                if (record.Order.Info.Type == OrderInfo.Types.Type.StopLimit)
                {
                    ActivateStopLimitOrder(record.Order, TradeReportInfo.Types.Reason.PendingOrderActivation);

                    // execution report
                    if (_sendReports)
                        _context.SendExtUpdate(TesterTradeTransaction.OnActivateStopLimit(record.Order));

                    // summary
                    _opSummary.AddStopLimitActivationAction(record.Order, record.ActivationPrice);
                }
                else
                {
                    fillInfo = FillOrder(record.Order, record.ActivationPrice, record.Order.Info.RemainingAmount, TradeReportInfo.Types.Reason.PendingOrderActivation);

                    // execution report
                    if (_sendReports)
                        _context.SendExtUpdate(TesterTradeTransaction.OnFill(record.Order, fillInfo, (double)_acc.Balance));

                    // summary
                    _opSummary.AddFillAction(record.Order, fillInfo);
                    if (fillInfo.NetPos != null)
                    {
                        _opSummary.AddNetCloseAction(fillInfo.NetPos.CloseInfo, record.Order.SymbolInfo, _acc.BalanceCurrencyInfo);
                        _opSummary.AddNetPositionNotification(fillInfo.NetPos.ResultingPosition, fillInfo.SymbolInfo);
                    }
                }
            }
            else if ((_acc.Type == AccountInfo.Types.Type.Gross) && (record.ActivationType == ActivationType.StopLoss || record.ActivationType == ActivationType.TakeProfit))
            {
                TradeReportInfo.Types.Reason trReason = TradeReportInfo.Types.Reason.DealerDecision;
                if (record.ActivationType == ActivationType.StopLoss)
                    trReason = TradeReportInfo.Types.Reason.StopLossActivation;
                else if (record.ActivationType == ActivationType.TakeProfit)
                    trReason = TradeReportInfo.Types.Reason.TakeProfitActivation;

                var smb = _context.Builder.Symbols.GetOrNull(record.Order.Info.Symbol).Info;
                ClosePosition(record.Order, trReason, null, null, record.Order.Info.RemainingAmount, record.Price, smb, 0, null);
            }
        }

        private void ActivateStopLimitOrder(OrderAccessor order, TradeReportInfo.Types.Reason reason)
        {
            UnregisterOrder(order);

            // Increase reported action number
            order.Entity.ActionNo++;

            // remove order
            _acc.Orders.Remove(order.Info);

            // journal
            //LogTransactionDetails(() => $"Activate StopLimit Order #{order.OrderId}, reason={reason}", JournalEntrySeverities.Info, order.Clone());

            //Order orderCopy = FinalizeOrderOperation(order, null, order.SymbolRef, acc, OrderStatuses.Activated, OrderExecutionEvents.Activated, null);

            // Update trade history
            //TradeReportModel report = TradeReportModel.Create(acc, TradeTransTypes.OrderActivated, Domain.TradeReportInfo.Types.Reason.DealerDecision);
            //report.FillGenericOrderData(order);
            //report.FillAccountSpecificFields();
            //report.OrderRemainingAmount = order.RemainingAmount >= 0 ? order.RemainingAmount : default(double?);
            //report.OrderMaxVisibleAmount = order.MaxVisibleAmount;

            //if (acc is MarginAccountModel)
            //{
            //    MarginAccountModel mAcc = (MarginAccountModel)acc;
            //    report.FillAccountBalanceConversionRates(mAcc.BalanceCurrency, mAcc.Balance);
            //}

            OpenOrder(order.SymbolInfo, OrderInfo.Types.Type.Limit, order.Info.Side, order.Info.RemainingAmount, null, order.Info.Price,
                order.Info.StopPrice, order.Info.StopLoss, order.Info.TakeProfit, order.Info.Comment, order.Info.Options.ToOrderExecOptions(), order.Info.UserTag, order.Info.Expiration.ToDateTime(), OpenOrderOptions.SkipDealing);
        }

        private void ClosePosition(OrderAccessor position, TradeReportInfo.Types.Reason trReason, double? reqAmount, double? reqPrice,
            double? amount, double? price, SymbolInfo smb, ClosePositionOptions options, string posById = null)
        {
            ISymbolCalculator fCalc = position.Info.Calculator;

            // normalize amount
            var actualCloseAmount = NormalizeAmount(amount, position.Info.RemainingAmount);

            bool partialClose = actualCloseAmount < position.Info.RemainingAmount;
            bool nullify = (options & ClosePositionOptions.Nullify) != 0;
            bool reopenRemaining = (options & ClosePositionOptions.ReopenRemaining) != 0;
            bool dropCommission = (options & ClosePositionOptions.DropCommision) != 0;

            // profit & closePrice
            double closePrice;
            double profit;

            if (nullify)
            {
                closePrice = price.Value;
                profit = 0;
            }
            else if ((price != null) && (price.Value > 0))
            {
                closePrice = price.Value;
                var profitRes = fCalc.Profit.Calculate(new ProfitRequest(position.Info.Price ?? 0, actualCloseAmount, position.Info.Side, closePrice));
                profit = RoundMoney(profitRes.Value, _calcFixture.RoundingDigits);
                //profit = RoundMoney(((OrderCalculator)fCalc).CalculateProfitInternal(position.Info.Price ?? 0, (double)actualCloseAmount, closePrice, position.Info.Side, out _), _calcFixture.RoundingDigits);
            }
            else
            {
                // calculator must be another
                // profit = RoundMoney(fCalc.CalculateProfit(position.Price, (double)actualCloseAmount, position.Side, out var error), _calcFixture.RoundingDigits);
                var profitRes = fCalc.Profit.Calculate(new ProfitRequest(position.Info.Price ?? 0, actualCloseAmount, position.Info.Side));
                profit = RoundMoney(profitRes.Value, _calcFixture.RoundingDigits);
                closePrice = (position.Info.Side.IsBuy() ? fCalc.SymbolInfo.Bid : fCalc.SymbolInfo.Ask) ?? -1.0; // can't calculate close price
            }

            //position.CloseConversionRate = profit >= 0 ? fCalc.PositiveProfitConversionRate.Value : fCalc.NegativeProfitConversionRate.Value;

            position.Entity.ClosePrice = closePrice;
            position.Info.LastFillPrice = closePrice;
            position.Info.LastFillAmount = actualCloseAmount;

            //if (managerComment != null)
            //    position.ManagerComment = managerComment;

            // Calculate commission & swap.

            //position.IsReducedCloseCommission = position.IsReducedCloseCommission && trReason == Domain.TradeReportInfo.Types.Reason.TakeProfitAct;

            var charges = new TradeChargesInfo();
            charges.CurrencyInfo = _acc.BalanceCurrencyInfo;

            // two-way commission is no longer used on TTS
            //CommisionEmulator.OnGrossPositionClosed(position, actualCloseAmount, smb, charges, _calcFixture);

            if (partialClose)
            {
                var newRemainingAmount = position.Info.RemainingAmount - actualCloseAmount;
                var k = newRemainingAmount / position.Info.RemainingAmount;

                position.Info.ChangeRemaningAmount(newRemainingAmount);
                //position.Status = OrderStatuses.Calculated;

                var swap = position.Info.Swap;
                if (!swap.E(0))
                {
                    var remainingSwap = k * swap;
                    charges.Swap = RoundMoney(swap - remainingSwap, _calcFixture.RoundingDigits);
                    position.Info.SetSwap(RoundMoney(remainingSwap, _calcFixture.RoundingDigits));
                }

                var commission = position.Info.Commission;
                if (!commission.E(0))
                {
                    var remainingCommission = k * commission;
                    charges.Commission = RoundMoney(commission - remainingCommission, _calcFixture.RoundingDigits);
                    position.Info.ChangeCommission(RoundMoney(remainingCommission, _calcFixture.RoundingDigits));
                }
            }
            else
            {
                charges.Commission = position.Info.Commission;
                charges.Swap = position.Info.Swap;
                position.Info.ChangeRemaningAmount(0);
            }

            //if (trReason == Domain.TradeReportInfo.Types.Reason.Rollover)
            //    CommissionStrategy.OnRollover(position, actualCloseAmount, charges, acc);
            //else
            //    CommissionStrategy.OnPositionClosed(position, actualCloseAmount, charges, acc);)

            if (dropCommission)
            {
                charges.Commission = 0;
                //charges.AgentCommission = 0;
                //charges.MinCommissionCurrency = null;
                //charges.MinCommissionConversionRate = null;
                position.Info.ChangeCommission(0);
                //position.AgentCommision = null;
            }

            bool remove = (!partialClose || reopenRemaining);

            // Remove remaining order / reset order activation.
            if (remove)
            {
                //position.Status = OrderStatuses.Filled;
                _acc.Orders.Remove(position.Info);
                UnregisterOrder(position);
            }
            //else
            //    ResetOrderActivation(position);

            // Reopen position with remaining amount.
            if (partialClose && reopenRemaining)
                CreatePosition(trReason, position, position.Info.Side, smb, position.Info.Price ?? 0, position.Info.RemainingAmount, false);

            // change balance
            var totalProfit = charges.Total + profit;
            _acc.Balance += totalProfit;

            // Update modify timestamp.
            position.Info.Modified = _scheduler.UnsafeVirtualTimestamp;

            var historyAmount = nullify ? 0 : actualCloseAmount;

            // Update comment for trade history entry.
            switch (trReason)
            {
                //case Domain.TradeReportInfo.Types.Reason.StopOut:
                //    position.UserComment = "[Stopout: MarginLevel = " + double.Round(acc.MarginLevel, 2) + ", Margin = " + double.Round(acc.Margin, acc.RoundingDigits) + ", Equity = " + double.Round(acc.Equity, acc.RoundingDigits) + "] " + position.UserComment;
                //    break;
                case TradeReportInfo.Types.Reason.TakeProfitActivation:
                    reqAmount = actualCloseAmount;
                    reqPrice = position.Info.TakeProfit;
                    //position.UserComment = "[TP] " + position.UserComment;
                    break;
                case TradeReportInfo.Types.Reason.StopLossActivation:
                    reqAmount = actualCloseAmount;
                    reqPrice = position.Info.StopLoss;
                    //position.UserComment = "[SL] " + position.UserComment;
                    break;
            }

            var trTime = _scheduler.UnsafeVirtualTimePoint;

            // update trade history
            var tReport = _history.Create(trTime, smb, TradeReportInfo.Types.ReportType.PositionClosed, trReason)
                .FillGenericOrderData(_calcFixture, position)
                .FillClosePosData(position, trTime, actualCloseAmount, closePrice, reqAmount, reqPrice, posById)
                .FillCharges(charges.Commission, charges.Swap, profit, totalProfit)
                .FillProfitConversionRates(_acc.BalanceCurrency, profit, _calcFixture)
                .FillAccountBalanceConversionRates(_calcFixture, _acc.BalanceCurrency, _acc.Balance)
                .FillAccountSpecificFields(_calcFixture);

            _history.Add(tReport);

            var orderCopy = position.Clone();

            //// journal
            //string jPrefix = remove ? "Closed " : "Partially Closed";
            //LogTransactionDetails(() => jPrefix + " #" + position.OrderId + ", symbol=" + smb.Name + " price=" + closePrice + " amount=" + historyAmount
            //                            + " remaining=" + position.RemainingAmount + " profit=" + profit + " charges=" + charges.Total + " totalProfit=" + totalProfit + " reason=" + trReason,
            //    JournalEntrySeverities.Info, orderCopy);
            //LogTransactionDetails(() => "Final position " + orderCopy, JournalEntrySeverities.Info, orderCopy);

            //if (!remove)
            //    position.FireChanged();

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
            position.Entity.ActionNo++;

            // execution report
            if (_sendReports)
                _context.SendExtUpdate(TesterTradeTransaction.OnClosePosition(remove, position, (double)_acc.Balance));

            // summary
            _opSummary.AddGrossCloseAction(position, profit, closePrice, charges, _acc.BalanceCurrencyInfo);
            _collector.OnPositionClosed(_scheduler.UnsafeVirtualTimePoint, (double)profit, (double)charges.Commission, (double)charges.Swap);

            //return profitInfo;
        }

        public void ConfirmPositionCloseBy(OrderAccessor position1, OrderAccessor position2, TradeReportInfo.Types.Reason trReason, bool usePartialClosing)
        {
            var smb = position1.SymbolInfo;
            ISymbolCalculator fCalc = position1.Info.Calculator;

            var closeAmount = Math.Min(position1.Info.RemainingAmount, position2.Info.RemainingAmount);

            if (position1.Info.RemainingAmount < position2.Info.RemainingAmount)
                Ref.Swap(ref position1, ref position2);

            // journal
            //LogTransactionDetails(() => "Confirmed Close #" + position1.OrderId + " By #" + position2.OrderId + " amount=" + closeAmount + ", reason=" + trReason, JournalEntrySeverities.Info);

            ClosePositionOptions pos1options = 0;
            ClosePositionOptions pos2options = 0;

            if (!usePartialClosing)
                pos1options |= ClosePositionOptions.ReopenRemaining;

            //if (grSecurity.CloseByMod == CloseByModifications.AllByCurrentPrice || acc.AccountingType == AccountingTypes.Net)
            //{
            //    double closeByPrice = fCalc.CurrentRate.Ask;
            //    ClosePosition(isExecutedAsClient, position1, acc, trReason, null, null, closeAmount, closeByPrice, smb, pos1options, clientRequestId, managerComment, true, position2.OrderId);
            //    ClosePosition(isExecutedAsClient, position2, acc, trReason, null, null, closeAmount, closeByPrice, smb, pos2options, clientRequestId, managerComment, true, position1.OrderId);
            //}
            //else
            //{
            pos2options |= ClosePositionOptions.Nullify;
            pos2options |= ClosePositionOptions.DropCommision;
            ClosePosition(position1, trReason, null, null, closeAmount, (double)position2.Info.Price, smb, pos1options, position2.Info.Id);
            ClosePosition(position2, trReason, null, null, closeAmount, (double)position2.Info.Price, smb, pos2options, position1.Info.Id);
            //}
        }

        private void CloseAllPositions(TradeReportInfo.Types.Reason reason)
        {
            var toClose = _acc.Orders.Values.Where(o => o.Info.Type == OrderInfo.Types.Type.Position).ToList();

            if (toClose.Count > 0)
            {
                _collector.LogTrade($"Closing {toClose.Count} positions remaining after startegy stopped.");

                foreach (var order in toClose)
                {
                    using (JournalScope())
                        ClosePosition(order, reason, null, null, null, null, order.SymbolInfo, ClosePositionOptions.None);
                }
            }

            var netPosToClose = _acc.NetPositions.Values.ToList();

            if (netPosToClose.Count > 0)
            {
                _collector.LogTrade($"Closing {netPosToClose.Count} positions remaining after startegy stopped.");

                foreach (var pos in netPosToClose)
                {
                    using (JournalScope())
                    {
                        var smbInfo = _context.Builder.Symbols.GetOrNull(pos.Info.Symbol).Info;
                        OpenOrder(smbInfo, OrderInfo.Types.Type.Market, pos.Info.Side.Revert(), pos.Info.Volume, null, null, null, null, null, "",
                            Domain.OrderExecOptions.None, null, null, OpenOrderOptions.SkipDealing | OpenOrderOptions.FakeOrder);
                    }
                }
            }
        }

        private void CancelAllPendings(TradeReportInfo.Types.Reason reason)
        {
            var toCancel = _acc.Orders.Values.Where(o => o.Info.Type.IsPending()).ToList();

            if (toCancel.Count > 0)
            {
                _collector.LogTrade($"Cancelling {toCancel.Count} orders remaining after startegy stopped.");

                foreach (var order in toCancel)
                {
                    using (JournalScope())
                        CancelOrder(order, reason);
                }
            }
        }

        private void CheckExpiration()
        {
            var expiredOrders = _expirationManager.GetExpiredOrders(this.ExecutionTime);

            if (expiredOrders != null)
            {
                foreach (var order in expiredOrders)
                {
                    //Logger.Debug(() => "Order id=" + order.OrderId + " has been expired.");
                    ConfirmOrderCancelation(TradeReportInfo.Types.Reason.Expired, order);
                    //MarkAsAffected(order.Account);
                }
            }
        }

        private async void ExpirationCheckLoop()
        {
            while (_expirationManager.Count > 0)
            {
                await _scheduler.EmulateAsyncDelay(TimeSpan.FromSeconds(1), true);

                CheckExpiration();
            }
        }

        private void RegisterForExpirationCheck(OrderAccessor order)
        {
            bool startLoop = _expirationManager.Count == 0;
            if (_expirationManager.AddOrder(order) && startLoop)
                ExpirationCheckLoop();
        }

        #endregion

        #region Rollover & Swap

        private void Rollover()
        {
            bool updated = false;
            double totalSwap = 0;
            int affectedSymbolsCount = 0;

            foreach (SymbolAccessor info in _context.Builder.Symbols)
            {
                if (info.Info.Swap.Enabled && info.Info.LastQuote != null)// && _scheduler.UnsafeVirtualTimePoint - info.Info.LastQuote.Time <= TimeSpan.FromHours(1))
                {
                    var symbolCalc = _context.MarketData.GetCalculator(info.Info);
                    double swapAmount = 0;

                    if (_acc.Type == AccountInfo.Types.Type.Gross)
                    {
                        if (UpdateGrossSwaps(symbolCalc, out swapAmount))
                            updated = true;
                    }
                    else if (_acc.Type == AccountInfo.Types.Type.Net)
                    {
                        if (UpdateNetSwaps(symbolCalc, out swapAmount))
                            updated = true;
                    }

                    totalSwap += swapAmount;
                    affectedSymbolsCount++;
                }
            }

            if (updated)
                RecalculateAccount();

            if (affectedSymbolsCount > 0)
                _collector.LogTrade("Rollover, totalSwap=" + totalSwap.FormatPlain(_acc.BalanceCurrencyFormat));
        }

        public bool UpdateGrossSwaps(ISymbolCalculator symbolCalc, out double totalSwap)
        {
            bool swapUpdated = false;
            totalSwap = 0;

            var positions = _acc.Orders.Values.Where(o => o.Info.Type == OrderInfo.Types.Type.Position && o.Info.Symbol == symbolCalc.SymbolInfo.Name).ToList(); // Perf. warning: .ToList()

            if (positions != null)
            {
                foreach (OrderAccessor order in positions)
                {
                    var calc = order.Info.Calculator;

                    var swapRes = calc.Swap.Calculate(order.Info, ExecutionTime);
                    //double swap = 0.0;/*((OrderCalculator)order.Info.Calculator).CalculateSwap((double)order.Info.RemainingAmount, order.Info.Side, ExecutionTime, out var error);*/

                    if (swapRes.Error != CalculationError.None)
                    {
                        //LogTransactionDetails(() => $"Swap not charged: account={acc.AccountLogin} symbol={smbInfo.Name} volume={order.RemainingAmount} reason={ex.CalcError}. {ex.Message}",
                        //JournalEntrySeverities.Error, TransactDetails.Create(order.OrderId, null), acc.SkipLogging);
                        return swapUpdated;
                    }

                    var roundedSwap = RoundMoney(swapRes.Value, _acc.BalanceCurrencyInfo.Digits);

                    if (roundedSwap != 0)
                    {
                        order.Info.SetSwap(order.Info.Swap + roundedSwap);
                        //LogTransactionDetails(() => $"Swap charged: account={acc.AccountLogin} symbol={order.Symbol} side={order.Side} volume={order.RemainingAmount:G29} charged={swap:G29} total={order.Swap:G29} currency={acc.BalanceCurrency}",
                        //    JournalEntrySeverities.Info, TransactDetails.Create(order.OrderId, null), acc.SkipLogging);

                        // execution report
                        if (_sendReports)
                            _context.SendExtUpdate(TesterTradeTransaction.OnRolloverUpdate(order));

                        swapUpdated = true;
                    }

                    totalSwap += roundedSwap;
                }
            }

            return swapUpdated;
        }

        public bool UpdateNetSwaps(ISymbolCalculator symbolCalc, out double totalSwap)
        {
            totalSwap = 0;

            PositionAccessor pos = _acc.NetPositions.GetOrNull(symbolCalc.SymbolInfo.Name);

            if (pos != null)
            {
                var swapRes = symbolCalc.Swap.Calculate(pos.Info, ExecutionTime);
                //double swap = ((OrderCalculator)pos.Info.Calculator).CalculateSwap((double)pos.Long.Amount, OrderInfo.Types.Side.Buy, ExecutionTime, out error)
                //               + ((OrderCalculator)pos.Info.Calculator).CalculateSwap((double)pos.Short.Amount, OrderInfo.Types.Side.Sell, ExecutionTime, out error);

                if (swapRes.Error != CalculationError.None)
                    return false;

                //if (error != CalcErrorCodes.None)
                //{
                //Func<string> errMsg = () => $"Swap not charged: account={acc.AccountLogin} side={netPos.Side} symbol={smbInfo.Name} volume={netPos.Amount:G29} reason={ex.CalcError}. {ex.Message}";
                //LogTransactionDetails(errMsg, JournalEntrySeverities.Error, TransactDetails.Create(netPos.Id, null), acc.SkipLogging);
                //return false;
                //}

                double roundedSwap = RoundMoney(swapRes.Value, _acc.BalanceCurrencyInfo.Digits);

                if (roundedSwap != 0)
                {
                    pos.Info.Swap += (double)roundedSwap;

                    totalSwap += roundedSwap;

                    //LogTransactionDetails(() => $"Swap charged: account={acc.AccountLogin} symbol={smbInfo.Name} side={netPos.Side} volume={netPos.Amount:G29} charged={roundedSwap:G29} total={netPos.Swap:G29} currency={acc.BalanceCurrency}",
                    //    JournalEntrySeverities.Info, TransactDetails.Create(netPos.Id, null), acc.SkipLogging);

                    // execution report
                    if (_sendReports)
                        _context.SendExtUpdate(TesterTradeTransaction.OnRolloverUpdate(pos));

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Price Logic

        private double? GetCurrentOpenPrice(OrderAccessor order, IRateInfo currentRate = null)
        {
            if (currentRate == null)
                currentRate = (QuoteInfo)_calcFixture.GetCurrentRateOrNull(order.Info.Symbol);

            return GetCurrentOpenPrice(order.Info.Side, currentRate);
        }

        private double? GetCurrentOpenPrice(OrderInfo.Types.Side side, string smb)
        {
            IRateInfo currentRate = (QuoteInfo)_calcFixture.GetCurrentRateOrNull(smb);
            return GetCurrentOpenPrice(side, currentRate);
        }

        private double? GetCurrentOpenPrice(OrderInfo.Types.Side side, IRateInfo currentRate)
        {
            return GetOpenOrderPrice(currentRate, side);
        }

        private double? GetCurrentClosePrice(IOrderCalcInfo order, IRateInfo currentRate = null)
        {
            if (currentRate == null)
                currentRate = (QuoteInfo)_calcFixture.GetCurrentRateOrNull(order.Symbol);

            return GetPositionClosePrice(currentRate, order.Side);
        }

        private static double? GetPositionClosePrice(IRateInfo tick, OrderInfo.Types.Side positionSide)
        {
            if (tick == null)
                return null;

            if (positionSide == OrderInfo.Types.Side.Buy)
                return tick.NullableBid();
            else if (positionSide == OrderInfo.Types.Side.Sell)
                return tick.NullableAsk();

            throw new Exception("Unknown order side: " + positionSide);
        }

        private static double? GetOpenOrderPrice(IRateInfo tick, OrderInfo.Types.Side orderSide)
        {
            if (tick == null)
                return null;

            if (orderSide == OrderInfo.Types.Side.Buy)
                return tick.NullableAsk();
            if (orderSide == OrderInfo.Types.Side.Sell)
                return tick.NullableBid();

            //try
            //{

            //}
            //catch (Exception)
            //{
            //    throw new OrderValidationError("Can not get open price for " + orderSide + " " + tick.Symbol + " order!", OrderCmdResultCodes.OffQuotes);
            //}

            throw new Exception("Unknown order side: " + orderSide);
        }

        #endregion Price Logic

        #region Amount logic

        private static double NormalizeAmount(double? requestedAmount, double remainingAmount)
        {
            if (requestedAmount == null || requestedAmount.Value > remainingAmount)
                return remainingAmount;

            return requestedAmount.Value;
        }

        #endregion

        #region Rounding

        public double RoundMoney(double rawValue, int? roundDigits)
        {
            if (roundDigits == null)
                return rawValue;
            else
                return rawValue.FloorBy(roundDigits.Value);
        }

        public double? RoundMoney(double? rawValue, int? roundDigits)
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
        //        ValidatePrice((double)limitPrice, symbol);
        //    else if (((type == OrderType.Stop) || (type == OrderType.StopLimit)) && stopPrice != null)
        //        ValidatePrice((double)stopPrice, symbol);
        //}

        private void ValidateLimitPrice(double? price, SymbolInfo smbInfo)
        {
            if (price == null || price <= 0.0)
                throw new OrderValidationError("Price not specified.", OrderCmdResultCodes.IncorrectPrice);

            ValidatePrice((double)price, smbInfo);
        }

        private void ValidateStopPrice(double? stopPrice, SymbolInfo smbInfo)
        {
            if (stopPrice == null || stopPrice <= 0.0)
                throw new OrderValidationError("Stop price not specified.", OrderCmdResultCodes.IncorrectStopPrice);

            ValidatePrice((double)stopPrice, smbInfo);
        }

        private void ValidatePrice(double price, SymbolInfo smbInfo)
        {
            if (price.IsPrecisionGreater(smbInfo.Digits))
                throw new OrderValidationError("Price precision is more than symbol digits.", OrderCmdResultCodes.IncorrectPrice);
        }

        private void ValidateOrderTypeForAccount(OrderType orderType, SymbolInfo symbolInfo)
        {
            var currentQuote = _calcFixture.GetCurrentRateOrNull(symbolInfo.Name);
            if (currentQuote == null)
            {
                if ((_acc.Type != AccountInfo.Types.Type.Cash) || (orderType == OrderType.Market))
                    throw new OrderValidationError("No quote for symbol " + symbolInfo.Name, OrderCmdResultCodes.OffQuotes);
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

        private void ValidateTypeAndPrice(OrderType orderType, double? price, double? stopPrice, double? sl, double? tp, double? maxVisibleVolume, Api.OrderExecOptions options, SymbolInfo symbol)
        {
            if ((orderType != OrderType.Limit) && (orderType != OrderType.Market) && (orderType != OrderType.Stop) && (orderType != OrderType.StopLimit))
                throw new OrderValidationError("Invalid order type.", OrderCmdResultCodes.Unsupported);

            if ((_acc.Type == AccountInfo.Types.Type.Cash) &&
                ((orderType == OrderType.Limit) || (orderType == OrderType.Stop) || (orderType == OrderType.StopLimit)) &&
                (sl.HasValue || tp.HasValue))
                throw new OrderValidationError("SL/TP is not supported by pending order for cash account!", OrderCmdResultCodes.Unsupported);

            if (orderType == OrderType.Market)
            {
                //if (Request.IsOptionSet(OrderExecutionOptions.MarketWithSlippage))
                //    throw new OrderValidationError("'MarketWithSlippage' flag is not supported for market orders", FaultCodes.InvalidOption);
                if (options.HasFlag(Api.OrderExecOptions.ImmediateOrCancel))
                    throw new OrderValidationError("'ImmediateOrCancel' flag is not supported for market orders", OrderCmdResultCodes.Unsupported);
                //if (Request.IsOptionSet(OrderExecutionOptions.HiddenIceberg))
                //    throw new OrderValidationError("'HiddenIceberg' flag is not supported for market orders", FaultCodes.InvalidOption);
                //if (Request.IsOptionSet(OrderExecutionOptions.FillOrKill))
                //    throw new OrderValidationError("'FillOrKill' flag is not supported for market orders", FaultCodes.InvalidOption);

                if (maxVisibleVolume.HasValue)
                    throw new OrderValidationError("Max visible amount is not valid for market orders", OrderCmdResultCodes.MaxVisibleVolumeNotSupported);

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
                if (options.HasFlag(Api.OrderExecOptions.ImmediateOrCancel))
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

        //private void ValidateVolumeLots(double? volumeLots, Symbol smbMetadata)
        //{
        //    if (!volumeLots.HasValue)
        //        return;

        //    if (volumeLots <= 0 || volumeLots < smbMetadata.MinTradeVolume || volumeLots > smbMetadata.MaxTradeVolume)
        //        throw new OrderValidationError(OrderCmdResultCodes.IncorrectVolume);
        //}

        private void VerifyCloseAmout(double? amount, SymbolInfo smbInfo)
        {
            if (!amount.HasValue)
                return;

            VerifyAmout(amount.Value, smbInfo);
        }

        private void VerifyAmout(double amount, SymbolInfo smbInfo)
        {
            //if (double.IsNaN(amount))
            //    throw new OrderValidationError("Invalid Amount", OrderCmdResultCodes.IncorrectVolume);

            var minTradeAmount = smbInfo.MinTradeVolume * smbInfo.LotSize;
            var maxTradeAmount = smbInfo.MaxTradeVolume * smbInfo.LotSize;
            if (amount < minTradeAmount || amount > maxTradeAmount) // || !CheckAmountBase(amount, smbInfo))
                throw new OrderValidationError("Invalid Amount", OrderCmdResultCodes.IncorrectVolume);
        }

        //private bool CheckAmountBase(double amount, Symbol smbInfo)
        //{
        //    double minAmountStep = smbInfo.TradeVolumeStep * smbInfo.ContractSize;
        //    double div = amount / minAmountStep;
        //    return div.E(Math.Round(div));
        //}

        private void EnsureOrderIsPosition(OrderAccessor order)
        {
            if (order.Info.Type != OrderInfo.Types.Type.Position)
                throw new OrderValidationError("Position #" + order.Info.Id + " was not found.", OrderCmdResultCodes.OrderNotFound);
        }

        #endregion

        #region Volume Logic

        private double RoundVolume(double volumeInLots, SymbolInfo smbMetadata)
        {
            var decVal = volumeInLots;
            var decStep = smbMetadata.TradeVolumeStep;

            return decVal.FloorToStep(decStep);
        }

        private double? RoundVolume(double? volumeInLots, SymbolInfo smbMetadata)
        {
            if (volumeInLots == null)
                return null;

            return RoundVolume(volumeInLots.Value, smbMetadata);
        }

        private static double RoundPrice(double price, SymbolInfo smbMetadata, OrderInfo.Types.Side side)
        {
            return side == OrderInfo.Types.Side.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        private static double? RoundPrice(double? price, SymbolInfo smbMetadata, OrderInfo.Types.Side side)
        {
            return side == OrderInfo.Types.Side.Buy ? price.Ceil(smbMetadata.Digits) : price.Floor(smbMetadata.Digits);
        }

        private double? ToUnits(double? volumeInLots, SymbolInfo smbMetadata)
        {
            if (volumeInLots == null)
                return null;
            return ToUnits(volumeInLots.Value, smbMetadata);
        }

        private double ToUnits(double volumeInLots, SymbolInfo smbMetadata)
        {
            return smbMetadata.LotSize * volumeInLots;
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

                _collector.AddEvent(_opSummary.Severity, record);
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

        //private void SendTradeUpdate(OrderEntity order, double balance, PositionEntity pos = null, AssetEntity asset1 = null, AssetEntity asset2 = null)
        //{
        //    if (_sendReports)
        //    {
        //        var update = new TesterTradeTransaction();
        //        update.OrderUpdate1 = order;
        //        update.Balance = balance;
        //        update.NetPositionUpdate = pos;
        //        update.AssetUpdate1 = asset1;
        //        update.AssetUpdate2 = asset2;

        //        _context.SendExtUpdate(order);
        //    }
        //}
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
        Stopout = 0x02,
        FakeOrder = 0x04
    }
}
