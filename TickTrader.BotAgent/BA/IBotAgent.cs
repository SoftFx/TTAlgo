using System;
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

        Task<List<PackageInfo>> GetPackages();
        Task<PackageInfo> GetPackage(string package);
        Task UpdatePackage(byte[] fileContent, string fileName);
        Task<byte[]> DownloadPackage(PackageKey package);
        Task RemovePackage(string package);
        Task RemovePackage(PackageKey package);
        Task<List<PluginInfo>> GetAllPlugins();
        Task<List<PluginInfo>> GetPluginsByType(AlgoTypes type);
        Task<MappingCollectionInfo> GetMappingsInfo();
        Task<string> GetPackageReadPath(PackageKey package);
        Task<string> GetPackageWritePath(PackageKey package);

        event Action<PackageInfo, ChangeAction> PackageChanged;
        event Action<PackageInfo> PackageStateChanged;
        
        // -------- Account Management --------

        Task<List<AccountModelInfo>> GetAccounts();
        Task AddAccount(AccountKey key, string password);
        Task RemoveAccount(AccountKey key);
        Task ChangeAccount(AccountKey key, string password);
        Task ChangeAccountPassword(AccountKey key, string password);
        Task<Tuple<ConnectionErrorInfo, AccountMetadataInfo>> GetAccountMetadata(AccountKey key);
        Task<ConnectionErrorInfo> TestAccount(AccountKey accountId);
        Task<ConnectionErrorInfo> TestCreds(AccountKey accountId, string password);

        event Action<AccountModelInfo, ChangeAction> AccountChanged;
        event Action<AccountModelInfo> AccountStateChanged;

        // -------- Bot Management --------

        Task<List<BotModelInfo>> GetBots();
        Task<BotModelInfo> GetBotInfo(string botId);
        Task<IBotFolder> GetAlgoData(string botId);
        Task<string> GenerateBotId(string botDisplayName);
        Task<BotModelInfo> AddBot(AccountKey accountId, PluginConfig config);
        Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false);
        Task ChangeBotConfig(string botId, PluginConfig newConfig);
        Task StartBot(string botId);
        Task StopBotAsync(string botId);
        void AbortBot(string botId);
        Task<IBotLog> GetBotLog(string botId);

        Task<IAlertStorage> GetAlertStorage();

        event Action<BotModelInfo, ChangeAction> BotChanged;
        event Action<BotModelInfo> BotStateChanged;

        // -------- Server Management --------

        // TO DO : server start and stop should not be managed from WebAdmin

        Task InitAsync(IFdkOptionsProvider fdkOptionsProvider);

        Task ShutdownAsync();
    }

    public interface IBotFolder
    {
        Task<string> GetFolder();
        Task<IFile[]> GetFiles();
        Task Clear();
        Task<IFile> GetFile(string decodedFile);
        Task DeleteFile(string name);
        Task SaveFile(string name, byte[] bytes);
        Task<string> GetFileReadPath(string name);
        Task<string> GetFileWritePath(string name);
    }

    public enum LogEntryType { Info, Trading, Error, Custom, TradingSuccess, TradingFail, Alert }

    public interface ILogEntry
    {
        TimeKey TimeUtc { get; }
        LogEntryType Type { get; }
        string Message { get; }
    }

    public interface IBotLog : IBotFolder
    {
        Task<ILogEntry[]> GetMessages();
        Task<string> GetStatusAsync();
        Task<List<ILogEntry>> QueryMessagesAsync(DateTime from, int maxCount);
    }

    public interface IAlertEntry
    {
        TimeKey TimeUtc { get; }
        string Message { get; }
        string BotId { get; }
    }

    public interface IAlertStorage
    {
        Task<List<IAlertEntry>> QueryAlertsAsync(DateTime from, int maxCount);
    }

    public interface IFdkOptionsProvider
    {
        ConnectionOptions GetConnectionOptions();
    }
}
