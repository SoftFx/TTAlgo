using System;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Currency Info Bot", Version = "2.2", Category = "Test Plugin Info",
        SetupMainSymbol = false, Description = "Prints info about all currencies plugin can see to bot status window")]
    public class CurrencyInfoBot : TradeBotCommon
    {
        protected override void Init()
        {
            var currenciesList = Currencies.ToList();
            Print(string.Join(Environment.NewLine, new[] { "Currencies order:" }.AsEnumerable().Concat(currenciesList.Select((s, i) => $"{i + 1} - {s.Name}"))));

            foreach (var currency in Currencies)
                Print(ToObjectPropertiesString(currency.Name, typeof(Currency), currency));

            Status.WriteLine("Done. Check bot logs");

            Exit();
        }
    }
}