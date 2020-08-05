using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class FeedSeriesCollector : IDisposable
    {
        private FeedSeriesEmulator _fixture;
        private BarVector _barVector;
        private Action<object> _sendUpdateAction;
        private string _symbol;

        public FeedSeriesCollector(FeedEmulator feed, TestDataSeriesFlags seriesFlags, string symbol, TimeFrames timeFrame, Action<object> sendUpdateAction)
        {
            _symbol = symbol;
            _sendUpdateAction = sendUpdateAction;

            _barVector = feed.GetBarBuilder(symbol, timeFrame, BarPriceType.Bid);

            if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
            {
                if (seriesFlags.HasFlag(TestDataSeriesFlags.Realtime))
                {
                    _fixture = feed.GetFeedSymbolFixture(symbol, timeFrame);
                    _fixture.RateUpdated += Fixture_RateUpdated;
                }
                else
                {
                    _barVector.BarClosed += _barVector_BarClosed;
                }
            }
        }

        public int BarCount => _barVector.Count;
        public IEnumerable<BarEntity> Snapshot => _barVector;

        public void Dispose()
        {
            if (_fixture != null)
                _fixture.RateUpdated -= Fixture_RateUpdated;
            if (_barVector != null)
                _barVector.BarClosed -= _barVector_BarClosed;
        }

        private void Fixture_RateUpdated(IRateInfo rate)
        {
            _sendUpdateAction(rate);
        }

        private void _barVector_BarClosed(BarEntity bar)
        {
            var update = new DataSeriesUpdate<BarEntity>(DataSeriesTypes.SymbolRate, _symbol, SeriesUpdateActions.Append, bar);
            _sendUpdateAction(update);
        }
    }
}
