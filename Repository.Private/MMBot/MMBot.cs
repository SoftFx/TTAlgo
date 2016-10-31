using BotCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot
{
    public class MMBotTOMLConfiguration
    {
        Dictionary<string, double> syntetics = new Dictionary<string, double>();
        public Dictionary<string, double> Syntetics
        {
            get
            {
                return syntetics;
            }
            set
            {
                syntetics = value;
                ParsedSyntetics.Clear();
                foreach (KeyValuePair<string, double> currPair in syntetics)
                {
                    string[] currencies = currPair.Key.Split(new char[] { '-' });
                    ParsedSyntetics.Add(currencies, currPair.Value);
                }
            }
        }
        public double MarkupInPercent { get; set; }
        public bool DebugStatus{ get; set; }
        public Dictionary<string[], double> ParsedSyntetics = new Dictionary<string[], double>();
        public string BotTag { get; set; }
        public bool AutoAddVolume2PartialFill { get; set; }
        public bool AutoUpdate2TradeEvent { get; set; }

        public MMBotTOMLConfiguration()
        {
            //Dictionary<string, double>  t = new Dictionary<string, double>();
            //t.Add("BTC-USD-EUR", 2);
            //Syntetics = t;
            //MarkupInPercent = 1;
        }
    }


    [TradeBot(DisplayName = "MMBot")]
    public class MMBot : TradeBot
    {
        MMBotTOMLConfiguration conf = new MMBotTOMLConfiguration();
        EdgeWeightedDigraph sellDigraph;
        EdgeWeightedDigraph buyDigraph;
        TradeAsync tradeAsync;

        [Parameter(DisplayName = "Config File", DefaultValue = "defaultConfig.tml")]
        [FileFilter("Toml Config (*.tml)", "*.tml")]
        public File FileConfig { get; set; }

        protected override void OnStart()
        {
            OrderKeeperSettings okSettings = OrderKeeperSettings.None;
            if (conf.AutoAddVolume2PartialFill)
                okSettings |= OrderKeeperSettings.AutoAddVolume2PartialFill;
            if (conf.AutoUpdate2TradeEvent)
                okSettings |= OrderKeeperSettings.AutoUpdate2TradeEvent;
            tradeAsync = new TradeAsync(this, okSettings);

            this.Status.WriteLine("Input parameters");
            Nett.Toml.WriteFile<MMBotTOMLConfiguration>(new MMBotTOMLConfiguration(), FileConfig.FullPath + "2");
            conf = Nett.Toml.ReadFile<MMBotTOMLConfiguration>(FileConfig.FullPath);
            foreach (KeyValuePair<string[], double> currPair in conf.ParsedSyntetics)
            {
                base.Status.WriteLine(String.Join<string>("-", currPair.Key) + " = " + currPair.Value);
                string[] currencies = currPair.Key;
                for (int i = 0; i < currencies.Length - 1; i++)
                {
                    string currSymbol = string.Empty;
                    if (this.Symbols.Select(p => p.Name).Contains(currencies[i] + currencies[i + 1]))
                        currSymbol = currencies[i] + currencies[i + 1];
                    if (this.Symbols.Select(p => p.Name).Contains(currencies[i + 1] + currencies[i]))
                        currSymbol = currencies[i + 1] + currencies[i];
                    if (currSymbol == string.Empty)
                        Print("There are not symbol with currencies {0} and {1}.", currencies[i], currencies[i + 1]);
                    Print("Subscribing to " + currSymbol);
                    Symbols[currSymbol].Subscribe(0);
                }
            }
            IEnumerable<string> allCurrencies = Symbols.Select(o => o.BaseCurrency).Union<string>(Symbols.Select(o => o.CounterCurrency)).Distinct<string>();
            sellDigraph = new EdgeWeightedDigraph(allCurrencies);
            buyDigraph = new EdgeWeightedDigraph(allCurrencies);

            base.Status.WriteLine("MarkupinPercent is " + conf.MarkupInPercent);
        }
        protected override void OnQuote(Quote quote)
        {
            string from = Symbols[quote.Symbol].BaseCurrency;
            string to = Symbols[quote.Symbol].CounterCurrency;

            sellDigraph.AddEdge(new DirectEdge(from, to, new BookSide(quote.BidBook )));
            sellDigraph.AddEdge(new DirectEdge(to, from, new BookSide(quote.AskBook, true)));
            buyDigraph.AddEdge(new DirectEdge(from, to, new BookSide(quote.AskBook)));
            buyDigraph.AddEdge(new DirectEdge(to, from, new BookSide(quote.BidBook, true)));

            Status.WriteLine("SELL (buy limit)");
            if( conf.DebugStatus )
                Status.WriteLine(sellDigraph.ToString());
            foreach (KeyValuePair<string[], double> currPair in conf.ParsedSyntetics)
            {
                string[] currencies = currPair.Key;
                string synteticSymbol = currencies[0] + currencies.Last();
                double reqVolume = currPair.Value * this.Symbols[synteticSymbol].ContractSize;
                double availableVolume;

                double cumPrice = CalculateSynteticVolumePrice(sellDigraph, currencies, reqVolume, out availableVolume);
                if (cumPrice == double.NaN)
                    cumPrice = cumPrice*1;
                cumPrice *= (100 - conf.MarkupInPercent) / 100;
                cumPrice = Math.Round(cumPrice, Symbols[synteticSymbol].Digits);
                Status.WriteLine(synteticSymbol + "=" + cumPrice + " " + availableVolume);

                tradeAsync.SetLimitOrderAsync(synteticSymbol, OrderSide.Buy, availableVolume, cumPrice, conf.BotTag + synteticSymbol, conf.BotTag + synteticSymbol);
            }

            Status.WriteLine("BUY (sell limit)");
            if (conf.DebugStatus)
                Status.WriteLine(buyDigraph.ToString());
            foreach (KeyValuePair<string[], double> currPair in conf.ParsedSyntetics)
            {
                string[] currencies = currPair.Key;
                string synteticSymbol = currencies[0] + currencies.Last();
                double reqVolume = currPair.Value * this.Symbols[synteticSymbol].ContractSize;
                double availableVolume;

                double cumPrice = CalculateSynteticVolumePrice(buyDigraph, currencies, reqVolume, out availableVolume);
                cumPrice *= (100 + conf.MarkupInPercent) / 100;
                cumPrice = Math.Round(cumPrice, Symbols[synteticSymbol].Digits);
                Status.WriteLine(synteticSymbol + "=" + cumPrice + " " + availableVolume);

                tradeAsync.SetLimitOrderAsync(synteticSymbol, OrderSide.Sell, availableVolume, cumPrice, conf.BotTag+synteticSymbol, conf.BotTag + synteticSymbol);
            }
        }

        double CalculateSynteticVolumePrice(EdgeWeightedDigraph digraph, string[] currencies, double reqVolume, out double availableVolume)
        {
            double cumPrice = 1;
            double convertedVolume = reqVolume;
            for (int i = 0; i < currencies.Length - 1; i++)
            {
                double priceOrVolume = digraph.GetEdge(currencies[i], currencies[i + 1]).Weight.GetPriceForVolume(convertedVolume);
                if (priceOrVolume < 0) // not enough volume for cross pair. Lets reduce required Volume
                {
                    double newRequestedVolume = Math.Floor(100*Math.Exp(Math.Floor( Math.Log(reqVolume * -priceOrVolume ))))/100;
                    return CalculateSynteticVolumePrice(digraph, currencies, newRequestedVolume, out availableVolume);
                }

                convertedVolume *= priceOrVolume;
                cumPrice *= priceOrVolume;
            }
            availableVolume = reqVolume;
            return cumPrice;
        }

    }
}
