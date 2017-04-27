using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Symbol Info Bot", Version = "2.0", Category = "Test Plugin Info",
        Description = "Prints info about current chart symbol and all symbols plugin can see to bot status window")]
    public class SymbolInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString("Current", Symbol));

            foreach (var symbol in Symbols)
                Status.WriteLine(ToObjectPropertiesString(symbol.Name, symbol));

            Exit();
        }
    }
}
