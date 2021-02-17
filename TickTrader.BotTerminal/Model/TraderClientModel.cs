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
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class TraderClientModel : EntityBase, IConnectionStatusInfo
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

            Account = _core.Cache.Account;
            Symbols = _core.Symbols;
            SortedSymbols = Symbols.Select((k, v)=> v).OrderBy((k, v) => k).AsObservable();

            var orderedCurrencies = Currencies.OrderBy((k, v) => k);
            SortedCurrencies = orderedCurrencies.AsObservable();
            SortedCurrenciesNames = orderedCurrencies.Select(c => c.Name).AsObservable();
            //this.History = new FeedHistoryProviderModel(connection, EnvService.Instance.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerHierarchy);

            _accountInfo = Account;
            _journal = journal;
        }

        public IAccountProxy GetAccountProxy()
        {
            var sync = new DispatcherSync();
            var accInfoProvider = new PluginTradeInfoProvider(Cache, sync);
            var feedProvider = new PluginFeedProvider(Cache, Distributor, FeedHistory, sync);
            return new LocalAccountProxy(_core.Id)
            {
                Feed = feedProvider,
                FeedHistory = feedProvider,
                Metadata = feedProvider,
                AccInfoProvider = accInfoProvider,
                TradeExecutor = _core.TradeApi,
                TradeHistoryProvider = _core.TradeHistory.AlgoAdapter,
            };
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
                _isConnected.Set();
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
                _isConnected.Clear();
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

        private void Account_BalanceUpdated(Algo.Domain.BalanceOperation report)
        {
            string action = report.Type == Algo.Domain.BalanceOperation.Types.Type.Dividend ? "Devidend" : report.TransactionAmount > 0 ? "Deposit" : "Withdrawal";
            _journal.Trading($"{action} {report.TransactionAmount} {report.Currency}. Balance: {report.Balance} {report.Currency}");
        }

        //private void Account_OrderUpdated(OrderUpdateInfo updateInfo)
        //{
        //    var order = updateInfo.Order;

        //    switch(updateInfo.ExecAction)
        //    {
        //        case Algo.Domain.OrderExecReport.Types.ExecAction.Opened:
        //            switch (order.OrderType)
        //            {
        //                case Algo.Domain.OrderInfo.Types.Type.Position:
        //                    _journal.Trading($"Order #{order.Id} was opened: {order.Side} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
        //                    break;
        //                case Algo.Domain.OrderInfo.Types.Type.Limit:
        //                case Algo.Domain.OrderInfo.Types.Type.Stop:
        //                    _journal.Trading($"Order #{order.Id} was placed: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
        //                    break;
        //            }
        //            break;
        //        case Algo.Domain.OrderExecReport.Types.ExecAction.Modified:
        //            switch (order.OrderType)
        //            {
        //                case Algo.Domain.OrderInfo.Types.Type.Position:
        //                    _journal.Trading($"Order #{order.Id} was modified: {order.Side} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
        //                    break;
        //                case Algo.Domain.OrderInfo.Types.Type.Limit:
        //                case Algo.Domain.OrderInfo.Types.Type.Stop:
        //                    _journal.Trading($"Order #{order.Id} was modified: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
        //                    break;
        //            }
        //            break;
        //        case Algo.Domain.OrderExecReport.Types.ExecAction.Closed:
        //            if (order.OrderType == Algo.Domain.OrderInfo.Types.Type.Position)
        //            {
        //                _journal.Trading($"Order #{order.Id} was closed: {order.Side} {order.Symbol} {order.LastFillAmountLots} lots at {order.LastFillPrice}");
        //            }
        //            break;
        //        case Algo.Domain.OrderExecReport.Types.ExecAction.Canceled:
        //            _journal.Trading($"Order #{order.Id} was canceled: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
        //            break;
        //        case Algo.Domain.OrderExecReport.Types.ExecAction.Expired:
        //            _journal.Trading($"Order #{order.Id} has expired: {order.Side} {order.OrderType} {order.Symbol} {order.RemainingAmountLots} lots at {order.Price}");
        //            break;
        //        case Algo.Domain.OrderExecReport.Types.ExecAction.Filled:
        //            _journal.Trading($"Order #{order.Id} was filled: {order.Side} {order.OrderType} {order.Symbol} {order.LastFillAmountLots} lots at {order.LastFillPrice}");
        //            break;
        //    }
        //}

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
        public IVarSet<string, SymbolInfo> Symbols { get; private set; }
        public EntityCache Cache => _core.Cache;
        public IObservableList<SymbolInfo> SortedSymbols { get; }
        public QuoteDistributor Distributor => _core.Distributor;
        public FeedHistoryProviderModel.Handler FeedHistory => _core.FeedHistory;
        public IVarSet<string, CurrencyInfo> Currencies => _core.Currencies;
        public IObservableList<CurrencyInfo> SortedCurrencies { get; }
        public IObservableList<string> SortedCurrenciesNames { get; }
    }
}
