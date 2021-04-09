﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TickTrader.BotAgent.BA.Repository;
using TickTrader.BotAgent.BA.Exceptions;
using System.Threading.Tasks;
using TickTrader.BotAgent.Infrastructure;
using TickTrader.BotAgent.Extensions;
using TickTrader.Algo.Common.Lib;
using ActorSharp;
using NLog;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

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

        private AlertStorage _alertStorage;
        private ThreadPoolManager _threadPoolManager;

        private AlgoServer _algoServer;

        public static EnvService Environment => envService;

        public static string GetWorkingFolderFor(string botId)
        {
            return Path.Combine(Environment.AlgoWorkingFolder, botId.Escape());
        }

        private async Task InitAsync(IFdkOptionsProvider fdkOptionsProvider)
        {
            _algoServer = new AlgoServer();
            await _algoServer.Start();
            _logger.Info($"Started AlgoServer on port {_algoServer.BoundPort}");

            _botIdHelper = new BotIdHelper();
            _allBots = new Dictionary<string, TradeBotModel>();
            _packageStorage = new PackageStorage(_algoServer);
            _alertStorage = new AlertStorage();
            _threadPoolManager = new ThreadPoolManager();
            _fdkOptionsProvider = fdkOptionsProvider;

            await _packageStorage.Library.WaitInit();

            _packageStorage.PackageChanged += (p, a) => PackageChanged?.Invoke(p, a);
            _packageStorage.PackageStateChanged += p => PackageStateChanged?.Invoke(new PackageStateUpdate { PackageId = p.PackageId, IsLocked = p.IsLocked, IsValid = p.IsValid });

            _threadPoolManager.Start(GetBotsCnt());

            foreach (var acc in _accounts)
                await InitAccount(acc);
        }

        private int GetBotsCnt()
        {
            return _accounts.Sum(a => a.TotalBotsCount);
        }

        public void Close()
        {
        }

        public class Handler : BlockingHandler<ServerModel>, IBotAgent
        {
            public Handler(Ref<ServerModel> actorRef) : base(actorRef) { }

            #region Repository Management

            public Task<List<PackageInfo>> GetPackages() => CallActorAsync(a => a.GetPackages());
            public Task<PackageInfo> GetPackage(string package) => CallActorAsync(a => a.GetPackage(package));
            public Task UpdatePackage(byte[] fileContent, string fileName) => CallActorAsync(a => a.UpdatePackage(fileContent, fileName));
            public Task<byte[]> DownloadPackage(string packageId) => CallActorAsync(a => a.DownloadPackage(packageId));
            public Task RemovePackage(RemovePackageRequest request) => CallActorAsync(a => a.RemovePackage(request));
            public Task<List<PluginInfo>> GetAllPlugins() => CallActorAsync(a => a.GetAllPlugins());
            public Task<List<PluginInfo>> GetPluginsByType(Metadata.Types.PluginType type) => CallActorAsync(a => a.GetPluginsByType(type));
            public Task<MappingCollectionInfo> GetMappingsInfo() => CallActorAsync(a => a.GetMappingsInfo());
            public Task<string> GetPackageReadPath(string packageId) => CallActorAsync(a => a.GetPackageReadPath(packageId));
            public Task<string> GetPackageWritePath(string packageId) => CallActorAsync(a => a.GetPackageWritePath(packageId));

            public event Action<PackageInfo, ChangeAction> PackageChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.PackageChanged += value);
                remove => CallActorFlatten(a => a.PackageChanged -= value);
            }

            public event Action<PackageStateUpdate> PackageStateChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.PackageStateChanged += value);
                remove => CallActorFlatten(a => a.PackageStateChanged -= value);
            }

            #endregion

            #region Account Management

            public Task AddAccount(AddAccountRequest request) => CallActorFlattenAsync(a => a.AddAccount(request));
            public Task ChangeAccount(ChangeAccountRequest request) => CallActorAsync(a => a.ChangeAccount(request));
            public Task<List<AccountModelInfo>> GetAccounts() => CallActorAsync(a => a._accounts.GetInfoCopy());
            public Task RemoveAccount(RemoveAccountRequest request) => CallActorFlattenAsync(a => a.RemoveAccount(request));
            public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request) => CallActorFlattenAsync(a => a.GetAccountOrThrow(request.AccountId).TestConnection());
            public Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request) => CallActorFlattenAsync(a => a.TestCreds(request));

            public event Action<AccountModelInfo, ChangeAction> AccountChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.AccountChanged += value);
                remove => CallActorFlatten(a => a.AccountChanged -= value);
            }

            public event Action<AccountStateUpdate> AccountStateChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.AccountStateChanged += value);
                remove => CallActorFlatten(a => a.AccountStateChanged -= value);
            }

            #endregion

            #region Bot Management
            public Task<IAlertStorage> GetAlertStorage() => CallActorAsync(a => a.GetAlertsStorage());
            public Task<string> GenerateBotId(string botDisplayName) => CallActorAsync(a => a.AutogenerateBotId(botDisplayName));
            public Task<PluginModelInfo> AddBot(AddPluginRequest request) => CallActorAsync(a => a.AddBot(request));
            public Task ChangeBotConfig(ChangePluginConfigRequest request) => CallActorAsync(a => a.ChangeBotConfig(request));
            public Task RemoveBot(RemovePluginRequest request) => CallActorAsync(a => a.RemoveBot(request));
            public Task StartBot(StartPluginRequest request) => CallActorAsync(a => a.GetBotOrThrow(request.PluginId).Start());
            public Task StopBotAsync(StopPluginRequest request) => CallActorFlattenAsync(a => a.GetBotOrThrow(request.PluginId).StopAsync());
            public void AbortBot(string botId) => ActorSend(a => a.GetBotOrThrow(botId).Abort());
            public Task<PluginModelInfo> GetBotInfo(string botId) => CallActorAsync(a => a.GetBotOrThrow(botId).GetInfoCopy());
            public Task<List<PluginModelInfo>> GetBots() => CallActorAsync(a => a._allBots.Values.GetInfoCopy());
            public async Task<IBotFolder> GetAlgoData(string botId)
            {
                var algoDataRef = await CallActorAsync(a => a.GetBotOrThrow(botId).AlgoDataRef);
                return new AlgoData.Handler(algoDataRef);
            }

            public async Task<IBotLog> GetBotLog(string botId)
            {
                var logRef = await CallActorAsync(a => a.GetBotOrThrow(botId).LogRef);
                return new BotLog.Handler(logRef);
            }

            public Task<Tuple<ConnectionErrorInfo, AccountMetadataInfo>> GetAccountMetadata(string accountId) => CallActorFlattenAsync(a => a.GetAccountMetadata(accountId));

            public event Action<PluginModelInfo, ChangeAction> BotChanged
            {
                // Warning! This violates actor model rules! Deadlocks are possible!
                add => CallActorFlatten(a => a.BotChanged += value);
                remove => CallActorFlatten(a => a.BotChanged -= value);
            }

            public event Action<PluginStateUpdate> BotStateChanged
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
        public event Action<AccountStateUpdate> AccountStateChanged;

        public async Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request)
        {
            var server = request.Server;
            var userId = request.UserId;
            var creds = request.Creds;

            Validate(server, userId, creds);

            var acc = new ClientModel(server, userId, creds);
            await acc.Init(_packageStorage, _fdkOptionsProvider, _alertStorage, _algoServer);

            var testResult = await acc.TestConnection();

            if (!await acc.ShutdownAsync().WaitAsync(5000))
            {
                _logger.Error($"Can't stop test connection to {server} - {userId}");
            }

            return testResult;
        }

        private async Task<Tuple<ConnectionErrorInfo, AccountMetadataInfo>> GetAccountMetadata(string accountId)
        {
            try
            {
                var metadata = await GetAccountOrThrow(accountId).GetMetadata();
                return Tuple.Create(ConnectionErrorInfo.Ok, metadata);
            }
            catch (CommunicationException ex)
            {
                return Tuple.Create(new ConnectionErrorInfo(ex.FdkCode, ex.Message), (AccountMetadataInfo)null);
            }
        }

        public async Task AddAccount(AddAccountRequest request)
        {
            var server = request.Server;
            var userId = request.UserId;
            var creds = request.Creds;

            Validate(server, userId, creds);

            var accountId = AccountId.Pack(server, userId);
            var existing = FindAccount(accountId);
            if (existing != null)
                throw new DuplicateAccountException($"Account '{accountId}' already exists");
            else
            {
                var newAcc = new ClientModel(server, userId, creds, request.DisplayName);
                _accounts.Add(newAcc);
                AccountChanged?.Invoke(newAcc.GetInfoCopy(), ChangeAction.Added);

                Save();

                await InitAccount(newAcc);
            }
        }

        private void Validate(string server, string userId, AccountCreds creds)
        {
            if (string.IsNullOrWhiteSpace(server))
                throw new InvalidAccountException("Server is required");
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidAccountException("UserId is required");
            switch(creds.AuthScheme)
            {
                case AccountCreds.SimpleAuthSchemeId:
                    if (string.IsNullOrWhiteSpace(creds.GetPassword()))
                        throw new InvalidAccountException("Password is required");
                    break;
                default:
                    throw new InvalidAccountException("Creds.AuthScheme is not supported");
            }
        }

        public async Task RemoveAccount(RemoveAccountRequest request)
        {
            ClientModel acc = FindAccount(request.AccountId);

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
                throw new BAException($"Can't stop connection to {acc.Address} - {acc.Username}");
        }

        public void ChangeAccount(ChangeAccountRequest request)
        {
            var acc = GetAccountOrThrow(request.AccountId);
            acc.Change(request);
        }

        private ClientModel FindAccount(string login, string server)
        {
            return _accounts.FirstOrDefault(a => a.Username == login && a.Address == server);
        }

        private ClientModel FindAccount(string accountId)
        {
            return _accounts.FirstOrDefault(a => a.AccountId == accountId);
        }

        private ClientModel GetAccountOrThrow(string accountId)
        {
            var acc = FindAccount(accountId);
            if (acc == null)
                throw new AccountNotFoundException($"Account with id '{accountId}' does not exist!");
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
            await acc.Init(_packageStorage, _fdkOptionsProvider, _alertStorage, _algoServer);
            _algoServer.RegisterAccountProxy(acc.GetAccountProxy());
        }

        private void OnBotStateChanged(TradeBotModel bot)
        {
            BotStateChanged?.Invoke(bot.GetStateUpdate());
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
            AccountStateChanged?.Invoke(acc.GetStateUpdate());
        }

        private void OnBotChanged(TradeBotModel bot, ChangeAction changeAction)
        {
            if (changeAction == ChangeAction.Removed)
            {
                _allBots.Remove(bot.Id);
                _threadPoolManager.OnNewBotsCnt(GetBotsCnt());
            }

            if (changeAction == ChangeAction.Added)
            {
                _threadPoolManager.OnNewBotsCnt(GetBotsCnt());
            }

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

        private event Action<PluginModelInfo, ChangeAction> BotChanged;
        private event Action<PluginStateUpdate> BotStateChanged;

        private PluginModelInfo AddBot(AddPluginRequest request)
        {
            var bot = GetAccountOrThrow(request.AccountId).AddBot(request.Config);
            return bot.GetInfoCopy();
        }

        private void ChangeBotConfig(ChangePluginConfigRequest request)
        {
            GetBotOrThrow(request.PluginId).ChangeBotConfig(request.NewConfig);
        }

        public void RemoveBot(RemovePluginRequest request)
        {
            _allBots.GetOrDefault(request.PluginId)?.Remove(request.CleanLog, request.CleanAlgoData);
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
                _logger.Error(ex, $"Failed to save config file! {ex.Message}");
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
        private event Action<PackageStateUpdate> PackageStateChanged;

        private List<PackageInfo> GetPackages()
        {
            return _packageStorage.Library.GetPackages().ToList();
        }

        private PackageInfo GetPackage(string packageName)
        {
            return _packageStorage.Library.GetPackage(PackageStorage.GetPackageId(packageName));
        }

        private void UpdatePackage(byte[] fileContent, string fileName)
        {
            _packageStorage.Update(fileContent, fileName);
        }

        private byte[] DownloadPackage(string packageId)
        {
            return _packageStorage.GetPackageBinary(packageId);
        }

        private void RemovePackage(RemovePackageRequest request)
        {
            var packageId = request.PackageId;

            var dPackage = _packageStorage.GetPackageRef(packageId);
            if (dPackage != null)
            {
                if (dPackage.IsLocked)
                    throw new PackageLockedException("Cannot remove Algo package: one or more trade bots from this package is being executed! Please stop all bots and try again!");

                var botsToDelete = _allBots.Values.Where(b => b.Package == dPackage).ToList();

                foreach (var bot in botsToDelete)
                    bot.Remove();

                _packageStorage.Remove(packageId);
            }
        }

        private IAlertStorage GetAlertsStorage() => _alertStorage;

        private List<PluginInfo> GetAllPlugins()
        {
            return _packageStorage.Library.GetPlugins().ToList();
        }

        private List<PluginInfo> GetPluginsByType(Metadata.Types.PluginType type)
        {
            return _packageStorage.Library.GetPlugins(type).ToList();
        }

        private MappingCollectionInfo GetMappingsInfo()
        {
            return _packageStorage.Mappings.ToInfo();
        }

        private string GetPackageReadPath(string packageId)
        {
            return _packageStorage.GetPackageReadPath(packageId);
        }

        private string GetPackageWritePath(string packageId)
        {
            return _packageStorage.GetPackageWritePath(packageId);
        }

        #endregion

        private async Task ShutdownAsync()
        {
            _logger.Debug("ServerModel is shutting down...");
            var shutdonwTasks = _accounts.Select(ac => ac.ShutdownAsync()).ToArray();
            await Task.WhenAll(shutdonwTasks);
            await _algoServer.Stop();
            await _threadPoolManager.Stop();
        }
    }

    public enum ChangeAction { Added, Removed, Modified }
}
