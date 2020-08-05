using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
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

        public void Start(Backtester backtester, IEnumerable<CurrencyEntity> currencies, IEnumerable<SymbolInfo> symbols)
        {
            var settings = backtester.CommonSettings;
            var accInfo = new AccountInfo(settings.InitialBalance, settings.BalanceCurrency, null);
            accInfo.Leverage = settings.Leverage;
            accInfo.Id = "1";
            accInfo.Type = settings.AccountType;

            _client.Init(accInfo, symbols, currencies);

            backtester.Executor.TradesUpdated += Executor_TradesUpdated;
            backtester.Executor.SymbolRateUpdated += Executor_SymbolRateUpdated;

            _connection.EmulateConnect();
        }

        public void Stop(Backtester backtester)
        {
            backtester.Executor.TradesUpdated -= Executor_TradesUpdated;
            backtester.Executor.SymbolRateUpdated -= Executor_SymbolRateUpdated;

            _connection.EmulateDisconnect();
            _client.Deinit();
        }

        private void Executor_SymbolRateUpdated(IRateInfo update)
        {
            _client.OnRateUpdate(update.LastQuote);
        }

        private void Executor_TradesUpdated(TesterTradeTransaction tt)
        {
            if (tt.OrderEntityAction != Algo.Domain.OrderExecReport.Types.EntityAction.NoAction)
                _client.Acc.UpdateOrderCollection(tt.OrderEntityAction, tt.OrderUpdate);

            if (tt.PositionEntityAction != Algo.Domain.OrderExecReport.Types.EntityAction.NoAction)
                _client.Acc.UpdateOrderCollection(tt.PositionEntityAction, tt.PositionUpdate);

            if (tt.NetPositionUpdate != null)
            {
                if (tt.NetPositionUpdate.PositionCopy.IsEmpty)
                    _client.Acc.RemovePosition(tt.NetPositionUpdate, true);
                else
                    _client.Acc.UpdatePosition(tt.NetPositionUpdate, true);
            }

            if (tt.Balance != null)
                _client.Acc.UpdateBalance((double)tt.Balance);
        }
    }
}
