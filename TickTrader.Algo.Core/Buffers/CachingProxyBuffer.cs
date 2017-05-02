using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public abstract class CachingProxyBuffer<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private List<TDst> cache = new List<TDst>();
        private BuffersCoordinator coordinator;

        internal CachingProxyBuffer(InputBuffer<TSrc> srcSeries)
        {
            coordinator = srcSeries.Coordinator;
            coordinator.BuffersCleared += () => cache.Clear();

            srcSeries.ItemAppended += i => cache.Add(Convert(i));
        }

        protected abstract TDst Convert(TSrc srcItem);

        public TDst this[int index]
        {
            get { return cache[index]; }
            set { throw new NotSupportedException("Proxy buffers cannot be modified directly!"); }
        }

        public int Count { get { return cache.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }
    }

    public class BarToQuoteAdapter : CachingProxyBuffer<BarEntity, QuoteEntity>
    {
        public BarToQuoteAdapter(InputBuffer<BarEntity> srcSeries) : base(srcSeries) { }

        protected override QuoteEntity Convert(BarEntity bar)
        {
            return new QuoteEntity() { Ask = bar.Open, Bid = bar.Open };
        }
    }

    public class QuoteToBarAdapter : CachingProxyBuffer<QuoteEntity, Bar>
    {
        public QuoteToBarAdapter(InputBuffer<QuoteEntity> srcSeries) : base(srcSeries) { }

        protected override Bar Convert(QuoteEntity quote)
        {
            return new BarEntity()
            {
                OpenTime = quote.Time,
                Open = quote.Bid,
                Close = quote.Bid,
                High = quote.Bid,
                Low = quote.Bid,
                CloseTime = quote.Time,
                Volume = 1
            };
        }
    }
}
