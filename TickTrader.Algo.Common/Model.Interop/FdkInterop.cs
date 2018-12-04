using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Lib;
using SoftFX.Extended.Reports;
using ActorSharp;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model
{
    internal class FdkInterop : CrossDomainObject, IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private static IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FdkInterop>();

        private object _lockObj = new object();
        private ConnectionOptions _options;
        private DataFeed _feedProxy;
        private DataTrade _tradeProxy;
        private FdkAsyncExecutor _executor;
        private ActionBlock<Task> requestProcessor;

        private TaskCompletionSource<ConnectionErrorInfo> _connectEvent;
        private bool isFeedLoggedIn;
        private bool isTradeLoggedIn;
        private bool isFeedCacheLoaded;
        private bool isTradeCacheLoaded;
        private bool isSymbolsLoaded;

        public event Action<IServerInterop, ConnectionErrorInfo> Disconnected;

        public IFeedServerApi FeedApi => this;
        public ITradeServerApi TradeApi => this;

        public bool AutoAccountInfo => true;
        public bool AutoSymbols => true;

        public FdkInterop(ConnectionOptions options)
        {
            _options = options;
        }

        public async Task<ConnectionErrorInfo> Connect(string address, string login, string password, CancellationToken cancelToken)
        {
            if (_options.EnableLogs)
            {
                if (!Directory.Exists(_options.LogsFolder))
                    Directory.CreateDirectory(_options.LogsFolder);
            }

            isFeedLoggedIn = false;
            isTradeLoggedIn = false;
            isFeedCacheLoaded = false;
            isTradeCacheLoaded = false;
            isSymbolsLoaded = false;

            CreateFeedProxy(address, login, password, _options.EnableLogs);
            CreateTradeProxy(address, login, password, _options.EnableLogs);

            _connectEvent = new TaskCompletionSource<ConnectionErrorInfo>();

            _feedProxy.Tick += (s, e) => Tick?.Invoke(FdkConvertor.Convert(e.Tick));
            _feedProxy.SymbolInfo += (s, e) => SymbolInfo?.Invoke(FdkConvertor.Convert(e.Information));
            _feedProxy.CurrencyInfo += (s, e) => CurrencyInfo?.Invoke(FdkConvertor.Convert(e.Information));
            _tradeProxy.PositionReport += (s, a) => PositionReport?.Invoke(FdkConvertor.Convert(a.Report));
            _tradeProxy.ExecutionReport += (s, a) => ExecutionReport?.Invoke(FdkConvertor.Convert(a.Report));
            _tradeProxy.BalanceOperation += (s, a) => BalanceOperation?.Invoke(FdkConvertor.Convert(a.Data));
            _tradeProxy.TradeTransactionReport += (s, a) => TradeTransactionReport?.Invoke(FdkConvertor.Convert(a.Report));

            _feedProxy.Start();
            _tradeProxy.Start();

            var result =  await _connectEvent.Task;

            if (result.Code == ConnectionErrorCodes.None)
            {
                requestProcessor = TaskMahcine.Create();
                _executor = new FdkAsyncExecutor(_tradeProxy);
            }

            return result;
        }

        private void CreateFeedProxy(string address, string login, string password, bool logsEnabled)
        {
            FixConnectionStringBuilder feedCs = new FixConnectionStringBuilder()
            {
                TargetCompId = "EXECUTOR",
                ProtocolVersion = FixProtocolVersion.TheLatestVersion.ToString(),
                SecureConnection = true,
                Port = 5003,
                DecodeLogFixMessages = true
            };

            feedCs.Address = address;
            feedCs.Username = login;
            feedCs.Password = password;

            if (logsEnabled)
            {
                feedCs.FixEventsFileName = "feed.events.log";
                feedCs.FixMessagesFileName = "feed.messages.log";
                feedCs.FixLogDirectory = _options.LogsFolder;
            }

            feedCs.ExcludeMessagesFromLogs = "y|0";

            _feedProxy = new DataFeed(feedCs.ToString());
            _feedProxy.Logout += feedProxy_Logout;
            _feedProxy.Logon += feedProxy_Logon;
            _feedProxy.CacheInitialized += FeedProxy_CacheInitialized;
            _feedProxy.SymbolInfo += FeedProxy_SymbolInfo;
        }

        private void CreateTradeProxy(string address, string login, string password, bool logsEnabled)
        {
            FixConnectionStringBuilder tradeCs = new FixConnectionStringBuilder()
            {
                TargetCompId = "EXECUTOR",
                ProtocolVersion = FixProtocolVersion.TheLatestVersion.ToString(),
                SecureConnection = true,
                Port = 5004,
                DecodeLogFixMessages = true
            };

            tradeCs.Address = address;
            tradeCs.Username = login;
            tradeCs.Password = password;

            if (logsEnabled)
            {
                tradeCs.FixEventsFileName = "trade.events.log";
                tradeCs.FixMessagesFileName = "trade.messages.log";
                tradeCs.FixLogDirectory = _options.LogsFolder;
            }

            _tradeProxy = new DataTrade(tradeCs.ToString());
            _tradeProxy.SynchOperationTimeout = 5 * 60 * 1000;
            _tradeProxy.Logout += tradeProxy_Logout;
            _tradeProxy.Logon += tradeProxy_Logon;
            _tradeProxy.CacheInitialized += TradeProxy_CacheInitialized;
        }

        private void UpdateState(Action updateAction)
        {
            lock (_lockObj)
            {
                updateAction();
                if (isFeedCacheLoaded && isTradeCacheLoaded && isSymbolsLoaded && isTradeLoggedIn && isFeedLoggedIn)
                    _connectEvent.TrySetResult(new ConnectionErrorInfo(ConnectionErrorCodes.None));
            }
        }

        public async Task Disconnect()
        {
            if (requestProcessor != null)
            {
                requestProcessor.Complete();
                await requestProcessor.Completion;
            }

            var stopTradeTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    _tradeProxy.Logout -= tradeProxy_Logout;
                    _tradeProxy.Logon -= tradeProxy_Logon;
                    _tradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
                    _tradeProxy.Stop();
                    _tradeProxy.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });

            var stopFeedTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    _feedProxy.Logout -= feedProxy_Logout;
                    _feedProxy.Logon -= feedProxy_Logon;
                    _feedProxy.CacheInitialized -= FeedProxy_CacheInitialized;
                    _feedProxy.SymbolInfo -= FeedProxy_SymbolInfo;
                    _feedProxy.Stop();
                    _feedProxy.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });

            await Task.WhenAll(stopFeedTask, stopTradeTask);
        }

        private void FeedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        {
            logger.Debug("EVENT Feed.SymbolInfo");
            UpdateState(() => isSymbolsLoaded = true);
        }

        void feedProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        {
            logger.Debug("EVENT Feed.Logon");
            UpdateState(() => isFeedLoggedIn = true);
        }

        void tradeProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        {
            logger.Debug("EVENT Trade.Logon");
            UpdateState(() => isTradeLoggedIn = true);
        }

        private void FeedProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            logger.Debug("EVENT Feed.CacheInitialized");
            UpdateState(() => isFeedCacheLoaded = true);
        }

        private void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            logger.Debug("EVENT Trade.CacheInitialized");
            UpdateState(() => isTradeCacheLoaded = true);
        }

        void feedProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            UpdateState(() =>
            {
                var info = new ConnectionErrorInfo(Convert(e.Reason), e.Text);

                if (!_connectEvent.TrySetResult(info))
                    Disconnected?.Invoke(this, info);
            });
        }

        void tradeProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            UpdateState(() =>
            {
                var info = new ConnectionErrorInfo(Convert(e.Reason), e.Text);

                if (!_connectEvent.TrySetResult(info))
                    Disconnected?.Invoke(this, info);
            });
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

        private Exception Convert(Exception fdkException)
        {
            if (fdkException is AggregateException)
                return Convert((AggregateException)fdkException).InnerException;

            return new InteropException(fdkException.Message, ConnectionErrorCodes.NetworkError);
        }

        #region IFeedServerApi

        public event Action<QuoteEntity> Tick;
        public event Action<SymbolEntity[]> SymbolInfo;
        public event Action<CurrencyEntity[]> CurrencyInfo;

        public CurrencyEntity[] Currencies => _feedProxy.Cache.Currencies.Select(FdkConvertor.Convert).ToArray();
        public SymbolEntity[] Symbols => _feedProxy.Cache.Symbols.Select(FdkConvertor.Convert).ToArray();

        public Task<CurrencyEntity[]> GetCurrencies()
        {
            return Task.FromResult(Currencies);
        }

        public Task<SymbolEntity[]> GetSymbols()
        {
            return Task.FromResult(Symbols);
        }

        public Task<QuoteEntity[]> SubscribeToQuotes(string[] symbols, int depth)
        {
            return requestProcessor.EnqueueTask(() =>
            {
                _feedProxy.Server.SubscribeToQuotes(symbols, depth);
                return new QuoteEntity[0];
            });
        }

        public Task<QuoteEntity[]> GetQuoteSnapshot(string[] symbols, int depth)
        {
            return Task.FromResult(new QuoteEntity[0]);
        }

        public void DownloadBars(BlockingChannel<BarEntity> stream, string symbol, DateTime from, DateTime to, BarPriceType priceType, TimeFrames barPeriod)
        {
            var fdkPriceType = FdkConvertor.Convert(priceType);
            var fdkBarPeriod = FdkConvertor.ToBarPeriod(barPeriod);

            requestProcessor.EnqueueTask(() =>
            {
                try
                {
                    var bars = _feedProxy.Server.GetBarsHistory(symbol, fdkPriceType, fdkBarPeriod, from, to);
                    stream.WriteAll(bars.ConvertAndFilter(from));
                    stream.Close();
                }
                catch (Exception ex)
                {
                    stream.Close(ex);
                }
            });
        }

        public Task<BarEntity[]> DownloadBarPage(string symbol, DateTime from, int count, BarPriceType priceType, TimeFrames barPeriod)
        {
            var fdkPriceType = FdkConvertor.Convert(priceType);
            var fdkBarPeriod = FdkConvertor.ToBarPeriod(barPeriod);

            return requestProcessor.EnqueueTask(() =>
            {
                try
                {
                    var result = _feedProxy.Server.GetHistoryBars(symbol, from, count, fdkPriceType, fdkBarPeriod);
                    var barArray = FdkConvertor.Convert(result.Bars).ToArray();
                    System.Diagnostics.Debug.WriteLine($"Downloaded {barArray.Length} bars");
                    if (count < 0)
                        Array.Reverse(barArray);
                    return barArray;
                }
                catch (Exception ex)
                {
                    throw Convert(ex);
                }
            });
        }

        public void DownloadQuotes(BlockingChannel<QuoteEntity> stream, string symbol, DateTime from, DateTime to, bool includeLevel2)
        {
            throw new NotImplementedException();
        }

        public Task<QuoteEntity[]> DownloadQuotePage(string symbol, DateTime from, int count, bool includeLevel2)
        {
            throw new NotSupportedException("FDK does not support getting quotes");
        }

        public Task<Tuple<DateTime, DateTime>> GetAvailableRange(string symbol, BarPriceType priceType, TimeFrames timeFrame)
        {
            return requestProcessor.EnqueueTask(() =>
            {
                if (timeFrame.IsTicks())
                {
                    var fakeTimePoint = new DateTime(2017, 1, 1);
                    var result = _feedProxy.Server.GetHistoryBars(symbol, fakeTimePoint, 1, FdkConvertor.Convert(priceType), FdkConvertor.ToBarPeriod(timeFrame));
                    return new Tuple<DateTime, DateTime>(result.FromAll, result.ToAll);
                }
                else //bars
                {
                    var fakeTimePoint = new DateTime(1990, 1, 1);
                    var result = _feedProxy.Server.GetHistoryBars(symbol, fakeTimePoint, 1, FdkConvertor.Convert(priceType), FdkConvertor.ToBarPeriod(timeFrame));
                    if (result.Bars != null && result.Bars.Length > 0)
                        return new Tuple<DateTime, DateTime>(result.Bars[0].From, result.ToAll);
                    else
                        return new Tuple<DateTime, DateTime>(result.FromAll, result.ToAll);
                }
            });
        }

        #endregion

        #region ITradeServerApi

        public AccountEntity AccountInfo => FdkConvertor.Convert(_tradeProxy.Cache.AccountInfo);
        public PositionEntity[] Positions => _tradeProxy.Cache.Positions.Select(FdkConvertor.Convert).ToArray();

        public event Action<PositionEntity> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<TradeReportEntity> TradeTransactionReport;
        public event Action<BalanceOperationReport> BalanceOperation;

        public Task<AccountEntity> GetAccountInfo()
        {
            return Task.FromResult(AccountInfo);
        }

        public Task<PositionEntity[]> GetPositions()
        {
            return Task.FromResult(Positions);
        }

        public void GetTradeRecords(BlockingChannel<OrderEntity> rxStream)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var order in _tradeProxy.Cache.TradeRecords)
                        rxStream.Write(FdkConvertor.Convert(order));

                    rxStream.Close();
                }
                catch(Exception ex)
                {
                    rxStream.Close(ex);
                }
            });
        }

        public void GetTradeHistory(BlockingChannel<TradeReportEntity> txStream,  DateTime? from, DateTime? to, bool skipCancelOrders, bool backwards)
        {
            var direction = backwards ? TimeDirection.Backward : TimeDirection.Forward;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var fdkStream = _tradeProxy.Server.GetTradeTransactionReports(direction, true, from, to, 1000, skipCancelOrders))
                    {

                        while (!fdkStream.EndOfStream)
                        {
                            var report = FdkConvertor.Convert(fdkStream.Item);
                            if (!txStream.Write(report))
                                break;
                            fdkStream.Next();
                        }

                        txStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    var aggeEx = ex as AggregateException;
                    if (aggeEx == null || !(aggeEx.InnerException is TaskCanceledException))
                        txStream.Close(ex);
                }
            });
        }

        public Task<OrderInteropResult> SendOpenOrder(OpenOrderRequest request)
        {
            return _executor.SendOpenOrder(request)
                .ContinueWith(t => new OrderInteropResult(t.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<OrderInteropResult> SendCancelOrder(CancelOrderRequest request)
        {
            return _executor.SendCancelOrder(request)
                .ContinueWith(t => new OrderInteropResult(t.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<OrderInteropResult> SendModifyOrder(ReplaceOrderRequest request)
        {
            return _executor.SendModifyOrder(request)
                .ContinueWith(t => new OrderInteropResult(t.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<OrderInteropResult> SendCloseOrder(CloseOrderRequest request)
        {
            return _executor.SendCloseOrder(request)
                .ContinueWith(t => new OrderInteropResult(t.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        #endregion
    }
}
