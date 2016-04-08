using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.DataflowConcept;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.CoreUsageSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt");

            IndicatorBuilder builder = new IndicatorBuilder(AlgoPluginDescriptor.Get(typeof(Alligator)));

            //builder.Account.Orders.Add(new OrderEntity(10) { Symbol = "EURUSD", TotalAmount = 5000, RemainingAmount = 5000 });

            builder.MainSymbol = "EURUSD";
            builder.GetBarSeries("EURUSD").Append(data);
            builder.MapBarInput("Input", "EURUSD");

            builder.BuildNext(data.Count);

            //builder.Account.Orders.Add(new OrderEntity(11) { Symbol = "EURUSD", TotalAmount = 6000, RemainingAmount = 6000 });

            builder.GetBarSeries("EURUSD").Last = new BarEntity() { High = 100 };
            builder.RebuildLast();

            var jaws = builder.GetOutput("Jaws");
            var teeth = builder.GetOutput("Teeth");
            var lips = builder.GetOutput("Lips");

            for (int i = 0; i < builder.DataSize; i++)
                Console.WriteLine(jaws[i] + " - " + teeth[i] + " - " + lips[i]);

            Console.Write("Press any key to continue...");      
            Console.Read();
        }
    }
}
