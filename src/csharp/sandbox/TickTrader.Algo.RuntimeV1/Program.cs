using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.RuntimeV1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                Environment.FailFast(string.Join(", ", args));

            var loader = new RuntimeV1Loader();
            loader.Init(args[0], int.Parse(args[1]), args[2]);

            while(true)
            {
                Task.Delay(1000).Wait();
            }
        }
    }
}
