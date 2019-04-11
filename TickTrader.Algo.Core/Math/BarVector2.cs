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
    public abstract class BarVectorBase2 : TimeVectorBase<BarEntity>
    {
        private TimeFrames _timeFrame;
        private BarSampler _sampler;

        protected BarVectorBase2(TimeFrames timeFrame)
        {
            TimeFrame = timeFrame;
            _sampler = BarSampler.Get(timeFrame);
        }

        public void AppendBar(BarEntity bar)
        {
            AppendBarIntenral(bar, false);
        }

        public bool TryAppendBar(BarEntity bar)
        {
            return AppendBarIntenral(bar, true);
        }

        private bool AppendBarIntenral(BarEntity bar, bool noThrow)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);
            var currentBar = GetLastItem();

            if (currentBar != null && currentBar.OpenTime >= boundaries.Open)
            {
                if (noThrow)
                    return false;
                else
                    throw new ArgumentException("Invalid time sequnce!");
            }

            if (boundaries.Open != bar.OpenTime || boundaries.Close != bar.CloseTime)
            {
                if (noThrow)
                    return false;
                else
                    throw new ArgumentException("Bar has invalid time boundaries!");
            }

            return Append(bar) != null;
        }

        public BarEntity AppendQuote(DateTime time, double price, double volume)
        {
            return AppendQuoteInternal(false, time, price, volume);
        }

        public BarEntity TryAppendQuote(DateTime time, double price, double volume)
        {
            return AppendQuoteInternal(true, time, price, volume);
        }

        private BarEntity AppendQuoteInternal(bool noThrow, DateTime time, double price, double volume)
        {
            var boundaries = _sampler.GetBar(time);
            int currentBarIndex;
            var currentBar = GetLastItem(out currentBarIndex);

            if (currentBar != null && currentBar.OpenTime > boundaries.Open)
            {
                if (noThrow)
                    return null;
                else
                    throw new ArgumentException("Invalid time sequnce!");
            }

            if (currentBar != null && currentBar.OpenTime == boundaries.Open)
            {
                // append last bar
                currentBar.AppendNanProof(price, volume);
                OnBarUpdated(currentBarIndex, currentBar);
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
            int currentBarIndex;
            var currentBar = GetLastItem(out currentBarIndex);

            if (currentBar != null && currentBar.OpenTime > boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (currentBar != null && currentBar.OpenTime == boundaries.Open)
            {
                // join
                currentBar.AppendPart(open, high, low, close, volume);
                OnBarUpdated(currentBarIndex, currentBar);
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

        public void AppendRange(IEnumerable<BarEntity> barRange)
        {
            foreach (var bar in barRange)
                AppendBar(bar);
        }

        protected virtual void OnBarUpdated(int barIndex, BarEntity bar) { }

        public TimeFrames TimeFrame
        {
            get => _timeFrame;
            set
            {
                if (_timeFrame != value)
                {
                    if (!IsEmpty)
                        throw new InvalidOperationException("Vector is not empty! Cannot change TimeFrame!");

                    _sampler = BarSampler.Get(value);
                    _timeFrame = value;
                }
            }
        }

        protected override DateTime GetItemTimeCoordinate(BarEntity item) => item.OpenTime;
    }

    public sealed class BarVector2 : BarVectorBase2
    {
        private CircularList<BarEntity> _barList = new CircularList<BarEntity>();

        private BarVector2(TimeFrames timeFrame) : base(timeFrame)
        {
        }

        public static BarVector2 Create(TimeFrames timeFrame)
        {
            return new BarVector2(timeFrame);
        }

        public static BarVector2 Create(BarVector2 master)
        {
            var newVector = new BarVector2(master.TimeFrame);
            newVector.InitSynchronization(master.Ref, i =>
            {
                var masterBar = master[i];
                return new BarEntity(masterBar.OpenTime, masterBar.CloseTime, double.NaN, double.NaN);
            });
            return newVector;
        }

        #region TimeVectorBase implementation

        public override int Count => _barList.Count;
        public override BarEntity this[int index] => _barList[index];

        protected override void AddToInternalCollection(BarEntity item) => _barList.Add(item);
        public override IEnumerator<BarEntity> GetEnumerator() => _barList.GetEnumerator();
        protected override void ClearInternalCollection() => _barList.Clear();

        #endregion
    }
}
