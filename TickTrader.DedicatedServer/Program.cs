using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Web.WebHost host = new Web.WebHost();
            host.Start();

            Console.Read();

            host.Stop();
        }
    }
}
