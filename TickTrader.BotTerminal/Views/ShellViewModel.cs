using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Controls;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen, IConnectionViewModel, iOrderUi, IShell, IProfileLoader
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ConnectionManager cManager;
        private TraderClientModel clientModel;
        private WindowManager wndManager;
        private PersistModel storage;
        private EventJournal eventJournal;
        private bool isClosed;
        private INotificationCenter notificationCenter;
        private AlgoEnvironment algoEnv;
        private BotsWarden botsWarden;
        private SymbolManagerViewModel _smbManager;
        private CustomFeedStorage _userSymbols = new CustomFeedStorage();

        public ShellViewModel()
        {
            DisplayName = EnvService.Instance.ApplicationName;

            //var botNameAggregator = new BotNameAggregator();

            notificationCenter = new NotificationCenter(new PopupNotification(), new SoundNotification());
            eventJournal = new EventJournal(1000);
            storage = new PersistModel();
            ThemeSelector.Instance.InitializeSettings(storage);

            wndManager = new WindowManager(this);

            algoEnv = new AlgoEnvironment();
            cManager = new ConnectionManager(storage, eventJournal, algoEnv);
            clientModel = new TraderClientModel(cManager.Connection, eventJournal);
            algoEnv.Init(clientModel.ObservableSymbolList);

            ProfileManager = new ProfileManagerViewModel(this, storage);

            ConnectionLock = new UiLock();
            AlgoList = new AlgoListViewModel(algoEnv.Repo);
            SymbolList = new SymbolListViewModel(clientModel.Symbols, this);

            Trade = new TradeInfoViewModel(clientModel);

            TradeHistory = new TradeHistoryViewModel(clientModel);

            Notifications = new NotificationsViewModel(notificationCenter, clientModel.Account, cManager, storage);

            Charts = new ChartCollectionViewModel(clientModel, this, algoEnv, storage);

            botsWarden = new BotsWarden(new BotAggregator(Charts));
            
            AccountPane = new AccountPaneViewModel(cManager, this, this);
            Journal = new JournalViewModel(eventJournal);
            BotJournal = new BotJournalViewModel(algoEnv.BotJournal);
            CanConnect = true;
            DockManagerService = new DockManagerNotification();

            UpdateCommandStates();
            cManager.StateChanged += (o, n) => UpdateDisplayName();
            cManager.StateChanged += (o, n) => UpdateCommandStates();
            ConnectionLock.PropertyChanged += (s, a) => UpdateCommandStates();

            clientModel.Initializing += LoadConnectionProfile;
            clientModel.Connected += OpenDefaultChart;

            storage.ProfileManager.SaveProfileSnapshot = Charts.SaveProfileSnapshot;

            cManager.StateChanged += (f, t) =>
            {
                NotifyOfPropertyChange(nameof(ConnectionState));
                NotifyOfPropertyChange(nameof(CurrentServerName));
                NotifyOfPropertyChange(nameof(ProtocolName));
            };
            //cManager.CredsChanged += () => NotifyOfPropertyChange(nameof(CurrentServerName));

            LogStateLoop();

            _userSymbols.Start(EnvService.Instance.CustomFeedCacheFolder);

        }

        private void OpenDefaultChart()
        {
            if (clientModel.Symbols.Snapshot.Count > 0 && Charts.Items.Count == 0)
            {
                var defaultSymbol = string.Empty;
                switch (clientModel.Account.Type)
                {
                    case AccountTypes.Gross:
                    case AccountTypes.Cash:
                        defaultSymbol = "EURUSD";
                        break;
                    case AccountTypes.Net:
                        defaultSymbol = "EUR/USD";
                        break;
                }

                Charts.Open(clientModel.Symbols.GetOrDefault(defaultSymbol)?.Name ?? clientModel.Symbols.Snapshot.First().Key);
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
            CanConnect = !ConnectionLock.IsLocked;
            CanDisconnect = state == ConnectionModel.States.Online && !ConnectionLock.IsLocked;
            NotifyOfPropertyChange(nameof(CanConnect));
            NotifyOfPropertyChange(nameof(CanDisconnect));
        }

        private async Task LoadConnectionProfile(object sender, CancellationToken token)
        {
            await ProfileManager.LoadConnectionProfile(cManager.Creds.Server.Address, cManager.Creds.Login, token);
        }

        public bool CanConnect { get; private set; }
        public bool CanDisconnect { get; private set; }

        public void SaveLayout()
        {
            DockManagerService.SaveLayout();
        }

        public void LoadLayout()
        {
            DockManagerService.LoadLayout();
        }

        public void Connect()
        {
            Connect(null);
        }

        public void Disconnect()
        {
            try
            {
                cManager.TriggerDisconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                Shutdown();
        }

        public override void CanClose(Action<bool> callback)
        {
            var exit = new ExitDialogViewModel(Charts.Items.Any(c => c.HasStartedBots));
            wndManager.ShowDialog(exit, this);
            if (exit.IsConfirmed)
            {
                storage.ProfileManager.Stop();
                if (exit.HasStartedBots)
                {
                    var shutdown = new ShutdownDialogViewModel(Charts);
                    wndManager.ShowDialog(shutdown, this);
                }
            }
            callback(exit.IsConfirmed);
        }

        public void ManageAccounts()
        {
        }

        public void Connect(AccountAuthEntry creds = null)
        {
            try
            {
                if (creds == null || !creds.HasPassword)
                {
                    LoginDialogViewModel model = new LoginDialogViewModel(cManager, creds);
                    wndManager.ShowDialog(model, this);
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
            TryClose();
        }

        public void OpenChart(string smb)
        {
            Charts.Open(smb);
        }

        public TradeInfoViewModel Trade { get; }
        public TradeHistoryViewModel TradeHistory { get; }
        public AlgoListViewModel AlgoList { get; set; }
        public SymbolListViewModel SymbolList { get; private set; }
        public ChartCollectionViewModel Charts { get; private set; }
        public AccountPaneViewModel AccountPane { get; private set; }
        public JournalViewModel Journal { get; set; }
        public BotJournalViewModel BotJournal { get; set; }
        public iOrderUi OrderCommands { get { return this; } }
        public UiLock ConnectionLock { get; private set; }
        public IProfileLoader ProfileLoader => this;
        public ProfileManagerViewModel ProfileManager { get; private set; }
        public SettingsStorage<PreferencesStorageModel> Preferences => storage.PreferencesStorage;
        public WindowManager ToolWndManager => wndManager;
        public DockManagerNotification DockManagerService { get; set; }

        public ConnectionModel.States ConnectionState => cManager.Connection.State;
        public string CurrentServerName => cManager.Connection.CurrentServer;
        public string ProtocolName => cManager.Connection.CurrentProtocol;

        public NotificationsViewModel Notifications { get; private set; }

        private async void Shutdown()
        {
            isClosed = true;

            try
            {
                await cManager.Disconnect();
                await Task.Factory.StartNew(() => _userSymbols.Stop());
            }
            catch (Exception) { }

            await storage.Stop();

            App.Current.Shutdown();
        }

        protected override void OnViewLoaded(object view)
        {
            eventJournal.Info("BotTrader started");
            PrintSystemInfo();
            ConnectLast();
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

        private void ConnectLast()
        {
            var last = cManager.GetLast();
            if (last != null)
                Connect(last);
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
                _smbManager = new SymbolManagerViewModel(clientModel, _userSymbols, ToolWndManager);

            wndManager.ShowDialog(_smbManager, this);
        }

        public void CloseChart(object chart)
        {
        }

        #region OrderUi implementation

        public void OpenMarkerOrder(string symbol)
        {
            try
            {
                using (var openOrderModel = new OpenOrderDialogViewModel(clientModel, symbol))
                    wndManager.OpenMdiWindow(openOrderModel);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void OpenMarkerOrder(string symbol, decimal volume, OrderSide side)
        {

        }

        #endregion

        #region IProfileLoader implementation

        public void ReloadProfile(CancellationToken token)
        {
            var loading = new ProfileLoadingDialogViewModel(Charts, storage.ProfileManager, token);
            wndManager.ShowDialog(loading, this);
        }

        #endregion
    }

    internal interface IConnectionViewModel
    {
        void Connect(AccountAuthEntry creds);
    }
}
