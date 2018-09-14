using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Asset Info Bot", Version = "1.1", Category = "Test Plugin Info",
        SetupMainSymbol = false, Description = "Prints info about asset with specified currency code to bot status window")]
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