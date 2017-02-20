using Machinarium.State;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    internal class ConnectionModel
    {
        private IAlgoCoreLogger logger;
        internal enum States { Offline, Connecting, WaitingLogon, Initializing, Online, Deinitializing, Disconnecting }
        public enum Events { Started, DoneConnecting, FailedConnecting, DoneInit, DoneDeinit, DoneDisconnecting, OnLogon, OnLogout, StopRequested }

        private DataFeed feedProxy;
        private DataTrade tradeProxy;
        private StateMachine<States> stateControl;
        private string address;
        private string username;
        private string password;
        private CancellationTokenSource connectCancelSrc;
        private Task startTask;
        private Task initTask;
        private bool isFeedLoggedIn;
        private bool isTradeLoggedIn;
        private bool isFeedCacheLoaded;
        private bool isTradeCacheLoaded;
        private bool isSymbolsLoaded;
        private ConnectionOptions options;

        public ConnectionModel(IAlgoCoreLogger logger, ConnectionOptions options, IStateMachineSync sync = null)
        {
            this.logger = logger;
            this.options = options;

            stateControl = new StateMachine<States>(sync);

            stateControl.AddTransition(States.Offline, Events.Started, States.Connecting);
            //stateControl.AddTransition(States.Offline, () => shouldReconnect, States.Initializing);
            stateControl.AddTransition(States.Connecting, Events.DoneConnecting, States.WaitingLogon);
            stateControl.AddTransition(States.Connecting, Events.FailedConnecting, States.Disconnecting);
            stateControl.AddTransition(States.Connecting, Events.OnLogout, States.Disconnecting);
            stateControl.AddTransition(States.Connecting, Events.StopRequested, States.Disconnecting);
            stateControl.AddTransition(States.WaitingLogon, () => isSymbolsLoaded && isFeedLoggedIn && isTradeLoggedIn && isFeedCacheLoaded && isTradeCacheLoaded, States.Initializing);
            stateControl.AddTransition(States.WaitingLogon, Events.OnLogout, States.Disconnecting);
            stateControl.AddTransition(States.WaitingLogon, Events.StopRequested, States.Disconnecting);
            stateControl.AddTransition(States.Initializing, Events.DoneInit, States.Online);
            stateControl.AddTransition(States.Initializing, Events.OnLogout, States.Deinitializing);
            stateControl.AddTransition(States.Initializing, Events.StopRequested, States.Deinitializing);
            stateControl.AddTransition(States.Online, Events.OnLogout, States.Deinitializing);
            stateControl.AddTransition(States.Online, Events.StopRequested, States.Deinitializing);
            stateControl.AddTransition(States.Deinitializing, Events.DoneDeinit, States.Disconnecting);
            stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, States.Offline);

            stateControl.OnEnter(States.Connecting, () => startTask = StartConnecting());
            stateControl.OnEnter(States.Initializing, () => initTask = Init());
            stateControl.OnEnter(States.Deinitializing, () => Deinit());
            stateControl.OnEnter(States.Disconnecting, () => StartDisconnecting());
            stateControl.OnEnter(States.Online, () => Connected());
            stateControl.OnEnter(States.Offline, () => Disconnected());

            stateControl.StateChanged += (from, to) => logger.Debug("STATE {0}", to);
            stateControl.EventFired += e => logger.Debug("EVENT {0}", e);
        }

        public DataFeed FeedProxy { get { return feedProxy; } }
        public DataTrade TradeProxy { get { return tradeProxy; } }
        public ConnectionErrorCodes LastError { get; private set; }
        public bool HasError { get { return LastError != ConnectionErrorCodes.None; } }

        public IStateProvider<States> State { get { return stateControl; } }

        public bool IsConnecting
        {
            get
            {
                var state = State.Current;
                return state == ConnectionModel.States.Connecting
                    || state == ConnectionModel.States.WaitingLogon
                    || state == ConnectionModel.States.Initializing;
            }
        }

        public event Action Connecting = delegate { };
        public event Action Connected = delegate { };
        public event Action Disconnecting = delegate { };
        public event Action Disconnected = delegate { };
        //public event AsyncEventHandler SysInitalizing; // main thread event (called after before Initalizing)
        public event AsyncEventHandler Initalizing; // main thread event
        public event AsyncEventHandler Deinitalizing; // background thread event
        //public event AsyncEventHandler SysDeinitalizing; // background thread event (called after Deinitalizing)

        public async Task<ConnectionErrorCodes> Connect(string username, string password, string address, CancellationToken cToken)
        {
            await stateControl.ModifyConditionsAndWait(() =>
            {
                if (stateControl.Current != States.Offline)
                    throw new InvalidOperationException("Connect() canot be called when connection in state '" + stateControl.Current + "'!");

                this.address = address;
                this.username = username;
                this.password = password;

                var cancelSrcCopy = new CancellationTokenSource();
                connectCancelSrc = cancelSrcCopy;
                cToken.Register(() => cancelSrcCopy.Cancel());

                stateControl.PushEvent(Events.Started);
            },
            (s) => s == States.Offline || s == States.Online);

            return LastError;
        }

        public Task DisconnectAsync()
        {
            return stateControl.PushEventAndWait(Events.StopRequested, States.Offline);
        }

        private Task StartConnecting()
        {
            LastError = ConnectionErrorCodes.None;
            connectCancelSrc = new CancellationTokenSource();

            return Task.Factory.StartNew(() =>
               {
                   try
                   {
                       bool logsEnabled = options.EnableFixLogs;

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
                       feedCs.Username = username;
                       feedCs.Password = password;
                       if (logsEnabled)
                       {
                           feedCs.FixEventsFileName = "feed.events.log";
                           feedCs.FixMessagesFileName = "feed.messages.log";
                           feedCs.FixLogDirectory = LogPath;
                       }
                       //feedCs.ExcludeMessagesFromLogs = "y|0";

                       feedProxy = new DataFeed(feedCs.ToString());
                       feedProxy.Logout += feedProxy_Logout;
                       feedProxy.Logon += feedProxy_Logon;
                       feedProxy.CacheInitialized += FeedProxy_CacheInitialized;
                       feedProxy.SymbolInfo += FeedProxy_SymbolInfo;

                       FixConnectionStringBuilder tradeCs = new FixConnectionStringBuilder()
                       {
                           TargetCompId = "EXECUTOR",
                           ProtocolVersion = FixProtocolVersion.TheLatestVersion.ToString(),
                           SecureConnection = true,
                           Port = 5004,
                           DecodeLogFixMessages = true
                       };

                       tradeCs.Address = address;
                       tradeCs.Username = username;
                       tradeCs.Password = password;
                       if (logsEnabled)
                       {
                           tradeCs.FixEventsFileName = "trade.events.log";
                           tradeCs.FixMessagesFileName = "trade.messages.log";
                           tradeCs.FixLogDirectory = LogPath;
                       }

                       tradeProxy = new DataTrade(tradeCs.ToString());
                       tradeProxy.Logout += tradeProxy_Logout;
                       tradeProxy.Logon += tradeProxy_Logon;
                       tradeProxy.CacheInitialized += TradeProxy_CacheInitialized;

                       TradeProxy.SynchOperationTimeout = 5 * 60 * 1000;

                       Connecting();

                       feedProxy.Start();
                       tradeProxy.Start();

                       stateControl.PushEvent(Events.DoneConnecting);
                   }
                   catch (Exception ex)
                   {
                       logger.Error("ConnectionModel.Init() failed!", ex);
                       LastError = ConnectionErrorCodes.Unknown;
                       stateControl.PushEvent(Events.FailedConnecting);
                   }
               });
        }

        private async Task Init()
        {
            try
            {
                var cToken = connectCancelSrc.Token;
                //await SysInitalizing.InvokeAsync(this, cToken);
                await Initalizing.InvokeAsync(this, cToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            stateControl.PushEvent(Events.DoneInit);
        }

        private void FeedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        {
            logger.Debug("EVENT Feed.SymbolInfo");
            stateControl.ModifyConditions(() => isSymbolsLoaded = true);
        }

        void feedProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        {
            logger.Debug("EVENT Feed.Logon");
            stateControl.ModifyConditions(() => isFeedLoggedIn = true);
        }

        void tradeProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        {
            logger.Debug("EVENT Trade.Logon");
            stateControl.ModifyConditions(() => isTradeLoggedIn = true);
        }

        private void FeedProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            logger.Debug("EVENT Feed.CacheInitialized");
            stateControl.ModifyConditions(() => isFeedCacheLoaded = true);
        }

        private void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            logger.Debug("EVENT Trade.CacheInitialized");
            stateControl.ModifyConditions(() => isTradeCacheLoaded = true);
        }

        void feedProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            stateControl.SyncContext.Synchronized(() =>
            {
                if (LastError == ConnectionErrorCodes.None)
                    LastError = Convert(e.Reason);
                stateControl.PushEvent(Events.OnLogout);
            });
        }

        void tradeProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            stateControl.SyncContext.Synchronized(() =>
            {
                if (LastError == ConnectionErrorCodes.None)
                    LastError = Convert(e.Reason);
                stateControl.PushEvent(Events.OnLogout);
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

        private async void Deinit()
        {
            try
            {
                connectCancelSrc.Cancel();

                // wait init task to stop
                await initTask;

                // fire events and wait all handlers
                logger.Debug("Deinit. Invoking Deinitalizing event...");
                await Deinitalizing.InvokeAsync(this, CancellationToken.None);
            }
            catch (Exception ex) { logger.Error(ex); }
            stateControl.PushEvent(Events.DoneDeinit);
        }

        private async void StartDisconnecting()
        {
            try
            {
                connectCancelSrc.Cancel();

                // wait start task to stop
                await startTask;

                try
                {
                    // fire disconnecting event
                    Disconnecting();
                }
                catch (Exception ex) { logger.Error(ex); }

                // start stoping feed
                Task stopFeed = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            feedProxy.Logout -= feedProxy_Logout;
                            feedProxy.Logon -= feedProxy_Logon;
                            feedProxy.CacheInitialized -= FeedProxy_CacheInitialized;
                            feedProxy.SymbolInfo -= FeedProxy_SymbolInfo;
                            feedProxy.Stop();
                            feedProxy.Dispose();
                        }
                        catch (Exception ex) { logger.Error(ex); }
                    });

                // start stopping trade
                Task stopTrade = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            tradeProxy.Logout -= tradeProxy_Logout;
                            tradeProxy.Logon -= tradeProxy_Logon;
                            tradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
                            tradeProxy.Stop();
                            tradeProxy.Dispose();
                        }
                        catch (Exception ex) { logger.Error(ex); }

                    });

                // wait Feed & Tarde stop
                await Task.WhenAll(stopFeed, stopTrade);

                feedProxy = null;
                tradeProxy = null;
            }
            catch (Exception ex) { logger.Error(ex); }
            stateControl.PushEvent(Events.DoneDisconnecting);
        }

        static string LogPath
        {
            get { return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Logs"); }
        }
    }

    public enum ConnectionErrorCodes
    {
        None,
        Unknown,
        NetworkError,
        Timeout,
        BlockedAccount,
        ClientInitiated,
        InvalidCredentials,
        SlowConnection,
        ServerError,
        LoginDeleted,
        ServerLogout,
        Canceled
    }

    public class ConnectionOptions
    {
        public bool EnableFixLogs { get; set; }
    }
}