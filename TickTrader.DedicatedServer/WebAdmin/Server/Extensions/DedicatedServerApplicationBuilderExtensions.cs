using Microsoft.AspNetCore.Builder;
using TickTrader.DedicatedServer.DS;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.DedicatedServer.DS.Models;
using System;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using TickTrader.DedicatedServer.WebAdmin.Server.Hubs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class DedicatedServerApplicationBuilderExtensions
    {
        private static IServiceProvider _services;
        private static ILogger _logger;

        private static Microsoft.AspNetCore.SignalR.IHubContext Hub
        {
            get
            {
                var signalRConnectionManager = _services.GetService<IConnectionManager>();
                var _hub = signalRConnectionManager.GetHubContext<DSFeed>();
                return _hub;
            }
        }


        public static IApplicationBuilder UseWardenOverBots(this IApplicationBuilder app)
        {
            _services = app.ApplicationServices;
            var _dedicatedServer = _services.GetService<IDedicatedServer>();
            _logger = _services.GetService<ILoggerFactory>().CreateLogger("BotsWarden");

            _dedicatedServer.BotStateChanged += WatchTheStop;

            return app;
        }

        private static async void WatchTheStop(ITradeBot bot)
        {
            if (bot.State != BotStates.Stopping)
                return;

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (bot.State == BotStates.Stopping)
            {
                bot.Abort();
                _logger.LogDebug($"Bot '{bot.Id}' was aborted");
            }
        }

        /// <summary>
        /// Use SignalR Hubs to notify clients about server changes
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder ObserveDedicatedServer(this IApplicationBuilder app)
        {
            _services = app.ApplicationServices;
            var _dedicatedServer = _services.GetService<IDedicatedServer>();

            _dedicatedServer.PackageChanged += OnPackageChanged;
            _dedicatedServer.AccountChanged += OnAccountChanged;
            _dedicatedServer.BotStateChanged += OnBotStateChanged;
            _dedicatedServer.BotChanged += OnBotChaged;

            return app;
        }

        private static void OnBotChaged(ITradeBot bot, ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                    Hub.Clients.All.AddBot(bot.ToDto());
                    break;
                case ChangeAction.Removed:
                    Hub.Clients.All.DeleteBot(bot.Id);
                    break;
                case ChangeAction.Modified:
                    Hub.Clients.All.UpdateBot(bot.ToDto());
                    break;
            }
        }

        private static void OnBotStateChanged(ITradeBot bot)
        {
            Hub.Clients.All.ChangeBotState(bot.ToBotStateDto());
        }

        private static void OnAccountChanged(IAccount account, ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                    Hub.Clients.All.AddAccount(account.ToDto());
                    break;
                case ChangeAction.Removed:
                    Hub.Clients.All.DeleteAccount(account.ToDto());
                    break;
            }
        }

        private static void OnPackageChanged(IPackage package, ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                case ChangeAction.Modified:
                    Hub.Clients.All.AddOrUpdatePackage(package.ToDto());
                    break;
                case ChangeAction.Removed:
                    Hub.Clients.All.DeletePackage(package.Name);
                    break;
            }
        }
    }
}
