using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            MarkersData.Append(TimeMs.ToUtc(bar.OpenTime), bar.Open);
            MarkersData.Metadata.Add(null);
        }
    }
}
