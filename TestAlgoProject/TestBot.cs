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
        private StringBuilder statusBuilder = new StringBuilder();

        [Parameter(DisplayName = "Symbols")]
        public string CfgSymbols { get; set; }

        [Parameter(DefaultValue = 1)]
        public int Depth { get; set; }

        [Parameter]
        public Options Option { get; set; }

        protected override void Init()
        {
            avgIndicator = new SimpleMovingAverage() { Input = this.MarketSeries.Bars.Low, Shift = 0, Window = 10 };
        }

        protected override void OnStart()
        {
            Print("Start!");

            Print("Symbols=" + CfgSymbols);
            Print("Depth=" + Depth);
            Print("Option=" + Option);

            foreach (var symbol in Symbols.Take(5))
            {
                Print("Subscribing for {0}", symbol.Code);
                symbol.Subscribe();
            }
        }

        protected override void OnStop()
        {
            Print("Stop!");
        }

        protected override void OnQuote(Quote quote)
        {
            snapshot[quote.SymbolCode] = quote;
            PrintSnapshot();
            //Print("OnQuote {0}", quote.SymbolCode);
        }

        private void PrintSnapshot()
        {
            statusBuilder.Clear();

            foreach (var pair in snapshot)
            {
                var smb = Symbols[pair.Key];

                statusBuilder.Append(pair.Key).Append(" ").Append(pair.Value.Bid).Append("/").Append(pair.Value.Ask)
                    .Append('[').Append(smb.Digits).Append(']').AppendLine();
            }

            UpdateStatus(statusBuilder.ToString());
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

    [TradeBot(DisplayName = "Exception Bot")]
    public class ExceptionBot : TradeBot
    {
        protected override void Init()
        {
            throw new Exception("Fail!");
        }

        protected override void OnStart()
        {
            throw new Exception("Fail!");

        }

        protected override void OnStop()
        {
            throw new Exception("Fail!");
        }

        protected override void OnQuote(Quote quote)
        {
            
        }
    }
}
