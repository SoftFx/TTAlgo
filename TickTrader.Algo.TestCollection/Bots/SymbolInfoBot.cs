using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Symbol Info Bot")]
    public class SymbolInfoBot : TradeBot
    {
        protected override void Init()
        {
            PrintSymbol("Current", Symbol);

            foreach (var symbol in Symbols)
                PrintSymbol(symbol.Name, symbol);

            Exit();
        }

        private void PrintSymbol(string name, Symbol symbol)
        {
            Status.WriteLine();
            Status.WriteLine(" ------------ {0} ------------", name);

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(symbol))
            {
                string pNname = descriptor.Name;
                object pValue = descriptor.GetValue(symbol);
                Status.WriteLine("{0}={1}", pNname, pValue);
            }
        }
    }
}
