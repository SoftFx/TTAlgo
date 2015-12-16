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

            using (var file = System.IO.File.OpenText("EURUSD-M1-bids.txt"))
            {
                StreamReader<Bar> reader = new StreamReader<Bar>(new TTQuoteFileReader(file));
                reader.AddMapping("Input", b => b);

                DirectWriter<Bar> writer = new DirectWriter<Bar>();
                writer.AddMapping("Output", results);

                IndicatorBuilder<Api.Bar> builder = new IndicatorBuilder<Bar>(typeof(MovingAverage), reader, writer);
                builder.SetParameter("Range", 10);

                reader.ReadAll();
            }

            results.ForEach(d => Console.WriteLine(d));
            Console.Read();
        }
    }   
}
