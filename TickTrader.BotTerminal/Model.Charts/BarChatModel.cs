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

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        private readonly OhlcDataSeries<DateTime, double> chartData = new OhlcDataSeries<DateTime, double>();
        private readonly List<Api.Bar> indicatorData = new List<Api.Bar>();
        private BarPeriod period;

        public BarChartModel(SymbolModel symbol, AlgoCatalog catalog, FeedModel feed)
            : base(symbol, catalog, feed)
        {
            ReserveTopSeries(1);

            Support(SelectableChartTypes.OHLC);
            Support(SelectableChartTypes.Candle);
            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);

            SelectedChartType = SelectableChartTypes.Candle;
        }

        protected IRenderableSeries MainSeries
        {
            get { return Series[0]; }
            set { Series[0] = value; }
        }

        public void Activate(BarPeriod period)
        {
            this.period = period;
            base.Activate();
        }

        protected override void ClearData()
        {
            MainSeries.DataSeries = null;
            chartData.Clear();
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            var periodCopy = this.period;

            var response = await Task.Factory.StartNew(
                () => Connection.FeedProxy.Server.GetHistoryBars(
                    Symbol, DateTime.Now + TimeSpan.FromDays(1),
                    -10000, SoftFX.Extended.PriceType.Ask, periodCopy));

            cToken.ThrowIfCancellationRequested();

            var rawData = response.Bars.Reverse().ToList();

            indicatorData.Clear();
            Convert(rawData, indicatorData);

            foreach (var bar in rawData)
                chartData.Append(bar.From, bar.Open, bar.High, bar.Low, bar.Close);

            MainSeries.DataSeries = chartData;
        }

        protected override void UpdateSeriesStyle()
        {
            switch (SelectedChartType)
            {
                case SelectableChartTypes.Candle:
                    MainSeries = new FastCandlestickRenderableSeries();
                    break;
                case SelectableChartTypes.Line:
                    MainSeries = new FastLineRenderableSeries();
                    break;
                case SelectableChartTypes.OHLC:
                    MainSeries = new FastOhlcRenderableSeries();
                    break;
                case SelectableChartTypes.Mountain:
                    MainSeries = new FastMountainRenderableSeries();
                    break;
            }

            MainSeries.DataSeries = chartData;
        }

        protected override bool IsIndicatorSupported(AlgoDescriptor descriptor)
        {
            return true;
        }

        protected override IIndicatorSetup CreateInidactorConfig(AlgoCatalogItem repItem)
        {
            return new IndicatorConfig(repItem, this);
        }

        private static void Convert(List<Bar> fdkData, List<Api.Bar> chartData)
        {
            chartData.AddRange(
            fdkData.Select(b => new Api.Bar()
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
            private AlgoCatalogItem repItem;

            public IndicatorConfig(AlgoCatalogItem repItem, BarChartModel chart, IndicatorSetup_Bars srcSetup = null)
            {
                this.chart = chart;
                this.repItem = repItem;
                this.InstanceId = chart.GetNextIndicatorId();
                this.UiModel = new IndicatorSetup_Bars(repItem.Descriptor);
            }

            public long InstanceId { get; private set; }
            public AlgoDescriptor Descriptor { get { return UiModel.Descriptor; } }
            public IndicatorSetupBase UiModel { get; private set; }

            public IndicatorModel CreateIndicator()
            {
                DirectReader<Api.Bar> reader = new DirectReader<Api.Bar>(chart.indicatorData);

                foreach (var input in UiModel.Inputs)
                    ((BarInputSetup)input).Configure(reader);

                DirectWriter<Api.Bar> writer = new DirectWriter<Api.Bar>();

                var outputSeries = new Dictionary<ColoredLineOutputSetup, IDataSeries>();

                foreach (var output in UiModel.Outputs)
                {
                    if (output is ColoredLineOutputSetup)
                    {
                        XyDataSeries<DateTime, double> series = new XyDataSeries<DateTime, double>();
                        writer.AddMapping(output.Id, new XySeriesWriter(series));
                        outputSeries[(ColoredLineOutputSetup)output] = series;
                    }
                }

                IndicatorBuilder<Api.Bar> buidler = new IndicatorBuilder<Api.Bar>(repItem.Descriptor, reader, writer);

                foreach (var parameter in UiModel.Parameters)
                    buidler.SetParameter(parameter.Id, parameter.ValueObj);

                IndicatorModel model = new IndicatorModel(this, buidler);

                foreach (var output in outputSeries)
                    model.AddSeries(output.Key, output.Value);

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
    