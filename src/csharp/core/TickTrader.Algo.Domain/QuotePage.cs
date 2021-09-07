using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class QuotePage
    {
        public static QuotePage Create(IEnumerable<QuoteInfo> quotes)
        {
            var page = new QuotePage();
            page.Quotes.Add(quotes.Select(q => q.GetFullQuote()));
            return page;
        }
    }
}
