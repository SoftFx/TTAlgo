using System;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.RpcTest
{
    class Program
    {
        public const string ApplicationName = "BotTrader";


        static void Main(string[] args)
        {
            CoreLoggerFactory.Init(name => new ConsoleLoggerAdapter(name));

            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appDocumentsFolder = Path.Combine(myDocumentsFolder, ApplicationName);
            var algoCommonRepositoryFolder = Path.Combine(appDocumentsFolder, "AlgoRepository");


            var repo = new PackageRepository(algoCommonRepositoryFolder, RepositoryLocation.CommonRepository, CoreLoggerFactory.GetLogger<PackageRepository>());
            repo.Start();
            repo.WaitInit().GetAwaiter().GetResult();

            var package = repo.Packages.Values.First(p => string.Equals(p.PackageRef.Name, "TickTrader.Algo.TestCollection.ttalgo", StringComparison.OrdinalIgnoreCase));
            var pluginRef = package.PackageRef.GetPluginRef("TickTrader.Algo.TestCollection.Bots.AccDisplayBot");
            Console.WriteLine(pluginRef.DisplayName);

            var server = new AlgoServer();

            server.Start().GetAwaiter().GetResult();
            var executor = server.CreateExecutor(pluginRef, new NullSyncContext());
            executor.Launch(server.Address, server.BoundPort);

            Console.ReadLine();
        }
    }
}
