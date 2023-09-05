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
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.AutoUpdate;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.SymbolManager;
using TickTrader.BotTerminal.Views.BotsRepository;
using TickTrader.FeedStorage.Api;
using TickTrader.SeriesStorage.Lmdb;

namespace TickTrader.BotTerminal
{
    internal sealed class ShellViewModel : Screen, IShell, IProfileLoader
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BotAgentManager _botAgentManager;
        private readonly ISymbolCatalog _symbolsCatalog;
        private readonly TraderClientModel _clientModel;
        private readonly ConnectionManager _cManager;
        private readonly EventJournal _eventJournal;
        private readonly WindowManager _wndManager;
        private readonly AlgoEnvironment _algoEnv;
        private readonly PersistModel _storage;
        private readonly AutoUpdateService _autoUpdateSvc;

        private BotsRepositoryViewModel _botsRepository;
        private SymbolManagerViewModel _smbManager;

        private bool isClosed;


        public ShellViewModel(ClientModel.Data commonClient)
        {
            DisplayName = EnvService.Instance.ApplicationName;

            _eventJournal = new EventJournal(1000);
            _storage = new PersistModel();
            ThemeSelector.Instance.InitializeSettings(_storage);

            _autoUpdateSvc = new AutoUpdateService(EnvService.Instance.UpdatesFolder, UpdateAppTypes.Terminal);
            InitAutoUpdateSources();
            _autoUpdateSvc.SetNewVersionCallback(OnNewVersionAvailable, false);
            _autoUpdateSvc.EnableAutoCheck();
            _autoUpdateSvc.SetExitCallback(Exit, false);

            ConnectionLock = new UiLock();
            _wndManager = new WindowManager(this);

            Agent = new LocalAlgoAgent2(_storage, _eventJournal);

            _cManager = new ConnectionManager(commonClient, _storage, _eventJournal, Agent);
            _clientModel = new TraderClientModel(commonClient);

            _ = new SoundsNotificationCenter(_cManager, _storage);

            _botAgentManager = new BotAgentManager(_storage, this);

            _algoEnv = new AlgoEnvironment(this, _clientModel, Agent, _botAgentManager);

            AlgoList = new AlgoListViewModel(_algoEnv);
            SymbolList = new SymbolListViewModel(_clientModel.Symbols, commonClient.Distributor, this);

            ProfileManager = new ProfileManagerViewModel(this, _storage);

            Trade = new TradeInfoViewModel(_clientModel, _cManager, _storage.ProfileManager);

            //setting for initialization binary storage
            StorageFactory.InitBinaryStorage((folder, readOnly) => new LmdbManager(folder, readOnly));

            _symbolsCatalog = StorageFactory.BuildCatalog(_clientModel);

            TradeHistory = new TradeHistoryViewModel(_clientModel, _cManager, _storage.ProfileManager);

            Charts = new ChartCollectionViewModel(_clientModel, _algoEnv);

            BotList = new BotListViewModel(_algoEnv);

            AccountPane = new AccountPaneViewModel(this);
            Journal = new JournalViewModel(_eventJournal);
            //BotJournal = new BotJournalViewModel(algoEnv.BotJournal);
            DockManagerService = new DockManagerService(_algoEnv, InitDockService);

            AlertsManager = new AlertViewModel(_wndManager, this);
            AlertsManager.SubscribeToModel(Agent.AlertModel);
            AlertsManager.SubcribeToModels(_botAgentManager.BotAgents.Values.Select(u => u.RemoteAgent.AlertModel));
            _botAgentManager.BotAgents.Updated += AlertsManager.UpdateBotAgents;

            CanConnect = true;
            UpdateCommandStates();
            _cManager.ConnectionStateChanged += (o, n) => UpdateDisplayName();
            _cManager.ConnectionStateChanged += (o, n) => UpdateCommandStates();
            _cManager.LoggedIn += () => UpdateCommandStates();
            _cManager.LoggedOut += () => UpdateCommandStates();
            ConnectionLock.PropertyChanged += (s, a) => UpdateCommandStates();

            _clientModel.Initializing += OnClientInit;
            _clientModel.Deinitializing += OnClientDeinit;
            _clientModel.Connected += OpenDefaultChart;

            _storage.ProfileManager.SaveProfileSnapshot = SaveProfileSnapshot;

            _cManager.ConnectionStateChanged += (f, t) =>
            {
                NotifyOfPropertyChange(nameof(ConnectionState));
                NotifyOfPropertyChange(nameof(CurrentServerName));
                NotifyOfPropertyChange(nameof(ProtocolName));
            };
            //cManager.CredsChanged += () => NotifyOfPropertyChange(nameof(CurrentServerName));

            LogStateLoop();
        }

        private void InitDockService()
        {
            AlertsManager.RegisterAlertWindow();
        }

        private void OpenDefaultChart()
        {
            if (_clientModel.Symbols.Snapshot.Count > 0 && Charts.Items.Count == 0)
            {
                Charts.Open(_clientModel.Cache.GetDefaultSymbol()?.Name ?? _clientModel.Symbols.Snapshot.First().Key);
                //clientModel.Connected -= OpenDefaultChart;
            }
        }

        private void UpdateDisplayName()
        {
            if (_cManager.State == ConnectionModel.States.Online)
                DisplayName = $"{_cManager.Creds.Login} {_cManager.Creds.Server.Address} - {EnvService.Instance.ApplicationName}";
        }

        private void UpdateCommandStates()
        {
            CanDisconnect = _cManager.IsLoggedIn;
            ProfileManager.CanLoadProfile = !ConnectionLock.IsLocked;

            NotifyOfPropertyChange(nameof(CanConnect));
            NotifyOfPropertyChange(nameof(CanDisconnect));
        }

        private async Task OnClientInit(object sender, CancellationToken token)
        {
            var login = _cManager.Creds.Login;
            var server = _cManager.Creds.Server.Address;

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

            Agent.AccountMetadataProvider = () => new AccountMetadataInfo(AccountId.Pack(_clientModel.Connection.CurrentServer, _clientModel.Connection.CurrentLogin),
                _clientModel.SortedSymbols.Select(s => s.ToInfo()).ToList(), _clientModel.Cache.GetDefaultSymbol().ToInfo());
            await Agent.IndicatorHost.SetAccountProxy(_clientModel.GetAccountProxy());

            await Agent.IndicatorHost.Start();
        }

        private async Task OnClientDeinit(object sender, CancellationToken token)
        {
            _smbManager = null;
            await _symbolsCatalog?.CloseCatalog();

            await Agent.IndicatorHost.Stop();
        }

        public bool CanConnect { get; private set; }
        public bool CanDisconnect { get; private set; }

        public void Connect()
        {
            Connect(null);
        }

        public void Disconnect()
        {
            try
            {
                _cManager.TriggerDisconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            bool hasRunningBots = _algoEnv.LocalAgent.HasRunningBots;

            var exit = new ConfirmationDialogViewModel(DialogButton.YesNo, hasRunningBots ? DialogMode.Warning : DialogMode.Question, DialogMessages.ExitTitle, DialogMessages.ExitMessage, _algoEnv.LocalAgent.HasRunningBots ? DialogMessages.BotsWorkError : null);
            await _wndManager.ShowDialog(exit, this);

            var isConfirmed = exit.DialogResult == DialogResult.OK;

            if (isConfirmed)
                await StopTerminal();

            return isConfirmed;
        }

        private async Task StopTerminal()
        {
            await _storage.ProfileManager.StopCurrentProfile();

            var shutdown = new ShutdownDialogViewModel(_algoEnv.LocalAgent);

            if (IsActive)
                await _wndManager.ShowDialog(shutdown, this);
            else
                await shutdown.WaitShutdownBackground();
        }

        public async void Connect(AccountAuthEntry creds = null)
        {
            try
            {
                if (creds == null || !creds.HasPassword)
                {
                    LoginDialogViewModel model = new LoginDialogViewModel(_cManager, creds);
                    await _wndManager.ShowDialog(model, this);
                }
                else
                    _cManager.TriggerConnect(creds);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void About()
        {
            AboutDialogViewModel model = new AboutDialogViewModel();
            _ = _wndManager.ShowDialog(model, this);
        }

        public void Exit()
        {
            TryCloseAsync();
        }

        public void Update()
        {
            AutoUpdateViewModel vm = new AutoUpdateViewModel(_autoUpdateSvc);
            _ = _wndManager.ShowDialog(vm, this);
        }

        public void OpenUpdate(BotAgentViewModel agent)
        {
            AutoUpdateViewModel vm = new AutoUpdateViewModel(_autoUpdateSvc, agent);
            _ = _wndManager.ShowDialog(vm, this);
        }

        public void OpenChart(string smb)
        {
            Charts.Open(smb);
        }

        public void ShowChart(string smb, Feed.Types.Timeframe period)
        {
            Charts.OpenOrActivate(smb, period);
        }

        public DialogResult ShowDialog(DialogButton buttons, DialogMode mode, string title, string message, string error)
        {
            var dialog = new ConfirmationDialogViewModel(buttons, mode, title, message, error);
            _wndManager.ShowDialog(dialog, this);

            return dialog.DialogResult;
        }

        public TradeInfoViewModel Trade { get; }
        public TradeHistoryViewModel TradeHistory { get; }
        public AlgoListViewModel AlgoList { get; }
        public SymbolListViewModel SymbolList { get; }
        public ChartCollectionViewModel Charts { get; private set; }
        public AccountPaneViewModel AccountPane { get; private set; }
        public JournalViewModel Journal { get; set; }
        //public BotJournalViewModel BotJournal { get; set; }
        public UiLock ConnectionLock { get; private set; }
        public IProfileLoader ProfileLoader => this;
        public ProfileManagerViewModel ProfileManager { get; private set; }
        public BacktesterViewModel Backtester { get; private set; }
        public SettingsStorage<PreferencesStorageModel> Preferences => _storage.PreferencesStorage;
        public BotListViewModel BotList { get; }
        public WindowManager ToolWndManager => _wndManager;
        public DockManagerService DockManagerService { get; set; }
        public LocalAlgoAgent2 Agent { get; }
        public ConnectionManager ConnectionManager => _cManager;
        public ConnectionModel.States ConnectionState => _cManager.Connection.State;
        public string CurrentServerName => _cManager.Connection.CurrentServer;
        public string ProtocolName => _cManager.Connection.CurrentProtocol;
        public EventJournal EventJournal => _eventJournal;

        //public NotificationsViewModel Notifications { get; private set; }

        public AlertViewModel AlertsManager { get; }

        public bool HasNewVersion => _autoUpdateSvc.HasNewVersion;

        public string NewVersionInfo => HasNewVersion ? $"New version available '{_autoUpdateSvc.NewVersion}'" : null;


        public async Task Shutdown()
        {
            isClosed = true;

            try
            {
                await _cManager.Disconnect();
                await _symbolsCatalog.CloseCatalog();
                await _botAgentManager.ShutdownDisconnect();
                await _storage.Stop();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Shutdown() failed.");
            }
        }

        protected override void OnViewLoaded(object view)
        {
            _eventJournal.Info("AlgoTerminal started");
            PrintSystemInfo();

            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(OnLoaded));
        }

        public void OnLoaded()
        {
            _botAgentManager.RestoreConnections();
            if (_autoUpdateSvc.HasPendingUpdate)
                Update(); // show update result first, if available
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
                    _eventJournal.Info("{0} ({1}), {2}, RAM: {3} / {4} Mb, UTC{5}{6:hh\\:mm}",
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
            var last = _cManager.GetLast();
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

                LogState(builder, "ConnectionManager", _cManager.State.ToString());
                LogState(builder, "Connection", _cManager.Connection.State.ToString());

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

        public Task OpenStorageManager()
        {
            _smbManager ??= new SymbolManagerViewModel(_clientModel, _symbolsCatalog, ToolWndManager);

            return _wndManager.ShowDialog(_smbManager, this);
        }

        public Task OpenBotsRepository()
        {
            _botsRepository ??= new BotsRepositoryViewModel(Agent);

            return _wndManager.ShowDialog(_botsRepository, this);
        }

        public void OpenBacktester()
        {
            if (Backtester == null)
            {
                Backtester = new BacktesterViewModel(_algoEnv, _clientModel, _symbolsCatalog, this, _storage.ProfileManager);
                Backtester.Deactivated += Backtester_Deactivated;
            }

            _wndManager.OpenMdiWindow(Backtester);
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
                DockManagerService.SaveLayoutSnapshot(profileStorage);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to save profile snapshot");
            }
        }

        private void InitAutoUpdateSources()
        {
            foreach (var src in _storage.PreferencesStorage.StorageModel.AppUpdateSources)
            {
                _autoUpdateSvc.AddSource(new UpdateDownloadSource(src.Name, src.Uri));
            }
        }

        private void OnNewVersionAvailable()
        {
            NotifyOfPropertyChange(nameof(HasNewVersion));
            NotifyOfPropertyChange(nameof(NewVersionInfo));
        }

        #region IProfileLoader implementation

        public void ReloadProfile(CancellationToken token)
        {
            var loading = new ProfileLoadingDialogViewModel(Charts, _storage.ProfileManager, token, Agent, DockManagerService);
            _wndManager.ShowDialog(loading, this);
        }

        #endregion
    }
}
