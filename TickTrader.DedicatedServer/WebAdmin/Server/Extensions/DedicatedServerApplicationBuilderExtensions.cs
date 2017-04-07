using Microsoft.AspNetCore.Builder;
using TickTrader.DedicatedServer.DS;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.DedicatedServer.DS.Models;
using System;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using TickTrader.DedicatedServer.WebAdmin.Server.Hubs;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class DedicatedServerApplicationBuilderExtensions
    {
        private static IServiceProvider _services;

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

            return app;
        }

        private static void OnBotStateChanged(ITradeBot bot)
        {
            var signalRConnectionManager = _services.GetService<IConnectionManager>();
            var _hub = signalRConnectionManager.GetHubContext<DSFeed>();
            _hub.Clients.All.ChangeBotState(bot.ToBotStateDto());
        }

        private static void OnAccountChanged(IAccount account, ChangeAction action)
        {
            var signalRConnectionManager = _services.GetService<IConnectionManager>();
            var _hub = signalRConnectionManager.GetHubContext<DSFeed>();
            switch (action)
            {
                case ChangeAction.Added:
                    _hub.Clients.All.AddAccount(account.ToDto());
                    break;
                case ChangeAction.Removed:
                    _hub.Clients.All.DeleteAccount(account.ToDto());
                    break;
            }
        }

        private static void OnPackageChanged(IPackage package, ChangeAction action)
        {
            var signalRConnectionManager = _services.GetService<IConnectionManager>();
            var _hub = signalRConnectionManager.GetHubContext<DSFeed>();
            switch (action)
            {
                case ChangeAction.Added:
                    _hub.Clients.All.AddPackage(package.ToDto());
                    break;
                case ChangeAction.Removed:
                    _hub.Clients.All.DeletePackage(package.Name);
                    break;
            }
        }
    }
}
