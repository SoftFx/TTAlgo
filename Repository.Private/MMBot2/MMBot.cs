using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot2
{
    [TradeBot(DisplayName = "MMBot2 (Async)")]
    public class MMBot : TradeBot
    {
        MMBotTOMLConfiguration conf = new MMBotTOMLConfiguration();
        Dictionary<string, PriceObserver> observers = new Dictionary<string, PriceObserver>();
        List<SynteticPriceMaker> priceMakers = new List<SynteticPriceMaker>();

        [Parameter(DisplayName = "Config File", DefaultValue = "defaultConfig.tml")]
        [FileFilter("Toml Config (*.tml)", "*.tml")]
        public File FileConfig { get; set; }

        protected override void OnStart()
        {
            Nett.Toml.WriteFile<MMBotTOMLConfiguration>(new MMBotTOMLConfiguration(), FileConfig.FullPath + "2");
            conf = Nett.Toml.ReadFile<MMBotTOMLConfiguration>(FileConfig.FullPath);

            foreach (var currPair in conf.ParsedSyntetics)
                priceMakers.Add(new SynteticPriceMaker(this, currPair.Key, currPair.Value, conf.MarkupInPercent, observers));

            foreach (var entry in observers)
            {
                Print("Subscribing to " + entry.Key);
                Symbols[entry.Key].Subscribe(0);
            }

            PrintState();   
        }

        protected override void OnQuote(Quote quote)
        {
            var observer = observers.GetOrDefault(quote.Symbol);
            if (observer != null)
            {
                observer.Update(quote);
                PrintState();
            }
        }

        private void PrintState()
        {
            base.Status.WriteLine("MarkupinPercent=" + conf.MarkupInPercent);

            foreach (SynteticPriceMaker maker in priceMakers)
                maker.PrintState();
        }
    }
}
