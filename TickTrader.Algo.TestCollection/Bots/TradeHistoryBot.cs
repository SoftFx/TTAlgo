using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Account History Bot", Version = "1.0", Category = "Test Plugin Info",
        SetupMainSymbol = false, Description = "Prints account trade history records.")]
    public class TradeHistoryBot : TradeBot
    {
        private readonly StringBuilder _builder = new StringBuilder(1 << 10);
        private int count;

        [Parameter(DisplayName = "Execution")]
        public SyncOrAsync ExecType { get; set; }

        [Parameter(DisplayName = "Time filter")]
        public TradeHistoryFilters Filter { get; set; }

        [Parameter]
        public HistoryType HistoryType { get; set; }

        [Parameter]
        public bool SkipCancelOrders { get; set; }

        protected override async void OnStart()
        {
            var options = SkipCancelOrders ? ThQueryOptions.SkipCanceled : ThQueryOptions.None;

            try
            {
                if (ExecType == SyncOrAsync.Sync)
                {
                    switch (Filter)
                    {
                        case TradeHistoryFilters.All:
                            if (HistoryType == HistoryType.Trades)
                                PrintList(Account.TradeHistory.Get(options));
                            else
                                PrintList(Account.TriggerHistory.Get(options));
                            break;

                        case TradeHistoryFilters.Today:
                            if (HistoryType == HistoryType.Trades)
                                PrintList(Account.TradeHistory.GetRange(DateTime.Today, DateTime.Today + TimeSpan.FromDays(1), options));
                            else
                                PrintList(Account.TriggerHistory.GetRange(DateTime.Today, DateTime.Today + TimeSpan.FromDays(1), options));
                            break;

                        case TradeHistoryFilters.Yesterday:
                            if (HistoryType == HistoryType.Trades)
                                PrintList(Account.TradeHistory.GetRange(DateTime.Today - TimeSpan.FromDays(1), DateTime.Today, options));
                            else
                                PrintList(Account.TriggerHistory.GetRange(DateTime.Today - TimeSpan.FromDays(1), DateTime.Today, options));
                            break;

                        case TradeHistoryFilters.ThisYear:
                            var from = new DateTime(DateTime.Now.Year, 1, 1);
                            var to = new DateTime(DateTime.Now.Year + 1, 1, 1);

                            if (HistoryType == HistoryType.Trades)
                                PrintList(Account.TradeHistory.GetRange(from, to, options));
                            else
                                PrintList(Account.TriggerHistory.GetRange(from, to, options));
                            break;

                        case TradeHistoryFilters.PreviousYear:
                            from = new DateTime(DateTime.Now.Year - 1, 1, 1);
                            to = new DateTime(DateTime.Now.Year, 1, 1);

                            if (HistoryType == HistoryType.Trades)
                                PrintList(Account.TradeHistory.GetRange(from, to, options));
                            else
                                PrintList(Account.TradeHistory.GetRange(from, to, options));
                            break;
                    }
                }
                else
                {
                    switch (Filter)
                    {
                        case TradeHistoryFilters.All:
                            if (HistoryType == HistoryType.Trades)
                                await PrintList(Account.TradeHistory.GetAsync(options));
                            else
                                await PrintList(Account.TriggerHistory.GetAsync(options));
                            break;

                        case TradeHistoryFilters.Today:
                            if (HistoryType == HistoryType.Trades)
                                await PrintList(Account.TradeHistory.GetRangeAsync(DateTime.Today, DateTime.Today + TimeSpan.FromDays(1), options));
                            else
                                await PrintList(Account.TriggerHistory.GetRangeAsync(DateTime.Today, DateTime.Today + TimeSpan.FromDays(1), options));
                            break;

                        case TradeHistoryFilters.Yesterday:
                            if (HistoryType == HistoryType.Trades)
                                await PrintList(Account.TradeHistory.GetRangeAsync(DateTime.Today - TimeSpan.FromDays(1), DateTime.Today, options));
                            else
                                await PrintList(Account.TriggerHistory.GetRangeAsync(DateTime.Today - TimeSpan.FromDays(1), DateTime.Today, options));
                            break;

                        case TradeHistoryFilters.ThisYear:
                            var from = new DateTime(DateTime.Now.Year, 1, 1);
                            var to = new DateTime(DateTime.Now.Year + 1, 1, 1);

                            if (HistoryType == HistoryType.Trades)
                                await PrintList(Account.TradeHistory.GetRangeAsync(from, to, options));
                            else
                                await PrintList(Account.TriggerHistory.GetRangeAsync(from, to, options));
                            break;

                        case TradeHistoryFilters.PreviousYear:
                            from = new DateTime(DateTime.Now.Year - 1, 1, 1);
                            to = new DateTime(DateTime.Now.Year, 1, 1);

                            if (HistoryType == HistoryType.Trades)
                                await PrintList(Account.TradeHistory.GetRangeAsync(from, to, options));
                            else
                                await PrintList(Account.TradeHistory.GetRangeAsync(from, to, options));
                            break;
                    }
                }

                Status.WriteLine($"Done! Printed {count} history records.");
            }
            catch (Exception ex)
            {
                Status.Write("History retrieval failed: " + ex.Message);
            }

            Exit();
        }

        private void PrintList<T>(IEnumerable<T> reports)
        {
            foreach (var item in reports)
            {
                PrintRecord(item);
                count++;
            }
        }

        private async Task PrintList<T>(IAsyncEnumerator<T> e) where T : class
        {
            while (await e.Next())
            {
                PrintRecord(e.Current);
                count++;
            }
        }

        private void PrintRecord(object item)
        {
            _builder.Clear();

            switch (item)
            {
                case TradeReport tradeReport:
                    PrintTrade(tradeReport);
                    break;

                case TriggerReport triggerReport:
                    PrintTrigger(triggerReport);
                    break;
            }

            Print(_builder.ToString());
        }

        private void PrintTrade(TradeReport item)
        {
            _builder.Append(item.ReportId).Append(' ')
                    .Append(item.ReportTime).Append(' ')
                    .Append(item.ActionType).Append(' ');

            if (item.Type != TradeRecordTypes.Unknown && item.Type != TradeRecordTypes.Withdrawal)
                _builder.Append(item.Symbol).Append(' ');
        }

        private void PrintTrigger(TriggerReport item)
        {
            _builder.Append(item.ContingentOrderId).Append(' ')
                    .Append(item.TriggerType).Append(' ')
                    .Append($"{item.TransactionTime:yyyy/MM/dd HH:mm:ss.fff}").Append(' ')
                    .Append(item.Symbol).Append(' ')
                    .Append(item.TriggerState);
        }
    }

    public enum SyncOrAsync
    {
        Sync,
        Async
    }

    public enum HistoryType
    {
        Trades,
        Triggers,
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
