using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class BarSequenceBuilder : ITimeSequenceRef
    {
        private readonly BarSampler _sampler;
        private BarData _currentBar;
        private static int IdSeed;
        private readonly int _id;

        private BarSequenceBuilder(Feed.Types.Timeframe timeframe)
        {
            _id = System.Threading.Interlocked.Increment(ref IdSeed);

            TimeFrame = timeframe;
            _sampler = BarSampler.Get(timeframe);
        }

        public static BarSequenceBuilder Create(Feed.Types.Timeframe timeframe)
        {
            return new BarSequenceBuilder(timeframe);
        }

        public static BarSequenceBuilder Create(ITimeSequenceRef master)
        {
            return new Slave(master);
        }

        public event Action<BarData> BarOpened;
        public event Action<BarData> BarClosed;
        public event Action<BarData> BarUpdated;
        public BarSampler Sampler => _sampler;
        public Feed.Types.Timeframe TimeFrame { get; }
        public long TimeEdge => _currentBar?.OpenTime ?? 0;

        public void AppendBar(BarData bar)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);

            if (_currentBar != null && _currentBar.OpenTime >= boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (boundaries.Open == bar.OpenTime || boundaries.Close != bar.CloseTime)
                throw new ArgumentException("Bar has invalid time boundaries!");

            Append(bar);
        }

        public BarData AppendQuote(Timestamp time, double price, double volume)
        {
            //if (_currentBar != null && _currentBar.OpenTime > time)
            //    throw new ArgumentException("Invalid time sequnce!");

            var timeMs = TimeMs.FromTimestamp(time);
            if (_currentBar != null)
            {
                if (timeMs < _currentBar.OpenTime)
                    throw new ArgumentException("Invalid time sequnce!");

                if (timeMs < _currentBar.CloseTime)
                {
                    // append last bar
                    _currentBar.Append(price, volume);
                    BarUpdated?.Invoke(_currentBar);
                    return null; ;
                }
            }

            // add new bar
            var boundaries = _sampler.GetBar(timeMs);
            var newBar = new BarData(boundaries.Open, boundaries.Close, price, volume);
            return Append(newBar);
        }

        public BarData AppendBarPart(BarData bar)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);

            if (_currentBar != null && _currentBar.OpenTime > boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (_currentBar != null && _currentBar.OpenTime == boundaries.Open)
            {
                // join
                _currentBar.AppendPart(bar);
                BarUpdated?.Invoke(_currentBar);
            }
            else
            {
                // append
                var entity = new BarData(bar)
                {
                    OpenTime = boundaries.Open,
                    CloseTime = boundaries.Close,
                };
                return Append(entity);
            }

            return null;
        }

        public BarData CloseSequence()
        {
            BarData closedBar = null;
            if (_currentBar != null)
            {
                BarClosed?.Invoke(_currentBar);
                closedBar = _currentBar;
                _currentBar = null;
                
            }
            return closedBar;
        }

        protected virtual BarData Append(BarData newBar)
        {
            BarData closedBar = null;
            if (_currentBar != null)
            {
                BarClosed?.Invoke(_currentBar);
                closedBar = _currentBar;
            }
            _currentBar = newBar;
            OnBarOpened(newBar);
            return closedBar;
        }

        protected void OnBarOpened(BarData newBar)
        {
            BarOpened?.Invoke(newBar);
        }

        private class Slave : BarSequenceBuilder
        {
            private ITimeSequenceRef _master;

            public Slave(ITimeSequenceRef masterSequence)
                : base(masterSequence.TimeFrame)
            {
                _master = masterSequence;
                //_master.BarOpened += MasterSequence_Appended;
                _master.BarOpened += _master_BarOpened;
                _master.BarClosed += _master_BarClosed;
            }

            private void _master_BarClosed(BarData closedBar)
            {
                var masterBarTime = closedBar.OpenTime;

                if (_currentBar == null || _currentBar.OpenTime > masterBarTime)
                {
                    // fill with Nan
                    var fillBoundaries = _sampler.GetBar(masterBarTime);
                    var fillBar = new BarData(fillBoundaries.Open, fillBoundaries.Close, double.NaN, 0);
                    base.OnBarOpened(fillBar);
                }
                else if (_currentBar.OpenTime < masterBarTime)
                {
                    // fill with previos
                    var fillBoundaries = _sampler.GetBar(masterBarTime);
                    var fillBar = new BarData(fillBoundaries.Open, fillBoundaries.Close, _currentBar.Close, 0);
                    base.OnBarOpened(fillBar);
                }
            }

            private void _master_BarOpened(BarData openedBar)
            {
                if (_currentBar != null && _currentBar.OpenTime == openedBar.OpenTime)
                    base.Append(_currentBar); // accept current bar
            }

            private bool IsSync(BarData someBar) => _master.TimeEdge == someBar.OpenTime;

            protected override BarData Append(BarData newBar)
            {
                var masterTime = _master.TimeEdge;

                if (IsSync(newBar))
                    return base.Append(newBar);
                else
                {
                    _currentBar = newBar;
                    return null;
                }
            }

            //private void MasterSequence_Appended()
            //{
            //    var masterTime = _master.TimeEdge.Value;

            //    if (_currentBar != null)
            //    {
            //        if (_currentBar.OpenTime > masterTime)
            //            throw new Exception("Synchronization with master sequence is broken!");
            //        else if (_currentBar.OpenTime == masterTime)
            //        {
            //            // accept current bar as synchronized
            //            base.Append(_currentBar);
            //            return;
            //        }
            //    }

            //    // close current bar & create empty
            //    var newBoundaries = _sampler.GetBar(masterTime);
            //    var newBar = new BarData(newBoundaries.Open, newBoundaries.Close, _currentBar?.Close ?? double.NaN, 0);
            //    base.Append(newBar);
            //}
        }

        public override string ToString()
        {
            return $"Builder {_id}";
        }
    }

    public interface ITimeSequenceRef
    {
        Feed.Types.Timeframe TimeFrame { get; }
        long TimeEdge { get; }

        event Action<BarData> BarOpened;
        event Action<BarData> BarClosed;
    }
}
