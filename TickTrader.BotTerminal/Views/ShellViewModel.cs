using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen, IConnectionViewModel, iOrderUi, IShell, ToolWindowsManager
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

        public ShellViewModel()
        {
            DisplayName = EnvService.Instance.ApplicationName;

            var botNameAggregator = new BotNameAggregator();

            notificationCenter = new NotificationCenter(new PopupNotification(), new SoundNotification());
            eventJournal = new EventJournal(1000);
            storage = new PersistModel();

            wndManager = new MdiWindowManager(this);

            cManager = new ConnectionManager(storage, eventJournal);
            clientModel = new TraderClientModel(cManager.Connection, eventJournal);
            algoEnv = new AlgoEnvironment(clientModel.ObservableSymbolList);

            ConnectionLock = new UiLock();
            AlgoList = new AlgoListViewModel(algoEnv.Repo);
            SymbolList = new SymbolListViewModel(clientModel.Symbols, this);

            Trade = new TradeInfoViewModel(clientModel);

            TradeHistory = new TradeHistoryViewModel(clientModel);

            Notifications = new NotificationsViewModel(notificationCenter, clientModel.Account, cManager);

            Charts = new ChartCollectionViewModel(clientModel, this, algoEnv);
            AccountPane = new AccountPaneViewModel(cManager, this, this);
            Journal = new JournalViewModel(eventJournal);
            BotJournal = new BotJournalViewModel(algoEnv.BotJournal);
            CanConnect = true;

            UpdateCommandStates();
            cManager.StateChanged += (o, n) => UpdateDisplayName();
            cManager.StateChanged += (o, n) => UpdateCommandStates();
            SymbolList.NewChartRequested += s => Charts.Open(s);
            ConnectionLock.PropertyChanged += (s, a) => UpdateCommandStates();

            clientModel.Connected += OpenDefaultChart;

            LogStateLoop();
        }

        private void OpenDefaultChart()
        {
            if (clientModel.Symbols.Snapshot.Count > 0 && Charts.Items.Count == 0)
            {
                var defaultSymbol = string.Empty;
                switch (clientModel.Account.Type)
                {
                    case SoftFX.Extended.AccountType.Gross:
                    case SoftFX.Extended.AccountType.Cash:
                        defaultSymbol = "EURUSD";
                        break;
                    case SoftFX.Extended.AccountType.Net:
                        defaultSymbol = "EUR/USD";
                        break;
                }

                Charts.Open(clientModel.Symbols.GetOrDefault(defaultSymbol)?.Name ?? clientModel.Symbols.Snapshot.First().Key);
                //clientModel.Connected -= OpenDefaultChart;
            }
        }

        private void UpdateDisplayName()
        {
            if (cManager.State == ConnectionManager.States.Online)
                DisplayName = $"{cManager.Creds.Login} {cManager.Creds.Server.Address} - {EnvService.Instance.ApplicationName}";
        }

        private void UpdateCommandStates()
        {
            var state = cManager.State;
            CanConnect = !ConnectionLock.IsLocked;
            CanDisconnect = state == ConnectionManager.States.Online && !ConnectionLock.IsLocked;
            NotifyOfPropertyChange(nameof(CanConnect));
            NotifyOfPropertyChange(nameof(CanDisconnect));
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
            wndManager.ShowDialog(exit);
            if (exit.HasStartedBots && exit.IsConfirmed)
            {
                var shutdown = new ShutdownDialogViewModel(Charts);
                wndManager.ShowDialog(shutdown);
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
                    wndManager.ShowDialog(model);
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
            wndManager.ShowDialog(model);
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
        public ToolWindowsManager ToolWndManager { get { return this; } }

        public NotificationsViewModel Notifications { get; private set; }

        private async void Shutdown()
        {
            isClosed = true;

            try
            {
                await cManager.Disconnect();
            }
            catch (Exception) { }

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
                LogState(builder, "Connection", cManager.Connection.State.Current.ToString());

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

        public void CloseChart(object chart)
        {
        }

        #region OrderUi implementation

        public void OpenMarkerOrder(string symbol)
        {
            try
            {
                using (var openOrderModel = new OpenOrderDialogViewModel(clientModel, symbol))
                    wndManager.ShowWindow(openOrderModel);
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

        #region ToolWindowsManager implementation

        private Dictionary<object, IScreen> wndModels = new Dictionary<object, IScreen>();

        public IScreen GetWindow(object key)
        {
            return wndModels.GetValueOrDefault(key);
        }

        public void OpenWindow(object wndKey, IScreen wndModel, bool closeExisting = false)
        {
            IScreen existing = GetWindow(wndKey);
            if (existing != null)
            {
                if (closeExisting)
                    existing.TryClose();
                else
                    throw new Exception("Window already opened!");
            }
            wndModel.Deactivated += WndModel_Deactivated;
            wndModels[wndKey] = wndModel;
            wndManager.ShowWindow(wndModel);
        }

        private void WndModel_Deactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed)
            {
                var wndModel = sender as IScreen;
                wndModel.Deactivated -= WndModel_Deactivated;

                var keyValue = wndModels.FirstOrDefault(m => m.Value == wndModel);
                if (keyValue.Key != null)
                    wndModels.Remove(keyValue.Key);
            }
        }

        public void CloseWindow(object wndKey)
        {
            GetWindow(wndKey)?.TryClose();
        }

        public bool? ShowDialog(IScreen dlgModel)
        {
            return wndManager.ShowDialog(dlgModel);
        }

        #endregion
    }

    internal interface IConnectionViewModel
    {
        void Connect(AccountAuthEntry creds);
    }
}
