using System;
using System.IO;

namespace TickTrader.FeedStorage.Api.Tests
{
    public abstract class StorageSettings
    {
        internal const string DatabaseFolder = "StorageFolder";

        private static int _number;


        public string FolderPath { get; }


        internal StorageSettings()
        {
            FolderPath = Path.Combine(Environment.CurrentDirectory, $"{DatabaseFolder}_{++_number}");
        }

        internal virtual string GetExpectedPath()
        {
            return FolderPath;
        }
    }


    public sealed class CustomStorageSettings : StorageSettings, ICustomStorageSettings
    {
    }


    public sealed class OnlineStorageSettings : StorageSettings, IOnlineStorageSettings
    {
        internal const string DefaultLogin = "TestUser";
        internal const string DefaultServer = "TestServer";
        internal const FeedStorageFolderOptions DefaultOptions = FeedStorageFolderOptions.ServerClientHierarchy;


        public string Login { get; set; } = DefaultLogin;

        public string Server { get; set; } = DefaultServer;

        public FeedStorageFolderOptions Options { get; set; } = FeedStorageFolderOptions.ServerClientHierarchy;


        internal override string GetExpectedPath()
        {
            switch (Options)
            {
                case FeedStorageFolderOptions.ServerHierarchy:
                    return Path.Combine(FolderPath, Server);
                case FeedStorageFolderOptions.ServerClientHierarchy:
                    return Path.Combine(FolderPath, Server, Login);
                default:
                    return base.GetExpectedPath();
            }
        }
    }
}
