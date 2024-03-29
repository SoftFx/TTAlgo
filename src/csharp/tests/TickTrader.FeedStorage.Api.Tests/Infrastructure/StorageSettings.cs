﻿using System;
using System.IO;
using System.Threading;

namespace TickTrader.FeedStorage.Api.Tests
{
    public abstract class StorageSettings
    {
        internal const string DatabaseFolder = "StorageFolder";

        [ThreadStatic]
        private static int _number;

        public string FolderPath { get; }


        internal StorageSettings()
        {
            _number = Interlocked.Increment(ref _number);
            FolderPath = Path.Combine(Environment.CurrentDirectory, $"{DatabaseFolder}_{_number}_{Environment.CurrentManagedThreadId}");
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
        internal const StorageFolderOptions DefaultOptions = StorageFolderOptions.ServerClientHierarchy;


        public string Login { get; set; } = DefaultLogin;

        public string Server { get; set; } = DefaultServer;

        public StorageFolderOptions Options { get; set; } = StorageFolderOptions.ServerClientHierarchy;


        internal override string GetExpectedPath()
        {
            switch (Options)
            {
                case StorageFolderOptions.ServerHierarchy:
                    return Path.Combine(FolderPath, Server);
                case StorageFolderOptions.ServerClientHierarchy:
                    return Path.Combine(FolderPath, Server, Login);
                default:
                    return base.GetExpectedPath();
            }
        }
    }
}
