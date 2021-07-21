using System.IO;
using Microsoft.AspNetCore.Hosting;
using TickTrader.BotAgent.WebAdmin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using NLog;
using System.Globalization;
using ActorSharp;
using NLog.Web;
using NLog.Extensions.Logging;
using System.Collections.Generic;
using TickTrader.BotAgent.Hosting;
using TickTrader.Algo.Server;
using TickTrader.Algo.ServerControl;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Package;
using TickTrader.Algo.Isolation.NetFx;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Logging;

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

                host.Launch();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var launchSettings = LaunchSettings.Read(args, SwitchMappings);

            Console.WriteLine(launchSettings);

            var pathToContentRoot = Directory.GetCurrentDirectory();

            if (launchSettings.Mode == LaunchMode.WindowsService)
                pathToContentRoot = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var pathToWebAdmin = Path.Combine(pathToContentRoot, "WebAdmin");
            var pathToWebRoot = Path.Combine(pathToWebAdmin, "wwwroot");
            var pathToAppSettings = Path.Combine(pathToWebAdmin, "appsettings.json");

            EnsureDefaultConfiguration(pathToAppSettings);

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

            return new WebHostBuilder()
                .UseConfiguration(config)
                .ConfigureAppConfiguration((context, builder) => 
                {
                    // Thanks Microsoft for not doing this in UseConfiguration
                    // I enjoy spending hours in framework sources
                    builder.Sources.Clear();
                    builder.AddConfiguration(config);
                })
                .UseKestrel()
                .ConfigureKestrel((context, options) =>
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ServerCertificate = cert;
                        httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    }))
                .UseContentRoot(pathToContentRoot)
                .UseWebRoot(pathToWebRoot)
                .ConfigureLogging(logging => logging.AddNLog())
                .ConfigureServices(services =>
                    services.Configure<LaunchSettings>(options =>
                    {
                        options.Environment = launchSettings.Environment;
                        options.Mode = launchSettings.Mode;
                    }))
                .UseStartup<Startup>();
        }


        private static void EnsureDefaultConfiguration(string configFile)
        {
            if (!File.Exists(configFile))
            {
                CreateDefaultConfig(configFile);
            }
            else
            {
                MigrateConfig(configFile);
            }
        }

        private static void CreateDefaultConfig(string configFile)
        {
            var appSettings = AppSettings.Default;
            SaveConfig(configFile, appSettings);
        }

        private static void MigrateConfig(string configFile)
        {
            var currentSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(configFile));

            var anyChanges = false;

            if (currentSettings.Protocol == null)
            {
                currentSettings.Protocol = AppSettings.Default.Protocol;
                anyChanges = true;
            }
            if (currentSettings.Credentials.Login != null)
            {
                var oldCreds = currentSettings.Credentials;
                currentSettings.Credentials = AppSettings.Default.Credentials;
                currentSettings.Credentials.AdminLogin = oldCreds.Login;
                currentSettings.Credentials.AdminPassword = oldCreds.Password;
                anyChanges = true;
            }
            if (currentSettings.Fdk == null)
            {
                currentSettings.Fdk = AppSettings.Default.Fdk;
                anyChanges = true;
            }

            if (anyChanges)
            {
                SaveConfig(configFile, currentSettings);
            }
        }

        private static void SaveConfig(string configFile, AppSettings appSettings)
        {
            File.WriteAllText(configFile, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
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

            Actor.UnhandledException += (ex) =>
            {
                log.Error(ex, "Unhandled Exception on Actor level!");
            };
        }
    }
}
