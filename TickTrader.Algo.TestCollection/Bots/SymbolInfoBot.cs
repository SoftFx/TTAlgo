using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Symbol Info Bot")]
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
