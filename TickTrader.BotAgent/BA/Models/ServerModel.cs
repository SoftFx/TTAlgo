﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.BotAgent.BA.Repository;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotAgent.BA.Exceptions;
using System.Threading.Tasks;
using TickTrader.BotAgent.Infrastructure;
using TickTrader.BotAgent.Extensions;
using TickTrader.Algo.Common.Lib;
using ActorSharp;
using NLog;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "server.config", Namespace = "")]
    public class ServerModel
    {
        private static readonly ILogger _logger = LogManager.GetLogger(nameof(ServerModel));

        private static readonly EnvService envService = new EnvService();
        private static readonly string cfgFilePath = Path.Combine(envService.AppFolder, "server.config.xml");

        [DataMember(Name = "accounts")]
        private List<ClientModel> _accounts = new List<ClientModel>();
        private Dictionary<string, TradeBotModel> _allBots;
        private BotIdHelper _botIdHelper;
        private IFdkOptionsProvider _fdkOptionsProvider;

        public static EnvService Environment => envService;

        public static string GetWorkingFolderFor(string botId)
        {
            return Path.Combine(Environment.AlgoWorkingFolder, botId.Escape());
        }

        private async Task InitAsync(IFdkOptionsProvider fdkOptionsProvider)
        {
            _botIdHelper = new BotIdHelper();
            _allBots = new Dictionary<string, TradeBotModel>();
            _packageStorage = new PackageStorage();
            _fdkOptionsProvider = fdkOptionsProvider;

            await _packageStorage.Library.WaitInit();

            _packageStorage.PackageChanged += (p, a) => PackageChanged?.Invoke(p, a);
            _packageStorage.PackageStateChanged += p => PackageStateChanged?.Invoke(p);
            foreach (var acc in _accounts)
                await InitAccount(acc);
        }

        public void Close()
        {
        }

        public class Handler : BlockingHandler<ServerModel>, IBotAgent
        {
            public Handler(Ref<ServerModel> actorRef) : base(actorRef) { }

            #region Repository Management

            public List<PackageInfo> GetPackages() => CallActorFlatten(a => a.GetPackages());
            public PackageInfo GetPackage(string package) => CallActorFlatten(a => a.GetPackage(package));
            public void UpdatePackage(byte[] fileContent, string fileName) => CallActorFlatten(a => a.UpdatePackage(fileContent, fileName));
            public byte[] DownloadPackage(PackageKey package) => CallActorFlatten(a => a.DownloadPackage(package));
            public void RemovePackage(string package) => CallActorFlatten(a => a.RemovePackage(package));
            public void RemovePackage(PackageKey package) => CallActorFlatten(a => a.RemovePackage(package));
            public List<PluginInfo> GetAllPlugins() => CallActorFlatten(a => a.GetAllPlugins());
            public List<PluginInfo> GetPluginsByType(AlgoTypes type) => CallActorFlatten(a => a.GetPluginsByType(type));
            public MappingCollectionInfo GetMappingsInfo() => CallActorFlatten(a => a.GetMappingsInfo());
            public string GetPackageReadPath(PackageKey package) => CallActorFlatten(a => a.GetPackageReadPath(package));
            public string GetPackageWritePath(PackageKey package) => CallActorFlatten(a => a.GetPackageWritePath(package));

            public event Action<PackageInfo, ChangeAction> PackageChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.PackageChanged += value);
                remove => CallActorFlatten(a => a.PackageChanged -= value);
            }

            public event Action<PackageInfo> PackageStateChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.PackageStateChanged += value);
                remove => CallActorFlatten(a => a.PackageStateChanged -= value);
            }

            #endregion

            #region Account Management

            public void AddAccount(AccountKey key, string password, bool useNewProtocol) => CallActorFlatten(a => a.AddAccount(key, password, useNewProtocol));
            public void ChangeAccount(AccountKey key, string password, bool useNewProtocol) => CallActorFlatten(a => a.ChangeAccount(key, password, useNewProtocol));
            public void ChangeAccountPassword(AccountKey key, string password) => CallActorFlatten(a => a.ChangeAccountPassword(key, password));
            public void ChangeAccountProtocol(AccountKey key) => CallActorFlatten(a => a.ChangeAccountProtocol(key));
            public List<AccountModelInfo> GetAccounts() => CallActorFlatten(a => a._accounts.GetInfoCopy());
            public void RemoveAccount(AccountKey key) => CallActorFlatten(a => a.RemoveAccount(key));
            public ConnectionErrorInfo TestAccount(AccountKey accountId) => CallActorFlatten(a => a.GetAccountOrThrow(accountId).TestConnection());
            public ConnectionErrorInfo TestCreds(AccountKey accountId, string password, bool useNewProtocol) => CallActorFlatten(a => a.TestCreds(accountId, password, useNewProtocol));

            public event Action<AccountModelInfo, ChangeAction> AccountChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.AccountChanged += value);
                remove => CallActorFlatten(a => a.AccountChanged -= value);
            }

            public event Action<AccountModelInfo> AccountStateChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.AccountStateChanged += value);
                remove => CallActorFlatten(a => a.AccountStateChanged -= value);
            }

            #endregion

            #region Bot Management

            public string GenerateBotId(string botDisplayName) => CallActorFlatten(a => a.AutogenerateBotId(botDisplayName));
            public BotModelInfo AddBot(AccountKey accountId, PluginConfig config) => CallActorFlatten(a => a.AddBot(accountId, config));
            public void ChangeBotConfig(string botId, PluginConfig config) => CallActorFlatten(a => a.GetBotOrThrow(botId).ChangeBotConfig(config));
            public void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false) => CallActorFlatten(a => a.RemoveBot(botId, cleanLog, cleanAlgoData));
            public void StartBot(string botId) => CallActorFlatten(a => a.GetBotOrThrow(botId).Start());
            public Task StopBotAsync(string botId) => CallActorFlattenAsync(a => a.GetBotOrThrow(botId).StopAsync());
            public void AbortBot(string botId) => CallActorFlatten(a => a.GetBotOrThrow(botId).Abort());
            public BotModelInfo GetBotInfo(string botId) => CallActorFlatten(a => a.GetBotOrThrow(botId).GetInfoCopy());
            public List<BotModelInfo> GetTradeBots() => CallActorFlatten(a => a._allBots.Values.GetInfoCopy());
            public IBotFolder GetAlgoData(string botId) => CallActorFlatten(a => a.GetBotOrThrow(botId).AlgoData);

            public IBotLog GetBotLog(string botId)
            {
                var logRef = CallActorFlatten(a => a.GetBotOrThrow(botId).LogRef);
                return new BotLog.Handler(logRef);
            }

            public ConnectionErrorInfo GetAccountMetadata(AccountKey key, out AccountMetadataInfo info)
            {
                var result = CallActorFlatten(a => a.GetAccountMetadata(key));
                info = result.Item2;
                return result.Item1;
            }

            public event Action<BotModelInfo, ChangeAction> BotChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.BotChanged += value);
                remove => CallActorFlatten(a => a.BotChanged -= value);
            }

            public event Action<BotModelInfo> BotStateChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.BotStateChanged += value);
                remove => CallActorFlatten(a => a.BotStateChanged -= value);
            }

            #endregion

            public Task InitAsync(IFdkOptionsProvider fdkOptionsProvider) => CallActorFlattenAsync(a => a.InitAsync(fdkOptionsProvider));

            public Task ShutdownAsync() => CallActorFlattenAsync(a => a.ShutdownAsync());
        }

        #region Account management

        public event Action<AccountModelInfo, ChangeAction> AccountChanged;
        public event Action<AccountModelInfo> AccountStateChanged;

        public async Task<ConnectionErrorInfo> TestCreds(AccountKey accountId, string password, bool useNewProtocol)
        {
            var acc = new ClientModel(accountId.Server, accountId.Login, password, useNewProtocol);
            await acc.Init(_packageStorage, _fdkOptionsProvider);

            var testResult = await acc.TestConnection();

            if (!await acc.ShutdownAsync().WaitAsync(5000))
            {
                _logger.Error($"Can't stop test connection to {accountId.Server} - {accountId.Login} via {(useNewProtocol ? "SFX" : "FIX")}");
            }

            return testResult;
        }

        private async Task<Tuple<ConnectionErrorInfo, AccountMetadataInfo>> GetAccountMetadata(AccountKey key)
        {
            try
            {
                var metadata = await GetAccountOrThrow(key).GetMetadata();
                return Tuple.Create(ConnectionErrorInfo.Ok, metadata);
            }
            catch (CommunicationException ex)
            {
                return Tuple.Create(new ConnectionErrorInfo(ex.FdkCode, ex.Message), (AccountMetadataInfo)null);
            }
        }

        public async Task AddAccount(AccountKey accountId, string password, bool useNewProtocol)
        {
            Validate(accountId.Login, accountId.Server, password);

            var existing = FindAccount(accountId);
            if (existing != null)
                throw new DuplicateAccountException($"Account '{accountId.Login}:{accountId.Server}' already exists");
            else
            {
                var newAcc = new ClientModel(accountId.Server, accountId.Login, password, useNewProtocol);
                _accounts.Add(newAcc);
                AccountChanged?.Invoke(newAcc.GetInfoCopy(), ChangeAction.Added);

                Save();

                await InitAccount(newAcc);
            }
        }

        private void Validate(string login, string server, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidAccountException("Login, password and server are required");
            }
        }

        public async Task RemoveAccount(AccountKey accountId)
        {
            ClientModel acc = FindAccount(accountId);

            if (acc == null)
                return;

            if (acc.HasRunningBots)
                throw new AccountLockedException("Account cannot be removed! Stop all bots and try again.");

            acc.RemoveAllBots();
            _accounts.Remove(acc);
            DisposeAccount(acc);

            Save();

            AccountChanged?.Invoke(acc.GetInfoCopy(), ChangeAction.Removed);

            if (!await acc.ShutdownAsync().WaitAsync(5000))
                throw new BAException($"Can't stop connection to {acc.Address} - {acc.Username} via {(acc.UseNewProtocol ? "SFX" : "FIX")}");
        }

        public void ChangeAccount(AccountKey key, string password, bool useNewProtocol)
        {
            var acc = GetAccountOrThrow(key);
            acc.ChangeConnectionSettings(password, useNewProtocol);
        }

        public void ChangeAccountPassword(AccountKey key, string password)
        {
            Validate(key.Login, key.Server, password);

            var acc = GetAccountOrThrow(key);
            acc.ChangePassword(password);
        }

        public void ChangeAccountProtocol(AccountKey key)
        {
            var acc = GetAccountOrThrow(key);
            acc.ChangeProtocol();
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

        private async Task InitAccount(ClientModel acc)
        {
            acc.BotValidation += OnBotValidation;
            acc.BotInitialized += OnBotInitialized;
            acc.Changed += OnAccountChanged;
            acc.StateChanged += OnAccountStateChanged;
            acc.BotChanged += OnBotChanged;
            acc.BotStateChanged += OnBotStateChanged;
            await acc.Init(_packageStorage, _fdkOptionsProvider);
        }

        private void OnBotStateChanged(TradeBotModel bot)
        {
            BotStateChanged?.Invoke(bot.GetInfoCopy());
        }

        private void DisposeAccount(ClientModel acc)
        {
            acc.BotValidation -= OnBotValidation;
            acc.BotInitialized -= OnBotInitialized;
            acc.Changed -= OnAccountChanged;
            acc.StateChanged -= OnAccountStateChanged;
            acc.BotChanged -= OnBotChanged;
            acc.BotStateChanged -= OnBotStateChanged;
        }

        private void OnAccountChanged(ClientModel acc)
        {
            Save();
            AccountChanged?.Invoke(acc.GetInfoCopy(), ChangeAction.Modified);
        }

        private void OnAccountStateChanged(ClientModel acc)
        {
            AccountStateChanged?.Invoke(acc.GetInfoCopy());
        }

        private void OnBotChanged(TradeBotModel bot, ChangeAction changeAction)
        {
            if (changeAction == ChangeAction.Removed)
                _allBots.Remove(bot.Id);

            Save();
            BotChanged?.Invoke(bot.GetInfoCopy(), changeAction);
        }

        private void OnBotValidation(TradeBotModel bot)
        {
            if (!_botIdHelper.Validate(bot.Id))
                throw new InvalidBotException($"The instance Id must be no more than {_botIdHelper.MaxLength} characters and consist of characters: a-z A-Z 0-9 and space");
            if (_allBots.ContainsKey(bot.Id))
                throw new DuplicateBotIdException("Bot with id '" + bot.Id + "' already exist!");
        }

        private void OnBotInitialized(TradeBotModel bot)
        {
            _allBots.Add(bot.Id, bot);
        }

        #endregion

        #region Bot management

        private event Action<BotModelInfo, ChangeAction> BotChanged;
        private event Action<BotModelInfo> BotStateChanged;

        private BotModelInfo AddBot(AccountKey account, PluginConfig config)
        {
            var bot = GetAccountOrThrow(account).AddBot(config);
            return bot.GetInfoCopy();
        }

        public void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            _allBots.GetOrDefault(botId)?.Remove(cleanLog, cleanAlgoData);
        }

        private string AutogenerateBotId(string botDescriptorName)
        {
            int seed = 1;

            while (true)
            {
                var botId = _botIdHelper.BuildId(botDescriptorName, seed.ToString());
                if (!_allBots.ContainsKey(botId))
                    return botId;

                seed++;
            }
        }

        private TradeBotModel GetBotOrThrow(string id)
        {
            var tradeBot = _allBots.GetOrDefault(id);
            if (tradeBot == null)
                throw new BotNotFoundException($"Bot {id} not found");
            else
                return tradeBot;
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
                _logger.Error(ex, "Failed to save config file! {0}");
            }
        }

        public static Ref<ServerModel> Load()
        {
            ServerModel instance;

            try
            {
                using (var stream = File.OpenRead(cfgFilePath))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ServerModel));
                    instance = (ServerModel)serializer.ReadObject(stream);
                }
            }
            catch (FileNotFoundException)
            {
                instance = new ServerModel();
            }

            return Actor.SpawnLocal(instance, null, "ServerModel");
        }

        #endregion

        #region Repository management

        private PackageStorage _packageStorage;

        private event Action<PackageInfo, ChangeAction> PackageChanged;
        private event Action<PackageInfo> PackageStateChanged;

        private List<PackageInfo> GetPackages()
        {
            return _packageStorage.Library.GetPackages().ToList();
        }

        private PackageInfo GetPackage(string packageName)
        {
            return _packageStorage.Library.GetPackage(PackageStorage.GetPackageKey(packageName));
        }

        private void UpdatePackage(byte[] fileContent, string fileName)
        {
            _packageStorage.Update(fileContent, fileName);
        }

        private byte[] DownloadPackage(PackageKey package)
        {
            return _packageStorage.GetPackageBinary(package);
        }

        private void RemovePackage(string package)
        {
            RemovePackage(PackageStorage.GetPackageKey(package));
        }

        private void RemovePackage(PackageKey package)
        {
            var dPackage = _packageStorage.GetPackageRef(package);
            if (dPackage != null)
            {
                if (dPackage.IsLocked)
                    throw new PackageLockedException("Cannot remove package: one or more trade bots from this package is being executed! Please stop all bots and try again!");

                var botsToDelete = _allBots.Values.Where(b => b.Package == dPackage).ToList();

                foreach (var bot in botsToDelete)
                    bot.Remove();

                _packageStorage.Remove(package);
            }
        }

        private List<PluginInfo> GetAllPlugins()
        {
            return _packageStorage.Library.GetPlugins().ToList();
        }

        private List<PluginInfo> GetPluginsByType(AlgoTypes type)
        {
            return _packageStorage.Library.GetPlugins(type).ToList();
        }

        private MappingCollectionInfo GetMappingsInfo()
        {
            return _packageStorage.Mappings.ToInfo();
        }

        private string GetPackageReadPath(PackageKey package)
        {
            return _packageStorage.GetPackageReadPath(package);
        }

        private string GetPackageWritePath(PackageKey package)
        {
            return _packageStorage.GetPackageWritePath(package);
        }

        #endregion

        private Task ShutdownAsync()
        {
            _logger.Debug("ServerModel is shutting down...");
            var shutdonwTasks = _accounts.Select(ac => ac.ShutdownAsync()).ToArray();
            return Task.WhenAll(shutdonwTasks);
        }
    }

    public enum ChangeAction { Added, Removed, Modified }
}