using System;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Symbol Info Bot", Version = "2.2", Category = "Test Plugin Info",
        Description = "Prints info about current chart symbol and all symbols plugin can see to bot status window")]
    public class SymbolInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            Print(ToObjectPropertiesString("Current", typeof(Symbol), Symbol));

            var symbolList = Symbols.ToList();
            Print(string.Join(Environment.NewLine, new[] { "Symbols order:" }.AsEnumerable().Concat(symbolList.Select((s, i) => $"{i + 1} - {s.Name}"))));

            foreach (var symbol in Symbols)
                Print(ToObjectPropertiesString(symbol.Name, typeof(Symbol), symbol));

            Status.WriteLine("Done. Check bot logs");

            Exit();
        }
    }
}
