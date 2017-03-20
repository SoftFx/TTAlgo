using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Currency Info Bot")]
    public class GetCurrencyInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Currency Code", DefaultValue = "USD")]
        public string CurrencyCode { get; set; }


        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString(CurrencyCode, Currencies[CurrencyCode]));

            Exit();
        }
    }
}