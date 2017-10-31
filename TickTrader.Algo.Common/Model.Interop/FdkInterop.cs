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

namespace TickTrader.Algo.Common.Model
{
    public class FdkInterop : IServerInterop, IFeedServerApi, ITradeServerApi
    {
        private static IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<FdkInterop>();

        private object _lockObj = new object();
        private ConnectionOptions _options;
        private DataFeed _feedProxy;
        private DataTrade _tradeProxy;
        private FdkAsyncExecutor _executor;

        private TaskCompletionSource<ConnectionErrorCodes> _connectEvent;
        private bool isFeedLoggedIn;
        private bool isTradeLoggedIn;
        private bool isFeedCacheLoaded;
        private bool isTradeCacheLoaded;
        private bool isSymbolsLoaded;

        public event Action<IServerInterop, ConnectionErrorCodes> Disconnected;

        public IFeedServerApi FeedApi => this;
        public ITradeServerApi TradeApi => this;

        public FdkInterop(ConnectionOptions options)
        {
            _options = options;
        }

        //private void Init()
        //{
        //    try
        //    {
        //        bool logsEnabled = options.EnableFixLogs;

        //        if (logsEnabled)
        //        {
        //            if (!Directory.Exists(LogPath))
        //                Directory.CreateDirectory(LogPath);
        //        }

        //        isFeedLoggedIn = false;
        //        isTradeLoggedIn = false;
        //        isFeedCacheLoaded = false;
        //        isTradeCacheLoaded = false;
        //        isSymbolsLoaded = false;

        //        FixConnectionStringBuilder feedCs = new FixConnectionStringBuilder()
        //        {
        //            TargetCompId = "EXECUTOR",
        //            ProtocolVersion = FixProtocolVersion.TheLatestVersion.ToString(),
        //            SecureConnection = true,
        //            Port = 5003,
        //            DecodeLogFixMessages = true
        //        };

        //        feedCs.Address = CurrentServer;
        //        feedCs.Username = CurrentLogin;
        //        feedCs.Password = CurrentPassword;

        //        if (logsEnabled)
        //        {
        //            feedCs.FixEventsFileName = "feed.events.log";
        //            feedCs.FixMessagesFileName = "feed.messages.log";
        //            feedCs.FixLogDirectory = LogPath;
        //        }
        //        feedCs.ExcludeMessagesFromLogs = "y|0";

        //        feedProxy = new DataFeed(feedCs.ToString());
        //        feedProxy.Logout += feedProxy_Logout;
        //        feedProxy.Logon += feedProxy_Logon;
        //        feedProxy.CacheInitialized += FeedProxy_CacheInitialized;
        //        feedProxy.SymbolInfo += FeedProxy_SymbolInfo;

        //        FixConnectionStringBuilder tradeCs = new FixConnectionStringBuilder()
        //        {
        //            TargetCompId = "EXECUTOR",
        //            ProtocolVersion = FixProtocolVersion.TheLatestVersion.ToString(),
        //            SecureConnection = true,
        //            Port = 5004,
        //            DecodeLogFixMessages = true
        //        };

        //        tradeCs.Address = address;
        //        tradeCs.Username = username;
        //        tradeCs.Password = password;
        //        if (logsEnabled)
        //        {
        //            tradeCs.FixEventsFileName = "trade.events.log";
        //            tradeCs.FixMessagesFileName = "trade.messages.log";
        //            tradeCs.FixLogDirectory = LogPath;
        //        }

        //        tradeProxy = new DataTrade(tradeCs.ToString());
        //        tradeProxy.Logout += tradeProxy_Logout;
        //        tradeProxy.Logon += tradeProxy_Logon;
        //        tradeProxy.CacheInitialized += TradeProxy_CacheInitialized;

        //        TradeProxy.SynchOperationTimeout = 5 * 60 * 1000;

        //        Connecting();

        //        feedProxy.Start();
        //        tradeProxy.Start();

        //        stateControl.PushEvent(Events.DoneConnecting);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("ConnectionModel.Init() failed!", ex);
        //        LastError = ConnectionErrorCodes.Unknown;
        //        stateControl.PushEvent(Events.FailedConnecting);
        //    }
        //}

        //public void Disconnect()
        //{
        //    try
        //    {
        //        connectCancelSrc.Cancel();

        //         wait start task to stop
        //        await startTask;

        //        try
        //        {
        //             fire disconnecting event
        //            Disconnecting();
        //        }
        //        catch (Exception ex) { logger.Error(ex); }

        //         start stoping feed
        //        Task stopFeed = Task.Factory.StartNew(
        //            () =>
        //            {
        //                try
        //                {
        //                    feedProxy.Logout -= feedProxy_Logout;
        //                    feedProxy.Logon -= feedProxy_Logon;
        //                    feedProxy.CacheInitialized -= FeedProxy_CacheInitialized;
        //                    feedProxy.SymbolInfo -= FeedProxy_SymbolInfo;
        //                    feedProxy.Stop();
        //                    feedProxy.Dispose();
        //                }
        //                catch (Exception ex) { logger.Error(ex); }
        //            });

        //         start stopping trade
        //        Task stopTrade = Task.Factory.StartNew(
        //            () =>
        //            {
        //                try
        //                {
        //                    tradeProxy.Logout -= tradeProxy_Logout;
        //                    tradeProxy.Logon -= tradeProxy_Logon;
        //                    tradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
        //                    tradeProxy.Stop();
        //                    tradeProxy.Dispose();
        //                }
        //                catch (Exception ex) { logger.Error(ex); }

        //            });

        //         wait Feed & Tarde stop
        //        await Task.WhenAll(stopFeed, stopTrade);

        //        feedProxy = null;
        //        tradeProxy = null;
        //    }
        //    catch (Exception ex) { logger.Error(ex); }
        //    stateControl.PushEvent(Events.DoneDisconnecting);
        //}

        //private ConnectionErrorCodes Convert(LogoutReason fdkCode)
        //{
        //    switch (fdkCode)
        //    {
        //        case LogoutReason.BlockedAccount: return ConnectionErrorCodes.BlockedAccount;
        //        case LogoutReason.InvalidCredentials: return ConnectionErrorCodes.InvalidCredentials;
        //        case LogoutReason.NetworkError: return ConnectionErrorCodes.NetworkError;
        //        case LogoutReason.ServerError: return ConnectionErrorCodes.ServerError;
        //        case LogoutReason.ServerLogout: return ConnectionErrorCodes.ServerLogout;
        //        case LogoutReason.SlowConnection: return ConnectionErrorCodes.SlowConnection;
        //        case LogoutReason.LoginDeleted: return ConnectionErrorCodes.LoginDeleted;
        //        default: return ConnectionErrorCodes.Unknown;
        //    }
        //}

        //private void FeedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        //{
        //    logger.Debug("EVENT Feed.SymbolInfo");
        //    stateControl.ModifyConditions(() => isSymbolsLoaded = true);
        //}

        //void feedProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        //{
        //    logger.Debug("EVENT Feed.Logon");
        //    stateControl.ModifyConditions(() => isFeedLoggedIn = true);
        //}

        //void tradeProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        //{
        //    logger.Debug("EVENT Trade.Logon");
        //    stateControl.ModifyConditions(() => isTradeLoggedIn = true);
        //}

        //private void FeedProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        //{
        //    logger.Debug("EVENT Feed.CacheInitialized");
        //    stateControl.ModifyConditions(() => isFeedCacheLoaded = true);
        //}

        //private void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        //{
        //    logger.Debug("EVENT Trade.CacheInitialized");
        //    stateControl.ModifyConditions(() => isTradeCacheLoaded = true);
        //}

        //void feedProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        //{
        //    stateControl.SyncContext.Synchronized(() =>
        //    {
        //        if (LastError == ConnectionErrorCodes.None)
        //            LastError = Convert(e.Reason);
        //        stateControl.PushEvent(Events.OnLogout);
        //    });
        //}

        //void tradeProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        //{
        //    stateControl.SyncContext.Synchronized(() =>
        //    {
        //        if (LastError == ConnectionErrorCodes.None)
        //            LastError = Convert(e.Reason);
        //        stateControl.PushEvent(Events.OnLogout);
        //    });
        //}

        public Task<ConnectionErrorCodes> Connect(string address, string login, string password, CancellationToken cancelToken)
        {
            bool logsEnabled = _options.EnableFixLogs;

            if (logsEnabled)
            {
                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);
            }

            isFeedLoggedIn = false;
            isTradeLoggedIn = false;
            isFeedCacheLoaded = false;
            isTradeCacheLoaded = false;
            isSymbolsLoaded = false;

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
                feedCs.FixLogDirectory = LogPath;
            }
            feedCs.ExcludeMessagesFromLogs = "y|0";

            _feedProxy = new DataFeed(feedCs.ToString());
            _feedProxy.Logout += feedProxy_Logout;
            _feedProxy.Logon += feedProxy_Logon;
            _feedProxy.CacheInitialized += FeedProxy_CacheInitialized;
            _feedProxy.SymbolInfo += FeedProxy_SymbolInfo;

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
                tradeCs.FixLogDirectory = LogPath;
            }

            _tradeProxy = new DataTrade(tradeCs.ToString());
            _tradeProxy.Logout += tradeProxy_Logout;
            _tradeProxy.Logon += tradeProxy_Logon;
            _tradeProxy.CacheInitialized += TradeProxy_CacheInitialized;

            _tradeProxy.SynchOperationTimeout = 5 * 60 * 1000;

            //Connecting();

            _connectEvent = new TaskCompletionSource<ConnectionErrorCodes>();

            _executor = new FdkAsyncExecutor(_tradeProxy);
            _feedProxy.Tick += (s, e) => Tick?.Invoke(FdkConvertor.Convert(e.Tick));
            _tradeProxy.PositionReport += (s, a) => PositionReport?.Invoke(FdkConvertor.Convert(a.Report));
            //_tradeProxy.ExecutionReport += (s, a) => ExecutionReport?.Invoke(FdkConvertor.Convert(a.Report));
            //_tradeProxy.BalanceOperation += (s, a) => BalanceOperation?.Invoke(FdkConvertor.Convert(a.Data));

            _feedProxy.Start();
            _tradeProxy.Start();

            return _connectEvent.Task;

            //stateControl.PushEvent(Events.DoneConnecting);
        }

        private void UpdateState(Action updateAction)
        {
            lock (_lockObj)
            {
                updateAction();
                if (isFeedCacheLoaded && isTradeCacheLoaded && isSymbolsLoaded && isTradeLoggedIn && isFeedLoggedIn)
                    _connectEvent.TrySetResult(ConnectionErrorCodes.None);
            }
        }

        static string LogPath
        {
            get { return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Logs"); }
        }

        Task IServerInterop.Disconnect()
        {
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

            return Task.WhenAll(stopFeedTask, stopTradeTask);
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
                var code = Convert(e.Reason);

                if (!_connectEvent.TrySetResult(code))
                    Disconnected?.Invoke(this, code);
            });
        }

        void tradeProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            UpdateState(() =>
            {
                var code = Convert(e.Reason);

                if (!_connectEvent.TrySetResult(code))
                    Disconnected?.Invoke(this, code);
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

        #region IFeedServerApi

        public event Action<QuoteEntity> Tick;
        public event Action<PositionEntity> PositionReport;
        public event Action<ExecutionReport> ExecutionReport;
        public event Action<TradeReportEntity> TradeTransactionReport;
        public event Action<BalanceOperationReport> BalanceOperation;

        public CurrencyEntity[] Currencies => _feedProxy.Cache.Currencies.Select(FdkConvertor.Convert).ToArray();
        public SymbolEntity[] Symbols => _feedProxy.Cache.Symbols.Select(FdkConvertor.Convert).ToArray();

        public void SubscribeToQuotes(string[] symbols, int depth)
        {
            _feedProxy.Server.SubscribeToQuotes(symbols, depth);
        }

        public BarHistoryReport GetHistoryBars(string symbol, DateTime startTime, int count, PriceType priceType, BarPeriod barPeriod)
        {
            throw new NotImplementedException();
        }

        public Task<HistoryFilesPackage> DownloadTickFiles(string symbol, DateTime refTimePoint, bool includeLevel2)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITradeServerApi

        public AccountEntity AccountInfo => FdkConvertor.Convert(_tradeProxy.Cache.AccountInfo);
        public IEnumerable<OrderEntity> TradeRecords =>  _tradeProxy.Cache.TradeRecords.Select(FdkConvertor.Convert).ToArray();
        public IEnumerable<PositionEntity> Positions => _tradeProxy.Cache.Positions.Select(FdkConvertor.Convert);

        public Task<OrderCmdResult> OpenOrder(OpenOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> CancelOrder(CancelOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> ModifyOrder(ReplaceOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCmdResult> CloseOrder(CloseOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerator<TradeReportEntity[]> GetTradeHistory(DateTime? from, DateTime? to, bool skipCancelOrders)
        {
            throw new NotImplementedException();
        }

        public void SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, OpenOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public void SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CancelOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public void SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public void SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CloseOrderRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
