using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotAgent.BA.Entities;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        // -------- Repository Management --------

        List<PackageInfo> GetPackages();
        PackageInfo UpdatePackage(byte[] fileContent, string fileName);
        void RemovePackage(string package);
        List<PluginInfo> GetAllPlugins();
        List<PluginInfo> GetPluginsByType(AlgoTypes type);

        event Action<PackageInfo, ChangeAction> PackageChanged;
        
        // -------- Account Management --------

        List<AccountInfo> GetAccounts();
        void AddAccount(AccountKey key, string password, bool useNewProtocol);
        void RemoveAccount(AccountKey key);
        void ChangeAccountPassword(AccountKey key, string password);
        void ChangeAccountProtocol(AccountKey key);
        ConnectionErrorCodes GetAccountMetadata(AccountKey key, out TradeMetadataInfo info);
        ConnectionErrorInfo TestAccount(AccountKey accountId);
        ConnectionErrorInfo TestCreds(string login, string password, string server, bool useNewProtocol);

        event Action<AccountInfo, ChangeAction> AccountChanged;
        event Action<AccountInfo> AccountStateChanged;

        // -------- Bot Management --------

        List<TradeBotInfo> GetTradeBots();
        TradeBotInfo GetBotInfo(string botId);
        IAlgoData GetAlgoData(string botId);
        string GenerateBotId(string botDisplayName);
        TradeBotInfo AddBot(AccountKey accountId, TradeBotConfig config);
        void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false);
        void ChangeBotConfig(string botId, TradeBotConfig newConfig);
        void StartBot(string botId);
        Task StopBotAsync(string botId);
        void AbortBot(string botId);
        IBotLog GetBotLog(string botId);

        event Action<TradeBotInfo, ChangeAction> BotChanged;
        event Action<TradeBotInfo> BotStateChanged;

        // -------- Server Management --------

        // TO DO : server start and stop should not be managed from WebAdmin

        Task ShutdownAsync();
    }

    public enum ConnectionStates { Offline, Connecting, Online, Disconnecting }
    public enum BotStates { Offline, Starting, Faulted, Online, Stopping, Broken, Reconnecting }

    public interface IAlgoData
    {
        string Folder { get; }
        IFile[] Files { get; }

        void Clear();
        IFile GetFile(string decodedFile);
        void DeleteFile(string name);
    }

    public enum LogEntryType { Info, Trading, Error, Custom, TradingSuccess, TradingFail }

    public interface ILogEntry
    {
        DateTime TimeUtc { get; }
        LogEntryType Type { get; }
        string Message { get; }
    }

    public interface IBotLog
    {
        IEnumerable<ILogEntry> Messages { get; }
        IFile GetFile(string file);
        string Status { get; }
        IFile[] Files { get; }
        void Clear();
        void DeleteFile(string file);
    }
}
