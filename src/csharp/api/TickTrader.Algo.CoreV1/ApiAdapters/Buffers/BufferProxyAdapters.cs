﻿using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class BarToQuoteAdapter : BufferProxy<BarEntity, QuoteEntity>
    {
        public BarToQuoteAdapter(InputBuffer<BarEntity> srcSeries) : base(srcSeries) { }

        protected override QuoteEntity Convert(BarEntity bar)
        {
            return new QuoteEntity(new QuoteInfo("", bar.OpenTime, bar.Open, bar.Open));
        }
    }

    public class QuoteToBarAdapter : BufferProxy<QuoteInfo, Bar>
    {
        public QuoteToBarAdapter(InputBuffer<QuoteInfo> srcSeries) : base(srcSeries) { }

        protected override Bar Convert(QuoteInfo quote)
        {
            var time = quote.Time.RoundMs();
            return new BarEntity(new BarData(time, time.AddMs(1), quote.Bid, 1));
        }
    }
}
