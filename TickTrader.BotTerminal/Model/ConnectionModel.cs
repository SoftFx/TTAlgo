using SoftFX.Extended;
using StateMachinarium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    class ConnectionModel
    {
        public enum States { Offline, Initializing, WaitingLogon, Online, Deinitializing }
        public enum Events { Started, DoneInit, DoneDeinit, InitFailed, OnLogon, OnLogout, StopRequested }

        private DataFeed feedProxy;
        private DataTrade tradeProxy;
        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private string address;
        private string username;
        private string password;
        private CancellationTokenSource stopSignal;
        private Task initTask;
        //private bool shouldReconnect;
        private bool isFeedLoggedIn;
        private bool isTradeLoggedIn;

        public ConnectionModel()
        {
            FeedCache = new FeedHistoryProviderModel();

            stateControl.AddTransition(States.Offline, Events.Started, States.Initializing);
            //stateControl.AddTransition(States.Offline, () => shouldReconnect, States.Initializing);
            stateControl.AddTransition(States.Initializing, Events.DoneInit, States.WaitingLogon);
            stateControl.AddTransition(States.Initializing, Events.InitFailed, States.Deinitializing);
            stateControl.AddTransition(States.Initializing, Events.OnLogout, States.Deinitializing);
            stateControl.AddTransition(States.Initializing, Events.StopRequested, States.Deinitializing);
            stateControl.AddTransition(States.WaitingLogon, () => isFeedLoggedIn && isTradeLoggedIn, States.Online);
            stateControl.AddTransition(States.WaitingLogon, Events.OnLogout, States.Deinitializing);
            stateControl.AddTransition(States.WaitingLogon, Events.StopRequested, States.Deinitializing);
            stateControl.AddTransition(States.Online, Events.OnLogout, States.Deinitializing);
            stateControl.AddTransition(States.Online, Events.StopRequested, States.Deinitializing);
            stateControl.AddTransition(States.Deinitializing, Events.DoneDeinit, States.Offline);

            stateControl.OnEnter(States.Initializing, () => initTask = Init());
            stateControl.OnEnter(States.Deinitializing, () => Deinit());
            stateControl.OnEnter(States.Online, () => { FeedCache.Start(feedProxy); Connected(); });

            stateControl.StateChanged += (from, to) => System.Diagnostics.Debug.WriteLine("ConnectionModel STATE " + from + " => " + to);
            stateControl.EventFired += e => System.Diagnostics.Debug.WriteLine("ConnectionModel EVENT " + e);
        }

        public DataFeed FeedProxy { get { return feedProxy; } }
        public DataTrade TradeProxy { get { return tradeProxy; } }
        public FeedHistoryProviderModel FeedCache { get; private set; }
        public Exception LastError { get; private set; }

        public IStateProvider<States> State { get { return stateControl; } }

        public async Task ChangeConnection(string address, string username, string password)
        {
            await DisconnectAsync();
            this.address = address;
            this.username = username;
            this.password = password;
        }

        public void StartConnecting()
        {
            stateControl.PushEvent(Events.Started);
        }

        public void StartDisconnecting()
        {
            stateControl.PushEvent(Events.StopRequested);
        }

        //public async Task<Exception> ConnectAsync(CancellationToken cToken)
        //{
        //    StartConnecting();
        //    await connectTask;
        //    return LastError;
        //}

        public Task DisconnectAsync()
        {
            return stateControl.PushEventAndAsyncWait(Events.StopRequested, States.Offline);
        }

        //public event AsyncEventHandler Connected;
        public event Action Initialized = delegate { };
        public event Action Deinitialized = delegate { };
        public event Action Connected = delegate { };
        public event AsyncEventHandler Disconnected;

        private Task Init()
        {
            stopSignal = new CancellationTokenSource();

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!Directory.Exists(LogPath))
                        Directory.CreateDirectory(LogPath);

                    isTradeLoggedIn = false;
                    isFeedLoggedIn = false;

                    feedProxy = new DataFeed();
                    feedProxy.Logout += feedProxy_Logout;
                    feedProxy.Logon += feedProxy_Logon;

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
                    feedCs.FixEventsFileName = "feed.events.log";
                    feedCs.FixMessagesFileName = "feed.messages.log";
                    feedCs.FixLogDirectory = LogPath;

                    feedProxy.Initialize(feedCs.ToString());

                    tradeProxy = new DataTrade();
                    tradeProxy.Logout += tradeProxy_Logout;
                    tradeProxy.Logon += tradeProxy_Logon;

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
                    tradeCs.FixEventsFileName = "trade.events.log";
                    tradeCs.FixMessagesFileName = "trade.messages.log";
                    tradeCs.FixLogDirectory = LogPath;

                    tradeProxy.Initialize(tradeCs.ToString());

                    Initialized();

                    feedProxy.Start();
                    tradeProxy.Start();

                    stateControl.PushEvent(Events.DoneInit);
                }
                catch (Exception ex)
                {
                    LastError = ex;
                    stateControl.PushEvent(Events.InitFailed);
                }
            });
        }

        void tradeProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        {
            stateControl.ModifyConditions(() => isTradeLoggedIn = true);
        }

        void tradeProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            stateControl.PushEvent(Events.OnLogout);
        }

        void feedProxy_Logon(object sender, SoftFX.Extended.Events.LogonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ConnectionModel EVENT Feed.Logon");
            stateControl.ModifyConditions(() => isFeedLoggedIn = true);
        }

        void feedProxy_Logout(object sender, SoftFX.Extended.Events.LogoutEventArgs e)
        {
            stateControl.PushEvent(Events.OnLogout);
        }

        private async void Deinit()
        {
            try
            {
                stopSignal.Cancel();

                await initTask; // wait init task to stop

                Task fireEvent = Disconnected.InvokeAsync(this);

                await FeedCache.Shutdown();

                Task stopFeed = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            feedProxy.Logout -= feedProxy_Logout;
                            feedProxy.Logon -= feedProxy_Logon;
                            feedProxy.Stop();
                            feedProxy.Dispose();
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
                    });

                Task stopTrade = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            tradeProxy.Logout -= tradeProxy_Logout;
                            tradeProxy.Logon -= tradeProxy_Logon;
                            tradeProxy.Stop();
                            tradeProxy.Dispose();
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
                        
                    });

                await Task.WhenAll(stopFeed, stopTrade, fireEvent);

                Deinitialized();

                feedProxy = null;
                tradeProxy = null;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            stateControl.PushEvent(Events.DoneDeinit);
        }

        static string LogPath
        {
            get { return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Logs"); }
        }
    }
}