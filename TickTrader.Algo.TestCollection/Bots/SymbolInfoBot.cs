using System;
using System.Linq;
using TickTrader.Algo.Api;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Symbol Info Bot", Version = "2.2", Category = "Test Plugin Info", SetupMainSymbol = false,
        Description = "Prints info about current chart symbol and all symbols plugin can see to bot status window")]
    public class SymbolInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Delay (sec)", DefaultValue = 5)]
        public int ExitDelay { get; set; }

        private List<Symbol> _symbolList;

        protected override async void Init()
        {
            _symbolList = Symbols.ToList();
            _symbolList.ForEach(s => s.Subscribe());

            Status.WriteLine(string.Join(Environment.NewLine, Symbols.Select((s, i) => $"{i} - {s.Name}")));

            await Task.Delay(ExitDelay * 1000);
            PrintInfo();
        }

        private void PrintInfo()
        {
            Print(ToObjectPropertiesString("Current", Symbol));

            Print(string.Join(Environment.NewLine, new[] { "Symbols order:" }.AsEnumerable().Concat(_symbolList.Select((s, i) => $"{i + 1} - {s.Name}"))));

            foreach (var symbol in Symbols)
                Print(ToObjectPropertiesString(symbol.Name, symbol));

            Status.WriteLine("Done. Check bot logs");
            Exit();
        }
    }
}
