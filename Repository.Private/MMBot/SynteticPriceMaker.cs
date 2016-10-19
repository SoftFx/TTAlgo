using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot
{
    internal class SynteticPriceMaker
    {
        private TradeBot api;
        private double priceMarkupInPercent;
        private OrderKeeper sellOrder;
        private OrderKeeper buyOrder;
        private List<string> initErros = new List<string>();
        private Symbol synteticSymbol;
        private double reqVolume;
        private List<Edge> edges = new List<Edge>();
        private double sellSyntVolume;
        private double buySyntVolume;
        private double? sellSyntPrice;
        private double? buySyntPrice;

        public SynteticPriceMaker(TradeBot api, string[] currencies, double reqVolume, double priceMarkupInPercent,
            Dictionary<string, PriceObserver> observers)
        {
            this.api = api;
            this.priceMarkupInPercent = priceMarkupInPercent;
            this.reqVolume = reqVolume;

            synteticSymbol = api.Symbols[currencies[0] + currencies.Last()];
            if (synteticSymbol.IsNull)
                initErros.Add(string.Format("There are not symbol with currencies {0} and {1}.", currencies[0], currencies.Last()));

            for (int i = 0; i < currencies.Length - 1; i++)
            {
                string currSymbol = string.Empty;
                if (api.Symbols.Select(p => p.Name).Contains(currencies[i] + currencies[i + 1]))
                    AddEdge(currencies[i] + currencies[i + 1], false, observers);
                else if (api.Symbols.Select(p => p.Name).Contains(currencies[i + 1] + currencies[i]))
                    AddEdge(currencies[i + 1] + currencies[i], true, observers);
                else
                    initErros.Add(string.Format("There are not symbol with currencies {0} and {1}.", currencies[i], currencies[i + 1]));
            }

            sellOrder = new OrderKeeper(api, synteticSymbol, OrderSide.Sell, synteticSymbol.Name + reqVolume.ToString());
            buyOrder = new OrderKeeper(api, synteticSymbol, OrderSide.Buy, synteticSymbol.Name + reqVolume.ToString());
        }

        private void AddEdge(string symbol, bool reversed, Dictionary<string, PriceObserver> observers)
        {
            var observer = observers.GetOrAdd(symbol);
            edges.Add(new Edge(symbol, observer));
            observer.Changed += Recalculate;
        }

        private void Recalculate()
        {
            if (initErros.Count > 0)
                return;

            sellSyntPrice = ApplyMarkup(CalculateSynteticVolumePrice(reqVolume, OrderSide.Buy, out sellSyntVolume));
            buySyntPrice = ApplyMarkup(CalculateSynteticVolumePrice(reqVolume, OrderSide.Buy, out buySyntVolume));
        }

        private double? ApplyMarkup(double? price)
        {
            if (price == null)
                return null;
            return Math.Round(price.Value * (100 - priceMarkupInPercent) / 100, synteticSymbol.Digits);
        }

        private double? CalculateSynteticVolumePrice(double reqVolume, OrderSide side, out double availableVolume)
        {
            double cumPrice = 1;
            double convertedVolume = reqVolume;

            foreach (Edge edge in edges)
            {
                if (!edge.HasPrice)
                {
                    availableVolume = reqVolume;
                    return null;
                }

                double priceOrVolume = edge.GetPriceForVolume(convertedVolume, side);
                if (priceOrVolume < 0) // not enough volume for cross pair. Lets reduce required Volume
                {
                    double newRequestedVolume = Math.Floor(Math.Exp(Math.Floor(Math.Log(reqVolume * -priceOrVolume * 100)))) / 100;
                    return CalculateSynteticVolumePrice(newRequestedVolume, side, out availableVolume);
                }

                convertedVolume *= priceOrVolume;
                cumPrice *= priceOrVolume;
            }

            availableVolume = reqVolume;
            return cumPrice;
        }

        private double? CalculateSynteticVolumePrice(EdgeWeightedDigraph digraph, string[] currencies, double reqVolume, out double availableVolume)
        {
            double cumPrice = 1;
            double convertedVolume = reqVolume;
            for (int i = 0; i < currencies.Length - 1; i++)
            {
                double priceOrVolume = digraph.GetEdge(currencies[i], currencies[i + 1]).Weight.GetPriceForVolume(convertedVolume);
                if (priceOrVolume < 0) // not enough volume for cross pair. Lets reduce required Volume
                {
                    double newRequestedVolume = Math.Floor(Math.Exp(Math.Floor(Math.Log(reqVolume * -priceOrVolume * 100)))) / 100;
                    return CalculateSynteticVolumePrice(digraph, currencies, newRequestedVolume, out availableVolume);
                }

                convertedVolume *= priceOrVolume;
                cumPrice *= priceOrVolume;
            }
            availableVolume = reqVolume;
            return cumPrice;
        }

        public void PrintState()
        {
        }

        internal class Edge
        {
            public Edge(string symbol, PriceObserver observer)
            {
                this.Observer = observer;
            }

            public PriceObserver Observer { get; private set; }
            public bool IsReversed { get; private set; }
            public bool HasPrice { get { return Observer.HasPrice; } }

            /// <summary>
            /// Return vwap price for requested volume or if it is negatice coef to reduce requested volume
            /// </summary>
            /// <param name="requiredVolume"></param>
            /// <returns></returns>
            public double GetPriceForVolume(double requiredVolume, OrderSide side)
            {
                if (side == OrderSide.Buy)
                    return GetPriceForVolume(requiredVolume, Observer.AskBook);
                else
                    return GetPriceForVolume(requiredVolume, Observer.BidBook);
            }

            private double GetPriceForVolume(double requiredVolume, BookEntry[] book)
            {
                if (IsReversed)
                {
                    var reversedEntries = book.Reverse().Select(e => new BookEntryEntity(1 / e.Price, e.Volume * e.Price));
                    return GetPriceForVolume(requiredVolume, reversedEntries);
                }
                else
                    return GetPriceForVolume(requiredVolume, book);
            }

            private double GetPriceForVolume(double requiredVolume, IEnumerable<BookEntry> entries)
            {
                double leftVolume = requiredVolume;
                double numerator = 0;
                foreach (BookEntry currEntry in entries)
                {
                    if (leftVolume >= currEntry.Volume)
                    {
                        leftVolume -= currEntry.Volume;
                        numerator += currEntry.Volume * currEntry.Price;
                    }
                    else
                    {
                        numerator += leftVolume * currEntry.Price;
                        leftVolume = 0;
                    }
                }
                if (leftVolume > float.Epsilon)
                    return leftVolume / requiredVolume - 1;
                return numerator / requiredVolume;
            }

            //public override string ToString()
            //{
            //    return string.Format("{0}-{1}={2}", From, To, Weight.ToString());
            //}
        }
    }
}
