using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Controls.Chart;

namespace TickTrader.BotTerminal
{
    internal class BarChartModel : ChartModelBase
    {
        internal const int BarsCount = 4000;

        public ObservableBarVector BarVector { get; } = new();

        public override IEnumerable<ChartTypes> AvailableChartTypes { get; }


        public BarChartModel(SymbolInfo symbol, AlgoEnvironment algoEnv) : base(symbol, algoEnv)
        {
            AvailableChartTypes = new List<ChartTypes>()
            {
                ChartTypes.Candle,
                ChartTypes.Line,
                ChartTypes.Mountain,
            };

            SelectedChartType = ChartTypes.Candle;
        }

        public void Activate(Feed.Types.Timeframe timeframe)
        {
            TimeFrame = timeframe;
            base.Activate();
        }

        //public override ITimeVectorRef TimeSyncRef => null; //_barVector.Ref;


        protected override void ClearData()
        {
            BarVector.Clear();
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            var aproximateTimeRef = DateTime.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15);
            var barArray = await ClientModel.FeedHistory.GetBarPage(SymbolCode, Feed.Types.MarketSide.Bid, TimeFrame, aproximateTimeRef.ToUniversalTime().ToTimestamp(), -BarsCount);

            cToken.ThrowIfCancellationRequested();

            BarVector.InitNewVector(Model.Name, TimeFrame, barArray);

            if (barArray.Length > 0)
                InitBoundaries(barArray.Length, barArray.First().OpenTime.ToUtcDateTime(), barArray.Last().OpenTime.ToUtcDateTime());
        }

        protected override IndicatorModel CreateIndicator(PluginConfig config)
        {
            return new IndicatorModel(config, null, this, this);
        }

        public override void InitializePlugin(ExecutorConfig config)
        {
            base.InitializePlugin(config);

            config.InitBarStrategy(Feed.Types.MarketSide.Bid);
            //config.SetMainSeries(_barVector.Select(u));
        }

        protected override void ApplyUpdate(QuoteInfo quote)
        {
            BarVector.ApplyQuote(quote);

            //if (quote.HasBid)
            //{
            //    //_barVector.TryAppendQuote(quote.Time, quote.Bid, 1);
            //    //ExtendBoundaries(_barVector.Count, quote.TimeUtc);
            //}
        }

        protected override void UpdateSeries()
        {
            //var seriesModel = CreateSeriesModel();

            //if (SeriesCollection.Count == 0)
            //    SeriesCollection.Add(seriesModel);
            //else
            //    SeriesCollection[0] = seriesModel;
        }
    }
}
