using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    public enum Options { Option1, Option2 }

    [TradeBot(DisplayName = "Random Subscription Bot")]
    public class TestBot : TradeBot
    {
        private SimpleMovingAverage avgIndicator;
        private Dictionary<string, Quote> snapshot = new Dictionary<string, Quote>();

        [Parameter(DisplayName = "Symbols")]
        public string CfgSymbols { get; set; }

        [Parameter(DefaultValue = 1)]
        public int Depth { get; set; }

        [Parameter]
        public Options Option { get; set; }

        protected override void Init()
        {
            avgIndicator = new SimpleMovingAverage() { Input = Bars.Low, Shift = 0, Window = 10 };
        }

        protected override void OnStart()
        {
            Print("Start!");

            Print("Symbols=" + CfgSymbols);
            Print("Depth=" + Depth);
            Print("Option=" + Option);

            foreach (var symbol in Symbols.Take(5))
            {
                Print("Subscribing for {0}", symbol.Name);
                symbol.Subscribe();
            }
        }

        protected override void OnStop()
        {
            Print("Stop!");
        }

        protected override void OnQuote(Quote quote)
        {
            snapshot[quote.Symbol] = quote;
            PrintSnapshot();
            Print("OnQuote {0}", quote.Symbol);
        }

        private void PrintSnapshot()
        {
            foreach (var pair in snapshot)
            {
                Symbol smb = Symbols[pair.Key];
                Status.WriteLine("{0} {1}/{2} [{3}]", pair.Key, pair.Value.Bid, pair.Value.Ask, smb.Digits);
            }
        }
    }

    [TradeBot(DisplayName = "Exit Bot")]
    public class ExitBot : TradeBot
    {
        protected override void OnStart()
        {
            Exit();
        }

        protected override void OnQuote(Quote quote)
        {
            Exit();
        }
    }
}
