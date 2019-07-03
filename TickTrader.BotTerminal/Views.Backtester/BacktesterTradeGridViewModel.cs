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
        private ObservableCollection<TransactionReport> _reports = new ObservableCollection<TransactionReport>();

        public BacktesterTradeGridViewModel(ProfileManager profile = null)
        {
            GridView = new TradeHistoryGridViewModel(new List<TransactionReport>(), profile, true);
            GridView.AutoSizeColumns = false;
            GridView.ConvertTimeToLocal = false;
            GridView.IsSlippageSupported = false;
            GridView.AccType.Value = Algo.Api.AccountTypes.Gross;
            GridView.SetCollection(_reports);
        }

        public TradeHistoryGridViewModel GridView { get; }

        public void OnTesterStart(AccountTypes newAccType)
        {
            _reports.Clear();
            GridView.AccType.Value = newAccType;
        }

        public void Append(TransactionReport report)
        {
            _reports.Add(report);
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
