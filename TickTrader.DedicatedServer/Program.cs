using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using TickTrader.DedicatedServer.WebAdmin;
using Microsoft.Extensions.Configuration;
using TickTrader.DedicatedServer.DS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var server = new ServerModel();

            CommandUi cmdEngine = new CommandUi();
            cmdEngine.RegsiterCommand("display state", () =>
            {
                lock (server.SyncObj)
                {
                    foreach (var acc in server.Accounts)
                    {
                        Console.WriteLine(GetDisplayName(acc));
                        foreach (var bot in acc.Bots)
                        {
                        }
                    }
                }
            });

            cmdEngine.RegsiterCommand("add account", () =>
            {
                server.AddAccount(
                    CommandUi.InputString("login"),
                    CommandUi.InputString("password"),
                    CommandUi.InputString("server"));
            });

            cmdEngine.RegsiterCommand("test account", () =>
            {
                List<AccountModel> accountsList;
                lock (server.SyncObj)
                    accountsList = server.Accounts.ToList();
                var acc = CommandUi.Choose("account", accountsList, GetDisplayName);
                var result = acc.TestConnection().Result;
                if (result == Algo.Common.Model.ConnectionErrorCodes.None)
                    Console.WriteLine("Valid connection.");
                else
                    Console.WriteLine("Error = " + acc.TestConnection().Result);
            });

            cmdEngine.Run();
        }

        private static string GetDisplayName(AccountModel acc)
        {
            return string.Format("account {0} - {1}", acc.Connection.Address, acc.Connection.Address);
        }
    }
}
