using SciChart.Charting.Model.DataSeries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.ChartSeries;
using TickTrader.Algo.Domain;
using Google.Protobuf.WellKnownTypes;
using TickTrader.Algo.Server;

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
                DateTime timeMargin = Model.LastQuote.TimeUtc;

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
                    ticks.Select(t => t.TimeUtc),
                    ticks.Select(t => t.Ask));
                bidData.Append(
                    ticks.Select(t => t.TimeUtc),
                    ticks.Select(t => t.Bid));

                if (ticks.Length > 0)
                {
                    lastSeriesQuote = ticks.Last();

                    var start = ticks.First().TimeUtc;
                    var end = ticks.Last().TimeUtc;
                    InitBoundaries(ticks.Length, start, end);
                }
            }
        }

        protected override void ApplyUpdate(QuoteInfo update)
        {
            if (lastSeriesQuote == null || update.Time > lastSeriesQuote.Time)
            {
                askData.Append(update.TimeUtc, update.Ask);
                bidData.Append(update.TimeUtc, update.Bid);
                ExtendBoundaries(askData.Count, update.TimeUtc);
            }
        }

        protected override IndicatorModel CreateIndicator(PluginConfig config)
        {
            return new IndicatorModel(config, Agent, this, this);
        }

        public override void InitializePlugin(ExecutorConfig config)
        {
            base.InitializePlugin(config);

            config.InitQuoteStrategy();
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
