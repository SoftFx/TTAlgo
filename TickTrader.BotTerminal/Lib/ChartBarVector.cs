using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal.Lib
{
    internal class ChartBarVector : BarVectorBase2
    {
        private readonly List<BarEntity> _cache = new List<BarEntity>();

        public ChartBarVector(TimeFrames timeframe) : base(timeframe)
        {
        }

        public override BarEntity this[int index] => _cache[index];
        public OhlcDataSeries<DateTime, double> SciChartdata { get; } = new OhlcDataSeries<DateTime, double>();

        public override int Count => _cache.Count;

        public override IEnumerator<BarEntity> GetEnumerator() => _cache.GetEnumerator();

        protected override void AddToInternalCollection(BarEntity bar)
        {
            _cache.Add(bar);
            SciChartdata.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);
        }

        protected override void OnBarUpdated(int barIndex, BarEntity bar)
        {
            SciChartdata.Update(barIndex, bar.Open, bar.High, bar.Low, bar.Close);
        }

        protected override void ClearInternalCollection()
        {
            SciChartdata.Clear();
            _cache.Clear();
        }
    }
}
