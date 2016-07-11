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

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        //private readonly OhlcDataSeries<DateTime, double> chartData = new OhlcDataSeries<DateTime, double>();
        private readonly List<Algo.Core.BarEntity> indicatorData = new List<Algo.Core.BarEntity>();
        private BarPeriod period;
        private Api.TimeFrames timeframe;
        //private List<QuoteEntity> updateQueue;
        private BarVector barCollection = new BarVector();

        public BarChartModel(SymbolModel symbol, PluginCatalog catalog, FeedModel feed)
            : base(symbol, catalog, feed)
        {
            Support(SelectableChartTypes.OHLC);
            Support(SelectableChartTypes.Candle);
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);

            SelectedChartType = SelectableChartTypes.Candle;

            var chartData = new OhlcDataSeries<DateTime, double>();

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

            AddSeries(chartData);
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
            //var response = await Task.Factory.StartNew(
            //    () => Connection.FeedProxy.Server.GetHistoryBars(
            //        Symbol, DateTime.Now + TimeSpan.FromDays(1),
            //        -10000, SoftFX.Extended.PriceType.Ask, periodCopy));

            var barArray = await Feed.History.GetBars(Symbol, PriceType.Bid, period, DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15), -4000);
            var loadedData = barArray.Reverse().ToArray();

            cToken.ThrowIfCancellationRequested();

            //var rawData = response.Bars.Reverse().ToList();

            indicatorData.Clear();
            FdkAdapter.Convert(loadedData, indicatorData);
            //Convert(rawData, indicatorData);

            //foreach (var bar in loadedData)
            //    chartData.Append(bar.From, bar.Open, bar.High, bar.Low, bar.Close);

            //if (indicatorData.Count > 0)
                //barCollection.SetBoundaries(indicatorData[0].OpenTime);

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

        protected override IIndicatorSetup CreateInidactorConfig(AlgoPluginRef repItem)
        {
            return new IndicatorConfig(repItem, this);
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

        private class IndicatorConfig : IIndicatorSetup
        {
            private BarChartModel chart;
            private AlgoPluginRef repItem;

            public IndicatorConfig(AlgoPluginRef repItem, BarChartModel chart, IndicatorSetup_Bars srcSetup = null)
            {
                this.chart = chart;
                this.repItem = repItem;
                this.InstanceId = chart.GetNextIndicatorId();
                this.UiModel = new IndicatorSetup_Bars(repItem.Descriptor, chart.Symbol);
            }

            public long InstanceId { get; private set; }
            public AlgoPluginDescriptor Descriptor { get { return UiModel.Descriptor; } }
            public IndicatorSetupBase UiModel { get; private set; }
            public int DataLen { get { return chart.indicatorData.Count; } }

            public IndicatorModel CreateIndicator()
            {
                IndicatorBuilder builder = new IndicatorBuilder(Descriptor);

                var mainBuffer = builder.GetBuffer<BarEntity>(chart.Symbol);
                mainBuffer.Append(chart.indicatorData);

                builder.MainSymbol = chart.Symbol;

                foreach (var input in UiModel.Inputs)
                    ((BarInputSetup)input).Configure(builder);

                foreach (var parameter in UiModel.Parameters)
                    builder.SetParameter(parameter.Id, parameter.ValueObj);

                IndicatorModel model = new IndicatorModel(this, builder, chart.Feed, i => mainBuffer[i].OpenTime);

                //foreach (var outputSetup in UiModel.Outputs)
                //{
                //    if (outputSetup is ColoredLineOutputSetup)
                //        new DoubleSeriesAdapter(model, (ColoredLineOutputSetup)outputSetup);
                //    else if (outputSetup is MarkerSeriesOutputSetup)
                //        new MarkerSeriesAdapter(model, (MarkerSeriesOutputSetup)outputSetup);
                //}

                return model;
            }

            public IIndicatorSetup CreateCopy()
            {
                var copy = new IndicatorConfig(repItem, chart);
                copy.UiModel.CopyFrom(UiModel);
                return copy;
            }
        }
    }
}
    