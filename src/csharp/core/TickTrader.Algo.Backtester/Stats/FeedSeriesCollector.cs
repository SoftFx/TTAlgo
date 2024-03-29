﻿using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public class FeedSeriesCollector : IDisposable
    {
        private FeedSeriesEmulator _fixture;
        private BarVector _barVector;
        private Action<object> _sendUpdateAction;
        private string _symbol;

        public FeedSeriesCollector(FeedEmulator feed, TestDataSeriesFlags seriesFlags, string symbol, Feed.Types.Timeframe timeFrame, Action<object> sendUpdateAction)
        {
            _symbol = symbol;
            _sendUpdateAction = sendUpdateAction;

            _barVector = feed.GetBarBuilder(symbol, timeFrame, Feed.Types.MarketSide.Bid);

            if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
            {
                if (seriesFlags.HasFlag(TestDataSeriesFlags.Realtime))
                {
                    _fixture = feed.GetFeedSymbolFixture(symbol, timeFrame);
                    _fixture.RateUpdated += Fixture_RateUpdated;
                }
                else
                {
                    _barVector.BarClosed += BarVector_BarClosed;
                }
            }
        }

        public int BarCount => _barVector.Count;
        public IEnumerable<BarData> Snapshot => _barVector;

        public void Dispose()
        {
            if (_fixture != null)
                _fixture.RateUpdated -= Fixture_RateUpdated;
            if (_barVector != null)
                _barVector.BarClosed -= BarVector_BarClosed;
        }

        private void Fixture_RateUpdated(IRateInfo rate)
        {
            _sendUpdateAction(rate);
        }

        private void BarVector_BarClosed(BarData bar)
        {
            var update = new BarSeriesUpdate(BarSeriesUpdate.Types.Type.SymbolRate, _symbol, DataSeriesUpdate.Types.Action.Append, bar);
            _sendUpdateAction(update);
        }
    }
}
