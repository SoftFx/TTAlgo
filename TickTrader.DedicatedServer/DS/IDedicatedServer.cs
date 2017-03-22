
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.DS.Models;

namespace TickTrader.DedicatedServer.DS
{
    public interface IDedicatedServer
    {
        IPackage AddPackage(byte[] fileContent, string fileName);
        IPackage[] GetPackages();
        void RemovePackage(string package);
        ServerPluginRef[] GetAllPlugins();
        ServerPluginRef[] GetPluginsByType(AlgoTypes type);

        IEnumerable<IAccount> Accounts { get; }
        IEnumerable<ITradeBot> TradeBots { get; }
        event Action<IAccount, ChangeAction> AccountChanged;
        event Action<ITradeBot, ChangeAction> BotChanged;
        event Action<IPackage, ChangeAction> PackageChanged;

        string AutogenerateBotId(string botDescriptorName);

        void AddAccount(string login, string password, string server);
        void RemoveAccount(string login, string server);
        ConnectionErrorCodes TestAccount(string login, string server);
        ConnectionErrorCodes TestAccount(string login, string password, string server);
    }

    public interface IAccount
    {
        string Address { get; }
        string Username { get; }
        ConnectionStates ConnectionState { get; }
        IEnumerable<ITradeBot> TradeBots { get; }

        Task<ConnectionErrorCodes> TestConnection();

        ITradeBot AddBot(string botId, string packageName, PluginSetup setup);
        void RemoveBot(string botId);
    }

    public enum ConnectionStates { Offline, Connecting, Online, Disconnecting }
    public enum BotStates { Offline, Started, Initializing, Faulted, Online, Stopping }

    public interface ITradeBot
    {
        string Id { get; }
        bool IsRunning { get; }
        IBotLog Log { get; }
        BotStates State { get; }
        void Start();
        Task StopAsync();
    }

    public interface IPackage
    {
        string Name { get; }
        DateTime Created { get; }
        bool IsValid { get; }

        IEnumerable<ServerPluginRef> GetPluginsByType(AlgoTypes type);
    }

    public interface IBotLog
    {
        string Status { get; }
        event Action<string> StatusUpdated;
    }

    public class ServerPluginRef
    {
        public ServerPluginRef(string pckg, AlgoPluginRef pRef)
        {
            PackageName = pckg;
            Ref = pRef;
        }

        public string PackageName { get; }
        public AlgoPluginRef Ref { get; }
    }
}
