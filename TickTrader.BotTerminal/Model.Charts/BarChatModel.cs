using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;
using TickTrader.Algo.Core;
using Api = TickTrader.Algo.Api;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        private OhlcDataSeries<DateTime, double> chartData = new OhlcDataSeries<DateTime, double>();
        private readonly List<Algo.Core.BarEntity> indicatorData = new List<Algo.Core.BarEntity>();
        private BarPeriod period;
        private Api.TimeFrames timeframe;
        private BarVector barCollection = new BarVector();

        public BarChartModel(SymbolModel symbol, PluginCatalog catalog, FeedModel feed, BotJournal journal)
            : base(symbol, catalog, feed, journal)
        {
            Support(SelectableChartTypes.OHLC);
            Support(SelectableChartTypes.Candle);
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);

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
            this.period = FdkAdapter.ToBarPeriod(timeframe);
            base.Activate();
        }

        public override Api.TimeFrames TimeFrame { get { return timeframe; } }

        protected override void ClearData()
        {
            barCollection.Clear();
        }

        protected async override Task<DataMetrics> LoadData(CancellationToken cToken)
        {
            var barArray = await Feed.History.GetBars(SymbolCode, PriceType.Bid, period, DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15), -4000);
            var loadedData = barArray.Reverse().ToArray();

            cToken.ThrowIfCancellationRequested();

            indicatorData.Clear();
            FdkAdapter.Convert(loadedData, indicatorData);

            barCollection.Update(indicatorData);

            var metrics = new DataMetrics();
            metrics.Count = loadedData.Length;
            if (loadedData.Length > 0)
            {
                metrics.StartDate = loadedData.First().From;
                metrics.EndDate = loadedData.Last().From;
            }
            return metrics;
        }

        protected override PluginSetup CreateSetup(AlgoPluginRef catalogItem)
        {
            return new BarBasedPluginSetup(catalogItem, SymbolCode);
        }

        protected override IndicatorModel2 CreateIndicator(PluginSetup setup)
        {
            return new IndicatorModel2(setup, this);
        }

        protected override FeedStrategy GetFeedStrategy()
        {
            return new BarStrategy();
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
    