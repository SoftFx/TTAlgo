using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.FDK.Common;
using SFX = TickTrader.FDK.Common;
using API = TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using ActorSharp;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model
{
    internal class SfxInterop : IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private const int ConnectTimeoutMs = 60 * 1000;
        private const int LogoutTimeoutMs = 60 * 1000;
        private const int DisconnectTimeoutMs = 60 * 1000;
        private const int DownloadTimeoutMs = 120 * 1000;

        private static IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<SfxInterop>();

        public IFeedServerApi FeedApi => this;
        public ITradeServerApi TradeApi => this;

        public bool AutoAccountInfo => false;
        public bool AutoSymbols => false;

        private FDK.Client.QuoteFeed _feedProxy;
        private FDK.Client.QuoteStore _feedHistoryProxy;
        private FDK.Client.OrderEntry _tradeProxy;
        private FDK.Client.TradeCapture _tradeHistoryProxy;

        private Fdk2TradeAdapter _tradeProxyAdapter;

        private AppType _appType;

        public event Action<IServerInterop, ConnectionErrorInfo> Disconnected;

        public SfxInterop(ConnectionOptions options)
        {
            const int connectInterval = 10000;
#if DEBUG
            const int heartbeatInterval = 120000;
            var logEvents = options.EnableLogs;
            var logStates = options.EnableLogs;
            var logMessages = options.EnableLogs;
#else
            const int heartbeatInterval = 10000;
            var logEvents = false;
            var logStates = false;
            var logMessages = options.EnableLogs;
#endif
            const int connectAttempts = 1;
            const int reconnectAttempts = 0;

            var logsDir = options.LogsFolder;

            _feedProxy = new FDK.Client.QuoteFeed("feed.proxy", logEvents, logStates, logMessages, port: 5041, validateClientCertificate: ValidateCertificate,
                connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir);
            _feedHistoryProxy = new FDK.Client.QuoteStore("feed.history.proxy", logEvents, logStates, logMessages, port: 5042, validateClientCertificate: ValidateCertificate,
                connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir);
            _tradeProxy = new FDK.Client.OrderEntry("trade.proxy", logEvents, logStates, logMessages, port: 5043, validateClientCertificate: ValidateCertificate,
                connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir);
            _tradeHistoryProxy = new FDK.Client.TradeCapture("trade.history.proxy", logEvents, logStates, logMessages, port: 5044, validateClientCertificate: ValidateCertificate,
                connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir);

            _feedProxy.InitTaskAdapter();
            _feedHistoryProxy.InitTaskAdapter();
            _tradeHistoryProxy.InitTaskAdapter();

            _tradeProxyAdapter = new Fdk2TradeAdapter(_tradeProxy, rep => ExecutionReport?.Invoke(ConvertToEr(rep)));

            _feedProxy.QuoteUpdateEvent += (c, q) => Tick?.Invoke(Convert(q));
            _feedProxy.LogoutEvent += (c, m) => OnLogout(m);
            _feedProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeProxy.LogoutEvent += (c, m) => OnLogout(m);
            _tradeProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeProxy.OrderUpdateEvent += (c, rep) => ExecutionReport?.Invoke(ConvertToEr(rep));
            _tradeProxy.PositionUpdateEvent += (c, rep) => PositionReport?.Invoke(Convert(rep));
            _tradeProxy.BalanceUpdateEvent += (c, rep) => BalanceOperation?.Invoke(Convert(rep));
            _tradeHistoryProxy.LogoutEvent += (c, m) => OnLogout(m);
            _tradeHistoryProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeHistoryProxy.TradeUpdateEvent += (c, rep) => TradeTransactionReport?.Invoke(Convert(rep));
            _feedHistoryProxy.LogoutEvent += (c, m) => OnLogout(m);
            _feedHistoryProxy.DisconnectEvent += (c, m) => OnDisconnect(m);

            _appType = options.Type;
        }

        public async Task<ConnectionErrorInfo> Connect(string address, string login, string password, CancellationToken cancelToken)
        {
            try
            {
                await Task.WhenAll(
                    ConnectFeed(address, login, password),
                    ConnectTrade(address, login, password),
                    ConnectFeedHistory(address, login, password),
                    ConnectTradeHistory(address, login, password))
                    .AddCancelation(cancelToken);
            }
            catch (AggregateException aex)
            {
                var ex = aex.InnerExceptions.First();
                if (ex is LoginException)
                {
                    var code = ((LoginException)ex).LogoutReason;
                    return new ConnectionErrorInfo(Convert(code), ex.Message);
                }
                if (ex is ConnectException)
                {
                    return new ConnectionErrorInfo(ConnectionErrorCodes.NetworkError, ex.Message);
                }

                return new ConnectionErrorInfo(ConnectionErrorCodes.Unknown, ex.Message);
            }
            catch (Exception ex)
            {
                return new ConnectionErrorInfo(ConnectionErrorCodes.Unknown, ex.Message);
            }

            return new ConnectionErrorInfo(ConnectionErrorCodes.None);
        }

        private async Task ConnectFeed(string address, string login, string password)
        {
            logger.Debug("Feed: Connecting...");
            await _feedProxy.ConnectAsync(address);
            logger.Debug("Feed: Connected.");

            await _feedProxy.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
            logger.Debug("Feed: Logged in.");
        }

        private async Task ConnectTrade(string address, string login, string password)
        {
            logger.Debug("Trade: Connecting...");
            await _tradeProxyAdapter.ConnectAsync(address);
            logger.Debug("Trade: Connected.");
            await _tradeProxyAdapter.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
            logger.Debug("Trade logged in.");
        }

        private async Task ConnectFeedHistory(string address, string login, string password)
        {
            logger.Debug("Feed.History: Connecting...");
            await _feedHistoryProxy.ConnectAsync(address);
            logger.Debug("Feed.History: Connected.");
            await _feedHistoryProxy.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
            logger.Debug("Feed.History: Logged in.");
        }

        private async Task ConnectTradeHistory(string address, string login, string password)
        {
            logger.Debug("Trade.History: Connecting...");
            await _tradeHistoryProxy.ConnectAsync(address);
            logger.Debug("Trade.History: Connected.");
            await _tradeHistoryProxy.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
            logger.Debug("Trade.History: Logged in.");
            await _tradeHistoryProxy.SubscribeTradesAsync(false);
            logger.Debug("Trade.History: Subscribed.");
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // TODO: Add message indicating that certificate is not valid
            return true;
        }

        private void OnLogout(LogoutInfo info)
        {
            Disconnected?.Invoke(this, new ConnectionErrorInfo(Convert(info.Reason), info.Message));
        }

        private void OnDisconnect(string text)
        {
            Disconnected?.Invoke(this, new ConnectionErrorInfo(ConnectionErrorCodes.Unknown, text));
        }

        public Task Disconnect()
        {
            return Task.WhenAll(
                DisconnectFeed(),
                DisconnectTrade(),
                DisconnectFeedHstory(),
                DisconnectTradeHstory());
        }

        private async Task DisconnectFeed()
        {
            logger.Debug("Feed: Disconnecting...");
            try
            {
                await _feedProxy.LogoutAsync("");
                logger.Debug("Feed: Logged out.");
                await _feedProxy.DisconnectAsync("");
            }
            catch (Exception) { }

            await Task.Factory.StartNew(() => _feedProxy.Dispose());

            logger.Debug("Feed: Disconnected.");
        }

        private async Task DisconnectFeedHstory()
        {
            logger.Debug("Feed.History: Disconnecting...");
            try
            {
                await _feedHistoryProxy.LogoutAsync("");
                logger.Debug("Feed.History: Logged out.");
                await _feedHistoryProxy.DisconnectAsync("");
            }
            catch (Exception) { }

            await Task.Factory.StartNew(() => _feedHistoryProxy.Dispose());

            logger.Debug("Feed.History: Disconnected.");
        }

        private async Task DisconnectTrade()
        {
            logger.Debug("Trade: Disconnecting...");
            try
            {
                await _tradeProxyAdapter.LogoutAsync("");
                logger.Debug("Trade: Logged out.");
                await _tradeProxyAdapter.DisconnectAsync("");
            }
            catch (Exception) { }

            await Task.Factory.StartNew(() => _tradeProxy.Dispose());

            logger.Debug("Trade: Disconnected.");
        }

        private async Task DisconnectTradeHstory()
        {
            logger.Debug("Trade.History: Disconnecting...");
            try
            {
                await _tradeHistoryProxy.LogoutAsync("");
                logger.Debug("Trade.History: Logged out.");
                await _tradeHistoryProxy.DisconnectAsync("");
            }
            catch (Exception) { }

            await Task.Factory.StartNew(() => _tradeHistoryProxy.Dispose());

            logger.Debug("Trade.History: Disconnected.");
        }

        #region IFeedServerApi

        public event Action<QuoteEntity> Tick;

        public async Task<CurrencyEntity[]> GetCurrencies()
        {
            var currencies = await _feedProxy.GetCurrencyListAsync();
            return currencies.Select(Convert).ToArray();
        }

        public async Task<SymbolEntity[]> GetSymbols()
        {
            var symbols = await _feedProxy.GetSymbolListAsync();
            return symbols.Select(Convert).ToArray();
        }

        public Task<QuoteEntity[]> SubscribeToQuotes(string[] symbols, int depth)
        {
            return _feedProxy.SubscribeQuotesAsync(symbols, depth);
        }

        public async Task<QuoteEntity[]> GetQuoteSnapshot(string[] symbols, int depth)
        {
            var array = await _feedProxy.GetQuotesAsync(symbols, depth);
            return array.Select(Convert).ToArray();
        }

        public void DownloadBars(BlockingChannel<BarEntity> stream, string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var e = _feedHistoryProxy.DownloadBars(symbol, ConvertBack(priceType), ToBarPeriod(barPeriod), from, to, DownloadTimeoutMs);
                    DateTime? timeEdge = null;

                    while (true)
                    {
                        var bar = e.Next(DownloadTimeoutMs);

                        if (bar != null)
                        {
                            if (timeEdge == null)
                            {
                                if (bar.From < from)
                                    continue;
                            }
                            else if (bar.From <= timeEdge.Value)
                                continue;

                            timeEdge = bar.From;
                        }

                        if (bar == null || !stream.Write(Convert(bar)))
                        {
                            e.Close();
                            break;
                        }
                    }
                    stream.Close();
                }
                catch (Exception ex)
                {
                    stream.Close(ex);
                }
            });
        }

        public async Task<BarEntity[]> DownloadBarPage(string symbol, DateTime from, int count, BarPriceType priceType, TimeFrames barPeriod)
        {
            var result = new List<BarEntity>();

            try
            {
                var bars = await _feedHistoryProxy.GetBarListAsync(symbol, ConvertBack(priceType), ToBarPeriod(barPeriod), from.ToUniversalTime(), count);
                return bars.Select(Convert).ToArray();
            }
            catch (Exception ex)
            {
                throw new InteropException(ex.Message, ConnectionErrorCodes.NetworkError);
            }
        }

        public void DownloadQuotes(BlockingChannel<QuoteEntity> stream, string symbol, DateTime from, DateTime to, bool includeLevel2)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var e = _feedHistoryProxy.DownloadQuotes(symbol, includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top, from.ToUniversalTime(), to.ToUniversalTime(), DownloadTimeoutMs);

                    while (true)
                    {
                        var tick = e.Next(DownloadTimeoutMs);
                        if (tick == null || !stream.Write(Convert(tick)))
                        {
                            e.Close();
                            break;
                        }
                    }
                    stream.Close();
                }
                catch (Exception ex)
                {
                    stream.Close(ex);
                }
            });
        }

        public async Task<QuoteEntity[]> DownloadQuotePage(string symbol, DateTime from, int count, bool includeLevel2)
        {
            var result = new List<QuoteEntity>();

            try
            {
                var quotes = await _feedHistoryProxy.GetQuoteListAsync(symbol, includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top, from.ToUniversalTime(), count);
                return quotes.Select(Convert).ToArray();
            }
            catch (Exception ex)
            {
                throw new InteropException(ex.Message, ConnectionErrorCodes.NetworkError);
            }
        }

        public async Task<Tuple<DateTime?, DateTime?>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            if (timeFrame.IsTicks())
            {
                var level2 = timeFrame == TimeFrames.TicksLevel2;
                var info = await _feedHistoryProxy.GetQuotesHistoryInfoAsync(symbol, level2);
                return new Tuple<DateTime?, DateTime?>(info.AvailFrom, info.AvailTo);
            }
            else // bars
            {
                var info = await _feedHistoryProxy.GetBarsHistoryInfoAsync(symbol, ToBarPeriod(timeFrame), ConvertBack(priceType));
                return new Tuple<DateTime?, DateTime?>(info.AvailFrom, info.AvailTo);
            }
        }

        #endregion

        #region ITradeServerApi

        public event Action<PositionEntity> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<TradeReportEntity> TradeTransactionReport;
        public event Action<BalanceOperationReport> BalanceOperation;
        public event Action<SymbolEntity[]> SymbolInfo { add { } remove { } }
        public event Action<CurrencyEntity[]> CurrencyInfo { add { } remove { } }

        public Task<AccountEntity> GetAccountInfo()
        {
            return _tradeProxyAdapter.GetAccountInfoAsync()
                .ContinueWith(t => Convert(t.Result));
        }

        public void GetTradeRecords(BlockingChannel<OrderEntity> rxStream)
        {
            _tradeProxy.GetOrdersAsync(rxStream);
        }

        public Task<PositionEntity[]> GetPositions()
        {
            return _tradeProxyAdapter.GetPositionsAsync()
                .ContinueWith(t => t.Result.Select(Convert).ToArray());
        }

        public void GetTradeHistory(BlockingChannel<TradeReportEntity> rxStream, DateTime? from, DateTime? to, bool skipCancelOrders, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            _tradeHistoryProxy.DownloadTradesAsync(direction, from?.ToUniversalTime(), to?.ToUniversalTime(), skipCancelOrders, rxStream);
        }

        public Task<OrderInteropResult> SendOpenOrder(OpenOrderCoreRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                var timeInForce = GetTimeInForce(r.Expiration);
                var ioc = GetIoC(r.Options);

                return _tradeProxyAdapter.NewOrderAsync(r.OperationId, r.Symbol, Convert(r.Type), Convert(r.Side), r.Volume, r.MaxVisibleVolume,
                    r.Price, r.StopPrice, timeInForce, r.Expiration, r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, ioc, r.Slippage);
            });
        }

        public Task<OrderInteropResult> SendCancelOrder(CancelOrderRequest request)
        {
            return ExecuteOrderOperation(request, r => _tradeProxyAdapter.CancelOrderAsync(r.OperationId, "", r.OrderId));
        }

        public Task<OrderInteropResult> SendModifyOrder(ReplaceOrderCoreRequest request)
        {
            if (_tradeProxy.ProtocolSpec.SupportsOrderReplaceQtyChange)
            {
                return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                    r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.VolumeChange,
                    r.MaxVisibleVolume, r.Price, r.StopPrice, GetTimeInForceReplace(r.Options, r.Expiration), r.Expiration,
                    r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, GetIoCReplace(r.Options), r.Slippage));
            }
            return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.NewVolume ?? r.CurrentVolume, r.CurrentVolume,
                r.MaxVisibleVolume, r.Price, r.StopPrice, GetTimeInForceReplace(r.Options, r.Expiration), r.Expiration,
                r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, GetIoCReplace(r.Options), r.Slippage));
        }

        public Task<OrderInteropResult> SendCloseOrder(CloseOrderCoreRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                if (request.ByOrderId != null)
                    return _tradeProxyAdapter.ClosePositionByAsync(r.OperationId, r.OrderId, r.ByOrderId);
                else
                    return _tradeProxyAdapter.ClosePositionAsync(r.OperationId, r.OrderId, r.Volume, r.Slippage);
            });
        }

        private async Task<OrderInteropResult> ExecuteOrderOperation<TReq>(TReq request, Func<TReq, Task<List<SFX.ExecutionReport>>> operationDef)
            where TReq : OrderCoreRequest
        {
            var operationId = request.OperationId;

            try
            {
                var result = await operationDef(request);
                return new OrderInteropResult(OrderCmdResultCodes.Ok, ConvertToEr(result, operationId));
            }
            catch (ExecutionException ex)
            {
                var reason = Convert(ex.Report.RejectReason, ex.Message);
                return new OrderInteropResult(reason, ConvertToEr(ex.Report, operationId));
            }
            catch (DisconnectException)
            {
                return new OrderInteropResult(OrderCmdResultCodes.ConnectionError);
            }
            catch (InteropException ex) when (ex.ErrorCode == ConnectionErrorCodes.RejectedByServer)
            {
                // workaround for inconsistent server logic
                return new OrderInteropResult(Convert(RejectReason.Other, ex.Message));
            }
            catch (NotSupportedException)
            {
                return new OrderInteropResult(OrderCmdResultCodes.Unsupported);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Session is not active"))
                    return new OrderInteropResult(OrderCmdResultCodes.ConnectionError);

                logger.Error(ex);
                return new OrderInteropResult(OrderCmdResultCodes.UnknownError);
            }
        }

        private OrderTimeInForce? GetTimeInForceReplace(OrderExecOptions? options, DateTime? expiration)
        {
            return expiration != null ? OrderTimeInForce.GoodTillDate
                : (options != null ? OrderTimeInForce.GoodTillCancel : (OrderTimeInForce?)null);
        }

        private bool? GetIoCReplace(OrderExecOptions? options)
        {
            return options.HasValue && options.Value.IsFlagSet(OrderExecOptions.ImmediateOrCancel);
        }

        private OrderTimeInForce GetTimeInForce(DateTime? expiration)
        {
            return expiration != null ? OrderTimeInForce.GoodTillDate : OrderTimeInForce.GoodTillCancel;
        }

        private bool GetIoC(OrderExecOptions options)
        {
            return options.IsFlagSet(OrderExecOptions.ImmediateOrCancel);
        }

        #endregion

        #region Convertors

        private static SymbolEntity Convert(SFX.SymbolInfo info)
        {
            return new SymbolEntity(info.Name)
            {
                Digits = info.Precision,
                LotSize = info.RoundLot,
                MinAmount = info.RoundLot != 0 ? info.MinTradeVolume / info.RoundLot : double.NaN,
                MaxAmount = info.RoundLot != 0 ? info.MaxTradeVolume / info.RoundLot : double.NaN,
                AmountStep = info.RoundLot != 0 ? info.TradeVolumeStep / info.RoundLot : double.NaN,
                BaseCurrencyCode = info.Currency,
                CounterCurrencyCode = info.SettlementCurrency,
                IsTradeAllowed = info.IsTradeEnabled,
                Commission = info.Commission,
                LimitsCommission = info.LimitsCommission,
                CommissionChargeMethod = Convert(info.CommissionChargeMethod),
                CommissionChargeType = Convert(info.CommissionChargeType),
                CommissionType = Convert(info.CommissionType),
                ContractSizeFractional = info.RoundLot,
                MarginFactor = info.MarginFactorFractional ?? 1,
                StopOrderMarginReduction = info.StopOrderMarginReduction ?? 0,
                MarginHedged = info.MarginHedge,
                MarginMode = Convert(info.MarginCalcMode),
                SwapEnabled = info.IsSwapEnabled(),
                SwapSizeLong = (float)(info.SwapSizeLong ?? 0),
                SwapSizeShort = (float)(info.SwapSizeShort ?? 0),
                Security = info.SecurityName,
                GroupSortOrder = info.GroupSortOrder,
                SortOrder = info.SortOrder,
                SwapType = Convert(info.SwapType),
                TripleSwapDay = info.TripleSwapDay,
                IsTradeEnabled = info.IsTradeEnabled,
                Description = info.Description,
                DefaultSlippage = info.DefaultSlippage,
                HiddenLimitOrderMarginReduction = info.HiddenLimitOrderMarginReduction
            };
        }

        private static BO.SwapType Convert(SwapType type)
        {
            switch (type)
            {
                case SwapType.PercentPerYear: return BO.SwapType.PercentPerYear;
                case SwapType.Points: return BO.SwapType.Points;
                default: throw new NotImplementedException();
            }
        }

        private static Api.CommissionChargeType Convert(SFX.CommissionChargeType fdkChargeType)
        {
            switch (fdkChargeType)
            {
                case SFX.CommissionChargeType.PerLot: return Api.CommissionChargeType.PerLot;
                case SFX.CommissionChargeType.PerTrade: return Api.CommissionChargeType.PerTrade;

                default: throw new ArgumentException("Unsupported commission charge type: " + fdkChargeType);
            }
        }

        private static Api.CommissionChargeMethod Convert(SFX.CommissionChargeMethod fdkChargeMethod)
        {
            switch (fdkChargeMethod)
            {
                case SFX.CommissionChargeMethod.OneWay: return Api.CommissionChargeMethod.OneWay;
                case SFX.CommissionChargeMethod.RoundTurn: return Api.CommissionChargeMethod.RoundTurn;

                default: throw new ArgumentException("Unsupported commission charge method: " + fdkChargeMethod);
            }
        }

        private static Api.CommissionType Convert(SFX.CommissionType fdkType)
        {
            switch (fdkType)
            {
                case SFX.CommissionType.Absolute: return Api.CommissionType.Absolute;
                case SFX.CommissionType.PerBond: return Api.CommissionType.PerBond;
                case SFX.CommissionType.PerUnit: return Api.CommissionType.PerUnit;
                case SFX.CommissionType.Percent: return Api.CommissionType.Percent;
                case SFX.CommissionType.PercentageWaivedCash: return Api.CommissionType.PercentageWaivedCash;
                case SFX.CommissionType.PercentageWaivedEnhanced: return Api.CommissionType.PercentageWaivedEnhanced;

                default: throw new ArgumentException("Unsupported commission type: " + fdkType);
            }
        }

        private static TickTrader.BusinessObjects.MarginCalculationModes Convert(MarginCalcMode mode)
        {
            switch (mode)
            {
                case MarginCalcMode.Cfd: return BusinessObjects.MarginCalculationModes.CFD;
                case MarginCalcMode.CfdIndex: return BusinessObjects.MarginCalculationModes.CFD_Index;
                case MarginCalcMode.CfdLeverage: return BusinessObjects.MarginCalculationModes.CFD_Leverage;
                case MarginCalcMode.Forex: return BusinessObjects.MarginCalculationModes.Forex;
                case MarginCalcMode.Futures: return BusinessObjects.MarginCalculationModes.Futures;
                default: throw new NotImplementedException();
            }
        }

        private static CurrencyEntity Convert(SFX.CurrencyInfo info)
        {
            return new CurrencyEntity(info.Name, info.Precision)
            {
                SortOrder = info.SortOrder
            };
        }

        private static Api.AccountTypes Convert(AccountType fdkType)
        {
            switch (fdkType)
            {
                case AccountType.Cash: return Algo.Api.AccountTypes.Cash;
                case AccountType.Gross: return Algo.Api.AccountTypes.Gross;
                case AccountType.Net: return Algo.Api.AccountTypes.Net;

                default: throw new ArgumentException("Unsupported account type: " + fdkType);
            }
        }

        private static Api.OrderType Convert(SFX.OrderType fdkType)
        {
            switch (fdkType)
            {
                case SFX.OrderType.Limit: return Algo.Api.OrderType.Limit;
                case SFX.OrderType.Market: return Algo.Api.OrderType.Market;
                case SFX.OrderType.Position: return Algo.Api.OrderType.Position;
                case SFX.OrderType.Stop: return Algo.Api.OrderType.Stop;
                case SFX.OrderType.StopLimit: return Algo.Api.OrderType.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        private static SFX.OrderType Convert(Api.OrderType type)
        {
            switch (type)
            {
                case API.OrderType.Limit: return SFX.OrderType.Limit;
                case API.OrderType.Market: return SFX.OrderType.Market;
                case API.OrderType.Position: return SFX.OrderType.Position;
                case API.OrderType.Stop: return SFX.OrderType.Stop;
                case API.OrderType.StopLimit: return SFX.OrderType.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + type);
            }
        }

        private static Api.OrderSide Convert(SFX.OrderSide fdkSide)
        {
            switch (fdkSide)
            {
                case SFX.OrderSide.Buy: return Api.OrderSide.Buy;
                case SFX.OrderSide.Sell: return Api.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + fdkSide);
            }
        }

        private static SFX.OrderSide Convert(Api.OrderSide side)
        {
            switch (side)
            {
                case Api.OrderSide.Buy: return SFX.OrderSide.Buy;
                case Api.OrderSide.Sell: return SFX.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + side);
            }
        }

        private static BarPeriod ToBarPeriod(Api.TimeFrames timeframe)
        {
            switch (timeframe)
            {
                case Api.TimeFrames.MN: return BarPeriod.MN1;
                case Api.TimeFrames.W: return BarPeriod.W1;
                case Api.TimeFrames.D: return BarPeriod.D1;
                case Api.TimeFrames.H4: return BarPeriod.H4;
                case Api.TimeFrames.H1: return BarPeriod.H1;
                case Api.TimeFrames.M30: return BarPeriod.M30;
                case Api.TimeFrames.M15: return BarPeriod.M15;
                case Api.TimeFrames.M5: return BarPeriod.M5;
                case Api.TimeFrames.M1: return BarPeriod.M1;
                case Api.TimeFrames.S10: return BarPeriod.S10;
                case Api.TimeFrames.S1: return BarPeriod.S1;

                default: throw new ArgumentException("Unsupported time frame: " + timeframe);
            }
        }

        private static AccountEntity Convert(AccountInfo info)
        {
            return new AccountEntity()
            {
                Id = info.AccountId,
                Balance = info.Balance ?? 0.0D,
                BalanceCurrency = info.Currency,
                Type = Convert(info.Type),
                Leverage = info.Leverage ?? 1,
                Assets = info.Assets.Select(Convert).ToArray()
            };
        }

        private static AssetEntity Convert(AssetInfo info)
        {
            return new AssetEntity(info.Balance, info.Currency)
            {
                TradeVolume = info.TradeAmount
            };
        }

        public static OrderEntity Convert(SFX.ExecutionReport record)
        {
            return new OrderEntity(record.OrderId)
            {
                Symbol = record.Symbol,
                Comment = record.Comment,
                InitialType = Convert(record.InitialOrderType),
                Type = Convert(record.OrderType),
                ClientOrderId = record.ClientOrderId,
                Price = record.Price,
                StopPrice = record.StopPrice,
                Side = Convert(record.OrderSide),
                Created = record.Created,
                Swap = (decimal)record.Swap,
                Modified = record.Modified,
                StopLoss = record.StopLoss,
                TakeProfit = record.TakeProfit,
                Slippage = record.Slippage,
                Commission = (decimal)record.Commission,
                ExecVolume = record.ExecutedVolume,
                UserTag = record.Tag,
                RemainingVolume = (decimal)record.LeavesVolume,
                RequestedVolume = (decimal?)record.InitialVolume ?? 0,
                Expiration = record.Expiration?.ToLocalTime(),
                MaxVisibleVolume = (decimal?)record.MaxVisibleVolume,
                ExecPrice = record.AveragePrice,
                ReqOpenPrice = record.InitialPrice,
                Options = GetOptions(record),
                LastFillPrice = record.TradePrice,
                LastFillVolume = record.TradeAmount,
                ParentOrderId = record.ParentOrderId,
            };
        }

        private static OrderOptions GetOptions(SFX.ExecutionReport record)
        {
            var result = OrderOptions.None;
            var isLimit = record.OrderType == SFX.OrderType.Limit || record.OrderType == SFX.OrderType.StopLimit;

            if (isLimit && record.ImmediateOrCancelFlag)
                result |= OrderOptions.ImmediateOrCancel;

            if (record.MarketWithSlippage)
                result |= OrderOptions.MarketWithSlippage;

            if (record.MaxVisibleVolume >= 0)
                result |= OrderOptions.HiddenIceberg;

            return result;
        }

        private static List<ExecutionReport> ConvertToEr(List<SFX.ExecutionReport> reports, string operationId = null)
        {
            var result = new List<ExecutionReport>(reports.Count);
            for (int i = 0; i < reports.Count; i++)
                result.Add(ConvertToEr(reports[i], operationId));
            return result;
        }

        private static ExecutionReport ConvertToEr(SFX.ExecutionReport report, string operationId = null)
        {
            if (report.ExecutionType == SFX.ExecutionType.Rejected && report.RejectReason == RejectReason.None)
                report.RejectReason = RejectReason.Other; // Some plumbing. Sometimes we recieve Rejects with no RejectReason

            return new ExecutionReport()
            {
                OrderId = report.OrderId,
                ParentOrderId = report.ParentOrderId,
                // ExecTime = report.???
                TradeRequestId = operationId,
                Expiration = report.Expiration?.ToLocalTime(),
                Created = report.Created,
                Modified = report.Modified,
                RejectReason = Convert(report.RejectReason, report.Text ?? ""),
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Slippage = report.Slippage,
                Text = report.Text,
                Comment = report.Comment,
                Tag = report.Tag,
                Magic = report.Magic,
                IsReducedOpenCommission = report.ReducedOpenCommission,
                IsReducedCloseCommission = report.ReducedCloseCommission,
                ImmediateOrCancel = report.ImmediateOrCancelFlag,
                MarketWithSlippage = report.MarketWithSlippage,
                TradePrice = report.TradePrice ?? 0,
                Assets = report.Assets.Select(Convert).ToArray(),
                StopPrice = report.StopPrice,
                AveragePrice = report.AveragePrice,
                ClientOrderId = report.ClientOrderId,
                OrderStatus = Convert(report.OrderStatus),
                ExecutionType = Convert(report.ExecutionType),
                Symbol = report.Symbol,
                ExecutedVolume = report.ExecutedVolume,
                InitialVolume = report.InitialVolume,
                LeavesVolume = report.LeavesVolume,
                MaxVisibleVolume = report.MaxVisibleVolume,
                TradeAmount = report.TradeAmount,
                Commission = report.Commission,
                AgentCommission = report.AgentCommission,
                Swap = report.Swap,
                InitialOrderType = Convert(report.InitialOrderType),
                OrderType = Convert(report.OrderType),
                OrderSide = Convert(report.OrderSide),
                Price = report.Price,
                Balance = report.Balance ?? double.NaN,
                ReqOpenPrice = report.InitialPrice,
            };
        }

        private static ExecutionType Convert(SFX.ExecutionType type)
        {
            return (ExecutionType)type;
        }

        private static OrderStatus Convert(SFX.OrderStatus status)
        {
            return (OrderStatus)status;
        }

        private static Api.OrderCmdResultCodes Convert(RejectReason reason, string message)
        {
            switch (reason)
            {
                case RejectReason.InternalServerError: return Api.OrderCmdResultCodes.TradeServerError;
                case RejectReason.DealerReject: return Api.OrderCmdResultCodes.DealerReject;
                case RejectReason.UnknownSymbol: return Api.OrderCmdResultCodes.SymbolNotFound;
                case RejectReason.UnknownOrder: return Api.OrderCmdResultCodes.OrderNotFound;
                case RejectReason.IncorrectQuantity: return Api.OrderCmdResultCodes.IncorrectVolume;
                case RejectReason.OffQuotes: return Api.OrderCmdResultCodes.OffQuotes;
                case RejectReason.OrderExceedsLImit: return Api.OrderCmdResultCodes.NotEnoughMoney;
                case RejectReason.CloseOnly: return Api.OrderCmdResultCodes.CloseOnlyTrading;
                case RejectReason.Other:
                    {
                        if (message != null)
                        {
                            if (message == "Trade Not Allowed" || message == "Trade is not allowed!")
                                return Api.OrderCmdResultCodes.TradeNotAllowed;
                            else if (message.StartsWith("Not Enough Money"))
                                return Api.OrderCmdResultCodes.NotEnoughMoney;
                            else if (message.StartsWith("Rejected By Dealer"))
                                return Api.OrderCmdResultCodes.DealerReject;
                            else if (message.StartsWith("Dealer") && message.EndsWith("did not respond."))
                                return Api.OrderCmdResultCodes.DealingTimeout;
                            else if (message.Contains("locked by another operation"))
                                return Api.OrderCmdResultCodes.OrderLocked;
                            else if (message.Contains("Invalid expiration"))
                                return Api.OrderCmdResultCodes.IncorrectExpiration;
                            else if (message.StartsWith("Price precision"))
                                return Api.OrderCmdResultCodes.IncorrectPricePrecision;
                            else if (message.EndsWith("because close-only mode on"))
                                return Api.OrderCmdResultCodes.CloseOnlyTrading;
                            else if (message == "Max visible amount is not valid for market orders" || message.StartsWith("Max visible amount is valid only for"))
                                return Api.OrderCmdResultCodes.MaxVisibleVolumeNotSupported;
                            else if (message.StartsWith("Order Not Found") || message.EndsWith("was not found."))
                                return Api.OrderCmdResultCodes.OrderNotFound;
                            else if (message.StartsWith("Invalid order type") || message.Contains("is not supported"))
                                return Api.OrderCmdResultCodes.Unsupported;
                            else if (message.StartsWith("Invalid AmountChange") || message == "Cannot modify amount.")
                                return Api.OrderCmdResultCodes.InvalidAmountChange;
                            else if (message == "Account Is Readonly")
                                return Api.OrderCmdResultCodes.ReadOnlyAccount;
                            else if (message == "Internal server error")
                                return Api.OrderCmdResultCodes.TradeServerError;
                        }
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message != null && (message.StartsWith("Order Not Found") || message.EndsWith("not found.")))
                            return Api.OrderCmdResultCodes.OrderNotFound;
                        return Api.OrderCmdResultCodes.Ok;
                    }
            }
            return Api.OrderCmdResultCodes.UnknownError;
        }

        private static PositionEntity Convert(SFX.Position p)
        {
            API.OrderSide side;
            double price;
            double amount;

            if (p.BuyAmount > 0)
            {
                side = API.OrderSide.Buy;
                price = p.BuyPrice ?? 0;
                amount = p.BuyAmount;
            }
            else
            {
                side = API.OrderSide.Sell;
                price = p.SellPrice ?? 0;
                amount = p.SellAmount;
            }

            return new PositionEntity(p.Symbol)
            {
                Id = p.PosId,
                Side = side,
                Volume = amount,
                Price = price,
                Commission = p.Commission,
                AgentCommission = p.AgentCommission,
                Swap = p.Swap,
                Modified = p.Modified,
                Type = Convert(p.PosReportType),
            };
        }

        private static OrderExecAction Convert(SFX.PosReportType type)
        {
            switch (type)
            {
                case SFX.PosReportType.CancelPosition:
                    return OrderExecAction.Canceled;

                case SFX.PosReportType.ModifyPosition:
                    return OrderExecAction.Modified;

                case SFX.PosReportType.ClosePosition:
                    return OrderExecAction.Closed;

                case SFX.PosReportType.Split:
                    return OrderExecAction.Splitted;

                default:
                    return OrderExecAction.None;
            }
        }


        internal static BarEntity Convert(SFX.Bar fdkBar)
        {
            return new BarEntity()
            {
                Open = fdkBar.Open,
                Close = fdkBar.Close,
                High = fdkBar.High,
                Low = fdkBar.Low,
                Volume = fdkBar.Volume,
                OpenTime = fdkBar.From,
                CloseTime = fdkBar.To
            };
        }

        public static QuoteEntity[] ConvertAndFilter(IEnumerable<SFX.Quote> src, ref DateTime timeEdge)
        {
            var list = new List<QuoteEntity>();

            foreach (var item in src)
            {
                if (item.CreatingTime > timeEdge)
                {
                    list.Add(Convert(item));
                    timeEdge = item.CreatingTime;
                }
            }

            return list.ToArray();
        }

        internal static QuoteEntity[] Convert(SFX.Quote[] fdkQuoteSnapshot)
        {
            var result = new QuoteEntity[fdkQuoteSnapshot.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = Convert(fdkQuoteSnapshot[i]);
            return result;
        }

        private static QuoteEntity Convert(SFX.Quote fdkTick)
        {
            return new QuoteEntity(fdkTick.Symbol, fdkTick.CreatingTime, ConvertLevel2(fdkTick.Bids), ConvertLevel2(fdkTick.Asks),
                fdkTick.TickType == SFX.TickTypes.IndicativeBid || fdkTick.TickType == SFX.TickTypes.IndicativeBidAsk,
                fdkTick.TickType == SFX.TickTypes.IndicativeAsk || fdkTick.TickType == SFX.TickTypes.IndicativeBidAsk);
        }

        public static Core.TradeTransactionReason Convert(SFX.TradeTransactionReason reason)
        {
            switch (reason)
            {
                case SFX.TradeTransactionReason.ClientRequest:
                    return Core.TradeTransactionReason.ClientRequest;
                case SFX.TradeTransactionReason.DealerDecision:
                    return Core.TradeTransactionReason.DealerDecision;
                case SFX.TradeTransactionReason.DeleteAccount:
                    return Core.TradeTransactionReason.DeleteAccount;
                case SFX.TradeTransactionReason.Expired:
                    return Core.TradeTransactionReason.Expired;
                case SFX.TradeTransactionReason.PendingOrderActivation:
                    return Core.TradeTransactionReason.PendingOrderActivation;
                case SFX.TradeTransactionReason.Rollover:
                    return Core.TradeTransactionReason.Rollover;
                case SFX.TradeTransactionReason.StopLossActivation:
                    return Core.TradeTransactionReason.StopLossActivation;
                case SFX.TradeTransactionReason.StopOut:
                    return Core.TradeTransactionReason.StopOut;
                case SFX.TradeTransactionReason.TakeProfitActivation:
                    return Core.TradeTransactionReason.TakeProfitActivation;
                case SFX.TradeTransactionReason.TransferMoney:
                    return Core.TradeTransactionReason.TransferMoney;
                case SFX.TradeTransactionReason.Split:
                    return Core.TradeTransactionReason.Split;
                case SFX.TradeTransactionReason.Dividend:
                    return Core.TradeTransactionReason.Dividend;
                default:
                    return Core.TradeTransactionReason.None;
            }
        }

        public static TradeReportEntity Convert(TradeTransactionReport report)
        {
            bool isBalanceTransaction = report.TradeTransactionReportType == TradeTransactionReportType.Credit
                || report.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

            return new TradeReportEntity()
            {
                Id = report.Id,
                OrderId = report.Id,
                OpenTime = report.OrderCreated,
                CloseTime = report.TransactionTime,
                Type = GetRecordType(report),
                ActionType = Convert(report.TradeTransactionReportType),
                Symbol = isBalanceTransaction ? report.TransactionCurrency : report.Symbol,
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                OpenPrice = report.Price,
                Comment = report.Comment,
                Commission = report.Commission,
                CommissionCurrency = report.DstAssetCurrency ?? report.TransactionCurrency,
                OpenQuantity = report.Quantity,
                CloseQuantity = report.PositionLastQuantity,
                ClosePrice = report.PositionClosePrice,
                Swap = report.Swap,
                RemainingQuantity = report.LeavesQuantity,
                AccountBalance = report.AccountBalance,
                ActionId = report.ActionId,
                AgentCommission = report.AgentCommission,
                ClientId = report.ClientId,
                CloseConversionRate = report.CloseConversionRate,
                CommCurrency = report.CommCurrency,
                DstAssetAmount = report.DstAssetAmount,
                DstAssetCurrency = report.DstAssetCurrency,
                DstAssetMovement = report.DstAssetMovement,
                DstAssetToUsdConversionRate = report.DstAssetToUsdConversionRate,
                Expiration = report.Expiration,
                ImmediateOrCancel = report.ImmediateOrCancel,
                IsReducedCloseCommission = report.ReducedCloseCommission,
                IsReducedOpenCommission = report.ReducedOpenCommission,
                LeavesQuantity = report.LeavesQuantity,
                Magic = report.Magic,
                MarginCurrency = report.MarginCurrency,
                MarginCurrencyToUsdConversionRate = report.MarginCurrencyToUsdConversionRate,
                MarketWithSlippage = report.MarketWithSlippage,
                MaxVisibleQuantity = report.MaxVisibleQuantity,
                MinCommissionConversionRate = report.MinCommissionConversionRate,
                MinCommissionCurrency = report.MinCommissionCurrency,
                NextStreamPositionId = report.NextStreamPositionId,
                OpenConversionRate = report.OpenConversionRate,
                OrderCreated = report.OrderCreated,
                OrderFillPrice = report.OrderFillPrice,
                OrderLastFillAmount = report.OrderLastFillAmount,
                OrderModified = report.OrderModified,
                PositionById = report.PositionById,
                PositionClosed = report.PositionClosed,
                PositionCloseRequestedPrice = report.PositionCloseRequestedPrice,
                PositionId = report.PositionId,
                PositionLeavesQuantity = report.PositionLeavesQuantity,
                PositionModified = report.PositionModified,
                PositionOpened = report.PositionOpened,
                PositionQuantity = report.PositionQuantity,
                PosOpenPrice = report.PosOpenPrice,
                PosOpenReqPrice = report.PosOpenReqPrice,
                PosRemainingPrice = report.PosRemainingPrice,
                PosRemainingSide = Convert(report.PosRemainingSide),
                Price = report.Price,
                ProfitCurrency = report.ProfitCurrency,
                ProfitCurrencyToUsdConversionRate = report.ProfitCurrencyToUsdConversionRate,
                ReqClosePrice = report.ReqClosePrice,
                ReqCloseQuantity = report.ReqCloseQuantity,
                ReqOpenPrice = report.ReqOpenPrice,
                SrcAssetAmount = report.SrcAssetAmount,
                SrcAssetCurrency = report.SrcAssetCurrency,
                SrcAssetMovement = report.SrcAssetMovement,
                SrcAssetToUsdConversionRate = report.SrcAssetToUsdConversionRate,
                TradeRecordSide = Convert(report.OrderSide),
                TradeRecordType = Convert(report.OrderType),
                ReqOpenQuantity = report.ReqOpenQuantity,
                StopPrice = report.StopPrice,
                Tag = report.Tag,
                TransactionAmount = report.TransactionAmount,
                TransactionCurrency = report.TransactionCurrency,
                TransactionTime = report.TransactionTime,
                UsdToDstAssetConversionRate = report.UsdToDstAssetConversionRate,
                UsdToMarginCurrencyConversionRate = report.UsdToMarginCurrencyConversionRate,
                UsdToProfitCurrencyConversionRate = report.UsdToProfitCurrencyConversionRate,
                UsdToSrcAssetConversionRate = report.UsdToSrcAssetConversionRate,
                ReqOrderType = Convert(report.ReqOrderType != null ? report.ReqOrderType.Value : report.OrderType),
                TradeTransactionReason = Convert(report.TradeTransactionReason),
                SplitRatio = report.SplitRatio,
                Tax = report.Tax,
                Slippage = report.Slippage,
            };
        }

        private static Api.TradeRecordTypes GetRecordType(TradeTransactionReport rep)
        {
            if (rep.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction)
            {
                if (rep.TransactionAmount >= 0)
                    return Api.TradeRecordTypes.Deposit;
                else
                    return Api.TradeRecordTypes.Withdrawal;
            }
            else if (rep.TradeTransactionReportType == TradeTransactionReportType.Credit)
            {
                return Api.TradeRecordTypes.Unknown;
            }
            else if (rep.OrderType == SFX.OrderType.Limit)
            {
                if (rep.OrderSide == SFX.OrderSide.Buy)
                    return Api.TradeRecordTypes.BuyLimit;
                else if (rep.OrderSide == SFX.OrderSide.Sell)
                    return Api.TradeRecordTypes.SellLimit;
            }
            else if (rep.OrderType == SFX.OrderType.Position || rep.OrderType == SFX.OrderType.Market)
            {
                if (rep.OrderSide == SFX.OrderSide.Buy)
                    return Api.TradeRecordTypes.Buy;
                else if (rep.OrderSide == SFX.OrderSide.Sell)
                    return Api.TradeRecordTypes.Sell;
            }
            else if (rep.OrderType == SFX.OrderType.Stop)
            {
                if (rep.OrderSide == SFX.OrderSide.Buy)
                    return Api.TradeRecordTypes.BuyStop;
                else if (rep.OrderSide == SFX.OrderSide.Sell)
                    return Api.TradeRecordTypes.SellStop;
            }

            return Api.TradeRecordTypes.Unknown;
        }

        private static Api.TradeExecActions Convert(TradeTransactionReportType type)
        {
            switch (type)
            {
                case TradeTransactionReportType.BalanceTransaction: return Api.TradeExecActions.BalanceTransaction;
                case TradeTransactionReportType.Credit: return Api.TradeExecActions.Credit;
                case TradeTransactionReportType.OrderActivated: return Api.TradeExecActions.OrderActivated;
                case TradeTransactionReportType.OrderCanceled: return Api.TradeExecActions.OrderCanceled;
                case TradeTransactionReportType.OrderExpired: return Api.TradeExecActions.OrderExpired;
                case TradeTransactionReportType.OrderFilled: return Api.TradeExecActions.OrderFilled;
                case TradeTransactionReportType.OrderOpened: return Api.TradeExecActions.OrderOpened;
                case TradeTransactionReportType.PositionClosed: return Api.TradeExecActions.PositionClosed;
                case TradeTransactionReportType.PositionOpened: return Api.TradeExecActions.PositionOpened;
                case TradeTransactionReportType.TradeModified: return Api.TradeExecActions.TradeModified;
                default: return Api.TradeExecActions.None;
            }
        }

        private static Api.BookEntry[] ConvertLevel2(List<QuoteEntry> book)
        {
            if (book == null || book.Count == 0)
                return QuoteEntity.EmptyBook;
            else
                return book.Select(b => Convert(b)).ToArray();
        }

        public static BookEntry Convert(QuoteEntry fdkEntry)
        {
            return new BookEntry(fdkEntry.Price, fdkEntry.Volume);
        }

        public static BalanceOperationReport Convert(SFX.BalanceOperation op)
        {
            return new BalanceOperationReport(op.Balance, op.TransactionCurrency, op.TransactionAmount, Convert(op.TransactionType));
        }

        public static BalanceOperationType Convert(SFX.BalanceTransactionType type)
        {
            switch (type)
            {
                case BalanceTransactionType.DepositWithdrawal:
                    return BalanceOperationType.DepositWithdrawal;
                case BalanceTransactionType.Dividend:
                    return BalanceOperationType.Dividend;
            }

            throw new NotImplementedException("Unsupported balance transaction type: " + type);
        }

        public static PriceType ConvertBack(BarPriceType priceType)
        {
            switch (priceType)
            {
                case Api.BarPriceType.Ask: return PriceType.Ask;
                case Api.BarPriceType.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported price type: " + priceType);
        }

        private ConnectionErrorCodes Convert(LogoutReason fdkCode)
        {
            switch (fdkCode)
            {
                case LogoutReason.BlockedAccount: return ConnectionErrorCodes.BlockedAccount;
                case LogoutReason.InvalidCredentials: return ConnectionErrorCodes.InvalidCredentials;
                case LogoutReason.NetworkError: return ConnectionErrorCodes.NetworkError;
                case LogoutReason.ServerError: return ConnectionErrorCodes.ServerError;
                case LogoutReason.ServerLogout: return ConnectionErrorCodes.ServerLogout;
                case LogoutReason.SlowConnection: return ConnectionErrorCodes.SlowConnection;
                case LogoutReason.LoginDeleted: return ConnectionErrorCodes.LoginDeleted;
                default: return ConnectionErrorCodes.Unknown;
            }
        }

        #endregion
    }
}
