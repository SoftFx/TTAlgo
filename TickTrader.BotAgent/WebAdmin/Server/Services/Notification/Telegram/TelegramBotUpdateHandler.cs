using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    public class TelegramBotUpdateHandler : IUpdateHandler
    {
        private readonly Logger _logger = LogManager.GetLogger(nameof(TelegramBotUpdateHandler));

        internal ConcurrentDictionary<long, Chat> Chats { get; } = new();


        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cToken)
        {
            var message = update.Message;

            if (message is not null)
            {
                var chat = message.Chat;
                var command = message.Text;

                var answer = command switch
                {
                    BotSettings.StartCommand => RegisterNewChat(chat),
                    BotSettings.EndCommand => RemoveChat(chat),
                    _ => string.Empty,
                };

                if (string.IsNullOrEmpty(answer))
                {
                    answer = $"Invalid command: {command}";

                    _logger.Warn(answer);
                }

                return botClient.SendTextMessageAsync(message.Chat, answer, cancellationToken: cToken);
            }

            return Task.CompletedTask;
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cToken)
        {
            _logger.Warn(exception);

            return Task.CompletedTask;
        }


        private string RegisterNewChat(Chat chat)
        {
            if (Chats.TryAdd(chat.Id, chat))
                return $"Hello. Your chat has been subscribed to AlgoServer.";
            else
                return "Your chat is already added.";
        }

        private string RemoveChat(Chat chat)
        {
            if (Chats.TryRemove(chat.Id, out _))
                return $"Chat {chat.Username} has been unsubscribed";
            else
                return $"Chat {chat.Username} is already removed.";
        }
    }
}