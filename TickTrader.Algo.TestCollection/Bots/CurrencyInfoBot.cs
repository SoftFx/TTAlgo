using System.ComponentModel;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Currency Info Bot")]
    public class CurrencyInfoBot : TradeBot
    {
        protected override void Init()
        {
            foreach (var currency in Currencies)
                PrintCurrency(currency.Name, currency);

            Exit();
        }

        private void PrintCurrency(string name, Currency currency)
        {
            Status.WriteLine();
            Status.WriteLine(" ------------ {0} ------------", name);

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(currency))
            {
                string pNname = descriptor.Name;
                object pValue = descriptor.GetValue(currency);
                Status.WriteLine("{0}={1}", pNname, pValue);
            }
        }
    }
}