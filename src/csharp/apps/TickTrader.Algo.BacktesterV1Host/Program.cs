using NLog;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.Lmdb;

namespace TickTrader.Algo.BacktesterV1Host
{
    class Program
    {
        static void Main(string[] args)
        {
            string logsDir = null;
            Func<Task> backtesterRunFactory = null;
            if (args.Length == 0)
            {
                RpcProxyParams rpcParams = null;
                try
                {
                    rpcParams = RpcProxyParams.ReadFromEnvVars(Environment.GetEnvironmentVariables());
                }
                catch (Exception ex)
                {
                    Environment.FailFast(ex.ToString());
                }

                logsDir = Directory.GetCurrentDirectory();
                backtesterRunFactory = () => RunBacktester(rpcParams);
            }
            if (args.Length == 1)
            {
                var configPath = args[0];
                if (!File.Exists(configPath))
                    Environment.FailFast($"Config file '{configPath}' not found");

                var resultsLocation = Path.Combine(Directory.GetCurrentDirectory(), "BacktesterResults");
                PathHelper.EnsureDirectoryCreated(resultsLocation);
                var resultsDir = BacktesterResults.Internal.CreateResultsDir(resultsLocation, configPath).Result;
                logsDir = resultsDir;
                backtesterRunFactory = () => RunBacktesterDetached(resultsDir);
            }

            if (backtesterRunFactory == null)
                Environment.FailFast("Can't determine specified action");
            if (string.IsNullOrEmpty(logsDir))
                Environment.FailFast("LogsDir can't be empty string");

            try
            {
                ConfigureLogging(logsDir, args.Length != 0);

                PackageLoadContext.Init(Isolation.NetCore.PackageLoadContextProvider.Create);
                PackageExplorer.Init(PackageV1Explorer.Create());
                PluginLogWriter.Init(NLogPluginLogWriter.Create);
                BinaryStorageManagerFactory.Init((folder, readOnly) => new LmdbManager(folder, readOnly));
            }
            catch (Exception ex)
            {
                Environment.FailFast(ex.ToString());
            }

            backtesterRunFactory().Wait();
        }

        private static async Task RunBacktester(RpcProxyParams rpcParams)
        {
            var resultsDir = Directory.GetCurrentDirectory();

            var logger = LogManager.GetLogger("MainLoop");

            SetupGlobalExceptionLogging(logger);

            var id = rpcParams.ProxyId;
            var address = rpcParams.Address;
            var port = rpcParams.Port;
            logger.Info("Starting backtester with id {backtesterId} at server {address}:{port}", id, address, port);

            BacktesterV1Loader loader = null;
            var isFailed = false;
            try
            {

                loader = new BacktesterV1Loader(new BacktesterV1HostHandler(id, resultsDir));
                await loader.Init(address, port);
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

        private static async Task RunBacktesterDetached(string resultsDir)
        {
            var logger = LogManager.GetLogger("MainLoop");

            SetupGlobalExceptionLogging(logger);

            var id = Path.GetFileName(resultsDir);
            var callbackStub = new BacktesterCallbackStub();
            var backtester = BacktesterV1Actor.Create(id, resultsDir, callbackStub);
            var started = false;
            try
            {
                logger.Info("Starting backtester with id {id}", id);

                await backtester.Ask(new StartBacktesterRequest());
                started = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to start");
            }

            if (started)
            {
                await callbackStub.AwaitStop();
                logger.Info("Backtester finished.");
            }

            try
            {
                await ActorSystem.StopActor(backtester);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to deinit backtester.");
            }

            try
            {
                LogManager.Shutdown();

                BacktesterResults.Internal.CompressResultsDir(resultsDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to shutdown backtester");
                Console.WriteLine(ex.ToString());
            }
        }

        private static void ConfigureLogging(string logsDir, bool addConsoleOutput)
        {
            var logDir = Path.Combine(logsDir, "Logs");

            var logConfig = NLogHelper.CreateRuntimeConfig(logDir, addConsoleOutput);
            LogManager.Configuration = logConfig;

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
