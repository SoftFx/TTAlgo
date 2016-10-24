using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot2
{
    internal class PriceObserver
    {
        public BookEntry[] AskBook { get; private set; }
        public BookEntry[] BidBook { get; private set; }
        public bool HasPrice { get; private set; }

        public void Update(Quote quote)
        {
            AskBook = quote.AskBook;
            BidBook = quote.BidBook;
            HasPrice = true;
            Changed();
        }

        public event Action Changed = delegate { };
    }

    internal class BookEntryEntity : BookEntry
    {
        public BookEntryEntity(double price, double volume)
        {
            this.Price = price;
            this.Volume = volume;
        }

        public double Price { get; private set; }
        public double Volume { get; private set; }
    }
}
