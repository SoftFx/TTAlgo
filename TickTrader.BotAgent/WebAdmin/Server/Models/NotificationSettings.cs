using Newtonsoft.Json;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class NotificationSettings
    {
        public TelegramSettings Telegram { get; set; }


        [JsonIgnore]
        public int MinDelay => Telegram.MessageDelay;

        [JsonIgnore]
        public bool Enable => Telegram.Enable;
    }


    public sealed class TelegramSettings
    {
        public bool Enable { get; set; }

        public string BotName { get; set; }

        public string BotToken { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MessageDelay { get; set; }

        [JsonIgnore]
        internal static TelegramSettings Default { get; } = new()
        {
            Enable = false,
            MessageDelay = 10000,
        };
    }
}
