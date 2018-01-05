using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.Algo.Common.Model;
using TickTrader.BotAgent.BA.Repository;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotAgent.BA.Exceptions;
using System.Threading.Tasks;
using TickTrader.BotAgent.Infrastructure;
using TickTrader.BotAgent.BA.Info;
using TickTrader.BotAgent.Extensions;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "server.config", Namespace = "")]
    public class ServerModel : IBotAgent
    {
        private static readonly EnvService envService = new EnvService();
        private static readonly string cfgFilePath = Path.Combine(envService.AppFolder, "server.config.xml");

        [DataMember(Name = "accounts")]
        private List<ClientModel> _accounts = new List<ClientModel>();
        private ILogger<ServerModel> _logger;
        private ILoggerFactory _loggerFactory;
        private Dictionary<string, TradeBotModel> _allBots;
        private BotIdHelper botIdHelper;

        private ServerModel(ILoggerFactory loggerFactory)
        {
            Init(loggerFactory);
        }

        public static EnvService Environment => envService;

        public static string GetWorkingFolderFor(string botId)
        {
            return Path.Combine(Environment.AlgoWorkingFolder, botId.Escape());
        }

        public object SyncObj { get; private set; }

        private void Init(ILoggerFactory loggerFactory)
        {
            SyncObj = new object();
            botIdHelper = new BotIdHelper();
            _allBots = new Dictionary<string, TradeBotModel>();
            _logger = loggerFactory.CreateLogger<ServerModel>();
            _loggerFactory = loggerFactory;
            _packageStorage = new PackageStorage(loggerFactory, SyncObj);
            _accounts.ForEach(InitAccount);
        }

        public void Close()
        {
        }

        #region Account management

        public IEnumerable<IAccount> Accounts { get { lock (SyncObj) { return _accounts.ToArray(); } } }

        public event Action<IAccount, ChangeAction> AccountChanged;

        public ConnectionErrorCodes TestAccount(AccountKey accountId)
        {
            Task<ConnectionErrorCodes> testTask;

            lock (SyncObj)
                testTask = GetAccountOrThrow(accountId).TestConnection();

            return testTask.Result;
        }

        public ConnectionErrorCodes TestCreds(string login, string password, string server, bool useNewProtocol)
        {
            Task<ConnectionErrorCodes> testTask;

            var acc = new ClientModel(server, login, password, useNewProtocol);
            acc.Init(SyncObj, _loggerFactory, _packageStorage);
            lock (SyncObj)
            {
                InitAccount(acc);
                testTask = acc.TestConnection();
            }

            var testResult = testTask.Result;

            if (!acc.ShutdownAsync().Wait(5000))
            {
                _logger.LogError($"Can't stop test connection to {server} - {login} via {(useNewProtocol ? "SFX" : "FIX")}");
            }

            return testResult;
        }

        public ConnectionErrorCodes GetAccountInfo(AccountKey key, out ConnectionInfo info)
        {
            Task<ConnectionInfo> task;

            lock (SyncObj) task = GetAccountOrThrow(key).GetInfo();

            try
            {
                info = task.Result;
                return ConnectionErrorCodes.None;
            }
            catch (CommunicationException ex)
            {
                info = null;
                return ex.FdkCode;
            }

        }

        public void AddAccount(AccountKey accountId, string password, bool useNewProtocol)
        {
            lock (SyncObj)
            {
                Validate(accountId.Login, accountId.Server, password);

                var existing = FindAccount(accountId);
                if (existing != null)
                    throw new DuplicateAccountException($"Account '{accountId.Login}:{accountId.Server}' already exists");
                else
                {
                    var newAcc = new ClientModel(accountId.Server, accountId.Login, password, useNewProtocol);
                    InitAccount(newAcc);
                    _accounts.Add(newAcc);
                    AccountChanged?.Invoke(newAcc, ChangeAction.Added);
                    Save();
                }
            }
        }

        private void Validate(string login, string server, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidAccountException("Login, password and server are required");
            }
        }

        public void RemoveAccount(AccountKey accountId)
        {
            ClientModel acc;
            lock (SyncObj)
            {
                acc = FindAccount(accountId);
                if (acc != null)
                {
                    if (acc.HasRunningBots)
                        throw new AccountLockedException("Account cannot be removed! Stop all bots and try again.");

                    acc.RemoveAllBots();
                }
            }

            if (!acc.ShutdownAsync().Wait(5000))
            {
                throw new BAException($"Can't stop connection to {acc.Address} - {acc.Username} via {(acc.UseNewProtocol ? "SFX" : "FIX")}");
            }

            lock (SyncObj)
            {
                if (acc != null)
                {
                    _accounts.Remove(acc);
                    DisposeAccount(acc);

                    Save();

                    AccountChanged?.Invoke(acc, ChangeAction.Removed);
                }
            }
        }

        public void ChangeAccountPassword(AccountKey key, string password)
        {
            lock (SyncObj)
            {
                Validate(key.Login, key.Server, password);

                var acc = GetAccountOrThrow(key);
                acc.ChangePassword(password);
            }
        }

        public void ChangeAccountProtocol(AccountKey key)
        {
            lock (SyncObj)
            {
                var acc = GetAccountOrThrow(key);
                acc.ChangeProtocol();
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
            acc.BotValidation += OnBotValidation;
            acc.BotInitialized += OnBotInitialized;
            acc.Init(SyncObj, _loggerFactory, _packageStorage);
            acc.Changed += OnAccountChanged;
            acc.BotChanged += OnBotChanged;
            acc.BotStateChanged += OnBotStateChanged;
        }

        private void DeinitAccount(ClientModel acc)
        {
            acc.BotValidation -= OnBotValidation;
            acc.Changed -= OnAccountChanged;
            acc.BotChanged -= OnBotChanged;
            acc.BotStateChanged -= OnBotStateChanged;
        }

        private void OnBotStateChanged(TradeBotModel bot)
        {
            this.BotStateChanged?.Invoke(bot);
        }

        private void DisposeAccount(ClientModel acc)
        {
            acc.Changed -= OnAccountChanged;
        }

        private void OnAccountChanged(ClientModel acc)
        {
            Save();
        }

        private void OnBotChanged(TradeBotModel bot, ChangeAction changeAction)
        {
            if (changeAction == ChangeAction.Removed)
                _allBots.Remove(bot.Id);

            Save();
            BotChanged?.Invoke(bot, changeAction);
        }

        private void OnBotValidation(TradeBotModel bot)
        {
            if (!botIdHelper.Validate(bot.Id))
                throw new InvalidBotException($"The instance Id must be no more than {botIdHelper.MaxLength} characters and consist of characters: a-z A-Z 0-9 and space");
            if (_allBots.ContainsKey(bot.Id))
                throw new DuplicateBotIdException("Bot with id '" + bot.Id + "' already exist!");
        }

        private void OnBotInitialized(TradeBotModel bot)
        {
            _allBots.Add(bot.Id, bot);
        }

        #endregion

        #region Bot management

        public IEnumerable<ITradeBot> TradeBots => _allBots.Values;

        public event Action<ITradeBot, ChangeAction> BotChanged;
        public event Action<ITradeBot> BotStateChanged;

        public ITradeBot AddBot(TradeBotModelConfig config)
        {
            lock (SyncObj)
                return GetAccountOrThrow(config.Account).AddBot(config);
        }

        public void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            lock (SyncObj)
                _allBots.GetOrDefault(botId)?.Account.RemoveBot(botId, cleanLog, cleanAlgoData);
        }

        public string AutogenerateBotId(string botDescriptorName)
        {
            lock (SyncObj)
            {
                int seed = 1;

                while (true)
                {
                    var botId = botIdHelper.BuildId(botDescriptorName, seed.ToString());
                    if (!_allBots.ContainsKey(botId))
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
                _logger.LogError("Failed to save config file! {0}", ex);
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

        public event Action<IPackage, ChangeAction> PackageChanged
        {
            add { _packageStorage.PackageChanged += value; }
            remove { _packageStorage.PackageChanged -= value; }
        }

        public IPackage UpdatePackage(byte[] fileContent, string fileName)
        {
            return _packageStorage.Update(fileContent, fileName);
        }

        public IPackage[] GetPackages()
        {
            return _packageStorage.GetAll();
        }

        public void RemovePackage(string package)
        {
            lock (SyncObj)
            {
                var dPackage = _packageStorage.Get(package);
                if (dPackage != null)
                {
                    if (dPackage.IsLocked)
                        throw new PackageLockedException("Cannot remove package: one or more trade bots from this package is being executed! Please stop all bots and try again!");

                    var botsToDelete = _allBots.Values.Where(b => b.Package == dPackage).ToList();

                    foreach (var bot in botsToDelete)
                        bot.Account.RemoveBot(bot.Id);

                    _packageStorage.Remove(package);
                }
            }
        }

        public PluginInfo[] GetAllPlugins()
        {
            lock (SyncObj)
            {
                return _packageStorage.GetAll()
                    .SelectMany(p => p.GetPlugins())
                    .ToArray();
            }
        }

        public PluginInfo[] GetPluginsByType(AlgoTypes type)
        {
            lock (SyncObj)
            {
                return _packageStorage.GetAll()
                .SelectMany(p => p.GetPluginsByType(type))
                .ToArray();
            }
        }

        public Task ShutdownAsync()
        {
            lock (SyncObj)
            {
                _logger.LogDebug("ServerModel shut down...");
                var shutdonwTasks = Accounts.Select(ac => ac.ShutdownAsync()).ToArray();
                return Task.WhenAll(shutdonwTasks);
            }
        }

        #endregion
    }

    public enum ChangeAction { Added, Removed, Modified }
}
