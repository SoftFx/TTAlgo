using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class TraderClientModel
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ClientCore _core;

        private IAccountInfoProvider _accountInfo;
        private EventJournal _journal;

        public TraderClientModel(ConnectionModel connection, EventJournal journal)
        {
            this.Connection = connection;

            connection.Initalizing += Connection_Initalizing;
            connection.State.StateChanged += State_StateChanged;
            connection.Deinitalizing += Connection_Deinitalizing;

            var sync = new DispatcherSync();
            _core = new ClientCore(connection, c => new SymbolCollectionModel(c), sync, sync);

            this.Symbols = (SymbolCollectionModel)_core.Symbols;
            this.TradeHistory = new TradeHistoryProviderModel(this);
            this.ObservableSymbolList = Symbols.Select((k, v)=> (SymbolModel)v).OrderBy((k, v) => k).AsObservable();
            if(Properties.Settings.Default.UseQuoteStorage)
                this.History = FeedHistoryProviderModel.CreateDiskStorage(connection, EnvService.Instance.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerHierarchy);
            else
                this.History = FeedHistoryProviderModel.CreateLightProxy(connection);
            this.TradeApi = new TradeExecutor(_core);
            this.Account = new AccountModel(_core, AccountModelOptions.EnableCalculator);

            _accountInfo = Account;
            _journal = journal;
        }

        private void State_StateChanged(ConnectionModel.States oldState, ConnectionModel.States newState)
        {
            if (newState == ConnectionModel.States.Connecting)
            {
                IsConnecting = true;
                IsConnectingChanged?.Invoke();
            }
            else
            {
                if (IsConnecting)
                {
                    IsConnecting = false;
                    IsConnectingChanged?.Invoke();
                }
            }
        }

        private void OnConnected()
        {
            try
            {
                IsConnected = true;
                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Connected() failed.");
            }
        }

        private void OnDisconnected()
        {
            try
            {
                IsConnected = false;
                Disconnected?.Invoke();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Disconnected() failed.");
            }
        }

        private async Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            try
            {
                var cache = Connection.FeedProxy.Cache;
                await History.Init();
                _core.Init();
                Account.Init();
                _accountInfo.BalanceUpdated += Account_BalanceUpdated;
                _accountInfo.OrderUpdated += Account_OrderUpdated;
                if (Initializing != null)
                    await Initializing.InvokeAsync(this, cancelToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Initalizing() failed.");
            }

            OnConnected();
        }

        private async Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            OnDisconnected();

            try
            {
                await _core.Deinit();
                Account.Deinit();
                await History.Deinit();
                if (Deinitializing != null)
                    await Deinitializing.InvokeAsync(this, cancelToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Deinitalizing() failed.");
            }
        }

        private void Account_BalanceUpdated(BalanceOperationReport report)
        {
            string action = report.Amount > 0 ? "Deposit" : "Withdrawal";
            _journal.Trading($"{action} {report.Amount} {report.CurrencyCode}. Balance: {report.Balance} {report.CurrencyCode}");
        }

        private void Account_OrderUpdated(OrderExecReport report)
        {
            var order = report.OrderCopy;
            switch(report.ExecAction)
            {
                case OrderExecAction.Opened:
                    switch (order.Type)
                    {
                        case OrderType.Position:
                            _journal.Trading($"Order #{order.Id} was opened: {order.Side} {order.Symbol} {order.RemainingVolume.Lots} lots at {order.Price}");
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            _journal.Trading($"Order #{order.Id} was placed: {order.Side} {order.Type} {order.Symbol} {order.RemainingVolume.Lots} lots at {order.Price}");
                            break;
                    }
                    break;
                case OrderExecAction.Modified:
                    switch (order.Type)
                    {
                        case OrderType.Position:
                            _journal.Trading($"Order #{order.Id} was modified: {order.Side} {order.Symbol} {order.RemainingVolume.Lots} lots at {order.Price}");
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            _journal.Trading($"Order #{order.Id} was modified: {order.Side} {order.Type} {order.Symbol} {order.RemainingVolume.Lots} lots at {order.Price}");
                            break;
                    }
                    break;
                case OrderExecAction.Closed:
                    if (order.Type == Algo.Api.OrderType.Position)
                    {
                        _journal.Trading($"Order #{order.Id} was closed: {order.Side} {order.Symbol} {order.LastFillVolume.Lots} lots at {order.LastFillPrice}");
                    }
                    break;
                case OrderExecAction.Canceled:
                    _journal.Trading($"Order #{order.Id} was canceled: {order.Side} {order.Type} {order.Symbol} {order.RemainingVolume.Lots} lots at {order.Price}");
                    break;
                case OrderExecAction.Expired:
                    _journal.Trading($"Order #{order.Id} has expired: {order.Side} {order.Type} {order.Symbol} {order.RemainingVolume.Lots} lots at {order.Price}");
                    break;
                case OrderExecAction.Filled:
                    _journal.Trading($"Order #{order.Id} was filled: {order.Side} {order.Type} {order.Symbol} {order.LastFillVolume.Lots} lots at {order.LastFillPrice}");
                    break;
            }
        }

        public bool IsConnecting { get; private set; }
        public bool IsConnected { get; private set; }

        public event AsyncEventHandler Initializing;
        public event Action IsConnectingChanged;
        public event Action Connected;
        public event AsyncEventHandler Deinitializing;
        public event Action Disconnected;

        public ConnectionModel Connection { get; private set; }
        public TradeExecutor TradeApi { get; private set; }
        public AccountModel Account { get; private set; }
        public TradeHistoryProviderModel TradeHistory { get; }
        public SymbolCollectionModel Symbols { get; private set; }
        public IReadOnlyList<SymbolModel> ObservableSymbolList { get; private set; }
        public QuoteDistributor Distributor { get { return (QuoteDistributor)Symbols.Distributor; } }
        public FeedHistoryProviderModel History { get; private set; }
        public Dictionary<string, CurrencyInfo> Currencies => _core.Currencies;
    }
}
