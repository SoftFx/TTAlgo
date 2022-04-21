using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;
using TickTrader.Algo.PkgStorage;

namespace PkgStorageUsageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            AlgoLoggerFactory.Init(ConsoleLoggerAdapter.Create);

            RunPkgStorage(args[0]).Wait();
        }

        static async Task RunPkgStorage(string pkgDir)
        {
            var settings = new PkgStorageSettings { UploadLocationId = "local" };
            settings.AddLocation("local", pkgDir);

            var pkgStorage = new PkgStoragePublic();

            pkgStorage.OnPkgUpdated.Subscribe(update => Console.WriteLine($"{update.Action} {update.Id}"));

            await pkgStorage.Init(settings);

            await pkgStorage.WhenLoaded();
        }
    }
}
