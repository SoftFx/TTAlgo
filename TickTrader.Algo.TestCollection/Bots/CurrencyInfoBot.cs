using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Currency Info Bot")]
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