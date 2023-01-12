using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Server.Persistence;
using TickTrader.BotAgent.BA.Models;
using File = System.IO.File;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    internal sealed class NotificationStorage
    {
        private const string FileName = "notifications.storage";

        private readonly static string _filePath = Path.Combine(ServerModel.Environment.AppDataFolder, FileName);


        internal NotificationStorageEntity Settings { get; } = LoadSettings();


        public Task<bool> TryAddTelegramChat(Chat chat)
        {
            return ActionWrapper(() => Settings.Telegram.Chats.TryAdd(chat.Id, chat));
        }

        public Task<bool> TryRemoveTelegramChat(Chat chat)
        {
            return ActionWrapper(() => Settings.Telegram.Chats.TryRemove(chat.Id, out _));
        }


        private async Task<bool> ActionWrapper(Func<bool> action)
        {
            var ok = action.Invoke();

            if (ok)
            {
                using var fs = new FileStream(_filePath, FileMode.Create);
                using var sw = new StreamWriter(fs);

                var byteArr = JsonSerializer.SerializeToUtf8Bytes(Settings);
                var cipherStr = CipherV1Helper.Encrypt(CipherOptionsStorage.V1, byteArr);

                await sw.WriteAsync(cipherStr);
            }

            return ok;
        }

        private static NotificationStorageEntity LoadSettings()
        {
            if (File.Exists(_filePath))
            {
                using var fs = new FileStream(_filePath, FileMode.Open);
                using var sr = new StreamReader(fs);

                var rawStr = sr.ReadToEnd();
                var decryptStr = CipherV1Helper.Decrypt(CipherOptionsStorage.V1, rawStr);

                return JsonSerializer.Deserialize<NotificationStorageEntity>(decryptStr.ToArray());
            }

            return new NotificationStorageEntity();
        }
    }
}
