using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.FDK.Common;
using SFX = TickTrader.FDK.Common;

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
            _tradeProxy.PositionUpdateEvent += (c, rep) => PositionReport?.Invoke(ConvertToReport(rep));
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

        public async Task<Domain.CurrencyInfo[]> GetCurrencies()
        {
            var currencies = await _feedProxy.GetCurrencyListAsync();
            return currencies.Select(Convert).ToArray();
        }

        public async Task<Domain.SymbolInfo[]> GetSymbols()
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

        public event Action<Domain.PositionExecReport> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<Domain.TradeReportInfo> TradeTransactionReport;
        public event Action<Domain.BalanceOperation> BalanceOperation;
        public event Action<Domain.SymbolInfo[]> SymbolInfo { add { } remove { } }
        public event Action<CurrencyEntity[]> CurrencyInfo { add { } remove { } }

        public Task<Domain.AccountInfo> GetAccountInfo()
        {
            return _tradeProxyAdapter.GetAccountInfoAsync()
                .ContinueWith(t => Convert(t.Result));
        }

        public void GetTradeRecords(BlockingChannel<Domain.OrderInfo> rxStream)
        {
            _tradeProxy.GetOrdersAsync(rxStream);
        }

        public Task<Domain.PositionInfo[]> GetPositions()
        {
            return _tradeProxyAdapter.GetPositionsAsync()
                .ContinueWith(t => t.Result.Select(Convert).ToArray());
        }

        public void GetTradeHistory(BlockingChannel<Domain.TradeReportInfo> rxStream, DateTime? from, DateTime? to, bool skipCancelOrders, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            _tradeHistoryProxy.DownloadTradesAsync(direction, from?.ToUniversalTime(), to?.ToUniversalTime(), skipCancelOrders, rxStream);
        }

        public Task<OrderInteropResult> SendOpenOrder(Domain.OpenOrderRequest request)
        {
            return ExecuteOrderOperation(request, r =>
            {
                var timeInForce = GetTimeInForce(r.Expiration);
                var ioc = GetIoC(r.ExecOptions);

                return _tradeProxyAdapter.NewOrderAsync(r.OperationId, r.Symbol, Convert(r.Type), Convert(r.Side), r.Amount, r.MaxVisibleAmount,
                    r.Price, r.StopPrice, timeInForce, r.Expiration?.ToDateTime(), r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, ioc, r.Slippage);
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
                return ExecuteOrderOperation(request, r => _tradeProxyAdapter.ReplaceOrderAsync(r.OperationId, "",
                    r.OrderId, r.Symbol, Convert(r.Type), Convert(r.Side), r.AmountChange,
                    r.MaxVisibleAmount, r.Price, r.StopPrice, GetTimeInForceReplace(r.ExecOptions, r.Expiration), r.Expiration?.ToDateTime(),
                    r.StopLoss, r.TakeProfit, r.Comment, r.Tag, null, GetIoCReplace(r.ExecOptions), r.Slippage));
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
                var result = await operationDef(request);
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.Ok, ConvertToEr(result, operationId));
            }
            catch (ExecutionException ex)
            {
                var reason = Convert(ex.Report.RejectReason, ex.Message);
                return new OrderInteropResult(reason, ConvertToEr(ex.Report, operationId));
            }
            catch (DisconnectException)
            {
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.ConnectionError);
            }
            catch (InteropException ex) when (ex.ErrorCode == ConnectionErrorCodes.RejectedByServer)
            {
                // workaround for inconsistent server logic
                return new OrderInteropResult(Convert(RejectReason.Other, ex.Message));
            }
            catch (NotSupportedException)
            {
                return new OrderInteropResult(Domain.OrderExecReport.Types.CmdResultCode.Unsupported);
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
            return options.HasValue && options.Value.IsFlagSet(Domain.OrderExecOptions.ImmediateOrCancel);
        }

        private OrderTimeInForce GetTimeInForce(Timestamp expiration)
        {
            return expiration != null ? OrderTimeInForce.GoodTillDate : OrderTimeInForce.GoodTillCancel;
        }

        private bool GetIoC(Domain.OrderExecOptions options)
        {
            return options.IsFlagSet(Domain.OrderExecOptions.ImmediateOrCancel);
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
                case Domain.OrderInfo.Types.Type.Limit: return SFX.OrderType.Limit;
                case Domain.OrderInfo.Types.Type.Market: return SFX.OrderType.Market;
                case Domain.OrderInfo.Types.Type.Position: return SFX.OrderType.Position;
                case Domain.OrderInfo.Types.Type.Stop: return SFX.OrderType.Stop;
                case Domain.OrderInfo.Types.Type.StopLimit: return SFX.OrderType.StopLimit;

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
                RequestedOpenPrice = report.InitialPrice,
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
                case RejectReason.OrderExceedsLImit: return Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney;
                case RejectReason.CloseOnly: return Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading;
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
                            else if (message == "Max visible amount is not valid for market orders" || message.StartsWith("Max visible amount is valid only for"))
                                return Domain.OrderExecReport.Types.CmdResultCode.MaxVisibleVolumeNotSupported;
                            else if (message.StartsWith("Order Not Found") || message.EndsWith("was not found."))
                                return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                            else if (message.StartsWith("Invalid order type") || message.Contains("is not supported"))
                                return Domain.OrderExecReport.Types.CmdResultCode.Unsupported;
                            else if (message.StartsWith("Invalid AmountChange") || message == "Cannot modify amount.")
                                return Domain.OrderExecReport.Types.CmdResultCode.InvalidAmountChange;
                            else if (message == "Account Is Readonly")
                                return Domain.OrderExecReport.Types.CmdResultCode.ReadOnlyAccount;
                            else if (message == "Internal server error")
                                return Domain.OrderExecReport.Types.CmdResultCode.TradeServerError;
                            else if (message.StartsWith("Only Limit, Stop and StopLimit orders can be canceled."))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectType;
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
            PositionSide buy, sell;

            if (p.BuyAmount > 0)
            {
                side = Domain.OrderInfo.Types.Side.Buy;
                price = p.BuyPrice ?? 0;
                amount = p.BuyAmount;
                buy = new PositionSide(amount, price);
                sell = new PositionSide(0, 0);
            }
            else
            {
                side = Domain.OrderInfo.Types.Side.Sell;
                price = p.SellPrice ?? 0;
                amount = p.SellAmount;
                buy = new PositionSide(0, 0);
                sell = new PositionSide(amount, price);
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
                Long = buy,
                Short = sell,
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

        public static Domain.TradeReportInfo Convert(TradeTransactionReport report)
        {
            bool isBalanceTransaction = report.TradeTransactionReportType == TradeTransactionReportType.Credit
                || report.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

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
                Tag = report.Tag,
                TransactionAmount = report.TransactionAmount,
                TransactionCurrency = report.TransactionCurrency,
                TransactionTime = report.TransactionTime.ToUniversalTime().ToTimestamp(),
                RequestedOrderType = Convert(report.ReqOrderType != null ? report.ReqOrderType.Value : report.OrderType),
                SplitRatio = report.SplitRatio,
                Tax = report.Tax,
                Slippage = report.Slippage,
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

            return res;
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

        public static PriceType ConvertBack(BarPriceType priceType)
        {
            switch (priceType)
            {
                case Api.BarPriceType.Ask: return PriceType.Ask;
                case Api.BarPriceType.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported price type: " + priceType);
        }

        private static ConnectionErrorCodes Convert(LogoutReason fdkCode)
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
