using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    public sealed class OnlineStorageSettings : IOnlineStorageSettings
    {
        public string Login { get; set; }

        public string Server { get; set; }

        public string FolderPath { get; set; }

        public StorageFolderOptions Options { get; set; }
    }


    public sealed class CustomStorageSettings : ICustomStorageSettings
    {
        public string FolderPath { get; set; }
    }
}
