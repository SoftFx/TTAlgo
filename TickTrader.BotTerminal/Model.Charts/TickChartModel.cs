﻿using SciChart.Charting.Model.DataSeries;
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
using Fdk = SoftFX.Extended;

namespace TickTrader.BotTerminal
{
    internal class TickChartModel : ChartModelBase
    {
        private XyDataSeries<DateTime, double> askData = new XyDataSeries<DateTime, double>();
        private XyDataSeries<DateTime, double> bidData = new XyDataSeries<DateTime, double>();
        private Fdk.Quote lastSeriesQuote;

        public TickChartModel(SymbolModel symbol, PluginCatalog catalog, FeedModel feed, TraderModel trade, BotJournal journal)
            : base(symbol, catalog, feed, trade, journal)
        {
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);
            Support(SelectableChartTypes.DigitalLine);
            Support(SelectableChartTypes.Scatter);

            Navigator = new RealTimeChartNavigator();

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

        protected override async Task LoadData(CancellationToken cToken)
        {
            lastSeriesQuote = null;

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

                if (tickArray.Length > 0)
                {
                    lastSeriesQuote = tickArray.Last();

                    var start = tickArray.First().CreatingTime;
                    var end = tickArray.Last().CreatingTime;
                    InitBoundaries(tickArray.Length, start, end);
                }
            }
        }

        protected override void ApplyUpdate(Fdk.Quote update)
        {
            if (lastSeriesQuote == null || update.CreatingTime > lastSeriesQuote.CreatingTime)
            {
                askData.Append(update.CreatingTime, update.Ask);
                bidData.Append(update.CreatingTime, update.Bid);
                ExtendBoundaries(askData.Count, update.CreatingTime);
            }
        }

        protected override PluginSetup CreateSetup(AlgoPluginRef catalogItem)
        {
            return new TickBasedPluginSetup(catalogItem, SymbolCode);
        }

        protected override IndicatorModel2 CreateIndicator(PluginSetup setup)
        {
            return new IndicatorModel2(setup, this);
        }

        protected override void InitPluign(PluginExecutor plugin)
        {
            var feedProvider = new QuoteBasedFeedProvider(Feed, () => null);
            plugin.InitQuoteStartegy(feedProvider);
            plugin.Metadata = feedProvider;
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
