using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Isolation.NetCore;
using TickTrader.Algo.Logging;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.Common;
using TickTrader.BotAgent.Hosting;
using TickTrader.BotAgent.WebAdmin;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Settings;
//using TickTrader.SeriesStorage;
//using TickTrader.SeriesStorage.Lmdb;

namespace TickTrader.BotAgent
{
    public class Program
    {
        public static readonly Dictionary<string, string> SwitchMappings =
            new Dictionary<string, string>
            {
                {"-e", WebHostDefaults.EnvironmentKey},
                {"-c", LaunchSettings.ConsoleKey },
            };


        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            NonBlockingFileCompressor.Setup();

            AlgoLoggerFactory.Init(NLogLoggerAdapter.Create);

            PackageLoadContext.Init(PackageLoadContextProvider.Create);
            PackageExplorer.Init(PackageV1Explorer.Create());

            //BinaryStorageManagerFactory.Init((folder, readOnly) => new LmdbManager(folder, readOnly));

            var logger = LogManager.GetLogger(nameof(Program));

            SetupGlobalExceptionLogging(logger);

            try
            {
                CertificateProvider.InitServer(SslImport.LoadServerCertificate(), SslImport.LoadServerPrivateKey());

                var hostBuilder = CreateWebHostBuilder(args);

                var host = hostBuilder
                    .AddBotAgent()
                    .AddProtocolServer()
                    .Build();

                logger.Info("Starting web host");

                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            var launchSettings = LaunchSettings.Read(args, SwitchMappings);

            Console.WriteLine(launchSettings);

            var pathToContentRoot = Directory.GetCurrentDirectory();

            if (launchSettings.Mode == LaunchMode.WindowsService)
                pathToContentRoot = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var pathToWebAdmin = Path.Combine(pathToContentRoot, "WebAdmin");
            var pathToWebRoot = Path.Combine(pathToWebAdmin, "wwwroot");
            var pathToAppSettings = Path.Combine(pathToWebAdmin, "appsettings.json");

            AppSettings.EnsureValidConfiguration(pathToAppSettings);

            var configBuilder = new ConfigurationBuilder();
            configBuilder
                .SetBasePath(pathToWebAdmin)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{launchSettings.Environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables(LaunchSettings.EnvironmentVariablesPrefix)
                .AddCommandLine(args)
                .AddInMemoryCollection(launchSettings.GenerateEnvironmentOverride());

            var config = configBuilder.Build();

            var cert = config.GetCertificate(pathToContentRoot);
            var kesterSection = config.GetSection("Kestrel");
            var publicApiSettings = config.GetProtocolSettings();

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Sources.Clear();
                    builder.AddConfiguration(config);
                })
                .ConfigureLogging(logging => logging.AddNLog())
                .ConfigureServices(services =>
                    services.Configure<LaunchSettings>(options =>
                    {
                        options.Environment = launchSettings.Environment;
                        options.Mode = launchSettings.Mode;
                    }))
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseKestrel(options =>
                    {
                        options.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            httpsOptions.ServerCertificate = cert;
                            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                        });

                        var kestrelLoader = options.Configure(kesterSection, true);
                        if (publicApiSettings.ListeningPort >= 0) // advanced configuration: less than zero disables additional port to use only kestrel section
                        {
                            if (publicApiSettings.IsLocalServer)
                            {
                                kestrelLoader.LocalhostEndpoint(publicApiSettings.ListeningPort, o => o.UseHttps());
                            }
                            else
                            {
                                kestrelLoader.AnyIPEndpoint(publicApiSettings.ListeningPort, o => o.UseHttps());
                            }
                        }
                    })
                    .UseContentRoot(pathToContentRoot)
                    .UseWebRoot(pathToWebRoot)
                    .UseStartup<Startup>());
            if (launchSettings.Mode == LaunchMode.WindowsService)
                builder.UseWindowsService();

            return builder;
        }


        private static void SetupGlobalExceptionLogging(Logger log)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                    log.Fatal(ex, "Unhandled Exception on Domain level!");
                else
                    log.Fatal("Unhandled Exception on Domain level! No exception specified!");
            };

            ActorSharp.Actor.UnhandledException += (ex) =>
            {
                log.Error(ex, "Unhandled Exception on Actor level!");
            };

            ActorSystem.ActorErrors.Subscribe(ex => log.Error(ex));
            ActorSystem.ActorFailed.Subscribe(ex => log.Fatal(ex));
        }
    }
}
