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
using SciChart.Charting.Model.ChartSeries;

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

            SelectedChartType = SelectableChartTypes.Scatter;
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
            var metrics = new DataMetrics();

            if (Model.LastQuote != null)
            {
                DateTime timeMargin = Model.LastQuote.CreatingTime;

                var tickArray = await Feed.History.GetTicks(SymbolCode, timeMargin - TimeSpan.FromMinutes(15), timeMargin, 0);

                //foreach (var tick in tickArray)
                //{
                //    askData.Append(tick.CreatingTime, tick.Ask);
                //    bidData.Append(tick.CreatingTime, tick.Bid);
                //}

                askData.Append(
                    tickArray.Select(t => t.CreatingTime),
                    tickArray.Select(t => t.Ask));
                bidData.Append(
                    tickArray.Select(t => t.CreatingTime),
                    tickArray.Select(t => t.Bid));
                
                metrics.Count = tickArray.Length;
                if (tickArray.Length > 0)
                {
                    metrics.StartDate = tickArray.First().CreatingTime;
                    metrics.EndDate = tickArray.Last().CreatingTime;
                }
                return metrics;
            }

            return metrics;
        }

        protected override PluginSetup CreateSetup(AlgoPluginRef catalogItem)
        {
            return new TickBasedPluginSetup(catalogItem, SymbolCode);
        }

        protected override IndicatorModel2 CreateIndicator(PluginSetup setup)
        {
            return new IndicatorModel2(setup, this);
        }

        protected override FeedStrategy GetFeedStrategy()
        {
            return new QuoteStrategy();
        }

        protected override void UpdateSeries()
        {
            var askSeriesModel = CreateSeriesModel(askData, "_Ask");
            var bidSeriesModel = CreateSeriesModel(bidData, "_Bid");

            if (SeriesCollection.Count == 0)
            {
                SeriesCollection.Add(askSeriesModel);
                SeriesCollection.Add(bidSeriesModel);
            }
            else
            {
                SeriesCollection[0] = askSeriesModel;
                SeriesCollection[1] = bidSeriesModel;
            }
        }

        private IRenderableSeriesViewModel CreateSeriesModel(IXyDataSeries data, string seriesType)
        {
            switch (SelectedChartType)
            {
                case SelectableChartTypes.Line:
                    return new LineRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_LineStyle" + seriesType };
                case SelectableChartTypes.Mountain:
                    return new MountainRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_MountainStyle" + seriesType };
                case SelectableChartTypes.DigitalLine:
                    return new LineRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_DigitalLineStyle" + seriesType, IsDigitalLine = true };
                case SelectableChartTypes.Scatter:
                    return new XyScatterRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_ScatterStyle" + seriesType };
            }

            throw new InvalidOperationException("Unsupported chart type: " + SelectedChartType);
        }
    }
}
