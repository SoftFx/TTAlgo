using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Symbol Info Bot", Version = "2.1", Category = "Test Plugin Info",
        Description = "Prints info about current chart symbol and all symbols plugin can see to bot status window")]
    public class SymbolInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString("Current", typeof(Symbol), Symbol));

            foreach (var symbol in Symbols)
                Status.WriteLine(ToObjectPropertiesString(symbol.Name, typeof(Symbol), symbol));

            Exit();
        }
    }
}
