using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using TickTrader.BotAgent.WebAdmin.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentApplicationBuilderExtensions
    {
        private static IServiceProvider _services;

        private static IHubContext<BAFeed, IBAFeed> Hub
        {
            get
            {
                var _hub = _services.GetRequiredService<IHubContext<BAFeed, IBAFeed>>();
                return _hub;
            }
        }


        /// <summary>
        /// Use SignalR Hubs to notify clients about server changes
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder ObserveBotAgent(this IApplicationBuilder app)
        {
            _services = app.ApplicationServices;
            var eventBus = _services.GetService<IAlgoServerLocal>().EventBus;

            eventBus.PackageUpdated.Subscribe(OnPackageUpdated);
            eventBus.AccountUpdated.Subscribe(OnAccountUpdated);
            eventBus.PluginUpdated.Subscribe(OnPluginUpdated);
            eventBus.PluginStateUpdated.Subscribe(OnPluginStateUpdated);

            return app;
        }

        private static void OnPluginUpdated(PluginModelUpdate update)
        {
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    Hub.Clients.All.AddBot(update.Plugin.ToDto());
                    break;
                case Update.Types.Action.Removed:
                    Hub.Clients.All.DeleteBot(update.Id);
                    break;
                case Update.Types.Action.Updated:
                    Hub.Clients.All.UpdateBot(update.Plugin.ToDto());
                    break;
            }
        }

        private static void OnPluginStateUpdated(PluginStateUpdate bot)
        {
            Hub.Clients.All.ChangeBotState(bot.ToBotStateDto());
        }

        private static void OnAccountUpdated(AccountModelUpdate update)
        {
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    Hub.Clients.All.AddAccount(update.Account.ToDto());
                    break;
                case Update.Types.Action.Removed:
                    Hub.Clients.All.DeleteAccount(update.Id.ToAccountDto());
                    break;
            }
        }

        private static void OnPackageUpdated(PackageUpdate update)
        {
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                case Update.Types.Action.Updated:
                    Hub.Clients.All.AddOrUpdatePackage(update.Package.ToDto());
                    break;
                case Update.Types.Action.Removed:
                    Hub.Clients.All.DeletePackage(update.Id);
                    break;
            }
        }
    }
}
