using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.CoreUsageSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //var data = TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt");

            //IndicatorBuilder builder = new IndicatorBuilder(AlgoPluginDescriptor.Get(typeof(Alligator)));

            //builder.Symbols.Add(new SymbolEntity("EURUSD") { Digits = 5, LotSize = 100000, MaxAmount = 10000000, MinAmount = 10000 });
            //builder.Account.Orders.Add(new OrderEntity(10) { Symbol = "EURUSD", RequestedAmount = new OrderVolume(), RemainingAmount = new OrderVolume() });

            //builder.MainSymbol = "EURUSD";
            //builder.GetBarBuffer("EURUSD").Append(data);
            //builder.MapBarInput("Input", "EURUSD");

            //builder.BuildNext(data.Count);

            ////builder.Account.Orders.Add(new OrderEntity(11) { Symbol = "EURUSD", TotalAmount = 6000, RemainingAmount = 6000 });

            //builder.GetBarSeries("EURUSD").Last = new BarEntity() { High = 100 };
            //builder.RebuildLast();

            //var descriptor = AlgoPluginDescriptor.Get(typeof(Alligator));
            //var setup = new BarBasedPluginSetup(descriptor, new SetupMetadata());
            //setup.SetParam("JawsPeriod", 14);

            //var builder = setup.CreateIndicatorBuilder();

            //builder.GetBarBuffer("EURUSD").Append(data);
            //builder.BuildNext(data.Count);

            //var jaws = builder.GetOutput("Jaws");
            //var teeth = builder.GetOutput("Teeth");
            //var lips = builder.GetOutput("Lips");

            //for (int i = 0; i < builder.DataSize; i++)
            //    Console.WriteLine(jaws[i] + " - " + teeth[i] + " - " + lips[i]);

            //Serialize(setup.Serialize(), "Alligator.cfg");

            var dataModel = new FeedModel(TimeFrames.M1);
            dataModel.Fill("EURUSD", TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt"));

            var descriptor = AlgoPluginDescriptor.Get(typeof(Alligator));
            var executor = new PluginExecutor(descriptor.Id);
            executor.MainSymbolCode = "EURUSD";
            executor.InvokeStrategy = new PriorityInvokeStartegy();
            executor.TimeFrame = dataModel.TimeFrame;
            executor.InitTimeSpanBuffering(DateTime.Parse("2015.11.02 00:25:00"), DateTime.Parse("2015.11.03 3:00:00"));

            var feedCfg = executor.InitBarStrategy(dataModel, BarPriceType.Bid);
            feedCfg.MapInput("Input", "EURUSD", BarPriceType.Bid);

            executor.Start();

            dataModel.Update(new QuoteEntity() { Symbol = "EURUSD", Time = DateTime.Parse("2015.11.03 00:00:24"), Ask = 1.10145, Bid = 1.10145 });
            dataModel.Update(new QuoteEntity() { Symbol = "EURUSD", Time = DateTime.Parse("2015.11.03 00:00:28"), Ask = 1.10148, Bid = 1.10151 });
            dataModel.Update(new QuoteEntity() { Symbol = "EURUSD", Time = DateTime.Parse("2015.11.03 00:00:31"), Ask = 1.10149, Bid = 1.10149 });

            executor.Stop();

            //executor.Reset();

            //executor.Start();

            Console.Write("Press any key to continue...");      
            Console.Read();
        }

        private static void Serialize<T>(T obj, string fileName)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stream = System.IO.File.Open(fileName, System.IO.FileMode.Create))
                serializer.Serialize(stream, obj);
        }
    }
}
