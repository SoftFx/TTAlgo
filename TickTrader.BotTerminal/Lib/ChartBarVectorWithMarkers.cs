using SciChart.Charting.Model.DataSeries;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Lib
{
    internal class ChartBarVectorWithMarkers : ChartBarVector
    {
        public ChartBarVectorWithMarkers(Feed.Types.Timeframe timeframe, bool autoFillMeatdata = false)
            : base(timeframe, autoFillMeatdata)
        {
        }

        public XyDataSeries<DateTime, double> MarkersData { get; } = new XyDataSeries<DateTime, double>();

        protected override void AddToInternalCollection(BarData bar)
        {
            base.AddToInternalCollection(bar);
            MarkersData.Append(bar.OpenTime.ToUtcDateTime(), bar.Open);
            MarkersData.Metadata.Add(null);
        }
    }
}
