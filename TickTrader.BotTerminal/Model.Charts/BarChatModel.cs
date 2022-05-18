using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
//using SciChart.Charting.Model.ChartSeries;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Domain;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        //private readonly ChartBarVector _barVector = new ChartBarVector(Feed.Types.Timeframe.M1);

        public BarChartModel(SymbolInfo symbol, AlgoEnvironment algoEnv)
            : base(symbol, algoEnv)
        {
            Support(SelectableChartTypes.OHLC);
            Support(SelectableChartTypes.Candle);
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);

            //Navigator = new UniformChartNavigator();

            SelectedChartType = SelectableChartTypes.Candle;
        }

        public void Activate(Feed.Types.Timeframe timeframe)
        {
            TimeFrame = timeframe;
            base.Activate();
        }

        public override ITimeVectorRef TimeSyncRef => null; //_barVector.Ref;

        protected override void ClearData()
        {
          //  _barVector.Clear();
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            var aproximateTimeRef = DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15);
            var barArray = await ClientModel.FeedHistory.GetBarPage(SymbolCode, Feed.Types.MarketSide.Bid, TimeFrame, aproximateTimeRef.ToUniversalTime().ToTimestamp(), -4000);

            cToken.ThrowIfCancellationRequested();

            //_barVector.Clear();
            //_barVector.TimeFrame = TimeFrame;
            //_barVector.AppendRange(barArray);

            if (barArray.Length > 0)
                InitBoundaries(barArray.Length, barArray.First().OpenTime.ToUtcDateTime(), barArray.Last().OpenTime.ToUtcDateTime());
        }

        protected override IndicatorModel CreateIndicator(PluginConfig config)
        {
            return new IndicatorModel(config, Agent, this, this);
        }

        public override void InitializePlugin(ExecutorConfig config)
        {
            base.InitializePlugin(config);

            config.InitBarStrategy(Feed.Types.MarketSide.Bid);
            //config.SetMainSeries(_barVector);
        }

        protected override void ApplyUpdate(QuoteInfo quote)
        {
            if (quote.HasBid)
            {
                //_barVector.TryAppendQuote(quote.Time, quote.Bid, 1);
                //ExtendBoundaries(_barVector.Count, quote.TimeUtc);
            }
        }

        protected override void UpdateSeries()
        {
            //var seriesModel = CreateSeriesModel();

            //if (SeriesCollection.Count == 0)
            //    SeriesCollection.Add(seriesModel);
            //else
            //    SeriesCollection[0] = seriesModel;
        }

        //private IRenderableSeriesViewModel CreateSeriesModel()
        //{
        //    switch (SelectedChartType)
        //    {
        //        case SelectableChartTypes.OHLC:
        //            return new OhlcRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_OhlcStyle" };
        //        case SelectableChartTypes.Candle:
        //            return new CandlestickRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_CandlestickStyle" };
        //        case SelectableChartTypes.Line:
        //            return new LineRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_LineStyle" };
        //        case SelectableChartTypes.Mountain:
        //            return new MountainRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_MountainStyle" };
        //    }

        //    throw new InvalidOperationException("Unsupported chart type: " + SelectedChartType);
        //}
    }
}
