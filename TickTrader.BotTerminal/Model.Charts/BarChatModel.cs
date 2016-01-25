using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
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

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        private OhlcDataSeries<DateTime, double> chartData;
        private BarPeriod period;

        public BarChartModel(SymbolModel symbol, AlgoRepositoryModel repository, FeedModel feed)
            : base(symbol, repository, feed)
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
            chartData = null;
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            //foreach (var indicator in this.Indicators)
            //    indicator.SetData(null);

            var periodCopy = this.period;

            var response = await Task.Factory.StartNew(
                () => Connection.FeedProxy.Server.GetHistoryBars(
                    Symbol, DateTime.Now + TimeSpan.FromDays(1),
                    -4000, SoftFX.Extended.PriceType.Ask, periodCopy));

            cToken.ThrowIfCancellationRequested();

            this.chartData = new OhlcDataSeries<DateTime, double>();

            var rawData = response.Bars.Reverse().ToArray();

            foreach (var bar in rawData)
                chartData.Append(bar.From, bar.Open, bar.High, bar.Low, bar.Close);

            //foreach (var indicator in this.Indicators)
            //    indicator.SetData(rawData);

            MainSeries.DataSeries = chartData;

            if (chartData.Count > 0)
            {
                //this.VisibleRange.Max = chartData.Count - 1;
                //this.VisibleRange.Min = Math.Max(0, chartData.Count - 101);
            }
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

        public override IndicatorSetup CreateSetup(AlgoRepositoryItem item)
        {
            throw new NotImplementedException();
            //return new IndicatorSetup_Bars(item.Descriptor);
        }

        protected override bool IsIndicatorSupported(AlgoInfo descriptor)
        {
            return true;
        }
    }
}
