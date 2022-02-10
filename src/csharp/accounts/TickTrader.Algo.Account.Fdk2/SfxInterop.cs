using ActorSharp;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
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
    public class SfxInterop : IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private const int ConnectTimeoutMs = 60 * 1000;
        private const int LogoutTimeoutMs = 60 * 1000;
        private const int DisconnectTimeoutMs = 60 * 1000;
        private const int DownloadTimeoutMs = 120 * 1000;

        private static readonly object _clientSessionCtorLock = new object();

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

            _feedProxyAdapter = new Fdk2FeedAdapter(_feedProxy, logger);
            _feedHistoryProxyAdapter = new Fdk2FeedHistoryAdapter(_feedHistoryProxy, logger);
            _tradeProxyAdapter = new Fdk2TradeAdapter(_tradeProxy, rep => ExecutionReport?.Invoke(ConvertToEr(rep)));
            _tradeHistoryProxyAdapter = new Fdk2TradeHistoryAdapter(_tradeHistoryProxy, logger);

            _feedProxy.QuoteUpdateEvent += (c, q) => Tick?.Invoke(Convert(q));
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
                    logger.Info("Can't login: " + code);
                    return new ConnectionErrorInfo(Convert(code), flatEx.Message);
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

        public Task<Domain.QuoteInfo[]> SubscribeToQuotes(string[] symbols, int depth)
        {
            return _feedProxyAdapter.SubscribeQuotesAsync(symbols, depth);
        }

        public async Task<Domain.QuoteInfo[]> GetQuoteSnapshot(string[] symbols, int depth)
        {
            var array = await _feedProxyAdapter.GetQuotesAsync(symbols, depth);
            return array.Select(Convert).ToArray();
        }

        public void DownloadBars(BlockingChannel<Domain.BarData> stream, string symbol, Timestamp from, Timestamp to, Domain.Feed.Types.MarketSide marketSide, Domain.Feed.Types.Timeframe timeframe)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var fromDt = from.ToDateTime();
                    var e = _feedHistoryProxy.DownloadBars(symbol, ConvertBack(marketSide), ToBarPeriod(timeframe), fromDt, to.ToDateTime(), DownloadTimeoutMs);
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

        public async Task<Domain.BarData[]> DownloadBarPage(string symbol, Timestamp from, int count, Domain.Feed.Types.MarketSide marketSide, Domain.Feed.Types.Timeframe timeframe)
        {
            var result = new List<Domain.BarData>();

            try
            {
                var bars = await _feedHistoryProxyAdapter.GetBarListAsync(symbol, ConvertBack(marketSide), ToBarPeriod(timeframe), from.ToDateTime(), count);
                return bars.Select(Convert).ToArray();
            }
            catch (Exception ex)
            {
                throw new InteropException(ex.Message, ConnectionErrorInfo.Types.ErrorCode.NetworkError);
            }
        }

        public void DownloadQuotes(BlockingChannel<Domain.QuoteInfo> stream, string symbol, Timestamp from, Timestamp to, bool includeLevel2)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var e = _feedHistoryProxy.DownloadQuotes(symbol, includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top, from.ToDateTime(), to.ToDateTime(), DownloadTimeoutMs);

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

        public async Task<Domain.QuoteInfo[]> DownloadQuotePage(string symbol, Timestamp from, int count, bool includeLevel2)
        {
            var result = new List<Domain.QuoteInfo>();

            try
            {
                var quotes = await _feedHistoryProxyAdapter.GetQuoteListAsync(symbol, includeLevel2 ? QuoteDepth.Level2 : QuoteDepth.Top, from.ToDateTime(), count);
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

        public void GetTradeHistory(ChannelWriter<Domain.TradeReportInfo> rxStream, DateTime? from, DateTime? to, bool skipCancelOrders, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            _tradeHistoryProxyAdapter.DownloadTradesAsync(direction, from?.ToUniversalTime(), to?.ToUniversalTime(), skipCancelOrders, rxStream);
        }

        public void GetTriggerReportsHistory(ChannelWriter<Domain.TriggerReportInfo> rxStream, DateTime? from, DateTime? to, bool skipFailedTriggers, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            _tradeHistoryProxyAdapter.DownloadTriggerReportsAsync(direction, from?.ToUniversalTime(), to?.ToUniversalTime(), skipFailedTriggers, rxStream);
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
                           timeInForce, r.Expiration?.ToDateTime(), r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, r.Slippage,

                           sub.OperationId, Convert(sub.Type), Convert(sub.Side), sub.Amount, sub.MaxVisibleAmount, sub.Price, sub.StopPrice,
                           subTimeInForce, sub.Expiration?.ToDateTime(), sub.StopLoss, sub.TakeProfit, sub.Comment, sub.Tag, null, sub.Slippage,

                           ConvertToServer(r.OtoTrigger?.Type), r.OtoTrigger?.TriggerTime?.ToDateTime(), otoTriggeredById);
                }

                return _tradeProxyAdapter.NewOrderAsync(r.OperationId, r.Symbol, Convert(r.Type), Convert(r.Side), r.Amount, r.MaxVisibleAmount,
                       r.Price, r.StopPrice, timeInForce, r.Expiration?.ToDateTime(), r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, isIoc, r.Slippage,
                       isOco, r.OcoEqualVolume, ocoRelatedOrderId, ConvertToServer(r.OtoTrigger?.Type), r.OtoTrigger?.TriggerTime?.ToDateTime(),
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
                    otoTriggerTime = request.OtoTrigger?.TriggerTime?.ToDateTime();

                    if (!string.IsNullOrEmpty(request.OtoTrigger.OrderIdTriggeredBy))
                    {
                        if (long.TryParse(request.OtoTrigger.OrderIdTriggeredBy, out var parsedOtoId))
                            otoTriggerById = parsedOtoId;
                    }
                }

                return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                    r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.AmountChange,
                    r.MaxVisibleAmount, r.Price, r.StopPrice, GetTimeInForceReplace(r.ExecOptions, r.Expiration), r.Expiration?.ToDateTime(),
                    r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, GetIoCReplace(r.ExecOptions), r.Slippage,
                    GetOCOReplace(r.ExecOptions), r.OcoEqualVolume, ocoRelatedOrderId,
                    otoTriggerType, otoTriggerTime, otoTriggerById));
            }
            return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.NewAmount ?? r.CurrentAmount, r.CurrentAmount,
                r.MaxVisibleAmount, r.Price, r.StopPrice, GetTimeInForceReplace(r.ExecOptions, r.Expiration), r.Expiration?.ToDateTime(),
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

        private OrderTimeInForce? GetTimeInForceReplace(Domain.OrderExecOptions? options, Timestamp expiration)
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

        private OrderTimeInForce GetTimeInForce(Timestamp expiration)
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

        #region Convertors

        private static Domain.SymbolInfo Convert(SFX.SymbolInfo info)
        {
            return new Domain.SymbolInfo
            {
                Name = info.Name,
                TradeAllowed = info.IsTradeEnabled,
                BaseCurrency = info.Currency,
                CounterCurrency = info.SettlementCurrency,
                Digits = info.Precision,
                LotSize = info.RoundLot,
                MinTradeVolume = info.RoundLot != 0 ? info.MinTradeVolume / info.RoundLot : double.NaN,
                MaxTradeVolume = info.RoundLot != 0 ? info.MaxTradeVolume / info.RoundLot : double.NaN,
                TradeVolumeStep = info.RoundLot != 0 ? info.TradeVolumeStep / info.RoundLot : double.NaN,

                Description = info.Description,
                Security = info.SecurityName,
                GroupSortOrder = info.GroupSortOrder,
                SortOrder = info.SortOrder,

                Slippage = new Domain.SlippageInfo
                {
                    DefaultValue = info.DefaultSlippage,
                    Type = Convert(info.SlippageType),
                },

                Commission = new Domain.CommissonInfo
                {
                    Commission = info.Commission,
                    LimitsCommission = info.LimitsCommission,
                    ValueType = Convert(info.CommissionType),
                    MinCommission = info.MinCommission,
                    MinCommissionCurrency = info.MinCommissionCurrency,
                },

                Margin = new Domain.MarginInfo
                {
                    Mode = Convert(info.MarginCalcMode),
                    Factor = info.MarginFactorFractional ?? 1,
                    Hedged = info.MarginHedge,
                    StopOrderReduction = info.StopOrderMarginReduction,
                    HiddenLimitOrderReduction = info.HiddenLimitOrderMarginReduction,
                },

                Swap = new Domain.SwapInfo
                {
                    Enabled = info.SwapEnabled,
                    Type = Convert(info.SwapType),
                    SizeLong = info.SwapSizeLong,
                    SizeShort = info.SwapSizeShort,
                    TripleSwapDay = info.TripleSwapDay,
                },
            };
        }

        private static Domain.SlippageInfo.Types.Type Convert(SFX.SlippageType type)
        {
            switch (type)
            {
                case SFX.SlippageType.Pips: return Domain.SlippageInfo.Types.Type.Pips;
                case SFX.SlippageType.Percent: return Domain.SlippageInfo.Types.Type.Percent;
                default: throw new NotImplementedException();
            }
        }

        private static Domain.SwapInfo.Types.Type Convert(SwapType type)
        {
            switch (type)
            {
                case SwapType.PercentPerYear: return Domain.SwapInfo.Types.Type.PercentPerYear;
                case SwapType.Points: return Domain.SwapInfo.Types.Type.Points;
                default: throw new NotImplementedException();
            }
        }

        private static Domain.CommissonInfo.Types.ValueType Convert(SFX.CommissionType fdkType)
        {
            switch (fdkType)
            {
                case SFX.CommissionType.Absolute: return Domain.CommissonInfo.Types.ValueType.Money;
                case SFX.CommissionType.PerUnit: return Domain.CommissonInfo.Types.ValueType.Points;
                case SFX.CommissionType.Percent: return Domain.CommissonInfo.Types.ValueType.Percentage;

                // Server is not using those anymore. Providing fallback value just in case
                case SFX.CommissionType.PerBond:
                case SFX.CommissionType.PercentageWaivedCash:
                case SFX.CommissionType.PercentageWaivedEnhanced:
                    return Domain.CommissonInfo.Types.ValueType.Percentage;

                default: throw new ArgumentException("Unsupported commission type: " + fdkType);
            }
        }

        private static Domain.MarginInfo.Types.CalculationMode Convert(MarginCalcMode mode)
        {
            switch (mode)
            {
                case MarginCalcMode.Cfd: return Domain.MarginInfo.Types.CalculationMode.Cfd;
                case MarginCalcMode.CfdIndex: return Domain.MarginInfo.Types.CalculationMode.CfdIndex;
                case MarginCalcMode.CfdLeverage: return Domain.MarginInfo.Types.CalculationMode.CfdLeverage;
                case MarginCalcMode.Forex: return Domain.MarginInfo.Types.CalculationMode.Forex;
                case MarginCalcMode.Futures: return Domain.MarginInfo.Types.CalculationMode.Futures;
                default: throw new NotImplementedException();
            }
        }

        private static Domain.CurrencyInfo Convert(SFX.CurrencyInfo info)
        {
            return new Domain.CurrencyInfo()
            {
                Name = info.Name,
                Digits = info.Precision,
                SortOrder = info.SortOrder,
                Type = info.TypeId,
            };
        }

        private static Domain.AccountInfo.Types.Type Convert(AccountType fdkType)
        {
            switch (fdkType)
            {
                case AccountType.Cash: return Domain.AccountInfo.Types.Type.Cash;
                case AccountType.Gross: return Domain.AccountInfo.Types.Type.Gross;
                case AccountType.Net: return Domain.AccountInfo.Types.Type.Net;

                default: throw new ArgumentException("Unsupported account type: " + fdkType);
            }
        }

        private static Domain.OrderInfo.Types.Type Convert(SFX.OrderType fdkType)
        {
            switch (fdkType)
            {
                case SFX.OrderType.Limit: return Domain.OrderInfo.Types.Type.Limit;
                case SFX.OrderType.Market: return Domain.OrderInfo.Types.Type.Market;
                case SFX.OrderType.Position: return Domain.OrderInfo.Types.Type.Position;
                case SFX.OrderType.Stop: return Domain.OrderInfo.Types.Type.Stop;
                case SFX.OrderType.StopLimit: return Domain.OrderInfo.Types.Type.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        private static SFX.OrderType Convert(Domain.OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.OrderInfo.Types.Type.Market: return SFX.OrderType.Market;
                case Domain.OrderInfo.Types.Type.Position: return SFX.OrderType.Position;
                case Domain.OrderInfo.Types.Type.StopLimit: return SFX.OrderType.StopLimit;
                case Domain.OrderInfo.Types.Type.Limit: return SFX.OrderType.Limit;
                case Domain.OrderInfo.Types.Type.Stop: return SFX.OrderType.Stop;

                default: throw new ArgumentException("Unsupported order type: " + type);
            }
        }

        private static Domain.OrderInfo.Types.Side Convert(SFX.OrderSide fdkSide)
        {
            switch (fdkSide)
            {
                case SFX.OrderSide.Buy: return Domain.OrderInfo.Types.Side.Buy;
                case SFX.OrderSide.Sell: return Domain.OrderInfo.Types.Side.Sell;

                default: throw new ArgumentException("Unsupported order side: " + fdkSide);
            }
        }

        private static SFX.OrderSide Convert(Domain.OrderInfo.Types.Side side)
        {
            switch (side)
            {
                case Domain.OrderInfo.Types.Side.Buy: return SFX.OrderSide.Buy;
                case Domain.OrderInfo.Types.Side.Sell: return SFX.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + side);
            }
        }

        private static BarPeriod ToBarPeriod(Domain.Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Domain.Feed.Types.Timeframe.MN: return BarPeriod.MN1;
                case Domain.Feed.Types.Timeframe.W: return BarPeriod.W1;
                case Domain.Feed.Types.Timeframe.D: return BarPeriod.D1;
                case Domain.Feed.Types.Timeframe.H4: return BarPeriod.H4;
                case Domain.Feed.Types.Timeframe.H1: return BarPeriod.H1;
                case Domain.Feed.Types.Timeframe.M30: return BarPeriod.M30;
                case Domain.Feed.Types.Timeframe.M15: return BarPeriod.M15;
                case Domain.Feed.Types.Timeframe.M5: return BarPeriod.M5;
                case Domain.Feed.Types.Timeframe.M1: return BarPeriod.M1;
                case Domain.Feed.Types.Timeframe.S10: return BarPeriod.S10;
                case Domain.Feed.Types.Timeframe.S1: return BarPeriod.S1;

                default: throw new ArgumentException("Unsupported time frame: " + timeframe);
            }
        }

        private static Domain.AccountInfo Convert(SFX.AccountInfo info)
        {
            return new Domain.AccountInfo(info.Type != AccountType.Cash ? info.Balance : null, info.Currency, info.Assets.Select(Convert))
            {
                Id = info.AccountId,
                Type = Convert(info.Type),
                Leverage = info.Leverage ?? 1,
            };
        }

        private static Domain.AssetInfo Convert(SFX.AssetInfo info)
        {
            return new Domain.AssetInfo
            {
                Currency = info.Currency,
                Balance = info.Balance,
            };
        }

        public static Domain.OrderInfo Convert(SFX.ExecutionReport record)
        {
            return new Domain.OrderInfo
            {
                Id = record.OrderId,
                Symbol = record.Symbol,
                Comment = record.Comment,
                InitialType = Convert(record.InitialOrderType),
                Type = Convert(record.OrderType),
                Price = record.Price,
                StopPrice = record.StopPrice,
                Side = Convert(record.OrderSide),
                Created = record.Created?.ToTimestamp(),
                Swap = record.Swap,
                Modified = record.Modified?.ToTimestamp(),
                StopLoss = record.StopLoss,
                TakeProfit = record.TakeProfit,
                Slippage = record.Slippage,
                Commission = record.Commission,
                ExecAmount = record.ExecutedVolume,
                UserTag = record.Tag,
                RemainingAmount = record.LeavesVolume,
                RequestedAmount = record.InitialVolume ?? 0,
                Expiration = record.Expiration?.ToUniversalTime().ToTimestamp(),
                MaxVisibleAmount = record.MaxVisibleVolume,
                ExecPrice = record.AveragePrice,
                RequestedOpenPrice = record.InitialPrice,
                Options = GetOptions(record),
                LastFillPrice = record.TradePrice,
                LastFillAmount = record.TradeAmount,
                ParentOrderId = record.ParentOrderId,
                OcoRelatedOrderId = record.RelatedOrderId?.ToString(),
                OtoTrigger = GetOtoTrigger(record),
            };
        }

        private static Domain.OrderOptions GetOptions(SFX.ExecutionReport record)
        {
            var result = Domain.OrderOptions.None;
            var isLimit = record.OrderType == SFX.OrderType.Limit || record.OrderType == SFX.OrderType.StopLimit;

            if (isLimit && record.ImmediateOrCancelFlag)
                result |= Domain.OrderOptions.ImmediateOrCancel;

            if (record.MarketWithSlippage)
                result |= Domain.OrderOptions.MarketWithSlippage;

            if (record.MaxVisibleVolume.HasValue)
                result |= Domain.OrderOptions.HiddenIceberg;

            if (record.OneCancelsTheOtherFlag)
                result |= Domain.OrderOptions.OneCancelsTheOther;

            if (record.ContingentOrderFlag)
                result |= Domain.OrderOptions.ContingentOrder;

            return result;
        }

        private static Domain.ContingentOrderTrigger GetOtoTrigger(SFX.ExecutionReport report)
        {
            if (!report.ContingentOrderFlag)
                return null;

            return new Domain.ContingentOrderTrigger
            {
                Type = ConvertToAlgo(report.TriggerType.Value),
                TriggerTime = report.TriggerTime?.ToTimestamp(),
                OrderIdTriggeredBy = report.OrderIdTriggeredBy?.ToString(),
            };
        }

        private static List<ExecutionReport> ConvertToEr(List<SFX.ExecutionReport> reports)
        {
            var result = new List<ExecutionReport>(reports.Count);

            for (int i = 0; i < reports.Count; i++)
                result.Add(ConvertToEr(reports[i], reports[i].OrigClientOrderId));

            return result;
        }

        private static ExecutionReport ConvertToEr(SFX.ExecutionReport report, string operationId = null)
        {
            if (report.ExecutionType == SFX.ExecutionType.Rejected && report.RejectReason == RejectReason.None)
                report.RejectReason = RejectReason.Other; // Some plumbing. Sometimes we recieve Rejects with no RejectReason

            return new ExecutionReport()
            {
                Id = report.OrderId,
                ParentOrderId = report.ParentOrderId,
                // ExecTime = report.???
                TradeRequestId = operationId,
                Expiration = report.Expiration,
                Created = report.Created,
                Modified = report.Modified,
                RejectReason = Convert(report.RejectReason, report.Text ?? ""),
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Slippage = report.Slippage,
                Text = report.Text,
                Comment = report.Comment,
                UserTag = report.Tag,
                Magic = report.Magic,
                IsReducedOpenCommission = report.ReducedOpenCommission,
                IsReducedCloseCommission = report.ReducedCloseCommission,
                ImmediateOrCancel = report.ImmediateOrCancelFlag,
                MarketWithSlippage = report.MarketWithSlippage,
                TradePrice = report.TradePrice ?? 0,
                RequestedOpenPrice = report.InitialPrice,
                Assets = report.Assets.Select(Convert).ToArray(),
                StopPrice = report.StopPrice,
                AveragePrice = report.AveragePrice,
                ClientOrderId = report.ClientOrderId,
                OrigClientOrderId = report.OrigClientOrderId,
                OrderStatus = Convert(report.OrderStatus),
                ExecutionType = Convert(report.ExecutionType),
                Symbol = report.Symbol,
                ExecutedVolume = report.ExecutedVolume,
                InitialVolume = report.InitialVolume,
                LeavesVolume = report.LeavesVolume,
                MaxVisibleAmount = report.MaxVisibleVolume,
                TradeAmount = report.TradeAmount,
                Commission = report.Commission,
                AgentCommission = report.AgentCommission,
                Swap = report.Swap,
                InitialType = Convert(report.InitialOrderType),
                Type = Convert(report.OrderType),
                Side = Convert(report.OrderSide),
                Price = report.Price,
                Balance = report.Balance ?? double.NaN,
                IsOneCancelsTheOther = report.OneCancelsTheOtherFlag,
                OcoRelatedOrderId = report.RelatedOrderId?.ToString(),
                IsContingentOrder = report.ContingentOrderFlag,
                OtoTrigger = GetOtoTrigger(report),
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

        private static ContingentOrderTriggerType? ConvertToServer(Domain.ContingentOrderTrigger.Types.TriggerType? type)
        {
            switch (type)
            {
                case ContingentOrderTrigger.Types.TriggerType.OnPendingOrderExpired:
                    return ContingentOrderTriggerType.OnPendingOrderExpired;
                case ContingentOrderTrigger.Types.TriggerType.OnPendingOrderPartiallyFilled:
                    return ContingentOrderTriggerType.OnPendingOrderPartiallyFilled;
                case ContingentOrderTrigger.Types.TriggerType.OnTime:
                    return ContingentOrderTriggerType.OnTime;

                default:
                    return null;
            }
        }

        private static Domain.ContingentOrderTrigger.Types.TriggerType ConvertToAlgo(ContingentOrderTriggerType type)
        {
            return (Domain.ContingentOrderTrigger.Types.TriggerType)type;
        }

        private static Domain.OrderExecReport.Types.CmdResultCode Convert(RejectReason reason, string message)
        {
            switch (reason)
            {
                case RejectReason.InternalServerError: return Domain.OrderExecReport.Types.CmdResultCode.TradeServerError;
                case RejectReason.DealerReject: return Domain.OrderExecReport.Types.CmdResultCode.DealerReject;
                case RejectReason.UnknownSymbol: return Domain.OrderExecReport.Types.CmdResultCode.SymbolNotFound;
                case RejectReason.UnknownOrder: return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                case RejectReason.IncorrectQuantity: return Domain.OrderExecReport.Types.CmdResultCode.IncorrectVolume;
                case RejectReason.OffQuotes: return Domain.OrderExecReport.Types.CmdResultCode.OffQuotes;
                case RejectReason.OrderExceedsLimit: return Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney; // open market order with low acc balance
                case RejectReason.OrdersLimitExceeded: return Domain.OrderExecReport.Types.CmdResultCode.ExceededOrderLimit;
                case RejectReason.CloseOnly: return Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading;
                case RejectReason.ThrottlingLimits: return Domain.OrderExecReport.Types.CmdResultCode.ThrottlingError;
                case RejectReason.Other:
                    {
                        if (message != null)
                        {
                            if (message == "Trade Not Allowed" || message == "Trade is not allowed!")
                                return Domain.OrderExecReport.Types.CmdResultCode.TradeNotAllowed;
                            else if (message.StartsWith("Not Enough Money"))
                                return Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney;
                            else if (message.StartsWith("Rejected By Dealer"))
                                return Domain.OrderExecReport.Types.CmdResultCode.DealerReject;
                            else if (message.StartsWith("Dealer") && message.EndsWith("did not respond."))
                                return Domain.OrderExecReport.Types.CmdResultCode.DealingTimeout;
                            else if (message.Contains("locked by another operation"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OrderLocked;
                            else if (message.Contains("Invalid expiration"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectExpiration;
                            else if (message.StartsWith("Price precision"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectPricePrecision;
                            else if (message.EndsWith("because close-only mode on"))
                                return Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading;
                            else if (message.StartsWith("Max visible amount is not valid for") || message.StartsWith("Max visible amount is valid only for"))
                                return Domain.OrderExecReport.Types.CmdResultCode.MaxVisibleVolumeNotSupported;
                            else if (message.StartsWith("Order Not Found") || message.EndsWith("was not found."))
                                return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                            else if (message.StartsWith("Invalid order type") || message.Contains("is not supported") ||
                                    (message.StartsWith("Option") && message.Contains("cannot be used")) ||
                                    message.StartsWith("OCO flag is used only for"))
                                return Domain.OrderExecReport.Types.CmdResultCode.Unsupported;
                            else if (message.StartsWith("Invalid AmountChange") || message == "Cannot modify amount.")
                                return Domain.OrderExecReport.Types.CmdResultCode.InvalidAmountChange;
                            else if (message == "Account Is Readonly")
                                return Domain.OrderExecReport.Types.CmdResultCode.ReadOnlyAccount;
                            else if (message == "Internal server error")
                                return Domain.OrderExecReport.Types.CmdResultCode.TradeServerError;
                            else if (message.StartsWith("Only Limit, Stop and StopLimit orders can be canceled."))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectType;
                            else if (message.Contains("was called too frequently"))
                                return Domain.OrderExecReport.Types.CmdResultCode.ThrottlingError;
                            else if (message.StartsWith("OCO flag requires to specify"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedIdNotFound;
                            else if (message.EndsWith("already has OCO flag!"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedOrderAlreadyExists;
                            else if (message.StartsWith("Buy price must be less than"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectPrice;
                            else if (message.Contains("has different Symbol"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectSymbol;
                            else if (message.EndsWith("Remove OCO relation first.") ||
                                    (message.StartsWith("Related order") && message.EndsWith("already has OCO flag.")))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoAlreadyExists;
                            else if (message.StartsWith("No Dealer"))
                                return Domain.OrderExecReport.Types.CmdResultCode.DealerReject;
                            else if (message.StartsWith("Trigger time cannot") ||
                                     message.Contains("must have the same trigger time"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerTime;
                            else if (message.StartsWith("Trigger Order Id"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerOrderId;
                            else if ((message.Contains("Trigger Order") && message.Contains("Type must be")) ||
                                      message.StartsWith("Contingent orders with type") && message.EndsWith("are not supported.") ||
                                      message.Contains("must have the same trigger type"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerOrderType;
                            else if (message.Contains("Trigger Order") && message.Contains("Expired"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectExpiration;
                            else if (message.EndsWith("cannot be Contingent order"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectConditionsForTrigger;
                            else if (message.StartsWith("Cannot create OCO relation between"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedOrderIncorrectOptions;
                            else if (message.StartsWith("OCO order cannot be related to itself"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoIncorrectRelatedId;
                        }
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message != null && (message.StartsWith("Order Not Found") || message.EndsWith("not found.")))
                            return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                        return Domain.OrderExecReport.Types.CmdResultCode.Ok;
                    }
            }
            return Domain.OrderExecReport.Types.CmdResultCode.UnknownError;
        }

        private static Domain.PositionInfo Convert(SFX.Position p)
        {
            Domain.OrderInfo.Types.Side side;
            double price;
            double amount;

            if (p.BuyAmount > 0)
            {
                side = Domain.OrderInfo.Types.Side.Buy;
                price = p.BuyPrice ?? 0;
                amount = p.BuyAmount;
            }
            else
            {
                side = Domain.OrderInfo.Types.Side.Sell;
                price = p.SellPrice ?? 0;
                amount = p.SellAmount;
            }

            return new Domain.PositionInfo
            {
                Id = p.PosId,
                Symbol = p.Symbol,
                Side = side,
                Volume = amount,
                Price = price,
                Commission = p.Commission,
                Swap = p.Swap,
                Modified = p.Modified?.ToTimestamp(),
            };
        }

        private static Domain.PositionExecReport ConvertToReport(SFX.Position p)
        {
            return new Domain.PositionExecReport
            {
                PositionCopy = Convert(p),
                ExecAction = Convert(p.PosReportType),
            };
        }

        private static Domain.OrderExecReport.Types.ExecAction Convert(SFX.PosReportType type)
        {
            switch (type)
            {
                case SFX.PosReportType.CancelPosition:
                    return Domain.OrderExecReport.Types.ExecAction.Canceled;

                case SFX.PosReportType.ModifyPosition:
                    return Domain.OrderExecReport.Types.ExecAction.Modified;

                case SFX.PosReportType.ClosePosition:
                    return Domain.OrderExecReport.Types.ExecAction.Closed;

                case SFX.PosReportType.Split:
                    return Domain.OrderExecReport.Types.ExecAction.Splitted;

                default:
                    return Domain.OrderExecReport.Types.ExecAction.None;
            }
        }


        internal static Domain.BarData Convert(SFX.Bar fdkBar)
        {
            return new Domain.BarData()
            {
                Open = fdkBar.Open,
                Close = fdkBar.Close,
                High = fdkBar.High,
                Low = fdkBar.Low,
                RealVolume = fdkBar.Volume,
                OpenTime = fdkBar.From.ToUniversalTime().ToTimestamp(),
                CloseTime = fdkBar.To.ToUniversalTime().ToTimestamp(),
            };
        }

        internal static Domain.QuoteInfo[] Convert(SFX.Quote[] fdkQuoteSnapshot)
        {
            var result = new Domain.QuoteInfo[fdkQuoteSnapshot.Length];

            for (var i = 0; i < result.Length; i++)
                result[i] = Convert(fdkQuoteSnapshot[i]);
            return result;
        }

        private static Domain.QuoteInfo Convert(SFX.Quote fdkTick)
        {
            var timeOfReceive = DateTime.UtcNow;

            var data = new Domain.QuoteData()
            {
                Time = fdkTick.CreatingTime.ToTimestamp(),
                IsBidIndicative = fdkTick.TickType == SFX.TickTypes.IndicativeBid || fdkTick.TickType == SFX.TickTypes.IndicativeBidAsk,
                IsAskIndicative = fdkTick.TickType == SFX.TickTypes.IndicativeAsk || fdkTick.TickType == SFX.TickTypes.IndicativeBidAsk,
                BidBytes = ConvertLevel2(fdkTick.Bids),
                AskBytes = ConvertLevel2(fdkTick.Asks),
            };

            return new Domain.QuoteInfo(fdkTick.Symbol, data, timeOfReceive: timeOfReceive);
        }

        private static ByteString ConvertLevel2(List<QuoteEntry> book)
        {
            var cnt = book.Count;
            var bands = cnt > 256
                ? new Domain.QuoteBand[cnt].AsSpan()
                : stackalloc Domain.QuoteBand[cnt];

            for (var i = 0; i < cnt; i++)
            {
                bands[i] = new Domain.QuoteBand(book[i].Price, book[i].Volume);
            }

            return Domain.ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<Domain.QuoteBand, byte>(bands));
        }

        public static Domain.TradeReportInfo Convert(TradeTransactionReport report)
        {
            bool isBalanceTransaction = report.TradeTransactionReportType == TradeTransactionReportType.Credit
                || report.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

            var userTag = report.Tag;
            var instanceId = "";

            if (Domain.CompositeTag.TryParse(report.Tag, out var tag))
            {
                instanceId = tag?.Key;
                userTag = tag?.Tag;
            }

            return new Domain.TradeReportInfo()
            {
                IsEmulated = false,
                Id = report.Id,
                OrderId = report.Id,
                TransactionReason = Convert(report.TradeTransactionReason),
                ReportType = Convert(report.TradeTransactionReportType),
                Symbol = isBalanceTransaction ? report.TransactionCurrency : report.Symbol,
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Comment = report.Comment,
                Commission = report.Commission,
                CommissionCurrency = report.CommCurrency ?? report.DstAssetCurrency ?? report.TransactionCurrency,
                OpenQuantity = report.Quantity,
                PositionCloseQuantity = report.PositionLastQuantity,
                PositionClosePrice = report.PositionClosePrice,
                Swap = report.Swap,
                RemainingQuantity = report.LeavesQuantity,
                AccountBalance = report.AccountBalance,
                ActionId = report.ActionId,
                DstAssetAmount = report.DstAssetAmount,
                DstAssetCurrency = report.DstAssetCurrency,
                DstAssetMovement = report.DstAssetMovement,
                Expiration = report.Expiration?.ToUniversalTime().ToTimestamp(),
                OrderOptions = GetOptions(report),
                MarginCurrency = report.MarginCurrency,
                MaxVisibleQuantity = report.MaxVisibleQuantity,
                OrderOpened = report.OrderCreated.ToUniversalTime().ToTimestamp(),
                OrderFillPrice = report.OrderFillPrice,
                OrderLastFillAmount = report.OrderLastFillAmount,
                OrderModified = report.OrderModified.ToUniversalTime().ToTimestamp(),
                PositionById = report.PositionById,
                PositionClosed = report.PositionClosed.ToUniversalTime().ToTimestamp(),
                PositionId = report.PositionId,
                PositionLeavesQuantity = report.PositionLeavesQuantity,
                PositionModified = report.PositionModified.ToUniversalTime().ToTimestamp(),
                PositionOpened = report.PositionOpened.ToUniversalTime().ToTimestamp(),
                PositionQuantity = report.PositionQuantity,
                PositionOpenPrice = report.PosOpenPrice,
                PositionRemainingPrice = report.PosRemainingPrice,
                PositionRemainingSide = Convert(report.PosRemainingSide),
                Price = report.Price,
                ProfitCurrency = report.ProfitCurrency,
                RequestedClosePrice = report.ReqClosePrice,
                RequestedCloseQuantity = report.ReqCloseQuantity,
                RequestedOpenPrice = report.ReqOpenPrice,
                SrcAssetAmount = report.SrcAssetAmount,
                SrcAssetCurrency = report.SrcAssetCurrency,
                SrcAssetMovement = report.SrcAssetMovement,
                OrderSide = Convert(report.OrderSide),
                OrderType = Convert(report.OrderType),
                RequestedOpenQuantity = report.ReqOpenQuantity,
                StopPrice = report.StopPrice,
                Tag = userTag,
                InstanceId = instanceId,
                TransactionAmount = report.TransactionAmount,
                TransactionCurrency = report.TransactionCurrency,
                TransactionTime = report.TransactionTime.ToUniversalTime().ToTimestamp(),
                RequestedOrderType = Convert(report.ReqOrderType != null ? report.ReqOrderType.Value : report.OrderType),
                SplitRatio = report.SplitRatio,
                Tax = report.Tax,
                Slippage = report.Slippage,
                OcoRelatedOrderId = report.RelatedOrderId?.ToString(),
            };
        }

        private static Domain.TradeReportInfo.Types.ReportType Convert(TradeTransactionReportType type)
        {
            switch (type)
            {
                case TradeTransactionReportType.BalanceTransaction: return Domain.TradeReportInfo.Types.ReportType.BalanceTransaction;
                case TradeTransactionReportType.Credit: return Domain.TradeReportInfo.Types.ReportType.Credit;
                case TradeTransactionReportType.OrderActivated: return Domain.TradeReportInfo.Types.ReportType.OrderActivated;
                case TradeTransactionReportType.OrderCanceled: return Domain.TradeReportInfo.Types.ReportType.OrderCanceled;
                case TradeTransactionReportType.OrderExpired: return Domain.TradeReportInfo.Types.ReportType.OrderExpired;
                case TradeTransactionReportType.OrderFilled: return Domain.TradeReportInfo.Types.ReportType.OrderFilled;
                case TradeTransactionReportType.OrderOpened: return Domain.TradeReportInfo.Types.ReportType.OrderOpened;
                case TradeTransactionReportType.PositionClosed: return Domain.TradeReportInfo.Types.ReportType.PositionClosed;
                case TradeTransactionReportType.PositionOpened: return Domain.TradeReportInfo.Types.ReportType.PositionOpened;
                case TradeTransactionReportType.TradeModified: return Domain.TradeReportInfo.Types.ReportType.TradeModified;
                default: return Domain.TradeReportInfo.Types.ReportType.NoType;
            }
        }

        private static Domain.TradeReportInfo.Types.Reason Convert(SFX.TradeTransactionReason reason)
        {
            switch (reason)
            {
                case SFX.TradeTransactionReason.ClientRequest: return Domain.TradeReportInfo.Types.Reason.ClientRequest;
                case SFX.TradeTransactionReason.PendingOrderActivation: return Domain.TradeReportInfo.Types.Reason.PendingOrderActivation;
                case SFX.TradeTransactionReason.StopOut: return Domain.TradeReportInfo.Types.Reason.StopOut;
                case SFX.TradeTransactionReason.StopLossActivation: return Domain.TradeReportInfo.Types.Reason.StopLossActivation;
                case SFX.TradeTransactionReason.TakeProfitActivation: return Domain.TradeReportInfo.Types.Reason.TakeProfitActivation;
                case SFX.TradeTransactionReason.DealerDecision: return Domain.TradeReportInfo.Types.Reason.DealerDecision;
                case SFX.TradeTransactionReason.Rollover: return Domain.TradeReportInfo.Types.Reason.Rollover;
                case SFX.TradeTransactionReason.DeleteAccount: return Domain.TradeReportInfo.Types.Reason.DeleteAccount;
                case SFX.TradeTransactionReason.Expired: return Domain.TradeReportInfo.Types.Reason.Expired;
                case SFX.TradeTransactionReason.TransferMoney: return Domain.TradeReportInfo.Types.Reason.TransferMoney;
                case SFX.TradeTransactionReason.Split: return Domain.TradeReportInfo.Types.Reason.Split;
                case SFX.TradeTransactionReason.Dividend: return Domain.TradeReportInfo.Types.Reason.Dividend;
                case SFX.TradeTransactionReason.OneCancelsTheOther: return Domain.TradeReportInfo.Types.Reason.OneCancelsTheOther;
                default: return Domain.TradeReportInfo.Types.Reason.NoReason;
            }
        }

        private static Domain.OrderOptions GetOptions(TradeTransactionReport report)
        {
            var res = Domain.OrderOptions.None;

            if (report.ImmediateOrCancel)
                res |= Domain.OrderOptions.ImmediateOrCancel;
            if (report.MarketWithSlippage)
                res |= Domain.OrderOptions.MarketWithSlippage;
            if (report.MaxVisibleQuantity.HasValue)
                res |= Domain.OrderOptions.HiddenIceberg;
            if (report.RelatedOrderId != null)
                res |= Domain.OrderOptions.OneCancelsTheOther;

            return res;
        }

        public static Domain.TriggerReportInfo Convert(SFX.ContingentOrderTriggerReport report)
        {
            return new Domain.TriggerReportInfo
            {
                Id = report.Id,
                ContingentOrderId = report.ContingentOrderId.ToString(),
                TransactionTime = report.TransactionTime.ToTimestamp(),
                TriggerType = ConvertToAlgo(report.TriggerType),
                TriggerState = Convert(report.TriggerState),
                TriggerTime = report.TriggerTime?.ToTimestamp(),
                OrderIdTriggeredBy = report.OrderIdTriggeredBy?.ToString(),
                Symbol = report.Symbol,
                Type = Convert(report.Type),
                Side = Convert(report.Side),
                Price = report.Price,
                StopPrice = report.StopPrice,
                Amount = report.Amount,
                RelatedOrderId = report.RelatedOrderId?.ToString(),
            };
        }

        public static Domain.TriggerReportInfo.Types.TriggerResultState Convert(TriggerResultState state)
        {
            return (Domain.TriggerReportInfo.Types.TriggerResultState)state;
        }

        public static Domain.BalanceOperation Convert(SFX.BalanceOperation op)
        {
            return new Domain.BalanceOperation
            {
                Balance = op.Balance,
                Currency = op.TransactionCurrency,
                TransactionAmount = op.TransactionAmount,
                Type = Convert(op.TransactionType),
            };
        }

        public static Domain.BalanceOperation.Types.Type Convert(SFX.BalanceTransactionType type)
        {
            switch (type)
            {
                case BalanceTransactionType.DepositWithdrawal:
                    return Domain.BalanceOperation.Types.Type.DepositWithdrawal;
                case BalanceTransactionType.Dividend:
                    return Domain.BalanceOperation.Types.Type.Dividend;
            }

            throw new NotImplementedException("Unsupported balance transaction type: " + type);
        }

        public static PriceType ConvertBack(Domain.Feed.Types.MarketSide marketSide)
        {
            switch (marketSide)
            {
                case Domain.Feed.Types.MarketSide.Ask: return PriceType.Ask;
                case Domain.Feed.Types.MarketSide.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported market side: " + marketSide);
        }

        private static ConnectionErrorInfo.Types.ErrorCode Convert(LogoutReason fdkCode)
        {
            switch (fdkCode)
            {
                case LogoutReason.BlockedAccount: return ConnectionErrorInfo.Types.ErrorCode.BlockedAccount;
                case LogoutReason.InvalidCredentials: return ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;
                case LogoutReason.NetworkError: return ConnectionErrorInfo.Types.ErrorCode.NetworkError;
                case LogoutReason.ServerError: return ConnectionErrorInfo.Types.ErrorCode.ServerError;
                case LogoutReason.ServerLogout: return ConnectionErrorInfo.Types.ErrorCode.ServerLogout;
                case LogoutReason.SlowConnection: return ConnectionErrorInfo.Types.ErrorCode.SlowConnection;
                case LogoutReason.LoginDeleted: return ConnectionErrorInfo.Types.ErrorCode.LoginDeleted;
                default: return ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
            }
        }
        #endregion
    }
}
