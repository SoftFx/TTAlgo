using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Common;
using SFX = TickTrader.FDK.Common;

namespace TickTrader.Algo.Account.Fdk2
{
    public sealed partial class SfxInterop : IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private const int ConnectTimeoutMs = 60 * 1000;
        private const int LogoutTimeoutMs = 60 * 1000;
        private const int DisconnectTimeoutMs = 60 * 1000;
        private const int DownloadTimeoutMs = 120 * 1000;

        private static readonly object _clientSessionCtorLock = new object();

        internal static readonly InteropException TwoFactorException = new ("2FA not supported", ConnectionErrorInfo.Types.ErrorCode.TwoFactorNotSupported);

        private IAlgoLogger logger;

        public IFeedServerApi FeedApi => this;
        public ITradeServerApi TradeApi => this;

        public bool AutoAccountInfo => false;
        public bool AutoSymbols => false;

        private FDK.Client.QuoteFeed _feedProxy;
        private FDK.Client.QuoteStore _feedHistoryProxy;
        private FDK.Client.OrderEntry _tradeProxy;
        private FDK.Client.TradeCapture _tradeHistoryProxy;

        private Fdk2FeedAdapter _feedProxyAdapter;
        private Fdk2FeedHistoryAdapter _feedHistoryProxyAdapter;
        private Fdk2TradeAdapter _tradeProxyAdapter;
        private Fdk2TradeHistoryAdapter _tradeHistoryProxyAdapter;

        private AppType _appType;

        public event Action<IServerInterop, ConnectionErrorInfo> Disconnected;


        static SfxInterop()
        {
            if (Environment.ProcessorCount > 8)
            {
                SoftFX.Net.Core.ClientSession.ConfigureLogServices(5);
                SoftFX.Net.Core.ClientSession.ConfigureSessionQueues(5);
            }
            else
            {
                SoftFX.Net.Core.ClientSession.ConfigureLogServices(3);
                SoftFX.Net.Core.ClientSession.ConfigureSessionQueues(3);
            }
        }


        public SfxInterop(ConnectionOptions options, string loggerId)
        {
            logger = AlgoLoggerFactory.GetLogger<SfxInterop>(loggerId);

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

            lock (_clientSessionCtorLock) // ensure ClientSessions get shared queues in specific order
            {
                _feedProxy = new FDK.Client.QuoteFeed("feed.proxy", logEvents, logStates, logMessages, port: 5041, validateClientCertificate: ValidateCertificate,
                    connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir, optimizationType: SoftFX.Net.Core.OptimizationType.Throughput2);
                _feedHistoryProxy = new FDK.Client.QuoteStore("feed.history.proxy", logEvents, logStates, logMessages, port: 5042, validateClientCertificate: ValidateCertificate,
                    connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir, optimizationType: SoftFX.Net.Core.OptimizationType.Throughput2);
                _tradeProxy = new FDK.Client.OrderEntry("trade.proxy", logEvents, logStates, logMessages, port: 5043, validateClientCertificate: ValidateCertificate,
                    connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir, optimizationType: SoftFX.Net.Core.OptimizationType.Throughput2);
                _tradeHistoryProxy = new FDK.Client.TradeCapture("trade.history.proxy", logEvents, logStates, logMessages, port: 5044, validateClientCertificate: ValidateCertificate,
                    connectAttempts: connectAttempts, reconnectAttempts: reconnectAttempts, connectInterval: connectInterval, heartbeatInterval: heartbeatInterval, logDirectory: logsDir, optimizationType: SoftFX.Net.Core.OptimizationType.Throughput2);
            }

            _feedProxyAdapter = new Fdk2FeedAdapter(_feedProxy, logger, b => BarUpdate?.Invoke(b));
            _feedHistoryProxyAdapter = new Fdk2FeedHistoryAdapter(_feedHistoryProxy, logger);
            _tradeProxyAdapter = new Fdk2TradeAdapter(_tradeProxy, rep => ExecutionReport?.Invoke(ConvertToEr(rep)));
            _tradeHistoryProxyAdapter = new Fdk2TradeHistoryAdapter(_tradeHistoryProxy, logger);

            _feedProxy.QuoteUpdateEvent += (c, q) => Tick?.Invoke(Convert(q));
            //_feedProxy.BarsUpdateEvent += (c, b) => BarUpdate?.Invoke(Convert(b));
            _feedProxy.LogoutEvent += (c, m) => OnLogout(m);
            _feedProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeProxy.LogoutEvent += (c, m) => OnLogout(m);
            _tradeProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeProxy.OrderUpdateEvent += (c, rep) => ExecutionReport?.Invoke(ConvertToEr(rep));
            _tradeProxy.PositionUpdateEvent += (c, rep) => PositionReport?.Invoke(ConvertToReport(rep));
            _tradeProxy.BalanceUpdateEvent += (c, rep) => BalanceOperation?.Invoke(Convert(rep));
            _tradeHistoryProxy.LogoutEvent += (c, m) => OnLogout(m);
            _tradeHistoryProxy.DisconnectEvent += (c, m) => OnDisconnect(m);
            _tradeHistoryProxy.TradeUpdateEvent += (c, rep) => TradeTransactionReport?.Invoke(Convert(rep));
            _tradeHistoryProxy.TriggerReportUpdateEvent += (c, rep) => TriggerTransactionReport?.Invoke(Convert(rep));
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
            catch (Exception ex)
            {
                var flatEx = ex is AggregateException aggrEx ? aggrEx.InnerExceptions.Last() : ex;

                if (flatEx is LoginException loginEx)
                {
                    var code = loginEx.LogoutReason;
                    var msg = loginEx.Message;
                    logger.Info($"Can't login ({code}): {msg}");
                    return new ConnectionErrorInfo(Convert(code), msg);
                }
                else if (flatEx is ConnectException)
                {
                    return new ConnectionErrorInfo(ConnectionErrorInfo.Types.ErrorCode.NetworkError, flatEx.Message);
                }
                else if (flatEx is InteropException interopEx)
                {
                    logger.Info($"Connection sequence failed: {interopEx.Message}");
                    return new ConnectionErrorInfo(interopEx.ErrorCode, interopEx.Message);
                }
                else if (flatEx is RejectException rejectEx)
                {
                    logger.Info($"Connection sequence failed: {rejectEx.Message}");
                    return new ConnectionErrorInfo(ConnectionErrorInfo.Types.ErrorCode.RejectedByServer, rejectEx.Message);
                }
                else if (flatEx is SoftFX.Net.Core.DisconnectedException disconnectEx)
                {
                    logger.Info($"Connection sequence failed: {disconnectEx.Message}");
                    return new ConnectionErrorInfo(ConnectionErrorInfo.Types.ErrorCode.NetworkError, disconnectEx.Message);
                }
                else
                {
                    logger.Error(ex);
                    return new ConnectionErrorInfo(ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError, ex.Message);
                }
            }

            return ConnectionErrorInfo.Ok;
        }

        private async Task ConnectFeed(string address, string login, string password)
        {
            logger.Debug("Feed: Connecting...");
            await _feedProxyAdapter.ConnectAsync(address);
            logger.Debug("Feed: Connected.");

            await _feedProxyAdapter.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
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
            await _feedHistoryProxyAdapter.ConnectAsync(address);
            logger.Debug("Feed.History: Connected.");
            await _feedHistoryProxyAdapter.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
            logger.Debug("Feed.History: Logged in.");
        }

        private async Task ConnectTradeHistory(string address, string login, string password)
        {
            logger.Debug("Trade.History: Connecting...");
            await _tradeHistoryProxyAdapter.ConnectAsync(address);
            logger.Debug("Trade.History: Connected.");
            await _tradeHistoryProxyAdapter.LoginAsync(login, password, "", _appType.ToString(), Guid.NewGuid().ToString());
            logger.Debug("Trade.History: Logged in.");
            await _tradeHistoryProxyAdapter.SubscribeTradesAsync(false);
            logger.Debug("Trade.History: Trades Subscribed.");
            await _tradeHistoryProxyAdapter.SubscribeTriggersAsync(false);
            logger.Debug("Trade.History: Triggers Subscribed.");
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
            Disconnected?.Invoke(this, new ConnectionErrorInfo(ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError, text));
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
                await _feedProxyAdapter.LogoutAsync("");
                logger.Debug("Feed: Logged out.");
                await _feedProxyAdapter.DisconnectAsync("");
            }
            catch (Exception) { }

            await _feedProxyAdapter.Deinit();

            logger.Debug("Feed: Disconnected.");
        }

        private async Task DisconnectFeedHstory()
        {
            logger.Debug("Feed.History: Disconnecting...");
            try
            {
                await _feedHistoryProxyAdapter.LogoutAsync("");
                logger.Debug("Feed.History: Logged out.");
                await _feedHistoryProxyAdapter.DisconnectAsync("");
            }
            catch (Exception) { }

            await _feedHistoryProxyAdapter.Deinit();

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

            await _tradeProxyAdapter.Deinit();

            logger.Debug("Trade: Disconnected.");
        }

        private async Task DisconnectTradeHstory()
        {
            logger.Debug("Trade.History: Disconnecting...");
            try
            {
                await _tradeHistoryProxyAdapter.LogoutAsync("");
                logger.Debug("Trade.History: Logged out.");
                await _tradeHistoryProxyAdapter.DisconnectAsync("");
            }
            catch (Exception) { }

            await _tradeHistoryProxyAdapter.Deinit();

            logger.Debug("Trade.History: Disconnected.");
        }

        #region IFeedServerApi

        public event Action<Domain.QuoteInfo> Tick;
        public event Action<Domain.BarUpdate> BarUpdate;

        public async Task<Domain.CurrencyInfo[]> GetCurrencies()
        {
            var currencies = await _feedProxyAdapter.GetCurrencyListAsync();
            return currencies.Select(Convert).ToArray();
        }

        public async Task<Domain.SymbolInfo[]> GetSymbols()
        {
            var symbols = await _feedProxyAdapter.GetSymbolListAsync();
            return symbols.Select(Convert).ToArray();
        }

        public Task<Domain.QuoteInfo[]> SubscribeToQuotes(string[] symbols, int depth, int? frequency)
        {
            return _feedProxyAdapter.SubscribeQuotesAsync(symbols, depth, frequency);
        }

        public async Task<Domain.QuoteInfo[]> GetQuoteSnapshot(string[] symbols, int depth)
        {
            var array = await _feedProxyAdapter.GetQuotesAsync(symbols, depth);
            return array.Select(Convert).ToArray();
        }

        public void DownloadBars(BlockingChannel<Domain.BarData> stream, string symbol, UtcTicks from, UtcTicks to, Domain.Feed.Types.MarketSide marketSide, Domain.Feed.Types.Timeframe timeframe)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var fromDt = from.ToUtcDateTime();
                    var e = _feedHistoryProxy.DownloadBars(symbol, ConvertBack(marketSide), ToBarPeriod(timeframe), fromDt, to.ToUtcDateTime(), DownloadTimeoutMs);
                    DateTime? timeEdge = null;

                    while (true)
                    {
                        var bar = e.Next(DownloadTimeoutMs);

                        if (bar != null)
                        {
                            if (timeEdge == null)
                            {
                                if (bar.From < fromDt)
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

        public async Task<Domain.BarData[]> DownloadBarPage(string symbol, UtcTicks from, int count, Domain.Feed.Types.MarketSide marketSide, Domain.Feed.Types.Timeframe timeframe)
        {
            var result = new List<Domain.BarData>();

            try
            {
                var bars = await _feedHistoryProxyAdapter.GetBarListAsync(symbol, ConvertBack(marketSide), ToBarPeriod(timeframe), from.ToUtcDateTime(), count);
                return bars.Select(Convert).ToArray();
            }
            catch (Exception ex)
            {
                throw new InteropException(ex.Message, ConnectionErrorInfo.Types.ErrorCode.NetworkError);
            }
        }

        public void DownloadQuotes(BlockingChannel<Domain.QuoteInfo> stream, string symbol, UtcTicks from, UtcTicks to, bool includeLevel2)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var e = _feedHistoryProxy.DownloadQuotes(symbol, includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top, from.ToUtcDateTime(), to.ToUtcDateTime(), DownloadTimeoutMs);

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

        public async Task<Domain.QuoteInfo[]> DownloadQuotePage(string symbol, UtcTicks from, int count, bool includeLevel2)
        {
            var result = new List<Domain.QuoteInfo>();

            try
            {
                var quotes = await _feedHistoryProxyAdapter.GetQuoteListAsync(symbol, includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top, from.ToUtcDateTime(), count);
                return quotes.Select(Convert).ToArray();
            }
            catch (Exception ex)
            {
                throw new InteropException(ex.Message, ConnectionErrorInfo.Types.ErrorCode.NetworkError);
            }
        }

        public async Task<(DateTime?, DateTime?)> GetAvailableRange(string symbol, Domain.Feed.Types.MarketSide marketSide, Domain.Feed.Types.Timeframe timeframe)
        {
            if (timeframe.IsTicks())
            {
                var level2 = timeframe == Domain.Feed.Types.Timeframe.TicksLevel2;
                var info = await _feedHistoryProxyAdapter.GetQuotesHistoryInfoAsync(symbol, level2);
                return (info.AvailFrom, info.AvailTo);
            }
            else // bars
            {
                var info = await _feedHistoryProxyAdapter.GetBarsHistoryInfoAsync(symbol, ToBarPeriod(timeframe), ConvertBack(marketSide));
                return (info.AvailFrom, info.AvailTo);
            }
        }

        public Task SubscribeToBars(List<BarSubUpdate> updates) => _feedProxyAdapter.ModifyBarSub(updates);

        #endregion

        #region ITradeServerApi

        public event Action<Domain.PositionExecReport> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<Domain.TradeReportInfo> TradeTransactionReport;
        public event Action<Domain.TriggerReportInfo> TriggerTransactionReport;
        public event Action<Domain.BalanceOperation> BalanceOperation;
        public event Action<Domain.SymbolInfo[]> SymbolInfo { add { } remove { } }
        public event Action<Domain.CurrencyInfo[]> CurrencyInfo { add { } remove { } }

        public Task<Domain.AccountInfo> GetAccountInfo()
        {
            return _tradeProxyAdapter.GetAccountInfoAsync()
                .ContinueWith(t => Convert(t.Result));
        }

        public void GetTradeRecords(BlockingChannel<Domain.OrderInfo> rxStream)
        {
            _tradeProxyAdapter.GetOrdersAsync(rxStream);
        }

        public Task<Domain.PositionInfo[]> GetPositions()
        {
            return _tradeProxyAdapter.GetPositionsAsync()
                .ContinueWith(t => t.Result.Select(Convert).ToArray());
        }

        public void GetTradeHistory(ChannelWriter<Domain.TradeReportInfo> rxStream, UtcTicks? from, UtcTicks? to, bool skipCancelOrders, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            _tradeHistoryProxyAdapter.DownloadTradesAsync(direction, from.ToDateTime(), to.ToDateTime(), skipCancelOrders, rxStream);
        }

        public void GetTriggerReportsHistory(ChannelWriter<Domain.TriggerReportInfo> rxStream, UtcTicks? from, UtcTicks? to, bool skipFailedTriggers, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            _tradeHistoryProxyAdapter.DownloadTriggerReportsAsync(direction, from.ToDateTime(), to.ToDateTime(), skipFailedTriggers, rxStream);
        }

        public Task<OrderInteropResult> SendOpenOrder(Domain.OpenOrderRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                var timeInForce = GetTimeInForce(r.Expiration);
                var isIoc = GetIoC(r.ExecOptions);
                var isOco = GetOCO(r.ExecOptions);
                long? ocoRelatedOrderId = null;
                long? otoTriggeredById = null;

                if (isOco)
                {
                    if (long.TryParse(r.OcoRelatedOrderId, out var ocoParsedId))
                        ocoRelatedOrderId = ocoParsedId;
                }

                if (request.OtoTrigger != null)
                {
                    if (long.TryParse(r.OtoTrigger.OrderIdTriggeredBy, out var otoParsedId))
                        otoTriggeredById = otoParsedId;
                }

                if (isOco && request.SubOpenRequests?.Count > 0)
                {
                    const int operationTimeout = 120000;
                    //const int operationTimeout = 10000;

                    var sub = request.SubOpenRequests[0];
                    var subTimeInForce = GetTimeInForce(sub.Expiration);

                    return _tradeProxyAdapter.NewOcoOrdersAsync(r.Symbol, operationTimeout,

                           r.OperationId, Convert(r.Type), Convert(r.Side), r.Amount, r.MaxVisibleAmount, r.Price, r.StopPrice,
                           timeInForce, r.Expiration.ToDateTime(), r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, r.Slippage,

                           sub.OperationId, Convert(sub.Type), Convert(sub.Side), sub.Amount, sub.MaxVisibleAmount, sub.Price, sub.StopPrice,
                           subTimeInForce, sub.Expiration.ToDateTime(), sub.StopLoss, sub.TakeProfit, sub.Comment, sub.Tag, null, sub.Slippage,

                           ConvertToServer(r.OtoTrigger?.Type), r.OtoTrigger?.TriggerTime.ToDateTime(), otoTriggeredById);
                }

                return _tradeProxyAdapter.NewOrderAsync(r.OperationId, r.Symbol, Convert(r.Type), Convert(r.Side), r.Amount, r.MaxVisibleAmount,
                       r.Price, r.StopPrice, timeInForce, r.Expiration.ToDateTime(), r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, isIoc, r.Slippage,
                       isOco, r.OcoEqualVolume, ocoRelatedOrderId, ConvertToServer(r.OtoTrigger?.Type), r.OtoTrigger?.TriggerTime.ToDateTime(),
                       otoTriggeredById);
            });
        }

        public Task<OrderInteropResult> SendCancelOrder(Domain.CancelOrderRequest request)
        {
            return ExecuteOrderOperation(request, r => _tradeProxyAdapter.CancelOrderAsync(r.OperationId, "", r.OrderId));
        }

        public Task<OrderInteropResult> SendModifyOrder(Domain.ModifyOrderRequest request)
        {
            if (_tradeProxy.ProtocolSpec.SupportsOrderReplaceQtyChange)
            {
                long? ocoRelatedOrderId = null;

                if (long.TryParse(request.OcoRelatedOrderId, out var ocoOrderId))
                    ocoRelatedOrderId = ocoOrderId;

                ContingentOrderTriggerType? otoTriggerType = null;
                DateTime? otoTriggerTime = null;
                long? otoTriggerById = null;

                if (request.OtoTrigger != null)
                {
                    otoTriggerType = ConvertToServer(request.OtoTrigger.Type);
                    otoTriggerTime = request.OtoTrigger?.TriggerTime.ToDateTime();

                    if (!string.IsNullOrEmpty(request.OtoTrigger.OrderIdTriggeredBy))
                    {
                        if (long.TryParse(request.OtoTrigger.OrderIdTriggeredBy, out var parsedOtoId))
                            otoTriggerById = parsedOtoId;
                    }
                }

                return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                    r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.AmountChange,
                    r.MaxVisibleAmount, r.Price, r.StopPrice, GetTimeInForceReplace(r.ExecOptions, r.Expiration), r.Expiration.ToDateTime(),
                    r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, GetIoCReplace(r.ExecOptions), r.Slippage,
                    GetOCOReplace(r.ExecOptions), r.OcoEqualVolume, ocoRelatedOrderId,
                    otoTriggerType, otoTriggerTime, otoTriggerById));
            }
            return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.NewAmount ?? r.CurrentAmount, r.CurrentAmount,
                r.MaxVisibleAmount, r.Price, r.StopPrice, GetTimeInForceReplace(r.ExecOptions, r.Expiration), r.Expiration.ToDateTime(),
                r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, GetIoCReplace(r.ExecOptions), r.Slippage));
        }

        public Task<OrderInteropResult> SendCloseOrder(Domain.CloseOrderRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                if (request.ByOrderId != null)
                    return _tradeProxyAdapter.ClosePositionByAsync(r.OperationId, r.OrderId, r.ByOrderId);
                else
                    return _tradeProxyAdapter.ClosePositionAsync(r.OperationId, r.OrderId, r.Amount, r.Slippage == null || !double.IsNaN(r.Slippage.Value) ? r.Slippage : null);
            });
        }

        private async Task<OrderInteropResult> ExecuteOrderOperation<TReq>(TReq request, Func<TReq, Task<List<SFX.ExecutionReport>>> operationDef)
            where TReq : Domain.ITradeRequest
        {
            var operationId = request.OperationId;

            try
            {
                try
                {
                    var result = await operationDef(request);
                    return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.Ok, ConvertToEr(result));
                }
                catch (ExecutionException ex)
                {
                    var reason = Convert(ex.Report.RejectReason, ex.Message);
                    return new OrderInteropResult(reason, ConvertToEr(ex.Report, operationId)); //ConvertToEr may throw an exception
                }
            }
            catch (DisconnectException)
            {
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);
            }
            catch (InteropException ex) when (ex.ErrorCode == ConnectionErrorInfo.Types.ErrorCode.RejectedByServer)
            {
                // workaround for inconsistent server logic
                return new OrderInteropResult(Convert(RejectReason.Other, ex.Message));
            }
            catch (NotSupportedException)
            {
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.Unsupported);
            }
            catch (SFX.TimeoutException)
            {
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.DealingTimeout);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Session is not active"))
                    return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);

                logger.Error(ex);
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.UnknownError);
            }
        }

        private OrderTimeInForce? GetTimeInForceReplace(Domain.OrderExecOptions? options, UtcTicks? expiration)
        {
            return expiration != null ? OrderTimeInForce.GoodTillDate
                : (options != null ? OrderTimeInForce.GoodTillCancel : (OrderTimeInForce?)null);
        }

        private bool? GetIoCReplace(Domain.OrderExecOptions? options)
        {
            return options?.HasFlag(Domain.OrderExecOptions.ImmediateOrCancel);
        }

        private bool? GetOCOReplace(Domain.OrderExecOptions? options)
        {
            return options?.HasFlag(Domain.OrderExecOptions.OneCancelsTheOther);
        }

        private OrderTimeInForce GetTimeInForce(UtcTicks? expiration)
        {
            return expiration != null ? OrderTimeInForce.GoodTillDate : OrderTimeInForce.GoodTillCancel;
        }

        private bool GetIoC(Domain.OrderExecOptions options)
        {
            return options.HasFlag(Domain.OrderExecOptions.ImmediateOrCancel);
        }

        private bool GetOCO(Domain.OrderExecOptions options)
        {
            return options.HasFlag(Domain.OrderExecOptions.OneCancelsTheOther);
        }

        #endregion
    }
}
