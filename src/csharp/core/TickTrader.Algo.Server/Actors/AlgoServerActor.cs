using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;
using TickTrader.Algo.Runtime;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AlgoServerActor : Actor
    {
        public const string InternalApiAddress = "127.0.0.1";

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoServerActor>();

        private readonly AlgoServerSettings _settings;

        private EnvService _env;
        private IActorRef _eventBus, _stateManager, _indicatorHost;
        private ServerStateModel _savedState;
        private AlertManagerModel _alerts;
        private AlgoServerPrivate _serverPrivate;
        private PackageStorage _pkgStorage;
        private RuntimeManager _runtimes;
        private AccountManager _accounts;
        private PluginManager _plugins;
        private PluginFileManager _pluginFiles;
        private RpcServer _internalApiServer;

        private MappingCollectionInfo _mappings;


        private AlgoServerActor(AlgoServerSettings settings)
        {
            _settings = settings;

            Receive<EventBusRequest, IActorRef>(_ => _eventBus);

            Receive<StartCmd>(Start);
            Receive<StopCmd>(Stop);
            Receive<NeedLegacyStateRequest, bool>(_ => !File.Exists(_env.ServerStateFilePath));
            Receive<LoadLegacyStateCmd>(cmd => _savedState.LoadSavedState(cmd.SavedState));

            Receive<AlgoServerPrivate.RuntimeRequest, IActorRef>(r => _runtimes.GetRuntime(r.Id));
            Receive<AlgoServerPrivate.PkgRuntimeIdRequest, string>(r => _runtimes.GetPkgRuntimeId(r.PkgId));
            Receive<AlgoServerPrivate.RuntimeStoppedMsg>(msg => _runtimes.OnRuntimeStopped(msg.Id));
            Receive<AlgoServerPrivate.PkgRuntimeInvalidMsg>(OnPkgRuntimeInvalid);
            Receive<AlgoServerPrivate.AccountControlRequest, IActorRef>(GetAccountControlInternal);

            Receive<LocalAlgoServer.PkgFileExistsRequest, bool>(r => _pkgStorage.PackageFileExists(r.PkgName));
            Receive<LocalAlgoServer.PkgBinaryRequest, byte[]>(r => _pkgStorage.GetPackageBinary(r.Id));
            Receive<LocalAlgoServer.UploadPackageCmd, string>(cmd => _pkgStorage.UploadPackage(cmd.Request, cmd.FilePath));
            Receive<RemovePackageRequest>(r => _pkgStorage.RemovePackage(r));
            Receive<MappingsInfoRequest, MappingCollectionInfo>(_ => _mappings);

            Receive<AddAccountRequest, string>(r => _accounts.AddAccount(r));
            Receive<ChangeAccountRequest>(r => _accounts.ChangeAccount(r));
            Receive<RemoveAccountRequest>(RemoveAccount);
            Receive<TestAccountRequest>(r => _accounts.TestAccount(r));
            Receive<TestAccountCredsRequest>(r => _accounts.TestAccountCreds(r));
            Receive<AccountMetadataRequest>(r => _accounts.GetMetadata(r));

            Receive<LocalAlgoServer.PluginExistsRequest, bool>(r => _plugins.PluginExists(r.Id));
            Receive<LocalAlgoServer.GeneratePluginIdRequest, string>(r => _plugins.GeneratePluginId(r.PluginDisplayName));
            Receive<AddPluginRequest>(r => _plugins.AddPlugin(r));
            Receive<ChangePluginConfigRequest>(r => _plugins.UpdateConfig(r));
            Receive<RemovePluginRequest>(RemovePlugin);
            Receive<StartPluginRequest>(r => _plugins.StartPlugin(r));
            Receive<StopPluginRequest>(r => _plugins.StopPlugin(r));
            Receive<PluginLogsRequest, PluginLogsResponse>(r => _plugins.GetPluginLogs(r));
            Receive<PluginStatusRequest, PluginStatusResponse>(r => _plugins.GetPluginStatus(r));

            Receive<PluginFolderInfoRequest, PluginFolderInfo>(r => _pluginFiles.GetFolderInfo(r));
            Receive<ClearPluginFolderRequest>(r => _pluginFiles.ClearFolder(r));
            Receive<DeletePluginFileRequest>(r => _pluginFiles.DeleteFile(r));
            Receive<DownloadPluginFileRequest, string>(r => _pluginFiles.GetFileReadPath(r));
            Receive<UploadPluginFileRequest, string>(r => _pluginFiles.GetFileWritePath(r));

            Receive<PluginAlertsRequest, AlertRecordInfo[]>(r => _alerts.GetAlerts(r));
            Receive<LocalAlgoServer.SubscribeToAlertsCmd>(cmd => _alerts.AttachAlertChannel(cmd.AlertSink));
            Receive<LocalAlgoServer.ExecPluginCmd>(cmd => _plugins.ExecCmd(cmd.PluginId, cmd.Command));
            Receive<LocalAlgoServer.IndicatorHostRequest, IndicatorHostModel>(_ => new IndicatorHostModel(_indicatorHost ?? throw new AlgoException("Indicator host not enabled")));
        }


        public static IActorRef Create(AlgoServerSettings settings)
        {
            return ActorSystem.SpawnLocal(() => new AlgoServerActor(settings), $"{nameof(AlgoServerActor)}");
        }


        protected override void ActorInit(object initMsg)
        {
            var reductions = new ReductionCollection();
            reductions.LoadDefaultReductions();
            _mappings = reductions.CreateMappings();

            _env = new EnvService(_settings.DataFolder);
            _eventBus = ServerBusActor.Create();
            _stateManager = ServerStateManager.Create(_env.ServerStateFilePath);
            _alerts = new AlertManagerModel(AlertManager.Create(_settings.MonitoringSettings));
            _savedState = new ServerStateModel(_stateManager);
            _serverPrivate = new AlgoServerPrivate(Self, _env, _eventBus, _savedState, _alerts)
            {
                AccountOptions = Account.ConnectionOptions.CreateForServer(_settings.EnableAccountLogs, _env.LogFolder),
                RuntimeSettings = _settings.RuntimeSettings,
                MonitoringSettings = _settings.MonitoringSettings,
            };

            _pkgStorage = new PackageStorage(_eventBus);
            _runtimes = new RuntimeManager(_serverPrivate);
            _accounts = new AccountManager(_serverPrivate);
            _plugins = new PluginManager(_serverPrivate);
            _pluginFiles = new PluginFileManager(_serverPrivate);

            _internalApiServer = new RpcServer(new TcpFactory(), _serverPrivate);

            if (_settings.EnableIndicatorHost)
                _indicatorHost = IndicatorHostActor.Create(_serverPrivate);
        }


        public async Task Start(StartCmd cmd)
        {
            _logger.Debug("Starting...");

            await _pkgStorage.Start(_settings.PkgStorage, OnPackageRefUpdate);

            await _pkgStorage.WhenLoaded();

            await _internalApiServer.Start(InternalApiAddress, 0);
            _logger.Info($"Started AlgoServer internal API on port {_internalApiServer.BoundPort}");

            _serverPrivate.Address = InternalApiAddress;
            _serverPrivate.BoundPort = _internalApiServer.BoundPort;

            var stateSnapshot = await _savedState.GetSnapshot();

            _accounts.Restore(stateSnapshot);

            _plugins.Restore(stateSnapshot);

            _logger.Debug("Started");
        }

        public async Task Stop(StopCmd cmd)
        {
            _logger.Debug("Stopping...");

            await _savedState.StopSaving();

            await _plugins.Shutdown();

            if (_indicatorHost != null)
                await _indicatorHost.Ask(IndicatorHostModel.ShutdownCmd.Instance);

            await _pkgStorage.Stop();

            await _runtimes.Shutdown();

            await _accounts.Shutdown();

            await _internalApiServer.Stop();

            _logger.Debug("Stopped");
        }


        private void OnPackageRefUpdate(PackageVersionUpdate update)
        {
            var pkgId = update.PkgId;
            var pkgRefId = update.LatestPkgRefId;

            _runtimes.MarkPkgRuntimeObsolete(pkgId);

            string runtimeId = null;
            if (!string.IsNullOrEmpty(pkgRefId))
            {
                runtimeId = CreatePkgRuntime(pkgId);
            }
            var pkgUpdate = new PkgRuntimeUpdate(pkgId, runtimeId);
            _plugins.TellAllPlugins(pkgUpdate);
            _indicatorHost?.Tell(pkgUpdate);
        }

        private void OnPkgRuntimeInvalid(AlgoServerPrivate.PkgRuntimeInvalidMsg msg)
        {
            var pkgId = msg.PkgId;

            // Runtime can be marked obsolete after sending notification about invalid state
            // We need to check if sender runtime is still package runtime
            if (_runtimes.GetPkgRuntimeId(pkgId) == msg.RuntimeId)
            {
                _runtimes.MarkPkgRuntimeObsolete(pkgId);

                var runtimeId = CreatePkgRuntime(pkgId);
                _plugins.TellAllPlugins(new PkgRuntimeUpdate(pkgId, runtimeId));
            }
        }

        private string CreatePkgRuntime(string pkgId)
        {
            var pkgRef = _pkgStorage.GetPkgRef(pkgId);

            if (pkgRef == null)
            {
                _logger.Debug($"Skipped creating runtime for package '{pkgId}': no package");
                return null;
            }
            else if (!pkgRef.PkgInfo.IsValid)
            {
                _logger.Debug($"Skipped creating runtime for pkg ref '{pkgRef.Id}': invalid package");
                return null;
            }
            else
            {
                var runtimeId = _runtimes.CreatePkgRuntime(pkgRef);
                _logger.Debug($"Package '{pkgId}' current runtime: {runtimeId}");
                return runtimeId;
            }
        }

        private async Task RemoveAccount(RemoveAccountRequest request)
        {
            var accId = request.AccountId;
            var account = _accounts.GetAccountRefOrThrow(accId);

            await _plugins.RemoveAllPluginsFromAccount(accId);

            await _accounts.RemoveAccountInternal(accId, account);
        }

        private async Task RemovePlugin(RemovePluginRequest request)
        {
            var pluginId = request.PluginId;

            await _plugins.RemovePlugin(request);

            if (request.CleanAlgoData)
            {
                try
                {
                    _pluginFiles.ClearFolder(new ClearPluginFolderRequest(pluginId, PluginFolderInfo.Types.PluginFolderId.AlgoData));
                }
                catch (Exception) { }
            }
            if (request.CleanLog)
            {
                try
                {
                    _pluginFiles.ClearFolder(new ClearPluginFolderRequest(pluginId, PluginFolderInfo.Types.PluginFolderId.BotLogs));
                }
                catch (Exception) { }
            }
        }

        private IActorRef GetAccountControlInternal(AlgoServerPrivate.AccountControlRequest request)
        {
            var accId = request.Id;
            if (accId == IndicatorHostActor.AccId)
                return _indicatorHost;

            return _accounts.GetAccountRefOrThrow(accId);
        }


        internal class EventBusRequest : Singleton<EventBusRequest> { }

        internal class StartCmd : Singleton<StartCmd> { }

        internal class StopCmd : Singleton<StopCmd> { }

        internal class NeedLegacyStateRequest : Singleton<NeedLegacyStateRequest> { }

        internal class LoadLegacyStateCmd
        {
            public ServerSavedState SavedState { get; }

            public LoadLegacyStateCmd(ServerSavedState savedState)
            {
                SavedState = savedState;
            }
        }

        internal class PkgRuntimeUpdate
        {
            public string PkgId { get; }

            public string RuntimeId { get; }

            public PkgRuntimeUpdate(string pkgId, string runtimeId)
            {
                PkgId = pkgId;
                RuntimeId = runtimeId;
            }
        }
    }
}
