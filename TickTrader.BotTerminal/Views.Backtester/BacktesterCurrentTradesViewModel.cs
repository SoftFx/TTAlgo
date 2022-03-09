using System.Collections.Generic;
using TickTrader.Algo.Account;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class BacktesterCurrentTradesViewModel : Page
    {
        private MockClient _client;
        private MockConnection _connection = new MockConnection();

        public BacktesterCurrentTradesViewModel(ProfileManager profile = null)
        {
            DisplayName = "Trades";
            IsVisible = false;

            _client = new MockClient();
            Trades = new TradeInfoViewModel(_client.Acc, _client.Symbols, _client.Currencies, _connection, false, profile, true);
            Rates = new SymbolListViewModel(_client.Symbols, _client.Distributor, null);
        }

        public TradeInfoViewModel Trades { get; }
        public SymbolListViewModel Rates { get; }

        public void Clear()
        {
            _client.Clear();
        }

        public void Start(BacktesterConfig config, IEnumerable<CurrencyInfo> currencies, IEnumerable<ISymbolInfo> symbols)
        {
            var accSettings = config.Account;
            var accInfo = new AccountInfo(accSettings.InitialBalance, accSettings.BalanceCurrency, null);
            accInfo.Leverage = accSettings.Leverage;
            accInfo.Id = "1";
            accInfo.Type = accSettings.Type;

            _client.Init(accInfo, symbols, currencies);

            _connection.EmulateConnect();
        }

        public void Stop()
        {
            _connection.EmulateDisconnect();
            _client.Deinit();
        }

        private void Executor_SymbolRateUpdated(IRateInfo update)
        {
            _client.OnRateUpdate(update.LastQuote);
        }

        //private void Executor_TradesUpdated(TesterTradeTransaction tt)
        //{
        //    if (tt.OrderEntityAction != Algo.Domain.OrderExecReport.Types.EntityAction.NoAction)
        //        _client.Acc.UpdateOrderCollection(tt.OrderEntityAction, tt.OrderUpdate);

        //    if (tt.PositionEntityAction != Algo.Domain.OrderExecReport.Types.EntityAction.NoAction)
        //        _client.Acc.UpdateOrderCollection(tt.PositionEntityAction, tt.PositionUpdate);

        //    if (tt.NetPositionUpdate != null)
        //    {
        //        if (tt.NetPositionUpdate.PositionCopy.IsEmpty)
        //            _client.Acc.RemovePosition(tt.NetPositionUpdate, true);
        //        else
        //            _client.Acc.UpdatePosition(tt.NetPositionUpdate, true);
        //    }

        //    if (tt.Balance != null)
        //        _client.Acc.UpdateBalance((double)tt.Balance);
        //}
    }
}
