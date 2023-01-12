using System.Collections.Concurrent;
using Telegram.Bot.Types;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    internal sealed class NotificationStorageEntity
    {
        public TelegramStorage Telegram { get; init; } = new(); // init for deserialization
    }


    internal sealed class TelegramStorage
    {
        public ConcurrentDictionary<long, Chat> Chats { get; init; } = new();
    }
}
