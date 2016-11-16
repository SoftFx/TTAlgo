using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "Symbol Info Bot")]
    public class SymbolInfoBot : TradeBot
    {
        protected override void Init()
        {
            foreach (var symbol in Symbols)
            {
                Status.WriteLine();
                Status.WriteLine(" ------------ {0} ------------", symbol.Name);

                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(symbol))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(symbol);
                    Status.WriteLine("{0}={1}", name, value);
                }
            }

            Exit();
        }
    }
}
