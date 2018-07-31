using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Math;

namespace TickTrader.Algo.CoreUsageSample
{
    internal class FeedModel : IPluginFeedProvider, ISynchronizationContext
    {
        private Action<QuoteEntity[]> FeedUpdated;
        private Dictionary<string, SymbolDataModel> dataBySymbol = new Dictionary<string, SymbolDataModel>();

        public TimeFrames TimeFrame { get; private set; }

        public ISynchronizationContext Sync { get { return this; } }

        public FeedModel(TimeFrames timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        public void Fill(string symbol, IEnumerable<BarEntity> data)
        {
            GetSymbolData(symbol).Fill(data);
        }

        public void Update(QuoteEntity update)
        {
            GetSymbolData(update.Symbol).Update(update);
            FeedUpdated?.Invoke(new QuoteEntity[] { update });
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

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return GetSymbolData(symbolCode).QueryBars(from, to, timeFrame).ToList();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, bool level2)
        {
            return null;
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, BarPriceType priceType, DateTime from, int size, TimeFrames timeFrame)
        {
            throw new NotImplementedException();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, int count, bool level2)
        {
            throw new NotImplementedException();
        }

        IEnumerable<QuoteEntity> IPluginFeedProvider.GetSnapshot()
        {
            return dataBySymbol.Values.Where(d => d.LastQuote != null).Select(d => d.LastQuote).ToList();
        }

        void IPluginFeedProvider.SetSymbolDepth(string symbolCode, int depth)
        {
        }

        void IPluginFeedProvider.Subscribe(Action<QuoteEntity[]> FeedUpdated)
        {
            this.FeedUpdated = FeedUpdated;
        }

        void IPluginFeedProvider.Unsubscribe()
        {
            this.FeedUpdated = null;
        }

        public IEnumerable<SymbolEntity> GetSymbolMetadata()
        {
            return null;
        }

        public void Invoke(Action action)
        {
        }

        private class SymbolDataModel
        {
            private List<BarEntity> data = new List<BarEntity>();
            private TimeFrames timeFrame;
            private BarSampler sampler;

            public SymbolDataModel(TimeFrames timeFrame)
            {
                this.timeFrame = timeFrame;
                sampler = BarSampler.Get(timeFrame);
            }

            public QuoteEntity LastQuote { get; private set; }

            public void Fill(IEnumerable<BarEntity> data)
            {
                if (this.data.Count > 0)
                    throw new InvalidOperationException("Already filled!");

                this.data.AddRange(data);
            }

            public void Update(QuoteEntity quote)
            {
                var barBoundaries = sampler.GetBar(quote.Time);
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

                data.Add(new BarEntity(barOpenTime, barBoundaries.Close, quote.Bid, 1));

                LastQuote = quote;
            }

            public IEnumerable<BarEntity> QueryBars(DateTime from, DateTime to, TimeFrames timeFrame)
            {
                if (timeFrame != this.timeFrame)
                    return null;

                return data.Where(b => b.OpenTime >= from && b.OpenTime < to);
            }
        }
    }
}
