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
    internal class ChartBarVectorWithMarkers : ChartBarVector
    {
        public ChartBarVectorWithMarkers(TimeFrames timeframe, bool autoFillMeatdata = false)
            : base(timeframe, autoFillMeatdata)
        {
        }

        public XyDataSeries<DateTime, double> MarkersData { get; } = new XyDataSeries<DateTime, double>();

        protected override void AddToInternalCollection(BarEntity bar)
        {
            base.AddToInternalCollection(bar);
            MarkersData.Append(bar.OpenTime, bar.Open);
            MarkersData.Metadata.Add(null);
        }
    }
}
