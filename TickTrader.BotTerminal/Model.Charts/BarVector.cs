using Machinarium.Qnil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Math;
using FDK = SoftFX.Extended;

namespace TickTrader.BotTerminal
{
    public class BarVector : IDynamicListSource<BarEntity>
    {
        private DynamicList<BarEntity> bars = new DynamicList<BarEntity>();
        private BarSampler sampler;

        public BarVector()
        {
            From = DateTime.MinValue;
            To = DateTime.MaxValue;
        }

        public BarEntity this[int index] { get { return bars[index]; } }
        public int Count { get { return bars.Count; } }
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        public BarEntity LastBar { get { return bars[Count - 1]; } private set { bars[Count - 1] = value; } }
        public IReadOnlyList<BarEntity> Snapshot { get { return bars.Values; } }

        public event ListUpdateHandler<BarEntity> Updated { add { bars.Updated += value; } remove { bars.Updated += value; } }

        public void Update(QuoteEntity quote)
        {
            var barBoundaries = sampler.GetBar(quote.Time);
            var barOpenTime = barBoundaries.Open;

            if (IsInBoundaries(barOpenTime))
            {
                if (Count > 0)
                {
                    var lastBar = LastBar;

                    // validate agains last bar
                    if (barOpenTime < lastBar.OpenTime)
                        return;
                    else if (barOpenTime == lastBar.OpenTime)
                    {
                        LastBar = LastBar.CopyAndAppend(quote.Bid);
                        return;
                    }
                }

                bars.Add(new BarEntity(barOpenTime, barBoundaries.Close, quote));
            }
        }

        public void Update(IEnumerable<BarEntity> barCollection)
        {
            foreach (var bar in barCollection)
                Update(bar);
        }

        public void Update(BarEntity bar)
        {
            if (IsInBoundaries(bar.OpenTime))
            {
                if (Count == 0 || LastBar.OpenTime < bar.OpenTime)
                    bars.Add(bar);
            }
        }

        public void Clear()
        {
            this.bars.Clear();
        }

        public void ChangeTimeframe(TimeFrames timeFrame)
        {
            if (timeFrame == TimeFrames.Ticks)
                throw new Exception("BarVector does not work with 'Ticks' timeframe!");

            Clear();

            this.sampler = BarSampler.Get(timeFrame);
        }

        public void SetBoundaries(DateTime from, DateTime? to = null)
        {
            if (to != null && from >= to.Value)
                throw new Exception("Invalid date range.");

            this.From = from;
            this.To = to ?? DateTime.MaxValue;

            // trim start

            while (Count > 0)
            {
                if (!IsInBoundaries(bars[0].OpenTime))
                    bars.RemoveAt(0);
                else
                    break;
            }

            // trim end

            while (Count > 0)
            {
                if (!IsInBoundaries(bars[Count - 1].OpenTime))
                    bars.RemoveAt(Count - 1);
                else
                    break;
            }
        }

        private bool IsInBoundaries(DateTime barOpenTime)
        {
            return barOpenTime >= From && barOpenTime < To;
        }

        public void Dispose()
        {
        }
    }
}
