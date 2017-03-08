
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.DedicatedServer.DS.Models;

namespace TickTrader.DedicatedServer.DS
{
    public interface IDedicatedServer
    {
        IPackage AddPackage(byte[] fileContent, string fileName);
        IPackage[] GetPackages();
        void RemovePackage(string package);

        IEnumerable<IAccount> Accounts { get; }
        IEnumerable<ITradeBot> TradeBots { get; }
        event Action<IAccount, ChangeAction> AccountChanged;
        event Action<ITradeBot, ChangeAction> BotChanged;

        void AddAccount(string login, string password, string server);
    }

    public interface IAccount
    {
        string Address { get; }
        string Username { get; }

        Task<ConnectionErrorCodes> TestConnection();
    }

    public interface ITradeBot
    {
    }

    public interface IPackage
    {
        string Name { get; }
        DateTime Created { get; }
        bool IsValid { get; }
        PluginContainer Container { get; }
    }
}
