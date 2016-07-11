using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class TickChartModel : ChartModelBase
    {
        private XyDataSeries<DateTime, double> askData = new XyDataSeries<DateTime, double>();
        private XyDataSeries<DateTime, double> bidData = new XyDataSeries<DateTime, double>();

        public TickChartModel(SymbolModel symbol, PluginCatalog catalog, FeedModel feed)
            : base(symbol, catalog, feed)
        {
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);
            Support(SelectableChartTypes.DigitalLine);
            Support(SelectableChartTypes.Scatter);

            AddSeries(askData);
            AddSeries(bidData);

            SelectedChartType = SelectableChartTypes.Line;
        }

        public override TimeFrames TimeFrame { get { return TimeFrames.Ticks; } }

        public new void Activate()
        {
            base.Activate();
        }

        protected override void ClearData()
        {
            askData.Clear();
            bidData.Clear();
        }

        protected override async Task<DataMetrics> LoadData(CancellationToken cToken)
        {
            if (Model.LastQuote != null)
            {
                DateTime timeMargin = Model.LastQuote.CreatingTime;

                var tickArray = await Feed.History.GetTicks(Symbol, timeMargin - TimeSpan.FromMinutes(15), timeMargin, 0);

                foreach (var tick in tickArray)
                {
                    askData.Append(tick.CreatingTime, tick.Ask);
                    bidData.Append(tick.CreatingTime, tick.Bid);
                }

                askData.Append(
                    tickArray.Select(t => t.CreatingTime),
                    tickArray.Select(t => t.Ask));
                bidData.Append(
                    tickArray.Select(t => t.CreatingTime),
                    tickArray.Select(t => t.Bid));
            }

            return new DataMetrics();
        }

        protected override IIndicatorSetup CreateInidactorConfig(AlgoPluginRef repItem)
        {
            throw new NotImplementedException();
        }

        private IPointMarker CreateMarker(Color fillColor, Color strokeColor)
        {
            EllipsePointMarker marker = new EllipsePointMarker();
            marker.Width = 6;
            marker.Height = 6;
            marker.Fill = fillColor;
            marker.Stroke = strokeColor;
            return marker;
        }
    }
}
