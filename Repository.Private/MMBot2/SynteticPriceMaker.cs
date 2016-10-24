using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot2
{
    internal class SynteticPriceMaker
    {
        private TradeBot api;
        private double priceMarkupInPercent;
        private string[] currencies;
        private double reqVolume;
        private OrderKeeper sellOrder;
        private OrderKeeper buyOrder;
        private List<string> initErros = new List<string>();
        private Symbol synteticSymbol;
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
            this.currencies = currencies;

            synteticSymbol = api.Symbols.GetByCurrencies(currencies[0], currencies.Last());
            if (synteticSymbol == null)
                initErros.Add(string.Format("There are not symbol with currencies {0} and {1}.", currencies[0], currencies.Last()));

            for (int i = 0; i < currencies.Length - 1; i++)
            {
                string currSymbol = string.Empty;
                if (api.Symbols.ExistByCurrencies(currencies[i], currencies[i + 1]))
                {
                    var smb = api.Symbols.GetByCurrencies(currencies[i], currencies[i + 1]);
                    AddEdge(smb.Name, false, observers);
                }
                else if (api.Symbols.ExistByCurrencies(currencies[i + 1], currencies[i]))
                {
                    var smb = api.Symbols.GetByCurrencies(currencies[i + 1], currencies[i]);
                    AddEdge(smb.Name, true, observers);
                }
                else
                    initErros.Add(string.Format("There are not symbol with currencies {0} and {1}.", currencies[i], currencies[i + 1]));
            }

            if (synteticSymbol != null)
            {
                sellOrder = new OrderKeeper(api, synteticSymbol, OrderSide.Sell, synteticSymbol.Name + reqVolume.ToString());
                buyOrder = new OrderKeeper(api, synteticSymbol, OrderSide.Buy, synteticSymbol.Name + reqVolume.ToString());
            }
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

            sellSyntPrice = ApplySellMarkup(CalculateSynteticVolumePrice(reqVolume, OrderSide.Buy, out sellSyntVolume));
            buySyntPrice = ApplyBuyMarkup(CalculateSynteticVolumePrice(reqVolume, OrderSide.Sell, out buySyntVolume));

            if (sellSyntPrice != null)
                buyOrder.SetTarget(sellSyntVolume, sellSyntPrice.Value);

            if (buySyntPrice != null)
                sellOrder.SetTarget(buySyntVolume, buySyntPrice.Value);
        }

        private double? ApplySellMarkup(double? price)
        {
            if (price == null)
                return null;
            return Math.Round(price.Value * (100 - priceMarkupInPercent) / 100, synteticSymbol.Digits);
        }

        private double? ApplyBuyMarkup(double? price)
        {
            if (price == null)
                return null;
            return Math.Round(price.Value * (100 + priceMarkupInPercent) / 100, synteticSymbol.Digits);
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

        public void PrintState()
        {
            api.Status.WriteLine();
            api.Status.WriteLine("{0} : {1}", string.Join("-", currencies), reqVolume);

            if (initErros.Count > 0)
            {
                foreach (string error in initErros)
                    api.Status.WriteLine(error);
                return;
            }

            api.Status.WriteLine("SELL (buy limit)");
            foreach (Edge edge in edges)
                api.Status.WriteLine(edge.PrintAsk());
            if (sellSyntPrice != null)
                api.Status.WriteLine("{0} = {1} {2}", synteticSymbol.Name, sellSyntPrice, sellSyntVolume);
            else
                api.Status.WriteLine("Off quotes");

            api.Status.WriteLine("BUY (sell limit)");
            foreach (Edge edge in edges)
                api.Status.WriteLine(edge.PrintBid());
            if (buySyntPrice != null)
                api.Status.WriteLine("{0} = {1} {2}", synteticSymbol.Name, buySyntPrice, buySyntVolume);
            else
                api.Status.WriteLine("Off quotes");
        }
    }
}
