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


        public static IApplicationBuilder UseWardenOverBots(this IApplicationBuilder app)
        {
            _services = app.ApplicationServices;
            var botAgent = _services.GetService<IBotAgent>();

            var warden = new BotsWarden(botAgent);

            return app;
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

        private static void OnBotChaged(PluginModelInfo bot, ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                    Hub.Clients.All.AddBot(bot.ToDto());
                    break;
                case ChangeAction.Removed:
                    Hub.Clients.All.DeleteBot(bot.InstanceId);
                    break;
                case ChangeAction.Modified:
                    Hub.Clients.All.UpdateBot(bot.ToDto());
                    break;
            }
        }

        private static void OnBotStateChanged(PluginStateUpdate bot)
        {
            Hub.Clients.All.ChangeBotState(bot.ToBotStateDto());
        }

        private static void OnAccountChanged(AccountModelInfo account, ChangeAction action)
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

        private static void OnPackageChanged(PackageInfo package, ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                case ChangeAction.Modified:
                    Hub.Clients.All.AddOrUpdatePackage(package.ToDto());
                    break;
                case ChangeAction.Removed:
                    Hub.Clients.All.DeletePackage(package.PackageId);
                    break;
            }
        }
    }
}
