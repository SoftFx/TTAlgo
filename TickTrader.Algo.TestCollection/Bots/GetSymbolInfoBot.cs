using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Symbol Info Bot", Version = "1.1", Category = "Test Plugin Info",
        Description = "Prints info about symbol with specified name to bot status window")]
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