using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Get Symbol Info Bot", Version = "1.1", Category = "Test Plugin Info",
        SetupMainSymbol = false, Description = "Prints info about symbol with specified name to bot status window")]
    public class GetSymbolInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Symbol Name", DefaultValue = "EURUSD")]
        public string SymbolName { get; set; }

        [Parameter(DisplayName = "Delay (sec)", DefaultValue = 1)]
        public int ExitDelay { get; set; }

        protected override async void Init()
        {
            if (Symbol.Name != SymbolName)
            {
                Symbol.Unsubscribe();
                Symbols[SymbolName].Subscribe();
            }

            await Task.Delay(ExitDelay * 1000);
            PrintInfo();
        }


        protected override void OnQuote(Quote quote)
        {
            PrintInfo();
        }

        private void PrintInfo()
        {
            Status.WriteLine(ToObjectPropertiesString(SymbolName, Symbols[SymbolName]));
            Exit();
        }
    }
}