using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotAgent.BA.Info;
using TickTrader.BotAgent.BA.Models;

namespace TickTrader.BotAgent.BA
{
    public interface IBotAgent
    {
        IPackage UpdatePackage(byte[] fileContent, string fileName);
        IPackage[] GetPackages();
        void RemovePackage(string package);
        PluginInfo[] GetAllPlugins();
        PluginInfo[] GetPluginsByType(AlgoTypes type);

        IEnumerable<IAccount> Accounts { get; }
        IEnumerable<ITradeBot> TradeBots { get; }
        event Action<IAccount, ChangeAction> AccountChanged;

        Task ShutdownAsync();

        event Action<ITradeBot, ChangeAction> BotChanged;
        event Action<ITradeBot> BotStateChanged;
        event Action<IPackage, ChangeAction> PackageChanged;

        string AutogenerateBotId(string botDisplayName);

        void AddAccount(AccountKey key, string password);
        void RemoveAccount(AccountKey key);
        void ChangeAccountPassword(AccountKey key, string password);
        ConnectionErrorCodes TestAccount(AccountKey accountId);
        ConnectionErrorCodes TestCreds(string login, string password, string server);

        ConnectionErrorCodes GetAccountInfo(AccountKey key, out ConnectionInfo info);

        ITradeBot AddBot(TradeBotModelConfig config);

        void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false);
    }

    public interface IAccount
    {
        string Address { get; }
        string Username { get; }
        ConnectionStates ConnectionState { get; }
        IEnumerable<ITradeBot> TradeBots { get; }

        Task<ConnectionErrorCodes> TestConnection();
        void ChangePassword(string password);

        ITradeBot AddBot(TradeBotModelConfig config);
        void RemoveBot(string botId, bool cleanLog = true, bool cleanAlgoData = true);
        Task ShutdownAsync();

    }

    public enum ConnectionStates { Offline, Connecting, Online, Disconnecting }
    public enum BotStates { Offline, Starting, Faulted, Online, Stopping, Broken, Reconnecting }

    public interface ITradeBot
    {
        string Id { get; }
        bool Isolated { get; }
        bool IsRunning { get; }
        string FaultMessage { get; }
        IBotLog Log { get; }
        IAlgoData AlgoData { get; }
        IAccount Account { get; }
        PluginPermissions Permissions { get; }
        PluginConfig Config { get; }
        PackageModel Package { get; }
        string PackageName { get; }
        string Descriptor { get; }
        string BotName { get; }
        BotStates State { get; }

        void Abort();
        void Configurate(TradeBotModelConfig config);
        void Start();
        Task StopAsync();
    }

    public interface IPackage
    {
        string Name { get; }
        DateTime Created { get; }
        bool IsValid { get; }

        bool NameEquals(string name);
        IEnumerable<PluginInfo> GetPluginsByType(AlgoTypes type);
    }

    public enum LogEntryType { Info, Trading, Error, Custom }

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
        string Folder { get; }
        string Status { get; }
        IFile[] Files { get; }
        event Action<string> StatusUpdated;
        void Clear();
        void DeleteFile(string file);
    }

    public class PluginInfo
    {
        public PluginInfo(PluginKey key, AlgoPluginDescriptor descriptor)
        {
            Id = key;
            Descriptor = descriptor;
        }

        public PluginKey Id { get; }
        public AlgoPluginDescriptor Descriptor { get; }
    }

    public class PluginKey
    {
        public PluginKey(string package, string id)
        {
            PackageName = package;
            DescriptorId = id;
        }

        public string PackageName { get; private set; }
        public string DescriptorId { get; private set; }
    }

    public class AccountKey
    {
        public AccountKey(string login, string server)
        {
            Login = login;
            Server = server;
        }

        public string Login { get; private set; }
        public string Server { get; private set; }
    }
}
