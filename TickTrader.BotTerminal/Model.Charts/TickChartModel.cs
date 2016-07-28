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
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class TickChartModel : ChartModelBase
    {
        private XyDataSeries<DateTime, double> askData = new XyDataSeries<DateTime, double>();
        private XyDataSeries<DateTime, double> bidData = new XyDataSeries<DateTime, double>();

        public TickChartModel(SymbolModel symbol, PluginCatalog catalog, FeedModel feed, BotJournal journal)
            : base(symbol, catalog, feed, journal)
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

                var tickArray = await Feed.History.GetTicks(SymbolCode, timeMargin - TimeSpan.FromMinutes(15), timeMargin, 0);

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

        protected override PluginSetup CreateSetup(AlgoPluginRef catalogItem)
        {
            throw new NotImplementedException();
        }

        protected override IndicatorBuilder CreateBuilder(PluginSetup setup)
        {
            throw new NotImplementedException();
        }

        protected override IndicatorModel2 CreateIndicator(PluginSetup setup)
        {
            throw new NotImplementedException();
        }

        protected override FeedStrategy GetFeedStrategy()
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
