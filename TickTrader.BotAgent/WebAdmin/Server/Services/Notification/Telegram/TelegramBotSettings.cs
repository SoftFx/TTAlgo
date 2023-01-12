using System;
using System.Collections.Generic;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    internal static class BotSettings
    {
        internal const string StartCommand = "/start";
        internal const string EndCommand = "/end";


        internal static List<BotCommand> BotCommands { get; } = new()
        {
            new()
            {
                Command = StartCommand,
                Description = "Subscription to alerts in AlgoServer"
            },

            new BotCommand()
            {
                Command = EndCommand,
                Description = "Unsubscribing from AlgeServer alerts"
            }
        };


        internal static ReceiverOptions Options { get; } = new()
        {
            AllowedUpdates = { }, // receive all update types
        };


        internal static TimeSpan AvailableDelay { get; } = new(0, 5, 0);
    }
}
