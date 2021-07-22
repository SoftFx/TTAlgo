using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Threading.Tasks;
using TickTrader.BotAgent.Infrastructure;
using ActorSharp;
using NLog;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server;
using TickTrader.Algo.Core;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server.Persistence;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "server.config", Namespace = "")]
    public class ServerModel
    {
        private static readonly ILogger _logger = LogManager.GetLogger(nameof(ServerModel));

        private static readonly EnvService envService = new EnvService(AppDomain.CurrentDomain.BaseDirectory);
        private static readonly string cfgFilePath = Path.Combine(envService.AppFolder, "server.config.xml");

        [DataMember(Name = "accounts")]
        private List<ClientModel> _accounts = new List<ClientModel>();
        private Dictionary<string, TradeBotModel> _allBots;
        private PluginIdHelper _botIdHelper;

        private ThreadPoolManager _threadPoolManager;

        private AlgoServer _algoServer;

        public static EnvService Environment => envService;

        public static string GetWorkingFolderFor(string botId)
        {
            return Path.Combine(Environment.AlgoWorkingFolder, PathHelper.Escape(botId));
        }

        private async Task InitAsync(IFdkOptionsProvider fdkOptionsProvider)
        {
            var settings = new AlgoServerSettings();
            settings.DataFolder = AppDomain.CurrentDomain.BaseDirectory;
            settings.ConnectionOptions = fdkOptionsProvider.GetConnectionOptions();
            settings.PkgStorage.AddLocation(SharedConstants.LocalRepositoryId, envService.AlgoRepositoryFolder);
            settings.PkgStorage.UploadLocationId = SharedConstants.LocalRepositoryId;

            _algoServer = new AlgoServer(settings);

            if (!File.Exists(_algoServer.Env.ServerStateFilePath))
            {
                await _algoServer.LoadSavedData(BuildServerSavedState());
            }

            await _algoServer.Start();

            _botIdHelper = new PluginIdHelper();
            _allBots = new Dictionary<string, TradeBotModel>();
            _threadPoolManager = new ThreadPoolManager();

            _reductions = new ReductionCollection();
            _reductions.LoadDefaultReductions();
            _mappingsInfo = _reductions.CreateMappings();

            var eventBus = _algoServer.EventBus;
            eventBus.PackageUpdated.Subscribe(p => PackageChanged?.Invoke(p));
            eventBus.PackageStateUpdated.Subscribe(p => PackageStateChanged?.Invoke(p));
            eventBus.AccountUpdated.Subscribe(a => AccountChanged?.Invoke(a));
            eventBus.AccountStateUpdated.Subscribe(a => AccountStateChanged?.Invoke(a));
            eventBus.PluginUpdated.Subscribe(p => BotChanged?.Invoke(p));
            eventBus.PluginStateUpdated.Subscribe(p => BotStateChanged?.Invoke(p));

            _threadPoolManager.Start(0);

            //foreach (var acc in _accounts)
            //    await InitAccount(acc);
        }


        private ServerSavedState BuildServerSavedState()
        {
            var state = new ServerSavedState();

            foreach (var acc in _accounts)
            {
                var server = acc.Address;
                var userId = acc.Username;
                var accState = new AccountSavedState
                {
                    Id = AccountId.Pack(server, userId),
                    UserId = userId,
                    Server = server,
                    DisplayName = string.IsNullOrEmpty(acc.DisplayName) ? $"{server} - {userId}" : acc.DisplayName,
                };

                accState.PackCreds(new AccountCreds(acc.Password));

                state.Accounts.Add(accState.Id, accState);

                acc.AddPluginsSavedStates(state, accState.Id);
            }

            return state;
        }

        public class Handler : BlockingHandler<ServerModel>, IBotAgent
        {
            public Handler(Ref<ServerModel> actorRef) : base(actorRef) { }

            #region Repository Management

            public Task<PackageListSnapshot> GetPackageSnapshot() => CallActorAsync(a => a._algoServer.EventBus.GetPackageSnapshot());
            public Task<bool> PackageWithNameExists(string pkgName) => CallActorAsync(a => a._algoServer.PkgStorage.PackageWithNameExists(pkgName));
            public Task UploadPackage(UploadPackageRequest request, string pkgFilePath) => CallActorAsync(a => a._algoServer.PkgStorage.UploadPackage(request, pkgFilePath));
            public Task<byte[]> DownloadPackage(string packageId) => CallActorAsync(a => a._algoServer.PkgStorage.GetPackageBinary(packageId));
            public Task RemovePackage(RemovePackageRequest request) => CallActorAsync(a => a._algoServer.PkgStorage.RemovePackage(request));
            public Task<MappingCollectionInfo> GetMappingsInfo() => CallActorAsync(a => a.GetMappingsInfo());

            public event Action<PackageUpdate> PackageChanged
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

            public Task<AccountListSnapshot> GetAccounts() => CallActorAsync(a => a._algoServer.EventBus.GetAccountSnapshot());
            public Task AddAccount(AddAccountRequest request) => CallActorFlattenAsync(a => a._algoServer.Accounts.Add(request));
            public Task ChangeAccount(ChangeAccountRequest request) => CallActorAsync(a => a._algoServer.Accounts.Change(request));
            public Task RemoveAccount(RemoveAccountRequest request) => CallActorFlattenAsync(a => a._algoServer.Accounts.Remove(request));
            public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request) => CallActorFlattenAsync(a => a._algoServer.Accounts.Test(request));
            public Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request) => CallActorFlattenAsync(a => a._algoServer.Accounts.TestCreds(request));

            public event Action<AccountModelUpdate> AccountChanged
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
            public Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request) => CallActorAsync(a => a._algoServer.Alerts.GetAlerts(request));
            public Task<string> GenerateBotId(string botDisplayName) => CallActorAsync(a => a.AutogenerateBotId(botDisplayName));
            public Task AddBot(AddPluginRequest request) => CallActorAsync(a => a._algoServer.Plugins.Add(request));
            public Task ChangeBotConfig(ChangePluginConfigRequest request) => CallActorAsync(a => a._algoServer.Plugins.UpdateConfig(request));
            public Task RemoveBot(RemovePluginRequest request) => CallActorAsync(a => a._algoServer.Plugins.Remove(request));
            public Task StartBot(StartPluginRequest request) => CallActorAsync(a => a._algoServer.Plugins.StartPlugin(request));
            public Task StopBotAsync(StopPluginRequest request) => CallActorFlattenAsync(a => a._algoServer.Plugins.StopPlugin(request));
            public Task<PluginModelInfo> GetBotInfo(string botId) => CallActorAsync(a => a._algoServer.EventBus.GetPluginInfo(botId));
            public Task<PluginListSnapshot> GetBots() => CallActorAsync(a => a._algoServer.EventBus.GetPluginSnapshot());
            public Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request) => CallActorAsync(a => a._algoServer.PluginFiles.GetFolderInfo(request));
            public Task ClearPluginFolder(ClearPluginFolderRequest request) => CallActorAsync(a => a._algoServer.PluginFiles.ClearFolder(request));
            public Task DeletePluginFile(DeletePluginFileRequest request) => CallActorAsync(a => a._algoServer.PluginFiles.DeleteFile(request));
            public Task<string> GetPluginFileReadPath(DownloadPluginFileRequest request) => CallActorAsync(a => a._algoServer.PluginFiles.GetFileReadPath(request));
            public Task<string> GetPluginFileWritePath(UploadPluginFileRequest request) => CallActorAsync(a => a._algoServer.PluginFiles.GetFileWritePath(request));

            public Task<PluginLogRecord[]> GetBotLogs(PluginLogsRequest request) => CallActorAsync(a => a._algoServer.Plugins.GetPluginLogs(request));
            public Task<string> GetBotStatus(PluginStatusRequest request) => CallActorAsync(a => a._algoServer.Plugins.GetPluginStatus(request));

            public Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request) => CallActorFlattenAsync(a => a._algoServer.Accounts.GetMetadata(request));

            public event Action<PluginModelUpdate> BotChanged
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

        public event Action<AccountModelUpdate> AccountChanged;
        public event Action<AccountStateUpdate> AccountStateChanged;

        #endregion

        #region Bot management

        private event Action<PluginModelUpdate> BotChanged;
        private event Action<PluginStateUpdate> BotStateChanged;

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

        private ReductionCollection _reductions;
        public MappingCollectionInfo _mappingsInfo;

        private event Action<PackageUpdate> PackageChanged;
        private event Action<PackageStateUpdate> PackageStateChanged;


        private MappingCollectionInfo GetMappingsInfo()
        {
            return _mappingsInfo;
        }

        #endregion

        private async Task ShutdownAsync()
        {
            _logger.Debug("ServerModel is shutting down...");

            await _algoServer.Stop();
            await _threadPoolManager.Stop();
        }
    }
}
