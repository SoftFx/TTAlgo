using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Symbol Info Bot")]
    public class GetSymbolInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Symbol Name", DefaultValue = "EURUSD")]
        public string SymbolName { get; set; }


        protected override void Init()
        {
            Status.WriteLine(ToObjectPropertiesString(SymbolName, Symbols[SymbolName]));

            Exit();
        }
    }
}