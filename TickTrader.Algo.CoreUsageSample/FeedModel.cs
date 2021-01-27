using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreUsageSample
{
    internal class FeedModel : IFeedProvider, IFeedHistoryProvider, ISyncContext
    {
        private Action<QuoteInfo[]> FeedUpdated;
        private Dictionary<string, SymbolDataModel> dataBySymbol = new Dictionary<string, SymbolDataModel>();

        public event Action<QuoteInfo> RateUpdated;
        public event Action<List<QuoteInfo>> RatesUpdated;

        public Feed.Types.Timeframe TimeFrame { get; private set; }

        public ISyncContext Sync { get { return this; } }

        public FeedModel(Feed.Types.Timeframe timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        public void Fill(string symbol, IEnumerable<BarData> data)
        {
            GetSymbolData(symbol).Fill(data);
        }

        public void Update(QuoteInfo update)
        {
            GetSymbolData(update.Symbol).Update(update);
            FeedUpdated?.Invoke(new QuoteInfo[] { update });
        }

        private SymbolDataModel GetSymbolData(string smbCode)
        {
            SymbolDataModel data;
            if(!dataBySymbol.TryGetValue(smbCode, out data))
            {
                data = new SymbolDataModel(TimeFrame);
                dataBySymbol.Add(smbCode, data);
            }
            return data;
        }

        List<BarData> IFeedHistoryProvider.QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return GetSymbolData(symbol).QueryBars(from, to, timeframe).ToList();
        }

        Task<List<BarData>> IFeedHistoryProvider.QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return Task.FromResult(GetSymbolData(symbol).QueryBars(from, to, timeframe).ToList());
        }

        List<QuoteInfo> IFeedHistoryProvider.QueryQuotes(string symbol, Timestamp from, Timestamp to, bool level2)
        {
            return null;
        }

        Task<List<QuoteInfo>> IFeedHistoryProvider.QueryQuotesAsync(string symbol, Timestamp from, Timestamp to, bool level2)
        {
            return null;
        }

        List<BarData> IFeedHistoryProvider.QueryBars(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            throw new NotImplementedException();
        }

        Task<List<BarData>> IFeedHistoryProvider.QueryBarsAsync(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            throw new NotImplementedException();
        }

        List<QuoteInfo> IFeedHistoryProvider.QueryQuotes(string symbol, Timestamp from, int count, bool level2)
        {
            throw new NotImplementedException();
        }

        Task<List<QuoteInfo>> IFeedHistoryProvider.QueryQuotesAsync(string symbol, Timestamp from, int count, bool level2)
        {
            throw new NotImplementedException();
        }

        List<QuoteInfo> IFeedProvider.GetSnapshot()
        {
            return dataBySymbol.Values.Where(d => d.LastQuote != null).Select(d => d.LastQuote).ToList();
        }

        Task<List<QuoteInfo>> IFeedProvider.GetSnapshotAsync()
        {
            return Task.FromResult(dataBySymbol.Values.Where(d => d.LastQuote != null).Select(d => d.LastQuote).ToList());
        }


        List<QuoteInfo> IFeedSubscription.Modify(List<FeedSubscriptionUpdate> updates)
        {
            return null;
        }

        Task<List<QuoteInfo>> IFeedSubscription.ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            return Task.FromResult<List<QuoteInfo>>(null);
        }

        void IFeedSubscription.CancelAll()
        {
        }

        Task IFeedSubscription.CancelAllAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<SymbolInfo> GetSymbolMetadata()
        {
            return null;
        }

        public void Invoke(Action action)
        {
            action();
        }

        public void Send(Action action)
        {
            action();
        }

        public void Invoke<T>(Action<T> action, T arg)
        {
            action(arg);
        }

        public T Invoke<T>(Func<T> syncFunc)
        {
            return syncFunc();
        }

        public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
        {
            return syncFunc(args);
        }

        private class SymbolDataModel
        {
            private List<BarData> data = new List<BarData>();
            private Feed.Types.Timeframe timeFrame;
            private BarSampler sampler;

            public SymbolDataModel(Feed.Types.Timeframe timeFrame)
            {
                this.timeFrame = timeFrame;
                sampler = BarSampler.Get(timeFrame);
            }

            public QuoteInfo LastQuote { get; private set; }

            public void Fill(IEnumerable<BarData> data)
            {
                if (this.data.Count > 0)
                    throw new InvalidOperationException("Already filled!");

                this.data.AddRange(data);
            }

            public void Update(QuoteInfo quote)
            {
                var barBoundaries = sampler.GetBar(quote.Timestamp);
                var barOpenTime = barBoundaries.Open;

                if (data.Count > 0)
                {
                    var lastBar = data.Last();

                    // validate agains last bar
                    if (barOpenTime < lastBar.OpenTime)
                        return;
                    else if (barOpenTime == lastBar.OpenTime)
                    {
                        lastBar.Append(quote.Bid, 1);
                        return;
                    }
                }

                data.Add(new BarData(barOpenTime, barBoundaries.Close, quote.Bid, 1));

                LastQuote = quote;
            }

            public IEnumerable<BarData> QueryBars(Timestamp from, Timestamp to, Feed.Types.Timeframe timeframe)
            {
                if (timeframe != this.timeFrame)
                    return null;

                return data.Where(b => b.OpenTime >= from && b.OpenTime < to);
            }
        }
    }
}
