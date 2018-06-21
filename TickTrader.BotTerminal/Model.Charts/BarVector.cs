using Machinarium.Qnil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class BarVector : IVarList<BarEntity>
    {
        private readonly VarList<BarEntity> bars = new VarList<BarEntity>();
        private BarSampler sampler;

        public BarVector()
        {
        }

        public BarEntity this[int index] { get { return bars[index]; } }
        public int Count { get { return bars.Count; } }
        public BarEntity LastBar { get { return bars[Count - 1]; } private set { bars[Count - 1] = value; } }
        public IReadOnlyList<BarEntity> Snapshot { get { return bars.Values; } }

        public event ListUpdateHandler<BarEntity> Updated { add { bars.Updated += value; } remove { bars.Updated += value; } }

        public void Update(DateTime time, double price, double volume)
        {
            var barBoundaries = sampler.GetBar(time);
            var barOpenTime = barBoundaries.Open;

            if (Count > 0)
            {
                var lastBar = LastBar;

                // validate agains last bar
                if (barOpenTime < lastBar.OpenTime)
                    return;
                else if (barOpenTime == lastBar.OpenTime)
                {
                    LastBar = LastBar.CopyAndAppend(price, volume);
                    return;
                }
            }

            bars.Add(new BarEntity(barOpenTime, barBoundaries.Close, price, volume));
        }

        public void Append(IEnumerable<BarEntity> barCollection)
        {
            foreach (var bar in barCollection)
                Append(bar);
        }

        public void Append(BarEntity bar)
        {
            if (Count == 0 || LastBar.OpenTime < bar.OpenTime)
                bars.Add(bar);
            else
                throw new Exception("Bars should be sorted by time!");
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

        public void Dispose()
        {
            bars.Dispose();
        }
    }
}
