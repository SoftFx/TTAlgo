using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AlgoServerActor : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoServerActor>();

        private readonly IActorRef _algoHost;
        private readonly AlgoServerSettings _settings;

        private EnvService _env;
        private IActorRef _eventBus, _stateManager;
        private ServerStateModel _savedState;
        private AlertManagerModel _alerts;
        private AlgoServerPrivate _serverPrivate;
        private AccountManager _accounts;
        private PluginManager _plugins;
        private PluginFileManager _pluginFiles;
        private AutoUpdateManager _updateSvc;


        private AlgoServerActor(IActorRef algoHost, AlgoServerSettings settings)
        {
            _algoHost = algoHost;
            _settings = settings;

            Receive<EventBusRequest, IActorRef>(_ => _eventBus);

            Receive<StartCmd>(Start);
            Receive<StopCmd>(Stop);
            Receive<NeedLegacyStateRequest, bool>(_ => !File.Exists(_env.ServerStateFilePath));
            Receive<LoadLegacyStateCmd>(cmd => _savedState.LoadSavedState(cmd.SavedState));

            Receive<PackageUpdate>(upd => _eventBus.Tell(upd));
            Receive<PackageStateUpdate>(upd => _eventBus.Tell(upd));
            Receive<RuntimeControlModel.PkgRuntimeUpdateMsg>(upd => OnPackageRuntimeUpdate(upd));

            Receive<RuntimeServerModel.RuntimeRequest, IActorRef>(r => _algoHost.Ask<IActorRef>(r));
            Receive<RuntimeServerModel.PkgRuntimeIdRequest, string>(r => _algoHost.Ask<string>(r));
            Receive<RuntimeServerModel.AccountControlRequest, IActorRef>(GetAccountControlInternal);

            Receive<AlgoHostModel.PkgFileExistsRequest, bool>(r => _algoHost.Ask<bool>(r));
            Receive<AlgoHostModel.PkgBinaryRequest, byte[]>(r => _algoHost.Ask<byte[]>(r));
            Receive<AlgoHostModel.UploadPackageCmd, string>(cmd => _algoHost.Ask<string>(cmd));
            Receive<RemovePackageRequest>(r => _algoHost.Ask(r));
            Receive<MappingsInfoRequest, MappingCollectionInfo>(r => _algoHost.Ask<MappingCollectionInfo>(r));

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
            Receive<PluginOwner.ExecPluginCmd>(cmd => _plugins.ExecCmd(cmd.PluginId, cmd.Command));

            Receive<ServerVersionRequest, string>(r => _updateSvc.GetCurrentVersion());
            Receive<ServerUpdateListRequest, ServerUpdateListResponse>(r => _updateSvc.GetUpdateList(r));
            Receive<StartServerUpdateRequest, StartServerUpdateResponse>(r => _updateSvc.StartUpdate(r));
        }


        public static IActorRef Create(IActorRef algoHost, AlgoServerSettings settings)
        {
            return ActorSystem.SpawnLocal(() => new AlgoServerActor(algoHost, settings), $"{nameof(AlgoServerActor)}");
        }


        protected override void ActorInit(object initMsg)
        {
            _env = new EnvService(_settings.DataFolder);
            _eventBus = ServerBusActor.Create();
            _stateManager = ServerStateManager.Create(_env.ServerStateFilePath);
            _alerts = new AlertManagerModel(AlertManager.Create(_settings.MonitoringSettings));
            _savedState = new ServerStateModel(_stateManager);
            _serverPrivate = new AlgoServerPrivate(Self, _env, _eventBus, _savedState, _alerts)
            {
                AccountOptions = Account.ConnectionOptions.CreateForServer(_settings.EnableAccountLogs, _env.LogFolder),
                MonitoringSettings = _settings.MonitoringSettings,
            };

            _accounts = new AccountManager(_serverPrivate);
            _plugins = new PluginManager(_serverPrivate);
            _pluginFiles = new PluginFileManager(_serverPrivate);
            _updateSvc = new AutoUpdateManager(_serverPrivate);
        }


        public async Task Start(StartCmd cmd)
        {
            _logger.Debug("Starting...");

            var stateSnapshot = await _savedState.GetSnapshot();

            _accounts.Restore(stateSnapshot);

            _plugins.Restore(stateSnapshot);

            _updateSvc.Start();

            _logger.Debug("Started");
        }

        public async Task Stop(StopCmd cmd)
        {
            _logger.Debug("Stopping...");

            await _savedState.StopSaving();

            await _plugins.Shutdown();

            await _accounts.Shutdown();

            _updateSvc.Stop();

            _logger.Debug("Stopped");
        }


        private void OnPackageRuntimeUpdate(RuntimeControlModel.PkgRuntimeUpdateMsg pkgUpdate)
        {
            _plugins.TellAllPlugins(pkgUpdate);
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

        private IActorRef GetAccountControlInternal(RuntimeServerModel.AccountControlRequest request)
        {
            var accId = request.Id;

            return _accounts.GetAccountRefOrDefault(accId);
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
    }
}
