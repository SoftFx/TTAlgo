using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using Api = TickTrader.Algo.Api;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using TickTrader.Algo.Core.Math;

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        private readonly OhlcDataSeries<DateTime, double> chartData = new OhlcDataSeries<DateTime, double>();
        private readonly List<Algo.Core.BarEntity> indicatorData = new List<Algo.Core.BarEntity>();
        private BarPeriod period;
        private Api.TimeFrames timeframe;
        private readonly BarVector barCollection = new BarVector();

        public BarChartModel(SymbolModel symbol, AlgoEnvironment algoEnv, TraderClientModel clientModel)
            : base(symbol, algoEnv, clientModel)
        {
            Support(SelectableChartTypes.OHLC);
            Support(SelectableChartTypes.Candle);
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);

            Navigator = new UniformChartNavigator();

            SelectedChartType = SelectableChartTypes.Candle;

            barCollection.Updated += a =>
            {
                if (a.Action == DLinqAction.Insert)
                {
                    var bar = a.NewItem;
                    chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);
                }
                else if (a.Action == DLinqAction.Replace)
                {
                    var bar = a.NewItem;
                    chartData.Update(a.Index, bar.Open, bar.High, bar.Low, bar.Close);
                }
                else if (a.Action == DLinqAction.Remove)
                    chartData.RemoveAt(a.Index);
            };
        }

        public void Activate(Api.TimeFrames timeframe)
        {
            this.timeframe = timeframe;
            this.period = FdkToAlgo.ToBarPeriod(timeframe);
            base.Activate();
        }

        public override Api.TimeFrames TimeFrame { get { return timeframe; } }

        protected override void ClearData()
        {
            barCollection.Clear();
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            var barArray = await ClientModel.History.GetBars(SymbolCode, PriceType.Bid, period, DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15), -4000);
            var loadedData = barArray.Reverse().ToArray();

            cToken.ThrowIfCancellationRequested();

            indicatorData.Clear();
            FdkToAlgo.Convert(loadedData, indicatorData);

            barCollection.ChangeTimeframe(TimeFrame);
            barCollection.Append(indicatorData);

            if (loadedData.Length > 0)
                InitBoundaries(loadedData.Length, loadedData.First().From, loadedData.Last().From);
        }

        protected override PluginSetup CreateSetup(AlgoPluginRef catalogItem)
        {
            return new BarBasedPluginSetup(catalogItem, SymbolCode, Algo.Api.BarPriceType.Bid, AlgoEnv);
        }

        protected override IndicatorModel CreateIndicator(PluginSetup setup)
        {
            return new IndicatorModel(setup, this);
        }

        protected override void InitPluign(PluginExecutor plugin)
        {
            var mainSeries = barCollection.Snapshot.ToList();
            var feed = new PluginFeedProvider(ClientModel.Symbols, ClientModel.History);
            plugin.InitBarStrategy(feed, Algo.Api.BarPriceType.Bid, mainSeries);
            plugin.Metadata = feed;
        }

        protected override void ApplyUpdate(Quote quote)
        {
            if (quote.HasBid)
            {
                barCollection.Update(quote.CreatingTime, quote.Bid, 1);
                ExtendBoundaries(barCollection.Count, quote.CreatingTime);
            }
        }

        private static void Convert(List<SoftFX.Extended.Bar> fdkData, List<Algo.Core.BarEntity> chartData)
        {
            chartData.AddRange(
            fdkData.Select(b => new Algo.Core.BarEntity()
            {
                Open = b.Open,
                Close = b.Close,
                High = b.High,
                Low = b.Low,
                OpenTime = b.From,
                CloseTime =  b.To,
                Volume = b.Volume
            }));
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
                    return new OhlcRenderableSeriesViewModel() { DataSeries = chartData, StyleKey = "BarChart_OhlcStyle" };
                case SelectableChartTypes.Candle:
                    return new CandlestickRenderableSeriesViewModel() { DataSeries = chartData, StyleKey = "BarChart_CandlestickStyle" };
                case SelectableChartTypes.Line:
                    return new LineRenderableSeriesViewModel() { DataSeries = chartData, StyleKey = "BarChart_LineStyle" };
                case SelectableChartTypes.Mountain:
                    return new MountainRenderableSeriesViewModel() { DataSeries = chartData, StyleKey = "BarChart_MountainStyle" };
            }

            throw new InvalidOperationException("Unsupported chart type: " + SelectedChartType);
        }
    }
}
    