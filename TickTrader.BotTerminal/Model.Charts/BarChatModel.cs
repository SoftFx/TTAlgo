using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Core;
using Api = TickTrader.Algo.Api;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        private readonly ChartBarVector _barVector = new ChartBarVector(Api.TimeFrames.M1);

        public BarChartModel(SymbolModel symbol, AlgoEnvironment algoEnv)
            : base(symbol, algoEnv)
        {
            Support(SelectableChartTypes.OHLC);
            Support(SelectableChartTypes.Candle);
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);

            Navigator = new UniformChartNavigator();

            SelectedChartType = SelectableChartTypes.Candle;
        }

        public void Activate(Api.TimeFrames timeframe)
        {
            TimeFrame = timeframe;
            base.Activate();
        }

        public override ITimeVectorRef TimeSyncRef => _barVector.Ref;

        protected override void ClearData()
        {
            _barVector.Clear();
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            var aproximateTimeRef = DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15);
            var barArray = await ClientModel.FeedHistory.GetBarPage(SymbolCode, Api.BarPriceType.Bid, TimeFrame, aproximateTimeRef, -4000);

            cToken.ThrowIfCancellationRequested();

            _barVector.Clear();
            _barVector.TimeFrame = TimeFrame;
            _barVector.AppendRange(barArray);

            if (barArray.Length > 0)
                InitBoundaries(barArray.Length, barArray.First().OpenTime, barArray.Last().OpenTime);
        }

        protected override IndicatorModel CreateIndicator(PluginConfig config)
        {
            return new IndicatorModel(config, Agent, this, this);
        }

        public override void InitializePlugin(PluginExecutorCore plugin)
        {
            base.InitializePlugin(plugin);
            var feed = new PluginFeedProvider(ClientModel.Cache, ClientModel.Distributor, ClientModel.FeedHistory, new DispatcherSync());
            plugin.Feed = feed;
            plugin.FeedHistory = feed;
            plugin.InitBarStrategy(Algo.Api.BarPriceType.Bid);
            plugin.Metadata = feed;
        }

        public override void UpdatePlugin(PluginExecutorCore plugin)
        {
            base.UpdatePlugin(plugin);
            plugin.GetFeedStrategy<BarStrategy>().SetMainSeries(_barVector.ToList());
        }

        protected override void ApplyUpdate(QuoteEntity quote)
        {
            if (quote.HasBid)
            {
                _barVector.TryAppendQuote(quote.CreatingTime, quote.Bid, 1);
                ExtendBoundaries(_barVector.Count, quote.CreatingTime);
            }
        }

        protected override void UpdateSeries()
        {
            var seriesModel = CreateSeriesModel();

            if (SeriesCollection.Count == 0)
                SeriesCollection.Add(seriesModel);
            else
                SeriesCollection[0] = seriesModel;
        }

        private IRenderableSeriesViewModel CreateSeriesModel()
        {
            switch (SelectedChartType)
            {
                case SelectableChartTypes.OHLC:
                    return new OhlcRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_OhlcStyle" };
                case SelectableChartTypes.Candle:
                    return new CandlestickRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_CandlestickStyle" };
                case SelectableChartTypes.Line:
                    return new LineRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_LineStyle" };
                case SelectableChartTypes.Mountain:
                    return new MountainRenderableSeriesViewModel() { DataSeries = _barVector.SciChartdata, StyleKey = "BarChart_MountainStyle" };
            }

            throw new InvalidOperationException("Unsupported chart type: " + SelectedChartType);
        }
    }
}
