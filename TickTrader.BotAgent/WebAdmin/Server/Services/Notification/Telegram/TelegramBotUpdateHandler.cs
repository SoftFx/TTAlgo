using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    internal sealed class TelegramBotUpdateHandler : IUpdateHandler
    {
        private readonly Logger _logger = LogManager.GetLogger(nameof(TelegramBotUpdateHandler));
        private readonly NotificationStorage _storage;


        internal TelegramBotUpdateHandler(NotificationStorage storage)
        {
            _storage = storage;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cToken)
        {
            var message = update.Message;

            if (message is not null)
            {
                var chat = message.Chat;
                var command = message.Text;

                var answer = command switch
                {
                    BotSettings.StartCommand => await RegisterNewChat(chat),
                    BotSettings.EndCommand => await RemoveChat(chat),
                    _ => string.Empty,
                };

                if (string.IsNullOrEmpty(answer))
                {
                    answer = $"Invalid command: {command}";

                    _logger.Warn(answer);
                }

                await botClient.SendTextMessageAsync(message.Chat, answer, cancellationToken: cToken);
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cToken)
        {
            _logger.Warn(exception);

            return Task.CompletedTask;
        }


        private async Task<string> RegisterNewChat(Chat chat)
        {
            if (await _storage.TryAddTelegramChat(chat))
                return $"Hello. Your chat has been subscribed to AlgoServer.";
            else
                return "Your chat is already added.";
        }

        private async Task<string> RemoveChat(Chat chat)
        {
            if (await _storage.TryRemoveTelegramChat(chat))
                return $"Chat {chat.Username} has been unsubscribed";
            else
                return $"Chat {chat.Username} is already removed.";
        }
    }
}