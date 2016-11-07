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
    internal class FeedModel : IBarBasedFeed, ISynchronizationContext
    {
        private event Action<QuoteEntity[]> FeedUpdated = delegate { };
        private Dictionary<string, SymbolDataModel> dataBySymbol = new Dictionary<string, SymbolDataModel>();

        public TimeFrames TimeFrame { get; private set; }

        public ISynchronizationContext Sync { get { return this; } }

        public FeedModel(TimeFrames timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        event Action<QuoteEntity[]> IPluginFeedProvider.FeedUpdated { add { FeedUpdated += value; } remove { FeedUpdated -= value; } }

        public void Fill(string symbol, IEnumerable<BarEntity> data)
        {
            GetSymbolData(symbol).Fill(data);
        }


        public void Update(QuoteEntity update)
        {
            GetSymbolData(update.Symbol).Update(update);
            FeedUpdated(new QuoteEntity[] { update });
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

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return GetSymbolData(symbolCode).QueryBars(from, to, timeFrame).ToList();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            return null;
        }

        void IPluginFeedProvider.Subscribe(string symbolCode, int depth)
        {
        }

        void IPluginFeedProvider.Unsubscribe(string symbolCode)
        {
        }

        public IEnumerable<SymbolEntity> GetSymbolMetadata()
        {
            return null;
        }

        public void Invoke(Action action)
        {
        }

        public List<BarEntity> GetMainSeries()
        {
            throw new NotImplementedException();
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
