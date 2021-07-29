using NLog;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Isolation.NetCore;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;
using TickTrader.Algo.Runtime;

namespace TickTrader.Algo.RuntimeV1Host.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                Environment.FailFast(string.Join(", ", args));

            ConfigureLogging(args[2]);

            RunRuntime(args).Wait();
        }

        private static async Task RunRuntime(string[] args)
        {
            var logger = LogManager.GetLogger("MainLoop");

            SetupGlobalExceptionLogging(logger);

            logger.Info("Starting runtime with id {runtimeId} at server {address}:{port}", args[2], args[0], args[1]);

            PackageLoadContext.Init(PackageLoadContextProvider.Create);
            PackageExplorer.Init(PackageV1Explorer.Create());
            PluginLogWriter.Init(NLogPluginLogWriter.Create);

            RuntimeV1Loader loader = null;
            var isFailed = false;
            try
            {

                loader = new RuntimeV1Loader();
                loader.Init(args[0], int.Parse(args[1]), args[2]);
            }
            catch (Exception ex)
            {
                isFailed = true;
                logger.Error(ex, "Failed to init runtime. Aborting");
            }

            if (loader == null || isFailed)
                Environment.FailFast("Invalid start");

            await loader.WhenFinished();
            logger.Info("Runtime finished.");

            try
            {
                loader.Deinit();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to deinit runtime.");
                Environment.FailFast("Failed to deinit runtime.");
            }
        }

        private static void ConfigureLogging(string runtimeId)
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Runtimes", runtimeId);

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
