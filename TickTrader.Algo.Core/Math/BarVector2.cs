using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class BarVector2 : IReadOnlyList<BarEntity>, IDisposable
    {
        private CircularList<BarEntity> _barList = new CircularList<BarEntity>();
        private BarSampler _sampler;

        protected internal BarVector2(TimeFrames timeFrame)
        {
            TimeFrame = timeFrame;
            _sampler = BarSampler.Get(timeFrame);
        }

        public static BarVector2 Create(TimeFrames timeFrame)
        {
            return new BarVector2(timeFrame);
        }

        public static BarVector2 Create(BarVector2 master)
        {
            return new Slave(master, i =>
            {
                var masterBar = master[i];
                return new BarEntity(masterBar.OpenTime, masterBar.CloseTime, double.NaN, double.NaN);
            });
        }

        public static BarVector2 Create(BarVector2 master, Func<int, BarEntity> fillBarFunc)
        {
            return new Slave(master, fillBarFunc);
        }

        public int Count => _barList.Count;
        public TimeFrames TimeFrame { get; }
        public BarEntity this[int index] => _barList[index];

        protected event Action Extended;

        public void AppendBar(BarEntity bar)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);
            var currentBar = GetLastBar();

            if (currentBar != null && currentBar.OpenTime >= boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (boundaries.Open == bar.OpenTime || boundaries.Close != bar.CloseTime)
                throw new ArgumentException("Bar has invalid time boundaries!");

            Append(bar);
        }

        public BarEntity AppendQuote(DateTime time, double price, double volume)
        {
            var boundaries = _sampler.GetBar(time);
            var currentBar = GetLastBar();

            if (currentBar != null && currentBar.OpenTime > boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (currentBar != null && currentBar.OpenTime == boundaries.Open)
            {
                // append last bar
                currentBar.AppendNanProof(price, volume);
            }
            else
            {
                // add new bar
                var newBar = new BarEntity(boundaries.Open, boundaries.Close, price, volume);
                return Append(newBar);
            }
            return null;
        }

        public BarEntity AppendBarPart(DateTime time, double open, double high, double low, double close, double volume)
        {
            var boundaries = _sampler.GetBar(time);
            var currentBar = GetLastBar();

            if (currentBar != null && currentBar.OpenTime > boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (currentBar != null && currentBar.OpenTime == boundaries.Open)
            {
                // join
                currentBar = UpdateBar(currentBar, open, high, low, close, volume);
            }
            else
            {
                // append
                var entity = new BarEntity();
                entity.OpenTime = boundaries.Open;
                entity.CloseTime = boundaries.Close;
                entity.Open = open;
                entity.High = high;
                entity.Low = low;
                entity.Close = close;
                entity.Volume = volume;
                return Append(entity);
            }

            return null;
        }

        protected virtual BarEntity Append(BarEntity newBar)
        {
            _barList.Add(newBar);
            Extended?.Invoke();
            return newBar;
        }

        protected virtual BarEntity GetLastBar()
        {
            if (Count == 0)
                return null;
            else
                return _barList[Count - 1];
        }

        private static BarEntity UpdateBar(BarEntity bar, double open, double high, double low, double close, double volume)
        {
            var entity = new BarEntity();
            entity.OpenTime = bar.OpenTime;
            entity.CloseTime = bar.CloseTime;
            entity.Open = bar.Open;
            entity.High = System.Math.Max(bar.High, high);
            entity.Low = System.Math.Min(bar.Low, low);
            entity.Close = close;
            entity.Volume = bar.Volume + volume;
            return entity;
        }

        public IEnumerator<BarEntity> GetEnumerator()
        {
            return _barList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _barList.GetEnumerator();
        }

        public virtual void Dispose()
        {
        }

        private class Slave : BarVector2
        {
            private BarVector2 _master;
            private int _syncIndex = 0;
            private CircularList<BarEntity> _futureBars = new CircularList<BarEntity>();
            private Func<int, BarEntity> _emptyBarFunc;

            public Slave(BarVector2 master, Func<int, BarEntity> emptyBarFunc) : base(master.TimeFrame)
            {
                _master = master;
                _emptyBarFunc = emptyBarFunc;

                _master.Extended += _master_Extended;
            }

            protected override BarEntity Append(BarEntity newBar)
            {
                if (_master.Count == 0)
                {
                    // no sync possible, cache
                    _futureBars.Enqueue(newBar);
                    return newBar;
                }

                return SyncTo(newBar);
            }

            private BarEntity SyncTo(BarEntity newBar)
            {
                while (_syncIndex < _master.Count)
                {
                    var masterBar = _master[_syncIndex];

                    if (masterBar.OpenTime == newBar.OpenTime)
                    {
                        // hit
                        _barList.Add(newBar);
                        _syncIndex++;
                        return newBar;
                    }
                    else if (newBar.OpenTime < masterBar.OpenTime)
                    {
                        // too old, skip
                        return null;
                    }
                    else
                    {
                        var fillBar = _emptyBarFunc(_syncIndex);
                        _barList.Add(fillBar);
                        _syncIndex++;
                    }
                }

                // no corresponding bar yet
                _futureBars.Enqueue(newBar);
                return newBar;
            }

            private void _master_Extended()
            {
                var masterCurrentBar = _master.GetLastBar();

                while (_futureBars.Count > 0)
                {
                    var cachedBar = _futureBars[0];

                    if (cachedBar.OpenTime < masterCurrentBar.OpenTime)
                    {
                        // skip
                        _futureBars.Dequeue();
                    }
                    else if (cachedBar.OpenTime == masterCurrentBar.OpenTime)
                    {
                        // take
                        var newBar = _futureBars.Dequeue();
                        SyncTo(newBar);
                        return;
                    }
                    else
                        return;
                }
            }

            protected override BarEntity GetLastBar()
            {
                if (_futureBars.Count > 0)
                    return _futureBars[_futureBars.Count - 1];

                return base.GetLastBar();
            }

            public override void Dispose()
            {
                _master.Extended -= _master_Extended;

                base.Dispose();
            }
        }
    }
}
