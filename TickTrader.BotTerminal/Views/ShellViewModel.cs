using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using System.Windows.Threading;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen, IConnectionViewModel, OrderUi, IShell, ToolWindowsManager
    {
        private ConnectionManager cManager;
        private TraderModel trade;
        private FeedModel feed;
        private WindowManager wndManager;
        private PluginCatalog catalog = new PluginCatalog();
        private PersistModel storage;
        private EventJournal eventJournal;
        private BotJournal botJournal;
        private Logger logger;
        private bool isClosed;

        public ShellViewModel()
        {
            DisplayName = "Bot Trader";

            logger = NLog.LogManager.GetCurrentClassLogger();
            eventJournal = new EventJournal(1000);
            botJournal = new BotJournal(1000);
            storage = new PersistModel();

            wndManager = new MdiWindowManager(this);

            cManager = new ConnectionManager(storage, eventJournal);
            trade = new TraderModel(cManager.Connection);
            feed = new FeedModel(cManager.Connection);

            ConnectionLock = new UiLock();
            AlgoList = new AlgoListViewModel(catalog);
            SymbolList = new SymbolListViewModel(feed.Symbols, this);

            var netPositions = new NetPositionListViewModel(trade.Account);
            var grossPositions = new GrossPositionListViewModel(trade.Account);
            var positionList = new PositionListViewModel(netPositions, grossPositions);
            var orderList = new OrderListViewModel(trade.Account);
            var assets = new AssetsViewModel(trade.Account);
            Trade = new TradeInfoViewModel(orderList, positionList, assets);
           
            TradeHistory = new TradeHistoryViewModel(trade.Account);
            

            Charts = new ChartCollectionViewModel(feed, catalog, this, botJournal);
            AccountPane = new AccountPaneViewModel(cManager, this, this);
            Journal = new JournalViewModel(eventJournal);
            BotJournal = new BotJournalViewModel(botJournal);
            CanConnect = true;

            UpdateCommandStates();
            cManager.StateChanged += s => UpdateCommandStates();
            SymbolList.NewChartRequested += s => Charts.Open(s);
            ConnectionLock.PropertyChanged += (s, a) => UpdateCommandStates();

            catalog.AddFolder(EnvService.Instance.AlgoRepositoryFolder);
            catalog.AddAssembly(Assembly.Load("TickTrader.Algo.Indicators"));

            LogStateLoop();
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
                //SetBusyConnecting(true);
                cManager.TriggerDisconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            //SetBusyConnecting(false);
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                cManager.TriggerDisconnect();
        }
        public override void CanClose(Action<bool> callback)
        {
            var exit = new ExitDialogViewModel();
            wndManager.ShowDialog(exit);
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

        public TradeInfoViewModel Trade { get; }
        public TradeHistoryViewModel TradeHistory { get; }
        public AlgoListViewModel AlgoList { get; set; }
        public SymbolListViewModel SymbolList { get; private set; }
        public ChartCollectionViewModel Charts { get; private set; }
        public AccountPaneViewModel AccountPane { get; private set; }
        public JournalViewModel Journal { get; set; }
        public BotJournalViewModel BotJournal { get; set; }
        public OrderUi OrderCommands { get { return this; } }
        public UiLock ConnectionLock { get; private set; }
        public ToolWindowsManager ToolWndManager { get { return this; } }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            isClosed = true;
            base.TryClose(dialogResult);
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
                LogState(builder, "Feed.Symbols", feed.Symbols.State.ToString());
                LogState(builder, "Trade.Account", trade.Account.State.Current.ToString());

                logger.Debug(builder.ToString());

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private static void LogState(StringBuilder builder, string name, string state)
        {
            builder.Append(name).Append(':').Append(state).Append(" ");
        }

        #region OrderUi implementation

        public void OpenMarkerOrder(string symbol)
        {
            try
            {
                using (var openOrderModel = new OpenOrderDialogViewModel(trade, feed, symbol))
                    wndManager.ShowWindow(openOrderModel);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void OpenMarkerOrder(string symbol, decimal volume, OrderSides side)
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

        #endregion
    }

    internal interface IConnectionViewModel
    {
        void Connect(AccountAuthEntry creds);
    }
}
