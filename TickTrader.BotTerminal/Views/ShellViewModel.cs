using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen
    {
        private AuthManager authModel;
        private ConnectionModel connection;
        private TraderModel trade;
        private FeedModel feed;
        private WindowManager wndManager;
        private AlgoCatalog catalog = new AlgoCatalog();
        private PersistModel storage;

        public ShellViewModel()
        {
            DisplayName = "Bot Trader";

            storage = new PersistModel();

            wndManager = new MdiWindowManager(this);

            authModel = new AuthManager(storage.AuthSettingsStorage);
            connection = new ConnectionModel();
            trade = new TraderModel(connection);
            feed = new FeedModel(connection);

            SymbolList = new SymbolListViewModel(feed.Symbols);
            PositionList = new PositionListViewModel(trade.Account);
            OrderList = new OrderListViewModel(trade.Account);
            Charts = new ChartCollectionViewModel(feed, catalog, wndManager);
            AccountPane = new AccountPaneViewModel();
            CanConnect = true;

            UpdateCommandStates(ConnectionModel.States.Offline, connection.State.Current);
            connection.State.StateChanged += UpdateCommandStates;

            SymbolList.NewChartRequested += s => Charts.Open(s);
        }

        private void UpdateCommandStates(ConnectionModel.States oldState, ConnectionModel.States connectionState)
        {
            CanConnect = connectionState != ConnectionModel.States.Deinitializing;
            CanDisconnect = connectionState == ConnectionModel.States.Online;
            CanCancel = connectionState == ConnectionModel.States.Initializing || connectionState == ConnectionModel.States.WaitingLogon;
            NotifyOfPropertyChange("CanConnect");
            NotifyOfPropertyChange("CanDisconnect");
            NotifyOfPropertyChange("CanCancel");
        }

        public bool CanConnect { get; private set; }
        public bool CanDisconnect { get; private set; }
        public bool CanCancel { get; private set; }

        public void Connect()
        {
            try
            {
                LoginDialogViewModel model = new LoginDialogViewModel(authModel, connection);
                wndManager.ShowDialog(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public async Task Disconnect()
        {
            try
            {
                //SetBusyConnecting(true);
                await connection.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            //SetBusyConnecting(false);
        }

        public async Task Cancel()
        {
            try
            {
                await connection.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                connection.StartDisconnecting();
        }

        public void ManageAccounts()
        {
            
        }

        public SymbolListViewModel SymbolList { get; private set; }
        public PositionListViewModel PositionList { get; private set; }
        public OrderListViewModel OrderList { get; private set; }
        public ChartCollectionViewModel Charts { get; private set; }
        public AccountPaneViewModel AccountPane { get; private set; }
    }
}
