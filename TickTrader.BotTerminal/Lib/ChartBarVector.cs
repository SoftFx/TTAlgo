using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Lib
{
    internal class ChartBarVector : BarVectorBase2
    {
        private readonly List<BarData> _cache = new List<BarData>();
        private bool _autoFillMetadata;

        public ChartBarVector(Feed.Types.Timeframe timeframe, bool autoFillMeatdata = false) : base(timeframe)
        {
            _autoFillMetadata = autoFillMeatdata;
        }

        public override BarData this[int index] => _cache[index];
        public OhlcDataSeries<DateTime, double> SciChartdata { get; } = new OhlcDataSeries<DateTime, double>();

        public override int Count => _cache.Count;

        public override IEnumerator<BarData> GetEnumerator() => _cache.GetEnumerator();

        protected override void AddToInternalCollection(BarData bar)
        {
            _cache.Add(bar);
            SciChartdata.Append(bar.OpenTime.ToDateTime(), bar.Open, bar.High, bar.Low, bar.Close);
            if (_autoFillMetadata)
                SciChartdata.Metadata.Add(null);
        }

        protected override void OnBarUpdated(int barIndex, BarData bar)
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
