using Microsoft.AspNetCore.Builder;
using TickTrader.BotAgent.BA;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.BotAgent.BA.Models;
using System;
using TickTrader.BotAgent.WebAdmin.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using TickTrader.Algo.Domain;

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
            var botAgent = _services.GetService<IBotAgent>();

            botAgent.PackageChanged += OnPackageChanged;
            botAgent.AccountChanged += OnAccountChanged;
            botAgent.BotStateChanged += OnBotStateChanged;
            botAgent.BotChanged += OnBotChaged;

            return app;
        }

        private static void OnBotChaged(PluginModelUpdate update)
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

        private static void OnBotStateChanged(PluginStateUpdate bot)
        {
            Hub.Clients.All.ChangeBotState(bot.ToBotStateDto());
        }

        private static void OnAccountChanged(AccountModelUpdate update)
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

        private static void OnPackageChanged(PackageUpdate update)
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
