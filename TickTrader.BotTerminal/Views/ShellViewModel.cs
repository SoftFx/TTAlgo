using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TickTrader.BotTerminal
{
    internal class ShellViewModel : Screen, IConnectionViewModel
    {
        private ConnectionManager cManager;
        private TraderModel trade;
        private FeedModel feed;
        private WindowManager wndManager;
        private AlgoCatalog catalog = new AlgoCatalog();
        private PersistModel storage;
        private EventJournal tradeJournal;
        private bool isClosed;

        public ShellViewModel()
        {
            DisplayName = "Bot Trader";

            storage = new PersistModel();

            wndManager = new MdiWindowManager(this);

            cManager = new ConnectionManager(storage);
            trade = new TraderModel(cManager.Connection);
            feed = new FeedModel(cManager.Connection);
            tradeJournal = new EventJournal();

            AlgoList = new AlgoListViewModel();
            SymbolList = new SymbolListViewModel(feed.Symbols);
            PositionList = new PositionListViewModel(trade.Account);
            OrderList = new OrderListViewModel(trade.Account);
            Charts = new ChartCollectionViewModel(feed, catalog, wndManager);
            AccountPane = new AccountPaneViewModel(cManager, this);
            Journal = new JournalViewModel(tradeJournal);
            CanConnect = true;

            UpdateCommandStates(cManager.State);
            cManager.StateChanged += UpdateCommandStates;

            SymbolList.NewChartRequested += s => Charts.Open(s);

            LogStateLoop();
        }

        private void UpdateCommandStates(ConnectionManager.States state)
        {
            CanConnect = true;
            CanDisconnect = state == ConnectionManager.States.Online;
            CanCancel = state == ConnectionManager.States.Connecting;
            NotifyOfPropertyChange("CanConnect");
            NotifyOfPropertyChange("CanDisconnect");
            NotifyOfPropertyChange("CanCancel");
        }

        public bool CanConnect { get; private set; }
        public bool CanDisconnect { get; private set; }
        public bool CanCancel { get; private set; }

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
                System.Diagnostics.Debug.WriteLine(ex);
            }
            //SetBusyConnecting(false);
        }

        public void Cancel()
        {
            try
            {
                //await cManager.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                cManager.TriggerDisconnect();
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
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public AlgoListViewModel AlgoList { get; set; }
        public SymbolListViewModel SymbolList { get; private set; }
        public PositionListViewModel PositionList { get; private set; }
        public OrderListViewModel OrderList { get; private set; }
        public ChartCollectionViewModel Charts { get; private set; }
        public AccountPaneViewModel AccountPane { get; private set; }
        public JournalViewModel Journal { get; set; }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            isClosed = true;
            base.TryClose(dialogResult);
        }

        protected override void OnViewLoaded(object view)
        {
            ConnectLast();
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

                Console.WriteLine(builder.ToString());

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private static void LogState(StringBuilder builder, string name, string state)
        {
            builder.Append(name).Append(':').Append(state).Append(" ");
        }
    }

    internal interface IConnectionViewModel
    {
        void Connect(AccountAuthEntry creds);
    }
}
