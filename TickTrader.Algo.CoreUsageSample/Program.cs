using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.CoreUsageSample
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Double> results = new List<double>();
            List<Bar> input = TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt");

            DirectReader<Bar> reader = new DirectReader<Bar>(input);
            reader.AddMapping("Input", b => b);

            DirectWriter<Bar> writer = new DirectWriter<Bar>();
            writer.AddMapping("Output", results);

            IndicatorBuilder<Api.Bar> builder = new IndicatorBuilder<Bar>(typeof(MovingAverage), reader, writer);
            builder.SetParameter("Range", 10);

            builder.Build();

            results.ForEach(d => Console.WriteLine(d));
            Console.Read();
        }
    }
}
