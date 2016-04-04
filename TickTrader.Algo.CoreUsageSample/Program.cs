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
            //List<Double> results = new List<double>();
            //List<Bar> input = TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt");

            //DirectReader<Bar> reader = new DirectReader<Bar>(input);
            //reader.AddMapping("Input", b => b);

            //DirectWriter<Bar> writer = new DirectWriter<Bar>();
            //writer.AddMapping("Output", results);

            //IndicatorBuilder<Bar> builder = new IndicatorBuilder<Bar>(typeof(MovingAverage), reader, writer);
            //builder.SetParameter("Range", 10);

            //builder.Build();

            //results.ForEach(d => Console.WriteLine(d));
            //Console.Read();

            var data = TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt");

            IndicatorBuilderSlim builder = new IndicatorBuilderSlim(AlgoPluginDescriptor.Get(typeof(MovingAverage)));

            builder.GetBarSeries("EURUSD").Append(data);
            builder.MapInput<Bar>("Input1", "EURUSD");
            builder.MapInput<Bar, double>("Input2", "EURUSD", b => b.High);
            builder.SetParameter("Range", 10);

            builder.BuildNext(data.Count);

            builder.GetBarSeries("EURUSD").Last = new Bar() { High = 100 };
            builder.RebuildLast();

            var output1 = builder.GetOutput("Output1");
            var output2 = builder.GetOutput("Output2");
            for (int i = 0; i < builder.DataSize; i++)
                Console.WriteLine(output1[i] + " - " + output2[i]);

            Console.Write("Press any key to continue...");
            Console.Read();
        }
    }
}
