using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Account History Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Prints account trade history records.")]
    public class TradeHistoryBot : TradeBot
    {
        private StringBuilder _builder = new StringBuilder();
        private int count;

        [Parameter(DisplayName = "Execution")]
        public SyncOrAsync ExecType { get; set; }

        [Parameter(DisplayName = "Time filter")]
        public TradeHistoryFilters Filter { get; set; }

        protected async override void OnStart()
        {
            var builder = new StringBuilder();

            try
            {
                if (ExecType == SyncOrAsync.Sync)
                {
                    if (Filter == TradeHistoryFilters.All)
                        Print(Account.TradeHistory);
                    else if (Filter == TradeHistoryFilters.Today)
                        Print(Account.TradeHistory.GetRange(DateTime.Today, DateTime.Today + TimeSpan.FromDays(1)));
                    else if (Filter == TradeHistoryFilters.Yesterday)
                        Print(Account.TradeHistory.GetRange(DateTime.Today - TimeSpan.FromDays(1), DateTime.Today));
                    else if (Filter == TradeHistoryFilters.ThisYear)
                    {
                        var from = new DateTime(DateTime.Now.Year, 1, 1);
                        var to = new DateTime(DateTime.Now.Year + 1, 1, 1);
                        Print(Account.TradeHistory.GetRange(from, to));
                    }
                    else if (Filter == TradeHistoryFilters.PreviousYear)
                    {
                        var from = new DateTime(DateTime.Now.Year -1, 1, 1);
                        var to = new DateTime(DateTime.Now.Year, 1, 1);
                        Print(Account.TradeHistory.GetRange(from, to));
                    }
                }
                else
                {
                    if (Filter == TradeHistoryFilters.All)
                        await Print(Account.TradeHistory.GetAsync());
                    else if (Filter == TradeHistoryFilters.Today)
                        await Print(Account.TradeHistory.GetRangeAsync(DateTime.Today, DateTime.Today + TimeSpan.FromDays(1)));
                    else if (Filter == TradeHistoryFilters.Yesterday)
                        await Print(Account.TradeHistory.GetRangeAsync(DateTime.Today - TimeSpan.FromDays(1), DateTime.Today));
                    else if (Filter == TradeHistoryFilters.ThisYear)
                    {
                        var from = new DateTime(DateTime.Now.Year, 1, 1);
                        var to = new DateTime(DateTime.Now.Year + 1, 1, 1);
                        await Print(Account.TradeHistory.GetRangeAsync(from, to));
                    }
                    else if (Filter == TradeHistoryFilters.PreviousYear)
                    {
                        var from = new DateTime(DateTime.Now.Year - 1, 1, 1);
                        var to = new DateTime(DateTime.Now.Year, 1, 1);
                        await Print(Account.TradeHistory.GetRangeAsync(from, to));
                    }
                }

                Status.WriteLine("Done! Printed {0} history records.", count);
            }
            catch (Exception ex)
            {
                Status.Write("History retrieval failed: " + ex.Message);
            }

            Exit();
        }

        private void Print(IEnumerable<TradeReport> reports)
        {
            foreach (var item in reports)
            {
                Print(item);
                count++;
            }
        }

        private async Task Print(IAsyncEnumerator<TradeReport> reports)
        {
            while (true)
            {
                var page = await reports.GetNextPage();
                if (page == null)
                    return;

                foreach (var item in page)
                {
                    Print(item);
                    count++;
                }
            }
        }

        private void Print(TradeReport item)
        {
            _builder.Clear();
            _builder.Append(item.ReportId).Append(" ");
            _builder.Append(item.ReportTime).Append(" ");
            _builder.Append(item.ActionType).Append(" ");

            if (item.Type != TradeRecordTypes.Unknown && item.Type != TradeRecordTypes.Withdrawal)
                _builder.Append(item.Symbol).Append(" ");

            Print(_builder.ToString());
        }
    }

    public enum SyncOrAsync
    {
        Sync,
        Async
    }

    public enum TradeHistoryFilters
    {
        All,
        Today,
        Yesterday,
        ThisYear,
        PreviousYear
    }
}
