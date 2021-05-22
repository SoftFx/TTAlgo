using System.IO;
using Microsoft.AspNetCore.Hosting;
using TickTrader.BotAgent.WebAdmin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TickTrader.BotAgent.BA;
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

            AlgoLoggerFactory.Init(cn => new LoggerAdapter(LogManager.GetLogger(cn)));

            PackageLoadContext.Init(isolated => PackageLoadContextProvider.Create(isolated));
            PackageExplorer.Init(new PackageV1Explorer());

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

        //private static void RunConsole()
        //{
        //    var serviceProvider = new ServiceCollection()
        //        .AddLogging()
        //        .BuildServiceProvider();

        //    var logFactory = serviceProvider.GetService<ILoggerFactory>();
        //    InitLogger(logFactory);
        //    var server = ServerModel.Load(logFactory);

        //    CommandUi cmdEngine = new CommandUi();
        //    cmdEngine.RegsiterCommand("info", () =>
        //    {
        //        lock (server.SyncObj)
        //        {
        //            foreach (var acc in server.Accounts)
        //            {
        //                var model = acc;
        //                Console.WriteLine(GetDisplayName(acc) + " - " + acc.ConnectionState);
        //                foreach (var bot in acc.TradeBots)
        //                    Console.WriteLine("\t{0} - {1} ", bot.Id, bot.State);
        //            }
        //        }
        //    });

        //    cmdEngine.RegsiterCommand("account", () =>
        //    {
        //        var cmd = CommandUi.Choose("cmd", "add", "remove", "change password", "test", "cancel", "info");

        //        IAccount acc;
        //        List<IAccount> accountsList;

        //        switch (cmd)
        //        {
        //            case "add":
        //                var newLogin = CommandUi.InputString("login");
        //                var newPassword = CommandUi.InputString("password");
        //                var newServer = CommandUi.InputString("server");
        //                server.AddAccount(new AccountKey(newLogin, newServer), newPassword, false);
        //                break;

        //            case "remove":
        //                lock (server.SyncObj)
        //                    accountsList = server.Accounts.ToList();
        //                acc = CommandUi.Choose("account", accountsList, GetDisplayName);
        //                server.RemoveAccount(new AccountKey(acc.Username, acc.Address));
        //                break;

        //            case "change password":
        //                lock (server.SyncObj)
        //                    accountsList = server.Accounts.ToList();
        //                acc = CommandUi.Choose("account", accountsList, GetDisplayName);
        //                var chgPassword = CommandUi.InputString("new password");
        //                server.ChangeAccountPassword(new AccountKey(acc.Username, acc.Address), chgPassword);
        //                break;

        //            case "test":
        //                lock (server.SyncObj)
        //                    accountsList = server.Accounts.ToList();
        //                acc = CommandUi.Choose("account", accountsList, GetDisplayName);
        //                var result = acc.TestConnection().Result;
        //                if (result.Code == ConnectionErrorCodes.None)
        //                    Console.WriteLine("Valid connection.");
        //                else
        //                    Console.WriteLine("Error = " + acc.TestConnection().Result);
        //                break;
        //            case "info":
        //                lock (server.SyncObj)
        //                    accountsList = server.Accounts.ToList();
        //                acc = CommandUi.Choose("account", accountsList, GetDisplayName);
        //                var accKey = new AccountKey(acc.Username, acc.Address);
        //                ConnectionInfo info;
        //                var getInfoError = server.GetAccountInfo(accKey, out info);
        //                if (getInfoError == ConnectionErrorCodes.None)
        //                {
        //                    Console.WriteLine();
        //                    Console.WriteLine("Symbols:");
        //                    Console.WriteLine(string.Join(",", info.Symbols.Select(s => s.Name)));
        //                    Console.WriteLine();
        //                    Console.WriteLine("Currencies:");
        //                    Console.WriteLine(string.Join(",", info.Currencies.Select(s => s.Name)));
        //                }
        //                else
        //                    Console.WriteLine("Error = " + acc.TestConnection().Result);
        //                break;
        //        }

        //    });

        //    cmdEngine.RegsiterCommand("trade bot", () =>
        //    {
        //        var cmd = CommandUi.Choose("cmd", "add", "remove", "start", "stop", "view status", "cancel");

        //        IAccount acc;
        //        List<IAccount> accountsList;
        //        ITradeBot[] bots;

        //        switch (cmd)
        //        {
        //            case "add":

        //                PluginInfo[] availableBots;

        //                lock (server.SyncObj)
        //                {
        //                    availableBots = server.GetPluginsByType(AlgoTypes.Robot);
        //                    accountsList = server.Accounts.ToList();
        //                }

        //                if (accountsList.Count == 0)
        //                    Console.WriteLine("Cannot add bot: no accounts!");
        //                else if (availableBots.Length == 0)
        //                    Console.WriteLine("Cannot add bot: no bots in repository!");
        //                else
        //                {
        //                    if (accountsList.Count == 1)
        //                        acc = accountsList[0];
        //                    else
        //                        acc = CommandUi.Choose("account", accountsList, GetDisplayName);

        //                    var botToAdd = CommandUi.Choose("bot", availableBots, b => b.Descriptor.DisplayName);

        //                    if (botToAdd.Descriptor.IsValid)
        //                    {
        //                        var botConfig = SetupBot(botToAdd.Descriptor);
        //                        var botId = server.AutogenerateBotId(botToAdd.Descriptor.DisplayName);

        //                        TradeBotModelConfig botCfg = new TradeBotModelConfig
        //                        {
        //                            InstanceId = botId,
        //                            Plugin = botToAdd.Id,
        //                            PluginConfig = botConfig,
        //                            Isolated = false
        //                        };

        //                        acc.AddBot(botCfg);
        //                    }
        //                    else
        //                        Console.WriteLine("Cannot add bot: bot is invalid!");
        //                }

        //                break;

        //            case "start":

        //                lock (server.SyncObj)
        //                    bots = server.TradeBots.ToArray();

        //                var botToStart = CommandUi.Choose("bot", bots, b => b.Id);

        //                botToStart.Start();

        //                break;

        //            case "remove":

        //                lock (server.SyncObj)
        //                    bots = server.TradeBots.ToArray();

        //                var botToRemove = CommandUi.Choose("bot", bots, b => b.Id);

        //                server.RemoveBot(botToRemove.Id);

        //                break;

        //            case "stop":

        //                lock (server.SyncObj)
        //                    bots = server.TradeBots.ToArray();

        //                var botToStop = CommandUi.Choose("bot", bots, b => b.Id);

        //                botToStop.StopAsync().Wait();

        //                break;

        //            case "view status":

        //                lock (server.SyncObj)
        //                    bots = server.TradeBots.ToArray();

        //                var botToView = CommandUi.Choose("bot", bots, b => b.Id);

        //                Action<string> printAction = st =>
        //                {
        //                    Console.Clear();
        //                    Console.WriteLine(st);
        //                };

        //                lock (server.SyncObj)
        //                {
        //                    printAction(botToView.Log.Status);
        //                    botToView.Log.StatusUpdated += printAction;
        //                }

        //                Console.ReadLine();

        //                lock (server.SyncObj)
        //                    botToView.Log.StatusUpdated -= printAction;

        //                break;
        //        }
        //    });

        //    cmdEngine.Run();

        //    server.Close();
        //}

        //private static string GetDisplayName(AccountInfo acc)
        //{
        //    return string.Format("account {0} : {1}", acc.Server, acc.Login);
        //}

        //private static PluginConfig SetupBot(PluginMetadata descriptor)
        //{
        //    var config = new Algo.Common.Model.Config.TradeBotConfig();

        //    //config.PriceType = BarPriceType.Bid;
        //    config.MainSymbol = CommandUi.InputString("symbol");

        //    foreach (var prop in descriptor.AllProperties)
        //        config.Properties.Add(InputBotParam(prop));

        //    Console.WriteLine();
        //    Console.WriteLine("Configuration:");
        //    Console.WriteLine("\tMain Symbol - {0}", config.MainSymbol);

        //    foreach (var p in config.Properties)
        //        PrintProperty(p);

        //    return config;
        //}

        //private static Property InputBotParam(PropertyMetadata descriptor)
        //{
        //    if (descriptor is ParameterMetadata)
        //    {
        //        var paramDescriptor = (ParameterMetadata)descriptor;
        //        var id = descriptor.Id;

        //        if (paramDescriptor.IsEnum)
        //        {
        //            var enumVal = CommandUi.ChooseNullable(descriptor.DisplayName, paramDescriptor.EnumValues.ToArray());
        //            return new EnumParameter() { Id = id, Value = enumVal ?? (string)paramDescriptor.DefaultValue };
        //        }

        //        switch (paramDescriptor.DataType)
        //        {
        //            case "System.Int32":
        //                var valInt32 = CommandUi.InputNullabelInteger(paramDescriptor.DisplayName);
        //                return new IntParameter() { Id = id, Value = valInt32 ?? (int)paramDescriptor.DefaultValue };
        //            case "System.Double":
        //                var valDouble = CommandUi.InputNullableDouble(paramDescriptor.DisplayName);
        //                return new DoubleParameter() { Id = id, Value = valDouble ?? (double)paramDescriptor.DefaultValue };
        //            case "System.String":
        //                var strVal = CommandUi.InputString(paramDescriptor.DisplayName);
        //                return new StringParameter() { Id = id, Value = CommandUi.InputString(paramDescriptor.DisplayName) };
        //            case "TickTrader.Algo.Api.File":
        //                return new FileParameter() { Id = id, FileName = CommandUi.InputString(paramDescriptor.DisplayName) };
        //        }
        //    }

        //    throw new ApplicationException($"Parameter '{descriptor.DisplayName}' is of unsupported type!");
        //}

        //private static void PrintProperty(Property p)
        //{
        //    if (p is Parameter)
        //        Console.WriteLine("\t{0} - {1}", p.Id, ((Parameter)p).ValObj);
        //}
    }
}
