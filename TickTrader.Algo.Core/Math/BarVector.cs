using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Math
{
    public class BarVector
    {
        private readonly List<BarEntity> _list = new List<BarEntity>();
        private readonly BarSampler _sampler;

        public BarVector(TimeFrames timeFrame)
        {
            _sampler = BarSampler.Get(timeFrame);
        }

        public BarSampler Sampler => _sampler;

        public BarEntity Last
        {
            get { return _list[_list.Count - 1]; }
            set { _list[_list.Count - 1] = value; }
        }

        public BarEntity First
        {
            get { return _list[0]; }
            set { _list[0] = value; }
        }

        public int Count => _list.Count;

        public BarEntity[] ToArray()
        {
            return _list.ToArray();
        }

        public BarEntity[] RemoveFromStart(int count)
        {
            if (Count < count)
                throw new InvalidOperationException("Not enough bars in vector!");

            var range = new BarEntity[count];
            for (int i = 0; i < count; i++)
                range[i] = _list[i];
            _list.RemoveRange(0, count);
            return range;
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void Append(DateTime time, double open, double high, double low, double close, double volume)
        {
            var boundaries = _sampler.GetBar(time);

            if (Count > 0 && Last.OpenTime == boundaries.Open)
            {
                // join
                Last = UpdateBar(Last, open, high, low, close, volume);
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
                InternalAdd(entity);
            }
        }

        private BarEntity UpdateBar(BarEntity bar, double open, double high, double low, double close, double volume)
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

        public void Append(BarEntity bar)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);

            if (boundaries.Open == bar.OpenTime || boundaries.Close != bar.CloseTime)
                throw new ArgumentException("Bar has invalid time boundaries!");

            InternalAdd(bar);
        }

        private void InternalAdd(BarEntity bar)
        {
            if (Count > 0 && Last.OpenTime >= bar.OpenTime)
                throw new ArgumentException("Invlid time sequnce!");

            _list.Add(bar);
        }
    }
}
