using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;
using TickTrader.Algo.PkgLoader;
using TickTrader.Algo.IndicatorHost;

namespace PkgStorageUsageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            AlgoLoggerFactory.Init(ConsoleLoggerAdapter.Create);
            PkgLoader.InitDefaults();

            RunPkgStorage(args[0]).Wait();
        }

        static async Task RunPkgStorage(string pkgDir)
        {
            var settings = new IndicatorHostSettings { DataFolder = AppDomain.CurrentDomain.BaseDirectory };
            settings.HostSettings.PkgStorage.UploadLocationId = "local";
            settings.HostSettings.PkgStorage.AddLocation("local", pkgDir);

            var indicatorHost = new IndicatorHostProxy();

            indicatorHost.PkgStorage.OnPkgUpdated.Subscribe(update => Console.WriteLine($"{update.Action} {update.Id}"));

            await indicatorHost.Init(settings);
        }
    }
}
