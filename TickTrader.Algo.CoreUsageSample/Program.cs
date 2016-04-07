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

            IndicatorBuilder builder = new IndicatorBuilder(AlgoPluginDescriptor.Get(typeof(MovingAverage)));

            builder.Account.Orders.Add(new OrderEntity(10) { Symbol = "EURUSD", TotalAmount = 5000, RemainingAmount = 5000 });

            builder.MainSymbol = "EURUSD";
            builder.GetBarSeries("EURUSD").Append(data);
            builder.MapBarInput("Input1", "EURUSD");
            builder.MapBarInput("Input2", "EURUSD", b => b.High);
            builder.SetParameter("Range", 10);

            builder.BuildNext(data.Count);

            builder.Account.Orders.Add(new OrderEntity(11) { Symbol = "EURUSD", TotalAmount = 6000, RemainingAmount = 6000 });

            builder.GetBarSeries("EURUSD").Last = new BarEntity() { High = 100 };
            builder.RebuildLast();

            var output1 = builder.GetOutput("Output1");
            var output2 = builder.GetOutput("Output2");
            var output3 = builder.GetOutput("Output3");

            for (int i = 0; i < builder.DataSize; i++)
                Console.WriteLine(output1[i] + " - " + output2[i] + " - " + output3[i]);

            Console.Write("Press any key to continue...");
            Console.Read();
        }
    }
}
