using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Custom Feed Query Bot", Version = "1.1", Category = "Test Plugin Info",
        Description = "Queries and prints series of quotes/bars for specified time period, periodicity and side.")]
    public class FeedQueryBot : TradeBot
    {
        [Parameter(DisplayName = "Symbol")]
        public string QuerySymbol { get; set; }

        [Parameter(DefaultValue = FeedQueryPeriods.Today)]
        public FeedQueryPeriods Period { get; set; }

        [Parameter(DefaultValue = TimeFrames.H1)]
        public new TimeFrames TimeFrame { get; set; }

        [Parameter(DefaultValue = QueryResultOrders.Backward)]
        public QueryResultOrders Direction { get; set; }

        [Parameter(DefaultValue = false, DisplayName = "Use count based request")]
        public bool UseCount { get; set; }

        [Parameter(DefaultValue = 100)]
        public int Count { get; set; }

        private bool IsBackwarOrder => Direction == QueryResultOrders.Backward;
        private string SafeSymbol => string.IsNullOrEmpty(QuerySymbol) ? Symbol.Name : QuerySymbol;

        protected override void OnStart()
        {
            if (Symbols.Select(v => v.Name).Contains(SafeSymbol))
                try
                {
                    //if (Period == FeedQueryPeriods.Input)
                    //{
                    //    if (TimeFrame != TimeFrames.Ticks)
                    //        PrintBarSeries();
                    //}
                    //else
                    //{
                    DateTime from, to;
                    GetBounds(out from, out to);
                    if (TimeFrame == TimeFrames.Ticks || TimeFrame == TimeFrames.TicksLevel2)
                        PrintQuotes(from, to, TimeFrame == TimeFrames.TicksLevel2);
                    else
                        PrintBars(from, to);
                    //}
                }
                catch (AggregateException aex)
                {
                    aex = aex.Flatten();

                    if (aex.InnerExceptions.Count == 1 && aex.InnerExceptions[0] is NotSupportedException)
                    {
                        var nsex = (NotSupportedException)aex.InnerExceptions[0];
                        Status.WriteLine(nsex.Message);
                    }
                    else
                    {
                        Status.WriteLine("Exception: " + aex);
                    }
                }
                catch (Exception ex)
                {
                    Status.WriteLine("Exception: " + ex);
                }
            else
                Status.WriteLine($"Symbols collection doesn't contain symbol {SafeSymbol}.");
            Exit();
        }

        private void PrintBars(DateTime from, DateTime to)
        {
            int count = 0;
            IEnumerable<Bar> bars;
            if (UseCount)
                bars = Feed.GetBars(SafeSymbol, TimeFrame, from, IsBackwarOrder ? -Count : Count, BarPriceType.Bid);
            else bars = Feed.GetBars(SafeSymbol, TimeFrame, from, to, BarPriceType.Bid, IsBackwarOrder);
            foreach (var bar in bars)
            {
                if (IsStopped)
                    break;
                PrintBar(bar);
                count++;
            }
            Status.WriteLine("Done. Printed {0} bars.", count);
        }

        private void PrintBarSeries()
        {
            int count = 0;
            var bars = Feed.GetBarSeries(SafeSymbol);
            if (IsBackwarOrder)
            {
                foreach (var bar in bars)
                {
                    if (IsStopped)
                        break;
                    PrintBar(bar);
                    count++;
                }
            }
            else
            {
                for (int i = bars.Count - 1; i >= 0; i--)
                {
                    if (IsStopped)
                        break;
                    PrintBar(bars[i]);
                    count++;
                }
            }
            Status.WriteLine("Done. Printed {0} bars.", count);
        }

        private void PrintBar(Bar bar)
        {
            Print("{0} o:{1} h:{2} l:{3} c:{4}", bar.OpenTime.ToLocalTime(), bar.Open, bar.High, bar.Low, bar.Close);
        }

        private void PrintQuotes(DateTime from, DateTime to, bool level2)
        {
            int count = 0;
            IEnumerable<Quote> quotes;
            if (UseCount)
                quotes = Feed.GetQuotes(SafeSymbol, from, IsBackwarOrder ? -Count : Count, level2);
            else quotes = Feed.GetQuotes(SafeSymbol, from, to, level2, IsBackwarOrder);
            foreach (var quote in quotes)
            {
                if (IsStopped)
                    break;

                Print("{0} b:{1} a:{2}", quote.Time.ToLocalTime(), quote.Bid, quote.Ask);
                count++;
            }
            Status.WriteLine("Done. Printed {0} quotes.", count);
        }

        private void GetBounds(out DateTime from, out DateTime to)
        {
            DateTime now = DateTime.UtcNow;

            switch (Period)
            {
                case FeedQueryPeriods.Today: from = DateTime.Today; to = DateTime.Today + TimeSpan.FromDays(1); return;
                case FeedQueryPeriods.Yesterday: from = DateTime.Today - TimeSpan.FromDays(1); to = DateTime.Today; return;
                case FeedQueryPeriods.ThisHour: from = GetHour(now, 0); to = GetHour(now, 1); return;
                case FeedQueryPeriods.PreviousHour: from = GetHour(now, -1); to = GetHour(now, 0); return;
                case FeedQueryPeriods.ThisMonth: from = GetMonth(now, 0); to = GetMonth(now, 1); return;
                case FeedQueryPeriods.PreviousMonth: from = GetMonth(now, -1); to = GetMonth(now, 0); return;
            }
            throw new NotImplementedException();
        }

        private DateTime GetHour(DateTime time, int shift)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, time.Kind).AddHours(shift);
        }

        private DateTime GetMonth(DateTime time, int shift)
        {
            return new DateTime(time.Year, time.Month, 1, 0, 0, 0, time.Kind).AddMonths(shift);
        }
    }

    public enum QueryResultOrders
    {
        Forward,
        Backward
    }

    public enum FeedQueryPeriods
    {
        //Input,
        ThisHour,
        PreviousHour,
        Today,
        Yesterday,
        ThisMonth,
        PreviousMonth
    }
}
