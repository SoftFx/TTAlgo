using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Quote Print Bot", Version = "1.0", Category = "Test Plugin Info",
       Description = "Prints every recieved quote to log.")]
    public class QuotePrintBot : TradeBot
    {
        private StringBuilder _builder = new StringBuilder();

        [Parameter]
        public QuotePrintModes Mode { get; set; }

        protected override void Init()
        {
            if (Mode == QuotePrintModes.AllSymbols)
            {
                foreach (var smb in Symbols)
                {
                    Print("Subscribing for " + smb.Name);
                    smb.Subscribe();
                }
            }
        }

        protected override void OnQuote(Quote quote)
        {
            var smb = Symbols[quote.Symbol];
            var format = new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = smb.Digits };

            var bidStr = quote.Bid.ToString("N", format);
            var askStr = quote.Ask.ToString("N", format);

            _builder.Clear();
            _builder.Append(quote.Symbol)
                .Append(' ').Append(quote.Time)
                .Append(' ').Append(askStr)
                .Append('/').Append(bidStr);

            Print(_builder.ToString());
        }
    }

    public enum QuotePrintModes { CurrentSymbol, AllSymbols }
}
