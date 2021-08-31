using ActorSharp;
using Google.Protobuf;
using Grpc.Core;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;

using AlgoServerApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl.Grpc
{
    internal class ServerSession : Actor
    {
        private string _sessionId;
        private string _username;
        private ILogger _logger;
        private VersionSpec _versionSpec;
        private MessageFormatter _messageFormatter;
        private AccessManager _accessManager;
        private IServerStreamWriter<AlgoServerApi.UpdateInfo> _updateStream;
        private Channel<IMessage> _updateChannel;
        private CancellationTokenSource _updateCancelSrc;
        private bool _isFaulted;


        private void Init(string sessionId, string username, ILogger logger, VersionSpec versionSpec, MessageFormatter messageFormatter, AccessManager accessManager)
        {
            _sessionId = sessionId;
            _username = username;
            _logger = logger;
            _versionSpec = versionSpec;
            _messageFormatter = messageFormatter;
            _accessManager = accessManager;

            _isFaulted = false;
        }


        public class Handler : BlockingHandler<ServerSession>
        {
            public string SessionId { get; }

            public string Username { get; }

            public ILogger Logger { get; }

            public VersionSpec VersionSpec { get; }

            public AccessManager AccessManager { get; }

            public bool IsFaulted { get; private set; }


            public Handler(string sessionId, string username, int clientMinorVersion, LogFactory logFactory, MessageFormatter messageFormatter, ClientClaims.Types.AccessLevel accessLevel)
                : base(SpawnLocal<ServerSession>(null, $"ServerSession: {sessionId}"))
            {
                SessionId = sessionId;
                Username = username;

                VersionSpec = new VersionSpec(Math.Min(clientMinorVersion, VersionSpec.MinorVersion));
                AccessManager = new AccessManager(accessLevel);
                Logger = logFactory.GetLogger($"{LoggerHelper.SessionLoggerPrefix}{SessionId}");
                CallActor(a => a.Init(SessionId, Username, Logger, VersionSpec, messageFormatter, AccessManager));
                IsFaulted = false;
            }

            public Task SetupUpdateStream(IServerStreamWriter<AlgoServerApi.UpdateInfo> updateStream, Func<Channel<IMessage>, Task> initDelegate)
                => CallActorAsync(a => a.SetupUpdateStream(updateStream, initDelegate));

            public void SendUpdate(IMessage update)
            {
                if (IsFaulted)
                    return;

                ActorSend(a =>
                {
                    if (a._isFaulted)
                    {
                        IsFaulted = a._isFaulted;
                        return;
                    }

                    a._updateChannel?.Writer.TryWrite(update);
                });
            }

            public void CloseUpdateStream() => CallActor(a => a.CloseUpdateStream());

            public void CancelUpdateStream() => CallActor(a => a.CancelUpdateStream());
        }


        private async Task SetupUpdateStream(IServerStreamWriter<AlgoServerApi.UpdateInfo> updateStream, Func<Channel<IMessage>, Task> initDelegate)
        {
            if (_updateStream != null)
                throw new AlgoServerException($"Session {_sessionId} already has opened update stream");

            _updateStream = updateStream;
            _updateChannel = DefaultChannelFactory.CreateUnbounded<IMessage>();
            _updateCancelSrc = new CancellationTokenSource();

            _logger.Info("Init update stream");
            await initDelegate(_updateChannel);
            await SendSnapshot(_updateCancelSrc.Token);

            var t = _updateChannel.Consume(SendUpdate, 1, 1, _updateCancelSrc.Token);
            _logger.Info("Opened update stream");
            await t;
        }

        private async Task SendSnapshot(CancellationToken cancelToken)
        {
            var cnt = 0;
            var reader = _updateChannel.Reader;
            var update = new AlgoServerApi.AlgoServerMetadataUpdate
            {
                ExecResult = BotAgentServerImpl.CreateSuccessResult(),
            };
            while (cnt < 6)
            {
                var msg = await reader.ReadAsync(cancelToken);
                switch (msg)
                {
                    case ApiMetadataInfo apiMetadata:
                        if (update.ApiMetadata == null)
                            cnt++;
                        update.ApiMetadata = apiMetadata.ToApi();
                        break;
                    case SetupContextInfo setupContext:
                        if (update.SetupContext == null)
                            cnt++;
                        update.SetupContext = setupContext.ToApi();
                        break;
                    case MappingCollectionInfo mappings:
                        if (update.MappingsCollection == null)
                            cnt++;
                        update.MappingsCollection = mappings.ToApi();
                        break;
                    case PackageListSnapshot pkgList:
                        if (update.Packages.Count == 0)
                            cnt++;
                        update.Packages.Clear();
                        update.Packages.AddRange(pkgList.Packages.Select(p => p.ToApi()));
                        break;
                    case AccountListSnapshot accList:
                        if (update.Accounts.Count == 0)
                            cnt++;
                        update.Accounts.Clear();
                        update.Accounts.AddRange(accList.Accounts.Select(a => a.ToApi()));
                        break;
                    case PluginListSnapshot pluginList:
                        if (update.Plugins.Count == 0)
                            cnt++;
                        update.Plugins.Clear();
                        update.Plugins.AddRange(pluginList.Plugins.Select(p => p.ToApi()));
                        break;
                    default:
                        _logger.Error($"Unexpected msg of type {msg.GetType().FullName}");
                        break;
                }
                _logger.Debug($"Send snapshot: cnt = {cnt}");
            }

            if (!AlgoServerApi.UpdateInfo.TryPack(update, out var packedUpdate, true))
            {
                _logger.Error("Failed to pack server metadata");
                CancelUpdateStream();
                return;
            }
            _messageFormatter.LogServerResponse(_logger, packedUpdate);
            await _updateStream.WriteAsync(packedUpdate);
        }

        private async Task SendUpdate(IMessage update)
        {
            try
            {
                var apiUpdate = ConvertToApiUpdate(update);
                if (apiUpdate == null)
                {
                    _logger.Error($"Unknown msg of type {update.Descriptor.Name}");
                    return;
                }

                _messageFormatter.LogServerResponse(_logger, apiUpdate);
                if (!AlgoServerApi.UpdateInfo.TryPack(apiUpdate, out var packedUpdate))
                {
                    _logger.Error($"Failed to pack msg '{apiUpdate.Descriptor.Name}'");
                    return;
                }
                await _updateStream.WriteAsync(packedUpdate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {update}");
                _isFaulted = true;
            }
        }

        private static IMessage ConvertToApiUpdate(IMessage update)
        {
            IMessage apiUpdate = null;
            switch (update)
            {
                case AlgoServerApi.UpdateInfo packedUpdate: apiUpdate = packedUpdate; break;
                case PackageUpdate pkgUpdate: apiUpdate = pkgUpdate.ToApi(); break;
                case AccountModelUpdate accUpdate: apiUpdate = accUpdate.ToApi(); break;
                case PluginModelUpdate pluginUpdate: apiUpdate = pluginUpdate.ToApi(); break;
                case PackageStateUpdate pkgStateUpdate: apiUpdate = pkgStateUpdate.ToApi(); break;
                case AccountStateUpdate accStateUpdate: apiUpdate = accStateUpdate.ToApi(); break;
                case PluginStateUpdate pluginStateUpdate: apiUpdate = pluginStateUpdate.ToApi(); break;
            }
            return apiUpdate;
        }

        private void SendUpdate(IUpdateInfo update)
        {
            if (_updateStream == null)
                return;

            try
            {
                //_updateStream.WriteAsync(update.Pack().ToApi()).Wait();
                _messageFormatter.LogServerUpdate(_logger, update);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {update}");
                _isFaulted = true;
            }
        }

        private void CloseUpdateStream() // client disconnect
        {
            if (_updateStream == null)
                return;

            _updateCancelSrc.Cancel();
            _updateChannel.Writer.TryComplete();
            _updateStream = null;

            _logger.Info("Closed update stream - client request");
        }

        private void CancelUpdateStream() // server disconnect
        {
            if (_updateStream == null)
                return;

            _updateCancelSrc.Cancel();
            _updateChannel.Writer.TryComplete();
            _updateStream = null;

            _logger.Info("Closed update stream - server request");
        }
    }
}
