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

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "server.config")]
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
        public IEnumerable<ITradeBot> TradeBots => _accounts.SelectMany(a => a.Bots);
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
                    var newAcc = new ClientModel(SyncObj, _loggerFactory);
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
            _packageStorage = new PackageStorage(loggerFactory);
            _accounts.ForEach(a => a.Init(SyncObj, loggerFactory));
        }

        private ClientModel FindAccount(string login, string server)
        {
            return _accounts.FirstOrDefault(a => a.Username == login && a.Address == server);
        }

        #region Serialization

        private void Save()
        {
            var settings = new XmlWriterSettings { Indent = true };
            DataContractSerializer serializer = new DataContractSerializer(typeof(ServerModel));
            using (var writer = XmlWriter.Create(cfgFilePath, settings))
                serializer.WriteObject(writer, this);
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


        #endregion
    }

    public enum ChangeAction { Added, Removed, Modified }
}
