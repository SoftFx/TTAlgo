using Microsoft.AspNetCore.Builder;
using TickTrader.BotAgent.BA;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.BotAgent.BA.Models;
using System;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using TickTrader.BotAgent.WebAdmin.Server.Hubs;
using Microsoft.Extensions.Logging;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentApplicationBuilderExtensions
    {
        private static IServiceProvider _services;
        private static ILogger _logger;

        private static Microsoft.AspNetCore.SignalR.IHubContext Hub
        {
            get
            {
                var signalRConnectionManager = _services.GetService<IConnectionManager>();
                var _hub = signalRConnectionManager.GetHubContext<BAFeed>();
                return _hub;
            }
        }


        public static IApplicationBuilder UseWardenOverBots(this IApplicationBuilder app)
        {
            _services = app.ApplicationServices;
            var _botAgent = _services.GetService<IBotAgent>();
            _logger = _services.GetService<ILoggerFactory>().CreateLogger("BotsWarden");

            //_botAgent.BotStateChanged += WatchTheStop;

            return app;
        }

        //private static async void WatchTheStop(TradeBotInfo bot)
        //{
        //    if (bot.State != BotStates.Stopping)
        //        return;

        //    await Task.Delay(TimeSpan.FromSeconds(5));

        //    if (bot.State == BotStates.Stopping)
        //    {
        //        bot.Abort();
        //        _logger.LogDebug($"Bot '{bot.Id}' was aborted");
        //    }
        //}

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

        private static void OnBotChaged(BotModelInfo bot, ChangeAction action)
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

        private static void OnBotStateChanged(BotModelInfo bot)
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
                    Hub.Clients.All.DeletePackage(package.Key.Name);
                    break;
            }
        }
    }
}
