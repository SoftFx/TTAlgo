using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public abstract class BarVectorBase
    {
        private readonly BarSampler _sampler;

        public BarVectorBase(TimeFrames timeframe)
        {
            _sampler = BarSampler.Get(timeframe);
        }

        public BarSampler Sampler => _sampler;

        public abstract bool HasElements { get; }
        public abstract BarEntity Last { get; set; }

        protected abstract void AddToVector(BarEntity entity);

        public void AppendBar(BarEntity bar)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);

            CheckTime(boundaries.Open);

            if (boundaries.Open == bar.OpenTime || boundaries.Close != bar.CloseTime)
                throw new ArgumentException("Bar has invalid time boundaries!");

            AddToVector(bar);
        }

        public void AppendQuote(DateTime time, double price, double volume)
        {
            var boundaries = _sampler.GetBar(time);
            var last = Last;

            CheckTime(boundaries.Open);

            if (HasElements && last.OpenTime == boundaries.Open)
            {
                // append last bar
                last.Append(price, volume);
            }
            else
            {
                // add new bar
                var newBar = new BarEntity(boundaries.Open, boundaries.Close, price, volume);
                AddToVector(newBar);
            }
        }

        public void AppendBarPart(DateTime time, double open, double high, double low, double close, double volume)
        {
            var boundaries = _sampler.GetBar(time);

            CheckTime(boundaries.Open);

            if (HasElements && Last.OpenTime == boundaries.Open)
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
                AddToVector(entity);
            }
        }

        private void CheckTime(DateTime barOpen)
        {
            if (HasElements && Last.OpenTime >= barOpen)
                throw new ArgumentException("Invlid time sequnce!");
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
    }
}
