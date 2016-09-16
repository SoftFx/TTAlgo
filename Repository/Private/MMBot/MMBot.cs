using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot
{
    class MMBotConfiguration
    {
        public Dictionary<string, double> Syntetics { get; set; }

        public MMBotConfiguration()
        {
            Syntetics = new Dictionary<string, double>();
            Syntetics.Add("BTC-USD-EUR", 2);
        }
    }

    [TradeBot(DisplayName = "MMBot")]
    public class MMBot : TradeBot
    {
        MMBotConfiguration conf = new MMBotConfiguration();

        [Parameter(DisplayName = "Config File")]
        [FileFilter("Toml Config (*.tml)", "*.tml")]
        public File FileConfig { get; set; }

        protected override void OnStart()
        {
            this.Status.WriteLine("Input parameters");
            conf = Nett.Toml.ReadFile<MMBotConfiguration>(FileConfig.FullPath);
            foreach (KeyValuePair<string, double> currPair in conf.Syntetics)
            {
                this.Status.WriteLine("{0} - {1}", currPair.Key, currPair.Value);
                string[] currencies = currPair.Key.Split(new char[] { '-' });
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
                    //this.Feed.Subscribe(currSymbol, 5);
                    Symbols[currSymbol].Subscribe(5);
                }
            }
        }
        protected override void OnQuote(Quote quote)
        {
            Status.Write(quote.ToString());
        }


    }
}
