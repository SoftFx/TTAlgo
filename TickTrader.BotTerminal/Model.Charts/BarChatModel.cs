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
using TickTrader.Algo.Core.Math;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        private readonly OhlcDataSeries<DateTime, double> chartData = new OhlcDataSeries<DateTime, double>();
        //private readonly List<Algo.Core.BarEntity> indicatorData = new List<Algo.Core.BarEntity>();
        private Api.TimeFrames timeframe;
        private readonly BarVector barCollection = new BarVector();

        public BarChartModel(SymbolModel symbol, LocalAlgoAgent agent)
            : base(symbol, agent)
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
            base.Activate();
        }

        public override Api.TimeFrames TimeFrame { get { return timeframe; } }

        protected override void ClearData()
        {
            barCollection.Clear();
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            var aproximateTimeRef = DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15);
            var barArray = await ClientModel.FeedHistory.GetBarPage(SymbolCode, Api.BarPriceType.Bid, timeframe, aproximateTimeRef, -4000);
            //var loadedData = barArray.Reverse().ToArray();

            cToken.ThrowIfCancellationRequested();

            //indicatorData.Clear();
            //FdkToAlgo.Convert(loadedData, indicatorData);

            barCollection.Clear();
            barCollection.ChangeTimeframe(TimeFrame);
            barCollection.Append(barArray);

            if (barArray.Length > 0)
                InitBoundaries(barArray.Length, barArray.First().OpenTime, barArray.Last().OpenTime);
        }

        protected override IndicatorModel CreateIndicator(PluginConfig config)
        {
            return new IndicatorModel(config, Agent, this, this);
        }

        public override void InitializePlugin(PluginExecutor plugin)
        {
            base.InitializePlugin(plugin);
            var feed = new PluginFeedProvider(ClientModel.Cache, ClientModel.Distributor, ClientModel.FeedHistory, new DispatcherSync());
            plugin.InitBarStrategy(feed, Algo.Api.BarPriceType.Bid);
            plugin.Metadata = feed;
        }

        public override void UpdatePlugin(PluginExecutor plugin)
        {
            base.UpdatePlugin(plugin);
            plugin.GetFeedStrategy<BarStrategy>().SetMainSeries(barCollection.Snapshot.ToList());
        }

        protected override void ApplyUpdate(QuoteEntity quote)
        {
            if (quote.HasBid)
            {
                barCollection.Update(quote.CreatingTime, quote.Bid, 1);
                ExtendBoundaries(barCollection.Count, quote.CreatingTime);
            }
        }

        //private static void Convert(List<SoftFX.Extended.Bar> fdkData, List<Algo.Core.BarEntity> chartData)
        //{
        //    chartData.AddRange(
        //    fdkData.Select(b => new Algo.Core.BarEntity()
        //    {
        //        Open = b.Open,
        //        Close = b.Close,
        //        High = b.High,
        //        Low = b.Low,
        //        OpenTime = b.From,
        //        CloseTime = b.To,
        //        Volume = b.Volume
        //    }));
        //}

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
