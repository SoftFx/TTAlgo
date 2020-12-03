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
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.ChartSeries;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Domain;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.BotTerminal
{
    internal class TickChartModel : ChartModelBase
    {
        private XyDataSeries<DateTime, double> askData = new XyDataSeries<DateTime, double>();
        private XyDataSeries<DateTime, double> bidData = new XyDataSeries<DateTime, double>();
        private QuoteInfo lastSeriesQuote;

        public TickChartModel(SymbolInfo symbol, AlgoEnvironment algoEnv)
            : base(symbol, algoEnv)
        {
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);
            Support(SelectableChartTypes.DigitalLine);
            Support(SelectableChartTypes.Scatter);

            TimeFrame = Feed.Types.Timeframe.Ticks;

            Navigator = new RealTimeChartNavigator();
            SelectedChartType = SelectableChartTypes.DigitalLine;
        }

        public override ITimeVectorRef TimeSyncRef => null;

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
                DateTime timeMargin = Model.LastQuote.Time.ToUniversalTime();

                var ticks = new QuoteInfo[0];

                try
                {
                    ticks = await ClientModel.FeedHistory.GetQuotePage(SymbolCode, (timeMargin + TimeSpan.FromMinutes(15)).ToTimestamp(), -100, false);
                }
                catch (Exception)
                {
                    // TO DO: dysplay error on chart
                }

                //foreach (var tick in tickArray)
                //{
                //    askData.Append(tick.CreatingTime, tick.Ask);
                //    bidData.Append(tick.CreatingTime, tick.Bid);
                //}

                askData.Append(
                    ticks.Select(t => t.Time),
                    ticks.Select(t => t.Ask));
                bidData.Append(
                    ticks.Select(t => t.Time),
                    ticks.Select(t => t.Bid));

                if (ticks.Length > 0)
                {
                    lastSeriesQuote = ticks.Last();

                    var start = ticks.First().Time;
                    var end = ticks.Last().Time;
                    InitBoundaries(ticks.Length, start, end);
                }
            }
        }

        protected override void ApplyUpdate(QuoteInfo update)
        {
            if (lastSeriesQuote == null || update.Time > lastSeriesQuote.Time)
            {
                askData.Append(update.Time, update.Ask);
                bidData.Append(update.Time, update.Bid);
                ExtendBoundaries(askData.Count, update.Time);
            }
        }

        protected override IndicatorModel CreateIndicator(PluginConfig config)
        {
            return new IndicatorModel(config, Agent, this, this);
        }

        public override void InitializePlugin(RuntimeModel runtime)
        {
            base.InitializePlugin(runtime);

            var feedProvider = new PluginFeedProvider(ClientModel.Cache, ClientModel.Distributor, ClientModel.FeedHistory, new DispatcherSync());
            runtime.Feed = feedProvider;
            runtime.FeedHistory = feedProvider;
            runtime.Config.InitQuoteStrategy();
            runtime.Metadata = feedProvider;
        }

        public override void UpdatePlugin(RuntimeModel runtime)
        {
            base.UpdatePlugin(runtime);
            //TO DO: runtime.GetFeedStrategy<QuoteStrategy>().SetMainSeries();
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
