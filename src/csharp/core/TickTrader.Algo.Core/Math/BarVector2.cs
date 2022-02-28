using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public abstract class BarVectorBase2 : TimeVectorBase<BarData>
    {
        private Feed.Types.Timeframe _timeFrame;
        private BarSampler _sampler;

        protected BarVectorBase2(Feed.Types.Timeframe timeFrame)
        {
            TimeFrame = timeFrame;
            _sampler = BarSampler.Get(timeFrame);
        }

        public void AppendBar(BarData bar)
        {
            AppendBarIntenral(bar, false);
        }

        public bool TryAppendBar(BarData bar)
        {
            return AppendBarIntenral(bar, true);
        }

        private bool AppendBarIntenral(BarData bar, bool noThrow)
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

        public BarData AppendQuote(Timestamp time, double price, double volume)
        {
            return AppendQuoteInternal(false, time, price, volume);
        }

        public BarData TryAppendQuote(Timestamp time, double price, double volume)
        {
            return AppendQuoteInternal(true, time, price, volume);
        }

        private BarData AppendQuoteInternal(bool noThrow, Timestamp time, double price, double volume)
        {
            var boundaries = _sampler.GetBar(TimeMs.FromTimestamp(time));
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
                currentBar.Append(price, volume);
                OnBarUpdated(currentBarIndex, currentBar);
            }
            else
            {
                // add new bar
                var newBar = new BarData(boundaries.Open, boundaries.Close, price, volume);
                return Append(newBar);
            }
            return null;
        }

        public BarData AppendBarPart(BarData bar)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);
            int currentBarIndex;
            var currentBar = GetLastItem(out currentBarIndex);

            if (currentBar != null && currentBar.OpenTime > boundaries.Open)
                throw new ArgumentException("Invalid time sequnce!");

            if (currentBar != null && currentBar.OpenTime == boundaries.Open)
            {
                // join
                currentBar.AppendPart(bar);
                OnBarUpdated(currentBarIndex, currentBar);
            }
            else
            {
                // append
                var entity = new BarData(bar)
                {
                    OpenTime = boundaries.Open,
                    CloseTime = boundaries.Close
                };
                return Append(entity);
            }

            return null;
        }

        public void AppendRange(IEnumerable<BarData> barRange)
        {
            foreach (var bar in barRange)
                AppendBar(bar);
        }

        protected virtual void OnBarUpdated(int barIndex, BarData bar) { }

        public Feed.Types.Timeframe TimeFrame
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

        protected override long GetItemTimeCoordinate(BarData item) => item.OpenTime;
    }

    public sealed class BarVector2 : BarVectorBase2
    {
        private CircularList<BarData> _barList = new CircularList<BarData>();

        private BarVector2(Feed.Types.Timeframe timeFrame) : base(timeFrame)
        {
        }

        public static BarVector2 Create(Feed.Types.Timeframe timeFrame)
        {
            return new BarVector2(timeFrame);
        }

        public static BarVector2 Create(BarVector2 master)
        {
            var newVector = new BarVector2(master.TimeFrame);
            newVector.InitSynchronization(master.Ref, i =>
            {
                var masterBar = master[i];
                return new BarData(masterBar.OpenTime, masterBar.CloseTime, 0, 0);
            });
            return newVector;
        }

        #region TimeVectorBase implementation

        public override int Count => _barList.Count;
        public override BarData this[int index] => _barList[index];

        protected override void AddToInternalCollection(BarData item) => _barList.Add(item);
        public override IEnumerator<BarData> GetEnumerator() => _barList.GetEnumerator();
        protected override void ClearInternalCollection() => _barList.Clear();

        #endregion
    }
}
