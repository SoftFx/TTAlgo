using ActorSharp;
using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TickTrader.Algo.Account;
using TickTrader.BotTerminal.SymbolManager;
using TickTrader.FeedStorage.Api;
using TickTrader.SeriesStorage.Lmdb;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen, IShell, IProfileLoader
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SoundsNotificationCenter _soundCenter;

        private ConnectionManager cManager;
        private TraderClientModel clientModel;
        private WindowManager wndManager;
        private PersistModel storage;
        private EventJournal eventJournal;
        private bool isClosed;
        private AlgoEnvironment algoEnv;
        private SymbolManagerViewModel _smbManager;
        private ISymbolCatalog _symbolsCatalog;
        //private CustomFeedStorage.Handler _userSymbols;
        private BotAgentManager _botAgentManager;

        public ShellViewModel(ClientModel.Data commonClient)
        {
            //_userSymbols = customFeedStorage;

            DisplayName = EnvService.Instance.ApplicationName;

            eventJournal = new EventJournal(1000);
            storage = new PersistModel();
            ThemeSelector.Instance.InitializeSettings(storage);

            ConnectionLock = new UiLock();
            wndManager = new WindowManager(this);

            cManager = new ConnectionManager(commonClient, storage, eventJournal);
            clientModel = new TraderClientModel(commonClient);

            _soundCenter = new SoundsNotificationCenter(cManager, storage);

            Agent = new LocalAlgoAgent(this, clientModel, storage);

            _botAgentManager = new BotAgentManager(storage, this);

            algoEnv = new AlgoEnvironment(this, Agent, _botAgentManager);

            AlgoList = new AlgoListViewModel(algoEnv);
            SymbolList = new SymbolListViewModel(clientModel.Symbols, commonClient.Distributor, this);

            ProfileManager = new ProfileManagerViewModel(this, storage);

            Trade = new TradeInfoViewModel(clientModel, cManager, storage.ProfileManager);

            //setting for initialization binary storage
            StorageFactory.InitBinaryStorage((folder, readOnly) => new LmdbManager(folder, readOnly));

            _symbolsCatalog = StorageFactory.BuildCatalog(clientModel);

            TradeHistory = new TradeHistoryViewModel(clientModel, cManager, storage.ProfileManager);

            Charts = new ChartCollectionViewModel(clientModel, this, algoEnv);

            BotList = new BotListViewModel(algoEnv);

            AccountPane = new AccountPaneViewModel(this);
            Journal = new JournalViewModel(eventJournal);
            //BotJournal = new BotJournalViewModel(algoEnv.BotJournal);
            DockManagerService = new DockManagerService(algoEnv);

            AlertsManager = new AlertViewModel(wndManager, this);
            AlertsManager.SubscribeToModel(Agent.AlertModel);
            AlertsManager.SubcribeToModels(_botAgentManager.BotAgents.Values.Select(u => u.RemoteAgent.AlertModel));
            _botAgentManager.BotAgents.Updated += AlertsManager.UpdateBotAgents;

            CanConnect = true;
            UpdateCommandStates();
            cManager.ConnectionStateChanged += (o, n) => UpdateDisplayName();
            cManager.ConnectionStateChanged += (o, n) => UpdateCommandStates();
            cManager.LoggedIn += () => UpdateCommandStates();
            cManager.LoggedOut += () => UpdateCommandStates();
            ConnectionLock.PropertyChanged += (s, a) => UpdateCommandStates();

            clientModel.Initializing += LoadConnectionProfile;
            clientModel.Deinitializing += CloseCatalog;
            clientModel.Connected += OpenDefaultChart;

            storage.ProfileManager.SaveProfileSnapshot = SaveProfileSnapshot;

            cManager.ConnectionStateChanged += (f, t) =>
            {
                NotifyOfPropertyChange(nameof(ConnectionState));
                NotifyOfPropertyChange(nameof(CurrentServerName));
                NotifyOfPropertyChange(nameof(ProtocolName));
            };
            //cManager.CredsChanged += () => NotifyOfPropertyChange(nameof(CurrentServerName));

            LogStateLoop();
        }

        private void OpenDefaultChart()
        {
            if (clientModel.Symbols.Snapshot.Count > 0 && Charts.Items.Count == 0)
            {
                //var defaultSymbol = string.Empty;
                //switch (clientModel.Account.Type)
                //{
                //    case AccountTypes.Gross:
                //    case AccountTypes.Cash:
                //        defaultSymbol = "EURUSD";
                //        break;
                //    case AccountTypes.Net:
                //        defaultSymbol = "EUR/USD";
                //        break;
                //}

                Charts.Open(clientModel.Cache.GetDefaultSymbol()?.Name ?? clientModel.Symbols.Snapshot.First().Key);
                //clientModel.Connected -= OpenDefaultChart;
            }
        }

        private void UpdateDisplayName()
        {
            if (cManager.State == ConnectionModel.States.Online)
                DisplayName = $"{cManager.Creds.Login} {cManager.Creds.Server.Address} - {EnvService.Instance.ApplicationName}";
        }

        private void UpdateCommandStates()
        {
            var state = cManager.State;
            CanDisconnect = cManager.IsLoggedIn;
            ProfileManager.CanLoadProfile = !ConnectionLock.IsLocked;
            NotifyOfPropertyChange(nameof(CanConnect));
            NotifyOfPropertyChange(nameof(CanDisconnect));
        }

        private async Task LoadConnectionProfile(object sender, CancellationToken token)
        {
            var login = cManager.Creds.Login;
            var server = cManager.Creds.Server.Address;

            var customStorageSettings = new CustomStorageSettings
            {
                FolderPath = EnvService.Instance.CustomFeedCacheFolder
            };

            var settings = new OnlineStorageSettings
            {
                Login = login,
                Server = server,
                FolderPath = EnvService.Instance.FeedHistoryCacheFolder,
                Options = StorageFolderOptions.ServerHierarchy,
            };

            await _symbolsCatalog.OpenCustomStorage(customStorageSettings);
            await _symbolsCatalog.ConnectClient(settings);
            await ProfileManager.LoadConnectionProfile(server, login, token);
        }

        private Task CloseCatalog(object sender, CancellationToken token)
        {
            _smbManager = null;
            return _symbolsCatalog?.CloseCatalog();
        }

        public bool CanConnect { get; private set; }
        public bool CanDisconnect { get; private set; }

        public void Connect()
        {
            ShootBots(out var isConfirmed);

            if (isConfirmed)
                Connect(null);
        }

        public void Disconnect()
        {
            try
            {
                ShootBots(out var isConfirmed);

                if (isConfirmed)
                    cManager.TriggerDisconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void ShootBots(out bool isConfirmed)
        {
            isConfirmed = true;

            if (ConnectionLock.IsLocked)
            {
                bool hasRunningBots = algoEnv.LocalAgent.HasRunningBots;

                var exit = new ConfirmationDialogViewModel(DialogButton.YesNo, hasRunningBots ? DialogMode.Warning : DialogMode.Question, DialogMessages.LogoutTitle, DialogMessages.LogoutMessage, hasRunningBots ? DialogMessages.BotsWorkError : null);
                wndManager.ShowDialog(exit, this);

                isConfirmed = exit.DialogResult == DialogResult.OK;

                if (isConfirmed)
                    StopTerminal(false);
            }
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            bool hasRunningBots = algoEnv.LocalAgent.HasRunningBots;

            var exit = new ConfirmationDialogViewModel(DialogButton.YesNo, hasRunningBots ? DialogMode.Warning : DialogMode.Question, DialogMessages.ExitTitle, DialogMessages.ExitMessage, algoEnv.LocalAgent.HasRunningBots ? DialogMessages.BotsWorkError : null);
            wndManager.ShowDialog(exit, this);

            var isConfirmed = exit.DialogResult == DialogResult.OK;

            if (isConfirmed)
                StopTerminal(true);

            return Task.FromResult(isConfirmed);
        }

        private async void StopTerminal(bool stopAlgoServer)
        {
            //await storage.ProfileManager.Stop();
            await storage.ProfileManager.StopCurrentProfile();

            var shutdown = new ShutdownDialogViewModel(algoEnv.LocalAgent, stopAlgoServer);

            if (IsActive)
                await wndManager.ShowDialog(shutdown, this);
        }

        public async void Connect(AccountAuthEntry creds = null)
        {
            try
            {
                if (creds == null || !creds.HasPassword)
                {
                    LoginDialogViewModel model = new LoginDialogViewModel(cManager, creds);
                    await wndManager.ShowDialog(model, this);
                }
                else
                    cManager.TriggerConnect(creds);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void About()
        {
            AboutDialogViewModel model = new AboutDialogViewModel();
            wndManager.ShowDialog(model, this);
        }

        public void Exit()
        {
            TryCloseAsync();
        }

        public void OpenChart(string smb)
        {
            Charts.Open(smb);
        }

        public void ShowChart(string smb, ChartPeriods period)
        {
            Charts.OpenOrActivate(smb, period);
        }

        public DialogResult ShowDialog(DialogButton buttons, DialogMode mode, string title, string message, string error)
        {
            var dialog = new ConfirmationDialogViewModel(buttons, mode, title, message, error);
            wndManager.ShowDialog(dialog, this);

            return dialog.DialogResult;
        }

        public TradeInfoViewModel Trade { get; }
        public TradeHistoryViewModel TradeHistory { get; }
        public AlgoListViewModel AlgoList { get; set; }
        public SymbolListViewModel SymbolList { get; private set; }
        public ChartCollectionViewModel Charts { get; private set; }
        public AccountPaneViewModel AccountPane { get; private set; }
        public JournalViewModel Journal { get; set; }
        //public BotJournalViewModel BotJournal { get; set; }
        public UiLock ConnectionLock { get; private set; }
        public IProfileLoader ProfileLoader => this;
        public ProfileManagerViewModel ProfileManager { get; private set; }
        public BacktesterViewModel Backtester { get; private set; }
        public SettingsStorage<PreferencesStorageModel> Preferences => storage.PreferencesStorage;
        public BotListViewModel BotList { get; }
        public WindowManager ToolWndManager => wndManager;
        public DockManagerService DockManagerService { get; set; }
        public LocalAlgoAgent Agent { get; }
        public ConnectionManager ConnectionManager => cManager;
        public ConnectionModel.States ConnectionState => cManager.Connection.State;
        public string CurrentServerName => cManager.Connection.CurrentServer;
        public string ProtocolName => cManager.Connection.CurrentProtocol;
        public EventJournal EventJournal => eventJournal;

        //public NotificationsViewModel Notifications { get; private set; }

        public AlertViewModel AlertsManager { get; }

        public async Task Shutdown()
        {
            isClosed = true;

            try
            {
                await cManager.Disconnect();
                await _symbolsCatalog.CloseCatalog();
                await _botAgentManager.ShutdownDisconnect();
                await storage.Stop();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Shutdown() failed.");
            }
        }

        protected override void OnViewLoaded(object view)
        {
            eventJournal.Info("AlgoTerminal started");
            PrintSystemInfo();

            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(OnLoaded));
        }

        public void OnLoaded()
        {
            _botAgentManager.RestoreConnections();
            Connect(null); // show connect window
        }

        private async void PrintSystemInfo()
        {
            await Task.Run(() =>
            {
                try
                {
                    var os = ComputerInfo.OperatingSystem;
                    var cpu = ComputerInfo.Processor;
                    var sign = TimeZoneInfo.Local.BaseUtcOffset < TimeSpan.Zero ? "-" : "+";
                    eventJournal.Info("{0} ({1}), {2}, RAM: {3} / {4} Mb, UTC{5}{6:hh\\:mm}",
                        os.Name, os.Architecture,
                        cpu.Name,
                        os.FreePhysicalMemory / 1024,
                        os.TotalVisibleMemorySize / 1024,
                        sign,
                        TimeZoneInfo.Local.BaseUtcOffset);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "PrintSystemInfo() failed!");
                }
            });
        }

        private void ConnectLastOrConnectDefault()
        {
            var last = cManager.GetLast();
            if (last != null)
                Connect(last);
            else
                Connect();
        }

        private async void LogStateLoop()
        {
            StringBuilder builder = new StringBuilder();

            while (!isClosed)
            {
                builder.Clear();
                builder.Append("STATE SNAPSHOT ");

                LogState(builder, "ConnectionManager", cManager.State.ToString());
                LogState(builder, "Connection", cManager.Connection.State.ToString());

                logger.Debug(builder.ToString());

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private static void LogState(StringBuilder builder, string name, string state)
        {
            builder.Append(name).Append(':').Append(state).Append(" ");
        }

        public void InstallVSPackage()
        {
            try
            {
                VsIntegration.InstallVsPackage();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "InstallVSPackage() failed!");
            }
        }

        public void OpenStorageManager()
        {
            if (_smbManager == null)
                _smbManager = new SymbolManagerViewModel(clientModel, _symbolsCatalog, ToolWndManager);

            wndManager.ShowDialog(_smbManager, this);
        }

        public void OpenBacktester()
        {
            if (Backtester == null)
            {
                Backtester = new BacktesterViewModel(algoEnv, clientModel, _symbolsCatalog, this, storage.ProfileManager);
                Backtester.Deactivated += Backtester_Deactivated;
            }

            wndManager.OpenMdiWindow(Backtester);
        }

        private Task Backtester_Deactivated(object sender, DeactivationEventArgs e)
        {
            Backtester.Deactivated -= Backtester_Deactivated;
            Backtester = null;

            return Task.CompletedTask;
        }

        private void SaveProfileSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                Charts.SaveChartsSnapshot(profileStorage);
                Agent.SaveBotsSnapshot(profileStorage);
                DockManagerService.SaveLayoutSnapshot(profileStorage);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to save profile snapshot");
            }
        }

        #region IProfileLoader implementation

        public void ReloadProfile(CancellationToken token)
        {
            var loading = new ProfileLoadingDialogViewModel(Charts, storage.ProfileManager, token, Agent, DockManagerService);
            wndManager.ShowDialog(loading, this);
            AlertsManager.RegisterAlertWindow();
        }

        #endregion
    }
}
