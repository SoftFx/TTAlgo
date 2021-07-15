using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        // -------- Repository Management --------

        Task<PackageListSnapshot> GetPackageSnapshot();
        Task<bool> PackageWithNameExists(string pkgName);
        Task UploadPackage(UploadPackageRequest request, string pkgFilePath);
        Task<byte[]> DownloadPackage(string packageId);
        Task RemovePackage(RemovePackageRequest request);
        Task<MappingCollectionInfo> GetMappingsInfo();

        event Action<PackageUpdate> PackageChanged;
        event Action<PackageStateUpdate> PackageStateChanged;
        
        // -------- Account Management --------

        Task<AccountListSnapshot> GetAccounts();
        Task AddAccount(AddAccountRequest request);
        Task RemoveAccount(RemoveAccountRequest request);
        Task ChangeAccount(ChangeAccountRequest request);
        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);
        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);
        Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request);

        event Action<AccountModelUpdate> AccountChanged;
        event Action<AccountStateUpdate> AccountStateChanged;

        // -------- Bot Management --------

        Task<PluginListSnapshot> GetBots();
        Task<PluginModelInfo> GetBotInfo(string botId);
        Task<IBotFolder> GetAlgoData(string botId);
        Task<string> GenerateBotId(string botDisplayName);
        Task AddBot(AddPluginRequest request);
        Task RemoveBot(RemovePluginRequest request);
        Task ChangeBotConfig(ChangePluginConfigRequest request);
        Task StartBot(StartPluginRequest request);
        Task StopBotAsync(StopPluginRequest request);
        Task<IBotLog> GetBotLog(string botId);
        Task<PluginLogRecord[]> GetBotLogs(PluginLogsRequest request);
        Task<string> GetBotStatus(PluginStatusRequest request);

        Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request);

        event Action<PluginModelUpdate> BotChanged;
        event Action<PluginStateUpdate> BotStateChanged;

        // -------- Server Management --------

        Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);
        Task ClearPluginFolder(ClearPluginFolderRequest request);
        Task DeletePluginFile(DeletePluginFileRequest request);
        Task<string> GetPluginFileReadPath(DownloadPluginFileRequest request);
        Task<string> GetPluginFileWritePath(UploadPluginFileRequest request);

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
