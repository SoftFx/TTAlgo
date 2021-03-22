using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Domain;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        // -------- Repository Management --------

        Task<List<PackageInfo>> GetPackages();
        Task<PackageInfo> GetPackage(string package);
        Task UpdatePackage(byte[] fileContent, string fileName);
        Task<byte[]> DownloadPackage(string packageId);
        Task RemovePackage(string packageId);
        Task<List<PluginInfo>> GetAllPlugins();
        Task<List<PluginInfo>> GetPluginsByType(Metadata.Types.PluginType type);
        Task<MappingCollectionInfo> GetMappingsInfo();
        Task<string> GetPackageReadPath(string packageId);
        Task<string> GetPackageWritePath(string packageId);

        event Action<PackageInfo, ChangeAction> PackageChanged;
        event Action<PackageStateUpdate> PackageStateChanged;
        
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
        event Action<AccountStateUpdate> AccountStateChanged;

        // -------- Bot Management --------

        Task<List<PluginModelInfo>> GetBots();
        Task<PluginModelInfo> GetBotInfo(string botId);
        Task<IBotFolder> GetAlgoData(string botId);
        Task<string> GenerateBotId(string botDisplayName);
        Task<PluginModelInfo> AddBot(AccountKey accountId, PluginConfig config);
        Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false);
        Task ChangeBotConfig(string botId, PluginConfig newConfig);
        Task StartBot(string botId);
        Task StopBotAsync(string botId);
        void AbortBot(string botId);
        Task<IBotLog> GetBotLog(string botId);

        Task<IAlertStorage> GetAlertStorage();

        event Action<PluginModelInfo, ChangeAction> BotChanged;
        event Action<PluginStateUpdate> BotStateChanged;

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

    public interface ILogEntry
    {
        Timestamp TimeUtc { get; }
        PluginLogRecord.Types.LogSeverity Severity { get; }
        string Message { get; }
    }

    public interface IBotLog : IBotFolder
    {
        Task<ILogEntry[]> GetMessages();
        Task<string> GetStatusAsync();
        Task<List<ILogEntry>> QueryMessagesAsync(Timestamp from, int maxCount);
    }

    public interface IAlertEntry
    {
        Timestamp TimeUtc { get; }
        string Message { get; }
        string BotId { get; }
    }

    public interface IAlertStorage
    {
        Task<List<IAlertEntry>> QueryAlertsAsync(Timestamp from, int maxCount);
    }

    public interface IFdkOptionsProvider
    {
        ConnectionOptions GetConnectionOptions();
    }
}
