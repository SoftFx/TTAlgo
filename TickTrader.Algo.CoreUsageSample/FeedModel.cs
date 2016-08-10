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
    internal class FeedModel : IPluginFeedProvider
    {
        private event Action<FeedUpdate[]> FeedUpdated = delegate { };
        private Dictionary<string, SymbolDataModel> dataBySymbol = new Dictionary<string, SymbolDataModel>();

        public TimeFrames TimeFrame { get; private set; }

        public FeedModel(TimeFrames timeFrame)
        {
            this.TimeFrame = timeFrame;
        }

        event Action<FeedUpdate[]> IPluginFeedProvider.FeedUpdated { add { FeedUpdated += value; } remove { FeedUpdated -= value; } }

        public void Fill(string symbol, IEnumerable<BarEntity> data)
        {
            GetSymbolData(symbol).Fill(data);
        }

        public void Update(string symbolCode, QuoteEntity quote)
        {
            Update(new FeedUpdate(symbolCode, quote));
        }

        public void Update(FeedUpdate update)
        {
            GetSymbolData(update.SymbolCode).Update(update.Quote);
            FeedUpdated(new FeedUpdate[] { update });
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

        IEnumerable<BarEntity> IPluginFeedProvider.CustomQueryBars(string symbolCode, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return GetSymbolData(symbolCode).QueryBars(from, to, timeFrame);
        }


        IEnumerable<QuoteEntity> IPluginFeedProvider.CustomQueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
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
                        lastBar.Append(quote.Bid);
                        return;
                    }
                }

                data.Add(new BarEntity(barOpenTime, barBoundaries.Close, quote));
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
