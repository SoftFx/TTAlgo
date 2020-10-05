using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class MultiRateUpdate : RateUpdate
    {
        private readonly RateUpdate _mockUpdate;


        public RateUpdate[] Updates { get; }


        public MultiRateUpdate(IEnumerable<RateUpdate> updates)
        {
            Updates = updates.ToArray();
            _mockUpdate = Updates[0];
        }


        string RateUpdate.Symbol => _mockUpdate.Symbol;

        DateTime RateUpdate.Time => _mockUpdate.Time;

        bool RateUpdate.HasAsk => _mockUpdate.HasAsk;

        bool RateUpdate.HasBid => _mockUpdate.HasBid;

        double RateUpdate.Ask => _mockUpdate.Ask;

        double RateUpdate.AskHigh => _mockUpdate.AskHigh;

        double RateUpdate.AskLow => _mockUpdate.AskLow;

        double RateUpdate.AskOpen => _mockUpdate.AskOpen;

        double RateUpdate.Bid => _mockUpdate.Bid;

        double RateUpdate.BidHigh => _mockUpdate.BidHigh;

        double RateUpdate.BidLow => _mockUpdate.BidLow;

        double RateUpdate.BidOpen => _mockUpdate.BidOpen;

        int RateUpdate.NumberOfQuotes => _mockUpdate.NumberOfQuotes;

        Quote RateUpdate.LastQuote => _mockUpdate.LastQuote;
    }
}
