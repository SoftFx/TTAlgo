using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.DS.Models.Exceptions;
using TickTrader.DedicatedServer.DS.Repository;
using TickTrader.Algo.Core.Metadata;

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
        public event Action<IPackage, ChangeAction> PackageChanged;

        public ConnectionErrorCodes TestAccount(string login, string server)
        {
            return TestAccount(login, null, server);
        }

        public ConnectionErrorCodes TestAccount(string login, string password, string server)
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                var acc = new ClientModel(SyncObj, _loggerFactory);
                acc.Change(server, login, password);
                return acc.TestConnection().Result;
            }
            else
            {
                var acc = FindAccount(login, server);
                if (acc == null)
                    throw new Exception();

                return acc.TestConnection().Result;
            }
        }


        public void AddAccount(string login, string password, string server)
        {
            lock (SyncObj)
            {
                var existing = FindAccount(login, server);
                if (existing != null)
                {
                    throw new DuplicateAccountException($"Account '{login}:{server}' already exists");
                }
                else
                {
                    var newAcc = new ClientModel();
                    InitAccount(newAcc);
                    newAcc.Change(server, login, password);
                    _accounts.Add(newAcc);
                    AccountChanged?.Invoke(newAcc, ChangeAction.Added);

                    Save();
                }
            }
        }

        public void RemoveAccount(string login, string server)
        {
            lock (SyncObj)
            {
                var acc = FindAccount(login, server);
                if (acc != null)
                {
                    _accounts.Remove(acc);

                    Save();

                    AccountChanged?.Invoke(acc, ChangeAction.Removed);
                }
            }
        }

        private void Init(ILoggerFactory loggerFactory)
        {
            SyncObj = new object();
            _loggerFactory = loggerFactory;
            _packageStorage = new PackageStorage(loggerFactory, SyncObj);
            _accounts.ForEach(InitAccount);
            _packageStorage.RemovingPackage += packageStorage_RemovingPackage;
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

        private ClientModel FindAccount(string login, string server)
        {
            return _accounts.FirstOrDefault(a => a.Username == login && a.Address == server);
        }

        public void RemoveAccount(string login, string server)
        {
            var acc = FindAccount(login, server);
            if (acc == null)
                throw new InvalidOperationException("Account not found!");
            _accounts.Remove(acc);
            DisposeAccount(acc);

            Save();

            AccountChanged?.Invoke(acc, ChangeAction.Removed);
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

        #region Repository API

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

        public ServerPluginRef[] GetAllPlugins()
        {
            return _packageStorage.GetAll()
                .SelectMany(p => p.Container.Plugins.Select(pRef => new ServerPluginRef(p.Name, pRef)))
                .ToArray();
        }

        public ServerPluginRef[] GetPluginsByType(AlgoTypes type)
        {
            return _packageStorage.GetAll()
                .SelectMany(p => p.Container.Plugins
                    .Where(pRef => pRef.Descriptor.AlgoLogicType == type)
                    .Select(pRef => new ServerPluginRef(p.Name, pRef)))
                .ToArray();
        }

        #endregion
    }

    public enum ChangeAction { Added, Removed, Modified }
}
