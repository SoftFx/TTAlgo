using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.DS.Repository;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.DedicatedServer.DS.Exceptions;
using System.Threading.Tasks;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "server.config", Namespace = "")]
    public class ServerModel : IDedicatedServer
    {
        private const string cfgFilePath = "server.config.xml";

        [DataMember(Name = "accounts")]
        private List<ClientModel> _accounts = new List<ClientModel>();
        private ILogger<ServerModel> _logger;
        private ILoggerFactory _loggerFactory;

        private ServerModel(ILoggerFactory loggerFactory)
        {
            Init(loggerFactory);
        }

        public object SyncObj { get; private set; }

        public IEnumerable<IAccount> Accounts { get { lock (SyncObj) { return _accounts.ToArray(); } } }
        public IEnumerable<ITradeBot> TradeBots => _accounts.SelectMany(a => a.TradeBots);
        public event Action<IAccount, ChangeAction> AccountChanged;
        public event Action<ITradeBot, ChangeAction> BotChanged;
        public event Action<ITradeBot, ChangeAction> BotStateChanged;
        public event Action<IPackage, ChangeAction> PackageChanged;

        private void Init(ILoggerFactory loggerFactory)
        {
            SyncObj = new object();
            _loggerFactory = loggerFactory;
            _packageStorage = new PackageStorage(loggerFactory, SyncObj);
            _accounts.ForEach(InitAccount);
            _packageStorage.RemovingPackage += packageStorage_RemovingPackage;
        }

        public void Close()
        {
        }

        #region Account management

        public ConnectionErrorCodes TestAccount(AccountKey accountId)
        {
            Task<ConnectionErrorCodes> testTask;

            lock (SyncObj)
                testTask = GetAccountOrThrow(accountId).TestConnection();

            return testTask.Result;
        }

        public ConnectionErrorCodes TestCreds(string login, string password, string server)
        {
            Task<ConnectionErrorCodes> testTask;

            var acc = new ClientModel();
            acc.Change(server, login, password);
            lock (SyncObj)
            {
                InitAccount(acc);
                testTask = acc.TestConnection();
            }

            return testTask.Result;
        }

        public void AddAccount(AccountKey accountId, string password)
        {
            lock (SyncObj)
            {
                var existing = FindAccount(accountId);
                if (existing != null)
                    throw new DuplicateAccountException($"Account '{accountId.Login}:{accountId.Server}' already exists");
                else
                {
                    var newAcc = new ClientModel();
                    InitAccount(newAcc);
                    newAcc.Change(accountId.Login, accountId.Server, password);
                    _accounts.Add(newAcc);
                    AccountChanged?.Invoke(newAcc, ChangeAction.Added);

                    Save();
                }
            }
        }

        public void RemoveAccount(AccountKey accountId)
        {
            lock (SyncObj)
            {
                var acc = FindAccount(accountId);
                if (acc != null)
                {
                    _accounts.Remove(acc);
                    DisposeAccount(acc);

                    Save();

                    AccountChanged?.Invoke(acc, ChangeAction.Removed);
                }
            }
        }

        private ClientModel FindAccount(string login, string server)
        {
            return _accounts.FirstOrDefault(a => a.Username == login && a.Address == server);
        }

        private ClientModel FindAccount(AccountKey key)
        {
            return _accounts.FirstOrDefault(a => a.Username == key.Login && a.Address == key.Server);
        }

        private ClientModel GetAccountOrThrow(AccountKey key)
        {
            var acc = FindAccount(key);
            if (acc == null)
                throw new AccountNotFoundException("Account with login '" + key.Login + "' and server address '" + key.Server + "' does not exist!");
            return acc;
        }

        private void InitAccount(ClientModel acc)
        {
            acc.Init(SyncObj, _loggerFactory, _packageStorage.Get);
            acc.Changed += Acc_Changed;
        }

        private void DisposeAccount(ClientModel acc)
        {
            acc.Changed -= Acc_Changed;
        }

        private void Acc_Changed(ClientModel acc)
        {
            Save();
        }

        #endregion

        #region Bot management

        public ITradeBot AddBot(string botId, AccountKey accountId, PluginKey pluginId, PluginConfig botConfig)
        {
            lock (SyncObj)
                return GetAccountOrThrow(accountId).AddBot(botId, pluginId, botConfig);
        }

        public void RemoveBot(string botId)
        {
            lock (SyncObj)
                TradeBots.FirstOrDefault(b => b.Id == botId)?.Account.RemoveBot(botId);
        }

        public string AutogenerateBotId(string botDescriptorName)
        {
            lock (SyncObj)
            {
                HashSet<string> idSet = new HashSet<string>(TradeBots.Select(b => b.Id));

                int seed = 1;

                while (true)
                {
                    var botId = botDescriptorName + " " + seed;
                    if (!idSet.Contains(botId))
                        return botId;

                    seed++;
                }
            }
        }

        #endregion

        #region Serialization

        private void Save()
        {
            try
            {
                var settings = new XmlWriterSettings { Indent = true };
                DataContractSerializer serializer = new DataContractSerializer(typeof(ServerModel));
                using (var writer = XmlWriter.Create(cfgFilePath, settings))
                    serializer.WriteObject(writer, this);
            }
            catch (Exception ex)
            {
                // TO DO : log
            }
        }

        public static ServerModel Load(ILoggerFactory loggerFactory)
        {
            try
            {
                using (var stream = File.OpenRead(cfgFilePath))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ServerModel));
                    var server = (ServerModel)serializer.ReadObject(stream);
                    server.Init(loggerFactory);
                    return server;
                }
            }
            catch (FileNotFoundException)
            {
                return new ServerModel(loggerFactory);
            }
        }

        #endregion

        #region Repository management

        private PackageStorage _packageStorage;

        public IPackage AddPackage(byte[] fileContent, string fileName)
        {
            var newPackage = _packageStorage.Add(fileContent, fileName);

            PackageChanged?.Invoke(newPackage, ChangeAction.Added);

            return newPackage;
        }

        public IPackage[] GetPackages()
        {
            return _packageStorage.GetAll();
        }

        public void RemovePackage(string package)
        {
            var dPackage = _packageStorage.Get(package);
            if (dPackage != null)
            {
                _packageStorage.Remove(package);

                PackageChanged?.Invoke(dPackage, ChangeAction.Removed);
            }
        }

        public PlguinInfo[] GetAllPlugins()
        {
            lock (SyncObj)
            {
                return _packageStorage.GetAll()
                    .SelectMany(p => p.GetPlugins())
                    .ToArray();
            }
        }

        public PlguinInfo[] GetPluginsByType(AlgoTypes type)
        {
            lock (SyncObj)
            {
                return _packageStorage.GetAll()
                .SelectMany(p => p.GetPluginsByType(type))
                .ToArray();
            }
        }

        private bool packageStorage_RemovingPackage(PackageModel pckg)
        {
            var hasRunningBots = _accounts.SelectMany(a => a.TradeBots).Any(b => b.IsRunning);
            if (hasRunningBots)
                return false;

            foreach (var acc in _accounts)
                acc.RemoveBotsFromPackage(pckg);

            Save();

            return true;
        }

        #endregion
    }

    public enum ChangeAction { Added, Removed, Modified }
}
