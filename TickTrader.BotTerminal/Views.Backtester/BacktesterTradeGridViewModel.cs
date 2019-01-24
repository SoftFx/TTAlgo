using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    class BacktesterTradeGridViewModel
    {
        private List<TransactionReport> _reports;

        public BacktesterTradeGridViewModel()
        {
            GridView = new TradeHistoryGridViewModel(new List<TransactionReport>());
            GridView.AutoSizeColumns = false;
            GridView.ConvertTimeToLocal = false;
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

        public async Task SaveAsCsv(Stream entryStream, IActionObserver observer)
        {
            long progress = 0;

            observer.SetMessage("Saving trades...");
            observer.StartProgress(0, _reports.Count);

            Action writeCsvAction = () => TradeReportCsvSerializer.Serialize(
                _reports, entryStream, GridView.GetAccTypeValue(), i => Interlocked.Exchange(ref progress, i));

            Action updateProgressAction = () => observer.SetProgress(Interlocked.Read(ref progress));

            using (new UiUpdateTimer(updateProgressAction))
                await Task.Factory.StartNew(writeCsvAction);
        }
    }
}
