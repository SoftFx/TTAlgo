using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public abstract class BufferProxy<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private BuffersCoordinator _coordinator;
        private InputBuffer<TSrc> _srcSeries;

        internal BufferProxy(InputBuffer<TSrc> srcSeries)
        {
            _coordinator = srcSeries.Coordinator;
            _srcSeries = srcSeries;
        }

        protected abstract TDst Convert(TSrc srcItem);

        public TDst this[int index]
        {
            get { return Convert(_srcSeries[index]); }
            set { throw new NotSupportedException("Proxy buffers cannot be modified directly!"); }
        }

        public int Count { get { return _srcSeries.Count; } }
        public int VirtualPos { get { return _coordinator.VirtualPos; } }
    }

    public class BarToQuoteAdapter : BufferProxy<BarEntity, QuoteEntity>
    {
        public BarToQuoteAdapter(InputBuffer<BarEntity> srcSeries) : base(srcSeries) { }

        protected override QuoteEntity Convert(BarEntity bar)
        {
            return new QuoteEntity(new QuoteInfo("", bar.OpenTime, bar.Open, bar.Open));
        }
    }

    public class QuoteToBarAdapter : BufferProxy<QuoteEntity, Bar>
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
