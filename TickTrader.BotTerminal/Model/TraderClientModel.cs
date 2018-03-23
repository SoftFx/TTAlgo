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
using Machinarium.Var;

namespace TickTrader.BotTerminal
{
    internal class TraderClientModel : EntityBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ClientModel.Data _core;

        private AccountModel _accountInfo;
        private EventJournal _journal;
        private BoolProperty _isConnected;

        public TraderClientModel(ClientModel.Data client, EventJournal journal)
        {
            _core = client;
            Connection = client.Connection;

            _isConnected = AddBoolProperty();

            Connection.Initalizing += Connection_Initalizing;
            Connection.StateChanged += State_StateChanged;
            Connection.Deinitalizing += Connection_Deinitalizing;

            this.Account = _core.Cache.Account;
            this.Symbols = _core.Symbols;
            this.ObservableSymbolList = Symbols.Select((k, v)=> (SymbolModel)v).OrderBy((k, v) => k).AsObservable();
            //this.History = new FeedHistoryProviderModel(connection, EnvService.Instance.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerHierarchy);

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
                IsConnected.Set();
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
                IsConnected.Unset();
                Disconnected?.Invoke();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Disconnected() failed.");
            }
        }

        private async Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            _core.StartCalculator();
            await Initializing.InvokeAsync(this, cancelToken);

            OnConnected();
        }

        private async Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            OnDisconnected();

            try
            {
                Account.Deinit();
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

        private void Account_OrderUpdated(OrderUpdateInfo updateInfo)
        {
            var order = updateInfo.Order;

            switch(updateInfo.ExecAction)
            {
                case OrderExecAction.Opened:
                    switch (order.OrderType)
                    {
                        case OrderType.Position:
                            _journal.Trading($"Order #{order.Id} was opened: {order.Side} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            _journal.Trading($"Order #{order.Id} was placed: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
                            break;
                    }
                    break;
                case OrderExecAction.Modified:
                    switch (order.OrderType)
                    {
                        case OrderType.Position:
                            _journal.Trading($"Order #{order.Id} was modified: {order.Side} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            _journal.Trading($"Order #{order.Id} was modified: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
                            break;
                    }
                    break;
                case OrderExecAction.Closed:
                    if (order.OrderType == Algo.Api.OrderType.Position)
                    {
                        _journal.Trading($"Order #{order.Id} was closed: {order.Side} {order.Symbol} {order.LastFillAmountLots} lots at {order.LastFillPrice}");
                    }
                    break;
                case OrderExecAction.Canceled:
                    _journal.Trading($"Order #{order.Id} was canceled: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
                    break;
                case OrderExecAction.Expired:
                    _journal.Trading($"Order #{order.Id} has expired: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
                    break;
                case OrderExecAction.Filled:
                    _journal.Trading($"Order #{order.Id} was filled: {order.Side} {order.OrderType} {order.Symbol} {order.LastFillAmountLots} lots at {order.LastFillPrice}");
                    break;
            }
        }

        public bool IsConnecting { get; private set; }
        public BoolVar IsConnected => _isConnected.Var;

        public event AsyncEventHandler Initializing;
        public event Action IsConnectingChanged;
        public event Action Connected;
        public event AsyncEventHandler Deinitializing;
        public event Action Disconnected;

        public ConnectionModel.Handler Connection { get; private set; }
        public ITradeExecutor TradeApi => _core.TradeApi;
        public AccountModel Account { get; private set; }
        public TradeHistoryProvider.Handler TradeHistory => _core.TradeHistory;
        public IVarSet<string, SymbolModel> Symbols { get; private set; }
        public EntityCache Cache => _core.Cache;
        public IReadOnlyList<SymbolModel> ObservableSymbolList { get; private set; }
        public QuoteDistributor Distributor => _core.Distributor;
        public FeedHistoryProviderModel.Handler FeedHistory => _core.FeedHistory;
        public IVarSet<string, CurrencyEntity> Currencies => _core.Currencies;
    }
}
