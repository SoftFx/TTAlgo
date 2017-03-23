using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using TickTrader.DedicatedServer.WebAdmin;
using Microsoft.Extensions.Configuration;
using TickTrader.DedicatedServer.DS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.DedicatedServer.DS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.DedicatedServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1 || args[0] != "console")
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("WebAdmin/appsettings.json", optional: true)
                    .AddEnvironmentVariables();
                var config = builder.Build();

                var host = new WebHostBuilder()
                    .UseConfiguration(config)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<WebAdminStartup>()
                    .Build();

                host.Run();
            }
            else
                RunConsole();
        }

        private static void RunConsole()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var logFactory = serviceProvider.GetService<ILoggerFactory>();
            InitLogger(logFactory);
            var server = ServerModel.Load(logFactory);

            CommandUi cmdEngine = new CommandUi();
            cmdEngine.RegsiterCommand("info", () =>
            {
                lock (server.SyncObj)
                {
                    foreach (var acc in server.Accounts)
                    {
                        var model = acc;
                        Console.WriteLine(GetDisplayName(acc) + " - " + acc.ConnectionState);
                        foreach (var bot in acc.TradeBots)
                            Console.WriteLine("\t{0} - {1} ", bot.Id, bot.State);
                    }
                }
            });

            cmdEngine.RegsiterCommand("account", () =>
            {
                var cmd = CommandUi.Choose("cmd", "add", "remove", "test", "cancel");

                IAccount acc;
                List<IAccount> accountsList;

                switch (cmd)
                {
                    case "add":
                        var newLogin = CommandUi.InputString("login");
                        var newPassword = CommandUi.InputString("password");
                        var newServer = CommandUi.InputString("server");
                        server.AddAccount(new AccountKey(newLogin, newServer), newPassword);
                        break;

                    case "remove":
                        lock (server.SyncObj)
                            accountsList = server.Accounts.ToList();
                        acc = CommandUi.Choose("account", accountsList, GetDisplayName);
                        server.RemoveAccount(new AccountKey(acc.Username, acc.Address));
                        break;

                    case "test":
                        lock (server.SyncObj)
                            accountsList = server.Accounts.ToList();
                        acc = CommandUi.Choose("account", accountsList, GetDisplayName);
                        var result = acc.TestConnection().Result;
                        if (result == Algo.Common.Model.ConnectionErrorCodes.None)
                            Console.WriteLine("Valid connection.");
                        else
                            Console.WriteLine("Error = " + acc.TestConnection().Result);
                        break;
                }
 
            });

            cmdEngine.RegsiterCommand("trade bot", () =>
            {
                var cmd = CommandUi.Choose("cmd", "add", "remove", "start", "stop", "view status", "cancel");

                IAccount acc;
                List<IAccount> accountsList;
                ITradeBot[] bots;

                switch (cmd)
                {
                    case "add":

                        PlguinInfo[] availableBots;

                        lock (server.SyncObj)
                        {
                            availableBots = server.GetPluginsByType(AlgoTypes.Robot);
                            accountsList = server.Accounts.ToList();
                        }

                        if (accountsList.Count == 0)
                            Console.WriteLine("Cannot add bot: no accounts!");
                        else if (availableBots.Length == 0)
                            Console.WriteLine("Cannot add bot: no bots in repository!");
                        else
                        {
                            if (accountsList.Count == 1)
                                acc = accountsList[0];
                            else
                                acc = CommandUi.Choose("account", accountsList, GetDisplayName);

                            var botToAdd = CommandUi.Choose("bot", availableBots, b => b.Descriptor.DisplayName);

                            if (botToAdd.Descriptor.IsValid)
                            {
                                var botConfig = new BarBasedConfig();
                                botConfig.MainSymbol = "EURUSD";
                                botConfig.PriceType = BarPriceType.Bid;
                                var botId = server.AutogenerateBotId(botToAdd.Descriptor.DisplayName);
                                acc.AddBot(botId, botToAdd.Id, botConfig);
                            }
                            else
                                Console.WriteLine("Cannot add bot: bot is invalid!");
                        }

                        break;

                    case "start":

                        lock (server.SyncObj)
                            bots = server.TradeBots.ToArray();

                        var botToStart = CommandUi.Choose("bot", bots, b => b.Id);

                        botToStart.Start();

                        break;

                    case "stop":

                        lock (server.SyncObj)
                            bots = server.TradeBots.ToArray();

                        var botToStop = CommandUi.Choose("bot", bots, b => b.Id);

                        botToStop.StopAsync().Wait();

                        break;

                    case "view status":

                        lock (server.SyncObj)
                            bots = server.TradeBots.ToArray();

                        var botToView = CommandUi.Choose("bot", bots, b => b.Id);

                        Action<string> printAction = st =>
                        {
                            Console.Clear();
                            Console.WriteLine(st);
                        };

                        lock (server.SyncObj)
                        {
                            printAction(botToView.Log.Status);
                            botToView.Log.StatusUpdated += printAction;
                        }

                        Console.ReadLine();

                        lock (server.SyncObj)
                            botToView.Log.StatusUpdated -= printAction;

                        break;
                }
            });

            cmdEngine.Run();

            server.Close();
        }

        private static string GetDisplayName(IAccount acc)
        {
            return string.Format("account {0} : {1}", acc.Address, acc.Username);
        }

        private static void InitLogger(ILoggerFactory factory)
        {
            CoreLoggerFactory.Init(cn => new LoggerAdapter(factory.CreateLogger(cn)));
        }
    }
}
