using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    class BacktesterTradeGridViewModel
    {
        private List<TransactionReport> _reports;

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
            _reports = reports;
            GridView.SetCollection(reports);
        }

        public Task SaveAsCsv(Stream entryStream)
        {
            return Task.Factory.StartNew(() =>
            {
                TradeReportCsvSerializer.Serialize(_reports, entryStream, GridView.GetAccTypeValue());
            });
        }
    }
}
