using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.DataflowConcept;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Setup;

namespace TickTrader.Algo.CoreUsageSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = TTQuoteFileReader.ReadFile("EURUSD-M1-bids.txt");

            //IndicatorBuilder builder = new IndicatorBuilder(AlgoPluginDescriptor.Get(typeof(Alligator)));

            //builder.Symbols.Add(new SymbolEntity("EURUSD") { Digits = 5, LotSize = 100000, MaxAmount = 10000000, MinAmount = 10000 });
            //builder.Account.Orders.Add(new OrderEntity(10) { Symbol = "EURUSD", TotalAmount = 5000, RemainingAmount = 5000 });

            //builder.MainSymbol = "EURUSD";
            //builder.GetBarSeries("EURUSD").Append(data);
            //builder.MapBarInput("Input", "EURUSD");

            //builder.BuildNext(data.Count);

            ////builder.Account.Orders.Add(new OrderEntity(11) { Symbol = "EURUSD", TotalAmount = 6000, RemainingAmount = 6000 });

            //builder.GetBarSeries("EURUSD").Last = new BarEntity() { High = 100 };
            //builder.RebuildLast();

            var descriptor = AlgoPluginDescriptor.Get(typeof(Alligator));
            var setup = new BarBasedPluginSetup(descriptor, new SetupMetadata());
            setup.SetParam("JawsPeriod", 14);

            var builder = setup.CreateIndicatorBuilder();

            builder.GetBarSeries("EURUSD").Append(data);
            builder.BuildNext(data.Count);

            var jaws = builder.GetOutput("Jaws");
            var teeth = builder.GetOutput("Teeth");
            var lips = builder.GetOutput("Lips");

            for (int i = 0; i < builder.DataSize; i++)
                Console.WriteLine(jaws[i] + " - " + teeth[i] + " - " + lips[i]);

            Serialize(setup.Serialize(), "Alligator.cfg");

            Console.Write("Press any key to continue...");      
            Console.Read();
        }

        private static void Serialize<T>(T obj, string fileName)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var stream = System.IO.File.Open(fileName, System.IO.FileMode.Create))
                serializer.Serialize(stream, obj);
        }

        public class SetupMetadata : ISetupMetadata
        {
            private HashSet<string> symbols = new HashSet<string>();

            public SetupMetadata()
            {
                symbols.Add(MainSymbol);
                symbols.Add("EURJPY");
            }

            public string MainSymbol { get { return "EURUSD"; } }

            public bool SymbolExist(string symbolCode)
            {
                return symbols.Contains(symbolCode);
            }
        }
    }
}
