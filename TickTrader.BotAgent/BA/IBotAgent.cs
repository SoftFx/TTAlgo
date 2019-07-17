﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        // -------- Repository Management --------

        List<PackageInfo> GetPackages();
        PackageInfo GetPackage(string package);
        void UpdatePackage(byte[] fileContent, string fileName);
        byte[] DownloadPackage(PackageKey package);
        void RemovePackage(string package);
        void RemovePackage(PackageKey package);
        List<PluginInfo> GetAllPlugins();
        List<PluginInfo> GetPluginsByType(AlgoTypes type);
        MappingCollectionInfo GetMappingsInfo();
        string GetPackageReadPath(PackageKey package);
        string GetPackageWritePath(PackageKey package);

        event Action<PackageInfo, ChangeAction> PackageChanged;
        event Action<PackageInfo> PackageStateChanged;
        
        // -------- Account Management --------

        List<AccountModelInfo> GetAccounts();
        void AddAccount(AccountKey key, string password, bool useNewProtocol);
        void RemoveAccount(AccountKey key);
        void ChangeAccount(AccountKey key, string password, bool useNewProtocol);
        void ChangeAccountPassword(AccountKey key, string password);
        void ChangeAccountProtocol(AccountKey key);
        ConnectionErrorInfo GetAccountMetadata(AccountKey key, out AccountMetadataInfo info);
        ConnectionErrorInfo TestAccount(AccountKey accountId);
        ConnectionErrorInfo TestCreds(AccountKey accountId, string password, bool useNewProtocol);

        event Action<AccountModelInfo, ChangeAction> AccountChanged;
        event Action<AccountModelInfo> AccountStateChanged;

        // -------- Bot Management --------

        List<BotModelInfo> GetTradeBots();
        BotModelInfo GetBotInfo(string botId);
        IBotFolder GetAlgoData(string botId);
        string GenerateBotId(string botDisplayName);
        BotModelInfo AddBot(AccountKey accountId, PluginConfig config);
        void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false);
        void ChangeBotConfig(string botId, PluginConfig newConfig);
        void StartBot(string botId);
        Task StopBotAsync(string botId);
        void AbortBot(string botId);
        IBotLog GetBotLog(string botId);

        event Action<BotModelInfo, ChangeAction> BotChanged;
        event Action<BotModelInfo> BotStateChanged;

        // -------- Server Management --------

        // TO DO : server start and stop should not be managed from WebAdmin

        Task InitAsync(IFdkOptionsProvider fdkOptionsProvider);

        Task ShutdownAsync();
    }

    public interface IBotFolder
    {
        string Folder { get; }
        IFile[] Files { get; }

        void Clear();
        IFile GetFile(string decodedFile);
        void DeleteFile(string name);
        void SaveFile(string name, byte[] bytes);
        string GetFileReadPath(string name);
        string GetFileWritePath(string name);
    }

    public enum LogEntryType { Info, Trading, Error, Custom, TradingSuccess, TradingFail }

    public interface ILogEntry
    {
        TimeKey TimeUtc { get; }
        LogEntryType Type { get; }
        string Message { get; }
    }

    public interface IBotLog : IBotFolder
    {
        IEnumerable<ILogEntry> Messages { get; }
        string Status { get; }
    }

    public interface IFdkOptionsProvider
    {
        ConnectionOptions GetConnectionOptions();
    }
}