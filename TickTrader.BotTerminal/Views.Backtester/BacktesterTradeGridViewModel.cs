using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    class BacktesterTradeGridViewModel
    {
        public BacktesterTradeGridViewModel()
        {
            GridView = new TradeHistoryGridViewModel(new List<TransactionReport>());
            GridView.AutoSizeColumns = false;
            GridView.AccType.Value = Algo.Api.AccountTypes.Gross;
        }

        public TradeHistoryGridViewModel GridView { get; }

        public void Clear(AccountTypes newAccType)
        {
            GridView.SetCollection(new List<TransactionReport>());
            GridView.AccType.Value = newAccType;
        }

        public void Fill(List<TransactionReport> reports)
        {
            GridView.SetCollection(reports);
        }
    }
}
