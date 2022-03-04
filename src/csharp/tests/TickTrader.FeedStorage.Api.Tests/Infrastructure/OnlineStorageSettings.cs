using System;
using System.IO;

namespace TickTrader.FeedStorage.Api.Tests
{
    internal sealed class OnlineStorageSettings : IOnlineStorageSettings
    {
        private static int _number;

        internal const string DefaultLogin = "TestUser";
        internal const string DefaultServer = "TestServer";
        internal const FeedStorageFolderOptions DefaultOptions = FeedStorageFolderOptions.ServerClientHierarchy;


        public string Login { get; set; } = DefaultLogin;

        public string Server { get; set; } = DefaultServer;

        public string FolderPath { get; set; }

        public FeedStorageFolderOptions Options { get; set; } = FeedStorageFolderOptions.ServerClientHierarchy;


        internal OnlineStorageSettings()
        {
            FolderPath = Path.Combine(Environment.CurrentDirectory, $"{TestsBase.DatabaseFolder}_{++_number}");
        }


        internal string GetExpectedPath()
        {
            switch (Options)
            {
                case FeedStorageFolderOptions.ServerHierarchy:
                    return Path.Combine(FolderPath, Server);
                case FeedStorageFolderOptions.ServerClientHierarchy:
                    return Path.Combine(FolderPath, Server, Login);
                default:
                    return FolderPath;
            }
        }
    }
}
