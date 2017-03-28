using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Asset Info Bot")]
    public class GetAssetInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Currency Code", DefaultValue = "USD")]
        public string CurrencyCode { get; set; }


        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString(CurrencyCode, Account.Assets[CurrencyCode]));

            Exit();
        }
    }
}