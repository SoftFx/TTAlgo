using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.Server.PublicAPI.Converters;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    /// <summary>
    /// Not thread-safe requires actor context
    /// </summary>
    internal sealed class ServerUpdateDistributor
    {
        private const int AlertsUpdateTimeout = 1000;

        private readonly ILogger _logger;
        private readonly AlgoServerAdapter _server;
        private readonly MessageFormatter _msgFormatter;
        private readonly Task<bool> _initTask;
        private readonly CancellationTokenSource _stopTokenSrc;
        private readonly LinkedList<SessionHandler> _sessions = new LinkedList<SessionHandler>();

        private ApiMetadataInfo _apiMetadata;
        private SetupContextInfo _setupContext;
        private MappingCollectionInfo _mappings;
        private Channel<IMessage> _updateChannel;
        private Task _consumeUpdatesTask, _dispatchAlertsTask;
        private Timestamp _lastAlertTimeUtc;
        private ulong _metadataVersion, _metadataSnapshotVersion;
        private UpdateInfo _metadataSnapshot;
        private string _metadataSnapshotLogMsg;
        private Dictionary<string, PackageInfo> _packages;
        private Dictionary<string, AccountModelInfo> _accounts;
        private Dictionary<string, PluginModelInfo> _plugins;
        private UpdateServiceInfo _updateSvcInfo;


        public ServerUpdateDistributor(AlgoServerAdapter server, ILogger logger, MessageFormatter msgFormatter)
        {
            _server = server;
            _logger = logger;
            _msgFormatter = msgFormatter;
            _stopTokenSrc = new CancellationTokenSource();
            _initTask = InitInternal(server);
        }


        public async Task Shudown()
        {
            _stopTokenSrc.Cancel();
            await _initTask;

            _updateChannel?.Writer.TryComplete();
            if (_consumeUpdatesTask != null)
                await _consumeUpdatesTask;
            if (_dispatchAlertsTask != null)
                await _dispatchAlertsTask;
        }

        public async Task AttachSession(SessionHandler session)
        {
            var initSuccess = await _initTask;
            if (!initSuccess)
                throw new Domain.AlgoException("Update distributor not initialized");

            if (!UpdateMetadataSnapshot())
                throw new Domain.AlgoException("Failed to update metadata snapshot");
            if (!session.TryWrite(_metadataSnapshot, _metadataSnapshotLogMsg))
                throw new Domain.AlgoException("Failed to send metadata");
            _sessions.AddLast(session);
        }


        private async Task<bool> InitInternal(AlgoServerAdapter server)
        {
            try
            {
                _apiMetadata = (await server.GetApiMetadata()).ToApi();
                _setupContext = (await server.GetSetupContext()).ToApi();
                _mappings = (await server.GetMappingsInfo()).ToApi();

                if (_stopTokenSrc.IsCancellationRequested)
                    return false;

                _updateChannel = DefaultChannelFactory.CreateUnbounded<IMessage>(true, true);
                await server.AttachSessionChannel(_updateChannel);

                var reader = _updateChannel.Reader;
                var cancelToken = _stopTokenSrc.Token;
                for (var i = 0; i < 4; i++)
                {
                    ProcessInitMsg(await _updateChannel.Reader.ReadAsync(_stopTokenSrc.Token));
                }
                if (_packages == null || _accounts == null || _plugins == null || _updateSvcInfo == null)
                {
                    _logger.Error($"Init sequence failed: pkg={_packages != null}, acc={_accounts != null}, plugin={_plugins != null}, updateSvc={_updateSvcInfo != null}");
                    _updateChannel.Writer?.TryComplete();
                    return false;
                }
                _metadataVersion++;

                _consumeUpdatesTask = _updateChannel.Consume(ProcessUpdate, 1, 1, _stopTokenSrc.Token);
                _dispatchAlertsTask = DispatchAlertsLoop(_stopTokenSrc.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to init");
                return false;
            }

            return true;
        }

        private void ProcessInitMsg(IMessage update)
        {
            switch (update)
            {
                case Domain.PackageListSnapshot pkgList: _packages = pkgList.Packages.ToDictionary(p => p.PackageId, p => p.ToApi()); break;
                case Domain.AccountListSnapshot accList: _accounts = accList.Accounts.ToDictionary(a => a.AccountId, a => a.ToApi()); break;
                case Domain.PluginListSnapshot pluginList: _plugins = pluginList.Plugins.ToDictionary(p => p.InstanceId, p => p.ToApi()); break;
                case Domain.ServerControl.UpdateServiceInfo updateSvcState: _updateSvcInfo = updateSvcState.ToApi(); break;
                default:
                    _logger.Error($"Unexpected msg of type {update.GetType().FullName}");
                    break;
            }
        }

        private bool UpdateMetadataSnapshot()
        {
            if (_metadataVersion == _metadataSnapshotVersion)
                return true;

            var update = new AlgoServerMetadataUpdate
            {
                ExecResult = AlgoServerPublicImpl.CreateSuccessResult(),
                ApiMetadata = _apiMetadata,
                SetupContext = _setupContext,
                MappingsCollection = _mappings,
                UpdateSvc = _updateSvcInfo,
            };

            update.Packages.AddRange(_packages.Values);
            update.Accounts.AddRange(_accounts.Values);
            update.Plugins.AddRange(_plugins.Values);

            if (UpdateInfo.TryPack(update, out _metadataSnapshot, true))
            {
                _metadataSnapshotLogMsg = _msgFormatter.FormatUpdateToClient(update, _metadataSnapshot.Payload.Length, _metadataSnapshot.Compressed);
                _metadataSnapshotVersion = _metadataVersion;

                return true;
            }

            return false;
        }

        private void ProcessUpdate(IMessage update)
        {
            try
            {
                bool handled = false;
                IMessage apiUpdate = null;
                switch (update)
                {
                    case Domain.PackageUpdate pkgUpdate: (handled, apiUpdate) = OnPackageUpdate(pkgUpdate.ToApi()); break;
                    case Domain.AccountModelUpdate accUpdate: (handled, apiUpdate) = OnAccountUpdate(accUpdate.ToApi()); break;
                    case Domain.PluginModelUpdate pluginUpdate: (handled, apiUpdate) = OnPluginUpdate(pluginUpdate.ToApi()); break;
                    case Domain.PackageStateUpdate pkgStateUpdate: (handled, apiUpdate) = OnPackageStateUpdate(pkgStateUpdate.ToApi()); break;
                    case Domain.AccountStateUpdate accStateUpdate: (handled, apiUpdate) = OnAccountStateUpdate(accStateUpdate.ToApi()); break;
                    case Domain.PluginStateUpdate pluginStateUpdate: (handled, apiUpdate) = OnPluginStateUpdate(pluginStateUpdate.ToApi()); break;
                    case Domain.ServerControl.UpdateServiceStateUpdate updateSvcUpdate: (handled, apiUpdate) = OnUpdateSvcStateUpdate(updateSvcUpdate.ToApi()); break;
                }

                if (!handled)
                {
                    _logger.Error($"Failed to process update of type '{update.Descriptor.FullName}'");
                    return;
                }

                _metadataVersion++;
                DispatchUpdate(apiUpdate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to process update of type '{update.Descriptor.FullName}'");
            }
        }

        private (bool, IMessage) OnPackageUpdate(PackageUpdate update)
        {
            var id = update.Id;
            var pkg = update.Package;
            switch (update.Action)
            {
                case Update.Types.Action.Added: _packages.Add(id, pkg); break;
                case Update.Types.Action.Updated: _packages[id] = pkg; break;
                case Update.Types.Action.Removed: _packages.Remove(id); break;
                default: _logger.Error($"Unknown update action: {update}"); return (false, update);
            }

            return (true, update);
        }

        private (bool, IMessage) OnAccountUpdate(AccountModelUpdate update)
        {
            var id = update.Id;
            var acc = update.Account;
            switch (update.Action)
            {
                case Update.Types.Action.Added: _accounts.Add(id, acc); break;
                case Update.Types.Action.Updated: _accounts[id] = acc; break;
                case Update.Types.Action.Removed: _accounts.Remove(id); break;
                default: _logger.Error($"Unknown update action: {update}"); return (false, update);
            }

            return (true, update);
        }

        private (bool, IMessage) OnPluginUpdate(PluginModelUpdate update)
        {
            var id = update.Id;
            var plugin = update.Plugin;
            switch (update.Action)
            {
                case Update.Types.Action.Added: _plugins.Add(id, plugin); break;
                case Update.Types.Action.Updated: _plugins[id] = plugin; break;
                case Update.Types.Action.Removed: _plugins.Remove(id); break;
                default: _logger.Error($"Unknown update action: {update}"); return (false, update);
            }

            return (true, update);
        }

        private (bool, IMessage) OnPackageStateUpdate(PackageStateUpdate update)
        {
            var id = update.Id;
            if (!_packages.TryGetValue(id, out var pkg))
            {
                _logger.Error($"Package not found. Can't update state: {update}");
                return (false, update);
            }

            pkg.IsLocked = update.IsLocked;

            return (true, update);
        }

        private (bool, IMessage) OnAccountStateUpdate(AccountStateUpdate update)
        {
            var id = update.Id;
            if (!_accounts.TryGetValue(id, out var acc))
            {
                _logger.Error($"Account not found. Can't update state: {update}");
                return (false, update);
            }

            acc.ConnectionState = update.ConnectionState;
            var lastError = update.LastError;
            if (lastError != null)
                acc.LastError = update.LastError;

            return (true, update);
        }

        private (bool, IMessage) OnPluginStateUpdate(PluginStateUpdate update)
        {
            var id = update.Id;
            if (!_plugins.TryGetValue(id, out var plugin))
            {
                _logger.Error($"Plugin not found. Can't update state: {update}");
                return (false, update);
            }

            plugin.State = update.State;
            var faultMsg = update.FaultMessage;
            if (faultMsg != null)
                plugin.FaultMessage = faultMsg;

            return (true, update);
        }

        private (bool, IMessage) OnUpdateSvcStateUpdate(UpdateServiceStateUpdate update)
        {
            _updateSvcInfo = update.Snapshot;
            return (true, update);
        }

        private void DispatchUpdate(IMessage update, bool compress = false)
        {
            if (!UpdateInfo.TryPack(update, out var packedUpdate, compress))
            {
                _logger.Error($"Failed to pack msg '{update.Descriptor.Name}'");
                return;
            }

            var logMsg = _msgFormatter.FormatUpdateToClient(update, packedUpdate.Payload.Length, packedUpdate.Compressed);

            var node = _sessions.First;
            while (node != null)
            {
                var session = node.Value;
                var nextNode = node.Next;
                if (!session.TryWrite(packedUpdate, logMsg))
                {
                    _sessions.Remove(node);
                }
                node = nextNode;
            }
        }

        private async Task DispatchAlertsLoop(CancellationToken stopToken)
        {
            _lastAlertTimeUtc = DateTime.UtcNow.ToTimestamp();

            while (!stopToken.IsCancellationRequested)
            {
                await DispatchAlerts();

                await Task.Delay(AlertsUpdateTimeout);
            }
        }

        private async Task DispatchAlerts()
        {
            try
            {
                var update = new AlertListUpdate();

                var alerts = await _server.GetAlertsAsync(new Domain.ServerControl.PluginAlertsRequest
                {
                    MaxCount = 1000,
                    LastLogTimeUtc = _lastAlertTimeUtc,
                });

                update.Alerts.AddRange(alerts.Select(u => u.ToApi()));

                _lastAlertTimeUtc = alerts.Max(u => u.TimeUtc) ?? _lastAlertTimeUtc;

                if (alerts.Length > 0)
                    DispatchUpdate(update, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to dispatch alerts");
            }
        }
    }
}
