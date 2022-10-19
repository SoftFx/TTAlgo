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

        public ObservableBarVector BarVector { get; }

        public override IEnumerable<ChartTypes> AvailableChartTypes { get; }


        public BarChartModel(SymbolInfo symbol, AlgoEnvironment algoEnv) : base(symbol, algoEnv)
        {
            AvailableChartTypes = new List<ChartTypes>()
            {
                ChartTypes.Candle,
                ChartTypes.Line,
                ChartTypes.Mountain,
            };

            BarVector = new ObservableBarVector(symbol.Name);
            SelectedChartType = ChartTypes.Candle;
        }


        public void Activate(Feed.Types.Timeframe timeframe)
        {
            TimeFrame = timeframe;
            base.Activate();
        }


        protected override void ClearData()
        {
            BarVector.Clear();
            _barSub?.Dispose();
            _barSub = null;
        }

        protected async override Task LoadData(CancellationToken cToken)
        {
            _barSub = ClientModel.BarDistributor.AddListener(OnBarUpdate, new BarSubEntry(SymbolCode, TimeFrame));

            var aproximateTimeRef = UtcTicks.Now + TimeSpan.FromDays(1) - TimeSpan.FromMinutes(15);
            var barArray = await ClientModel.FeedHistory.GetBarPage(SymbolCode, Feed.Types.MarketSide.Bid, TimeFrame, aproximateTimeRef, -BarsCount);

            cToken.ThrowIfCancellationRequested();

            BarVector.InitNewVector(TimeFrame, barArray);

            if (barArray.Length > 0)
                InitBoundaries(barArray.Length, barArray.First().OpenTime.ToUtcDateTime(), barArray.Last().OpenTime.ToUtcDateTime());
        }


        protected override void ApplyBarUpdate(BarUpdate bar)
        {
            BarVector.ApplyTickUpdate(bar.BidData?.Close, bar.AskData?.Close);
            BarVector.ApplyBarUpdate(bar.BidData);
            //ExtendBoundaries(BarVector.Count, bar.Data.CloseTime.ToUtcDateTime());
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
