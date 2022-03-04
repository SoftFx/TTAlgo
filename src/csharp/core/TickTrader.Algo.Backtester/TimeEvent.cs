using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Backtester
{
    internal readonly struct TimeEvent : IComparable<TimeEvent>
    {
        public TimeEvent(DateTime time, bool isTrade, object content)
        {
            Time = time;
            IsTrade = isTrade;
            Content = content;
        }

        public DateTime Time { get; }
        public bool IsTrade { get; }
        public object Content { get; }

        public int CompareTo(TimeEvent other)
        {
            return Time.CompareTo(other.Time);
        }
    }

    internal interface ITimeEventSeries
    {
        DateTime NextOccurrance { get; }
        TimeEvent Take();
    }

    internal class TimeSeriesAggregator
    {
        private static readonly Comparer<DateTime> comparer = Comparer<DateTime>.Default;

        private List<ITimeEventSeries> _seriesList = new List<ITimeEventSeries>();

        public void Add(ITimeEventSeries series)
        {
            _seriesList.Add(series);
        }

        public TimeEvent Dequeue()
        {
            var nextSeries = GetMin(); //_seriesList.MinBy(s => s.NextOccurrance, comparer);
            return nextSeries.Take();
        }

        private ITimeEventSeries GetMin()
        {
            DateTime minTime = DateTime.MaxValue;
            ITimeEventSeries minSeries = null;

            for (int i = 0; i < _seriesList.Count; i++)
            {
                var series = _seriesList[i];
                var readerTime = series.NextOccurrance;

                if (minTime > readerTime)
                {
                    minSeries = series;
                    minTime = readerTime;
                }
            }

            return minSeries;
        }

        //public bool TryDequeue(out TimeEvent nextEvent)
        //{
        //    if (_seriesList.Count == 0)
        //    {
        //        nextEvent = default(TimeEvent);
        //        return false;
        //    }

        //    var nextSeries = _seriesList.MinBy(s => s.NextOccurrance);
        //    nextEvent = nextSeries.Take();
        //    CheckSeriesState(nextSeries);
        //    return true;
        //}

        //private void CheckSeriesState(ITimeEventSeries series)
        //{
        //    if (series.IsCompleted)
        //        _seriesList.Remove(series);

        //    if (_seriesList.All(s => !s.IsMandatory))
        //        _seriesList.Clear(); // the end
        //}
    }
}
