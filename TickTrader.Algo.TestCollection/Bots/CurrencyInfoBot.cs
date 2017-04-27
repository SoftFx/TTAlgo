using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Currency Info Bot", Version = "2.0", Category = "Test Plugin Info",
        Description = "Prints info about all currencies plugin can see to bot status window")]
    public class CurrencyInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            foreach (var currency in Currencies)
                Status.WriteLine(ToObjectPropertiesString(currency.Name, currency));

            Exit();
        }
    }
}