using NLog;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Isolation.NetFx;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.BacktesterV1Host
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                Environment.FailFast(string.Join(", ", args));

            ConfigureLogging(args[2]);

            RunBacktester(args).Wait();
        }

        private static async Task RunBacktester(string[] args)
        {
            AlgoLoggerFactory.Init(NLogLoggerAdapter.Create);

            var logger = LogManager.GetLogger("MainLoop");

            SetupGlobalExceptionLogging(logger);

            var id = args[2];
            var address = args[0];
            var port = args[1];
            logger.Info("Starting backtester with id {backtesterId} at server {address}:{port}", id, address, port);

            PackageLoadContext.Init(PackageLoadContextProvider.Create);
            PackageExplorer.Init(PackageV1Explorer.Create());
            PluginLogWriter.Init(NLogPluginLogWriter.Create);

            BacktesterV1Loader loader = null;
            var isFailed = false;
            try
            {

                loader = new BacktesterV1Loader(id);
                await loader.Init(address, int.Parse(port));
            }
            catch (Exception ex)
            {
                isFailed = true;
                logger.Error(ex, "Failed to init backtester. Aborting");
            }

            if (loader == null || isFailed)
                Environment.FailFast("Invalid start");

            await loader.WhenFinished();
            logger.Info("Backtester finished.");

            try
            {
                await loader.Deinit();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to deinit backtester.");
                Environment.FailFast("Failed to deinit backtester.");
            }
        }

        private static void ConfigureLogging(string id)
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Backtester", id);

            LogManager.Configuration = NLogHelper.CreateRuntimeConfig(logDir);

            AlgoLoggerFactory.Init(NLogLoggerAdapter.Create);
            NonBlockingFileCompressor.Setup();
        }

        private static void SetupGlobalExceptionLogging(Logger logger)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                    logger.Fatal(ex, "Unhandled Exception on Domain level!");
                else
                    logger.Fatal("Unhandled Exception on Domain level! No exception specified!");
            };

            ActorSharp.Actor.UnhandledException += (ex) =>
            {
                logger.Error(ex, "Unhandled Exception on Actor level!");
            };

            ActorSystem.ActorErrors.Subscribe(ex => logger.Error(ex));
            ActorSystem.ActorFailed.Subscribe(ex => logger.Fatal(ex));
        }
    }
}
