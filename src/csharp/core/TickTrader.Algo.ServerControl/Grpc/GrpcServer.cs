using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.Server.Common.Grpc;

using AlgoServerApi = TickTrader.Algo.Server.PublicAPI;
using GrpcCore = Grpc.Core;


namespace TickTrader.Algo.ServerControl.Grpc
{
    public class GrpcServer : ProtocolServer
    {
        private IJwtProvider _jwtProvider;
        private BotAgentServerImpl _impl;
        private GrpcCore.Server _server;


        public GrpcServer(IAlgoServerProvider agentServer, ServerSettings settings, IJwtProvider jwtProvider) : base(agentServer, settings)
        {
            _jwtProvider = jwtProvider;
        }


        protected override Task StartServer()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            _impl = new BotAgentServerImpl(AlgoSrv, _jwtProvider, Logger, Settings.LogMessages, VersionSpec);
            var creds = new SslServerCredentials(new[] { new KeyCertificatePair(CertificateProvider.ServerCertificate, CertificateProvider.ServerKey), }); //,CertificateProvider.RootCertificate, true);
            _server = new GrpcCore.Server
            {
                Services = { AlgoServerApi.AlgoServerPublic.BindService(_impl) },
                Ports = { new ServerPort("0.0.0.0", Settings.ServerPort, creds) },
            };
            _server.Start();

            return Task.FromResult(this);
        }

        protected override async Task StopServer()
        {
            _impl?.Dispose();
            await _server.ShutdownAsync();
        }
    }


    internal class BotAgentServerImpl : AlgoServerApi.AlgoServerPublic.AlgoServerPublicBase, IDisposable
    {
        private const int AlertsUpdateTimeout = 1000;
        private const int PluginStatusUpdateTimeout = 1000;
        private const int PluginLogsUpdateTimeout = 1000;
        private const int HeartbeatUpdateTimeout = 1000;
        private const int HeartbeatTimeout = 10000;

        private readonly UpdateInfo<AlgoServerApi.HeartbeatUpdate> _heartbeat;

        private HashSet<string> _subscribedPluginsToStatus;
        private HashSet<string> _subscribedPluginsToLogs;

        private IAlgoServerProvider _algoServer;
        private IJwtProvider _jwtProvider;
        private ILogger _logger;
        private MessageFormatter _messageFormatter;
        private Dictionary<string, ServerSession.Handler> _sessions;
        private VersionSpec _version;

        private Timer _pluginStatusTimer, _pluginLogsTimer, _alertTimer, _heartbeatTimer;
        private Timestamp _lastAlertTimeUtc;
        private Timestamp _lastLogTimeUtc;


        public BotAgentServerImpl(IAlgoServerProvider algoServer, IJwtProvider jwtProvider, ILogger logger, bool logMessages, VersionSpec version)
        {
            _version = version;
            _algoServer = algoServer;
            _jwtProvider = jwtProvider;
            _logger = logger;

            _messageFormatter = new MessageFormatter(AlgoServerApi.AlgoServerPublicAPIReflection.Descriptor) { LogMessages = logMessages };
            _sessions = new Dictionary<string, ServerSession.Handler>();

            _algoServer.PackageUpdated += OnPackageUpdate;
            _algoServer.PackageStateUpdated += OnPackageStateUpdate;
            _algoServer.AccountUpdated += OnAccountUpdate;
            _algoServer.AccountStateUpdated += OnAccountStateUpdate;
            _algoServer.BotUpdated += OnBotUpdate;
            _algoServer.BotStateUpdated += OnBotStateUpdate;

            _algoServer.AdminCredsChanged += OnAdminCredsChanged;
            _algoServer.DealerCredsChanged += OnDealerCredsChanged;
            _algoServer.ViewerCredsChanged += OnViewerCredsChanged;

            _heartbeat = new UpdateInfo<AlgoServerApi.HeartbeatUpdate>(UpdateInfo.Types.UpdateType.Replaced, new AlgoServerApi.HeartbeatUpdate());
            _lastAlertTimeUtc = DateTime.UtcNow.ToTimestamp();

            _heartbeatTimer = new Timer(HeartbeatUpdate, null, HeartbeatTimeout, -1);
            _alertTimer = new Timer(OnAlertsUpdate, null, AlertsUpdateTimeout, -1);
        }

        private void HeartbeatUpdate(object o)
        {
            if (_heartbeatTimer == null)
                return;

            _heartbeatTimer.Change(-1, -1);

            SendUpdate(_heartbeat);

            _heartbeatTimer.Change(HeartbeatTimeout, -1);
        }

        public static AlgoServerApi.RequestResult CreateSuccessResult(string message = "")
        {
            return new AlgoServerApi.RequestResult { Status = AlgoServerApi.RequestResult.Types.RequestStatus.Success, Message = message ?? "" };
        }

        public static AlgoServerApi.RequestResult CreateErrorResult(string message = "")
        {
            return new AlgoServerApi.RequestResult { Status = AlgoServerApi.RequestResult.Types.RequestStatus.InternalServerError, Message = message ?? "" };
        }

        public static AlgoServerApi.RequestResult CreateErrorResult(Exception ex)
        {
            return CreateErrorResult(ex.Flatten().Message);
        }

        public static AlgoServerApi.RequestResult CreateUnauthorizedResult(string message = "")
        {
            return new AlgoServerApi.RequestResult { Status = AlgoServerApi.RequestResult.Types.RequestStatus.Unauthorized, Message = message ?? "" };
        }

        public static AlgoServerApi.RequestResult CreateUnauthorizedResult(Exception ex)
        {
            return CreateUnauthorizedResult(ex.Flatten().Message);
        }

        public static AlgoServerApi.RequestResult CreateRejectResult(string message = "")
        {
            return new AlgoServerApi.RequestResult { Status = AlgoServerApi.RequestResult.Types.RequestStatus.Reject, Message = message ?? "" };
        }

        public static AlgoServerApi.RequestResult CreateRejectResult(Exception ex)
        {
            return CreateRejectResult(ex.Flatten().Message);
        }

        public static AlgoServerApi.RequestResult CreateNotAllowedResult(string message = "")
        {
            return new AlgoServerApi.RequestResult { Status = AlgoServerApi.RequestResult.Types.RequestStatus.NotAllowed, Message = message ?? "" };
        }

        public static AlgoServerApi.RequestResult CreateNotAllowedResult(Exception ex)
        {
            return CreateNotAllowedResult(ex.Flatten().Message);
        }

        public static AlgoServerApi.RequestResult CreateNotAllowedResult(ServerSession.Handler session, string requestName)
        {
            return CreateNotAllowedResult($"{session.AccessManager.Level} is not allowed to execute {requestName}");
        }


        public void DisconnectAllClients()
        {
            lock (_sessions)
            {
                foreach (var session in _sessions.Values)
                {
                    session.CancelUpdateStream();
                }
                _sessions.Clear();
            }
        }


        #region Grpc request handlers overrides

        public override Task<AlgoServerApi.LoginResponse> Login(AlgoServerApi.LoginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequest(LoginInternal, request, context);
        }

        public override Task<AlgoServerApi.LogoutResponse> Logout(AlgoServerApi.LogoutRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(LogoutInternal, request, context);
        }

        public override Task<AlgoServerApi.PluginStatusSubscribeResponse> SubscribeToPluginStatus(AlgoServerApi.PluginStatusSubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(SubscribeToPluginStatusInternal, request, context);
        }

        public override Task<AlgoServerApi.PluginLogsSubscribeResponse> SubscribeToPluginLogs(AlgoServerApi.PluginLogsSubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(SubscribeToPluginLogsInternal, request, context);
        }

        public override Task<AlgoServerApi.PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatus(AlgoServerApi.PluginStatusUnsubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(UnsubscribeToPluginStatusInternal, request, context);
        }

        public override Task<AlgoServerApi.PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogs(AlgoServerApi.PluginLogsUnsubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(UnsubscribeToPluginLogsInternal, request, context);
        }

        public override Task<AlgoServerApi.AccountMetadataResponse> GetAccountMetadata(AlgoServerApi.AccountMetadataRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, request, context);
        }

        public override Task SubscribeToUpdates(AlgoServerApi.SubscribeToUpdatesRequest request, IServerStreamWriter<AlgoServerApi.UpdateInfo> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(SubscribeToUpdatesInternal, request, responseStream, context);
        }

        public override Task<AlgoServerApi.AddPluginResponse> AddPlugin(AlgoServerApi.AddPluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(AddBotInternal, request, context);
        }

        public override Task<AlgoServerApi.RemovePluginResponse> RemovePlugin(AlgoServerApi.RemovePluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemoveBotInternal, request, context);
        }

        public override Task<AlgoServerApi.StartPluginResponse> StartPlugin(AlgoServerApi.StartPluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StartBotInternal, request, context);
        }

        public override Task<AlgoServerApi.StopPluginResponse> StopPlugin(AlgoServerApi.StopPluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StopBotInternal, request, context);
        }

        public override Task<AlgoServerApi.ChangePluginConfigResponse> ChangePluginConfig(AlgoServerApi.ChangePluginConfigRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ChangeBotConfigInternal, request, context);
        }

        public override Task<AlgoServerApi.AddAccountResponse> AddAccount(AlgoServerApi.AddAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(AddAccountInternal, request, context);
        }

        public override Task<AlgoServerApi.RemoveAccountResponse> RemoveAccount(AlgoServerApi.RemoveAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemoveAccountInternal, request, context);
        }

        public override Task<AlgoServerApi.ChangeAccountResponse> ChangeAccount(AlgoServerApi.ChangeAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ChangeAccountInternal, request, context);
        }

        public override Task<AlgoServerApi.TestAccountResponse> TestAccount(AlgoServerApi.TestAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(TestAccountInternal, request, context);
        }

        public override Task<AlgoServerApi.TestAccountCredsResponse> TestAccountCreds(AlgoServerApi.TestAccountCredsRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(TestAccountCredsInternal, request, context);
        }

        public override Task<AlgoServerApi.UploadPackageResponse> UploadPackage(IAsyncStreamReader<AlgoServerApi.FileTransferMsg> requestStream, ServerCallContext context)
        {
            return ExecuteClientStreamingRequestAuthorized(UploadPackageInternal, requestStream, context);
        }

        public override Task<AlgoServerApi.RemovePackageResponse> RemovePackage(AlgoServerApi.RemovePackageRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemovePackageInternal, request, context);
        }

        public override Task DownloadPackage(AlgoServerApi.DownloadPackageRequest request, IServerStreamWriter<AlgoServerApi.FileTransferMsg> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadPackageInternal, request, responseStream, context);
        }

        public override Task<AlgoServerApi.PluginFolderInfoResponse> GetPluginFolderInfo(AlgoServerApi.PluginFolderInfoRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotFolderInfoInternal, request, context);
        }

        public override Task<AlgoServerApi.ClearPluginFolderResponse> ClearPluginFolder(AlgoServerApi.ClearPluginFolderRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ClearBotFolderInternal, request, context);
        }

        public override Task<AlgoServerApi.DeletePluginFileResponse> DeletePluginFile(AlgoServerApi.DeletePluginFileRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(DeleteBotFileInternal, request, context);
        }

        public override Task DownloadPluginFile(AlgoServerApi.DownloadPluginFileRequest request, IServerStreamWriter<AlgoServerApi.FileTransferMsg> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal, request, responseStream, context);
        }

        public override Task<AlgoServerApi.UploadPluginFileResponse> UploadPluginFile(IAsyncStreamReader<AlgoServerApi.FileTransferMsg> requestStream, ServerCallContext context)
        {
            return ExecuteClientStreamingRequestAuthorized(UploadBotFileInternal, requestStream, context);
        }

        #endregion Grpc request handlers overrides


        #region Credentials handlers

        private void DisconnectAllClients(ClientClaims.Types.AccessLevel accessLevel)
        {
            lock (_sessions)
            {
                var sessionsToRemove = _sessions.Values.Where(s => s.AccessManager.Level == accessLevel).ToList();
                foreach (var session in sessionsToRemove)
                {
                    session.CancelUpdateStream();
                    _sessions.Remove(session.SessionId);
                }
            }
        }

        private void OnAdminCredsChanged()
        {
            DisconnectAllClients(ClientClaims.Types.AccessLevel.Admin);
        }

        private void OnDealerCredsChanged()
        {
            DisconnectAllClients(ClientClaims.Types.AccessLevel.Dealer);
        }

        private void OnViewerCredsChanged()
        {
            DisconnectAllClients(ClientClaims.Types.AccessLevel.Viewer);
        }

        #endregion


        private async Task<TResponse> ExecuteUnaryRequest<TRequest, TResponse>(
            Func<TRequest, ServerCallContext, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogClientRequest(_logger, request);
                var response = await requestAction(request, context);
                _messageFormatter.LogClientResponse(_logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteUnaryRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, ServerCallContext, ServerSession.Handler, AlgoServerApi.RequestResult, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var session = GetSession(context, typeof(TRequest), out var execResult);

                _messageFormatter.LogClientRequest(session?.Logger, request);
                var response = await requestAction(request, context, session, execResult);
                _messageFormatter.LogClientResponse(session?.Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private Task ExecuteServerStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, ServerSession.Handler, AlgoServerApi.RequestResult, Task> requestAction,
            TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var session = GetSession(context, typeof(TRequest), out var execResult);

                _messageFormatter.LogClientRequest(session?.Logger, request);
                return requestAction(request, responseStream, context, session, execResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteClientStreamingRequestAuthorized<TRequest, TResponse>(
            Func<IAsyncStreamReader<TRequest>, ServerCallContext, ServerSession.Handler, AlgoServerApi.RequestResult, Task<TResponse>> requestAction,
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var session = GetSession(context, typeof(TRequest), out var execResult);

                var response = await requestAction(requestStream, context, session, execResult);
                _messageFormatter.LogClientResponse(session?.Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute request of type {typeof(TRequest).Name}");
                throw;
            }
        }

        private ServerSession.Handler GetSession(ServerCallContext context, System.Type requestType, out AlgoServerApi.RequestResult execResult)
        {
            execResult = CreateSuccessResult();

            try
            {
                var entry = context.RequestHeaders.LastOrDefault(e => e.Key == "authorization");
                if (entry == null)
                {
                    _logger.Error($"Missing authorization header for request of type {requestType.Name}");
                    execResult = CreateUnauthorizedResult("Missing authorization header");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(entry.Value) || !entry.Value.StartsWith("Bearer "))
                {
                    _logger.Error($"Authorization header value({entry.Value}) doesn't match Bearer authentication scheme for request of type {requestType.Name}");
                    execResult = CreateUnauthorizedResult("Authorization header doesn't match Bearer authentication scheme");
                    return null;
                }

                var accessToken = entry.Value.Substring(7);
                JwtPayload jwtPayload = null;
                try
                {
                    jwtPayload = _jwtProvider.ParseToken(accessToken);
                }
                catch (UnauthorizedException uex)
                {
                    _logger.Error($"{uex.Message}. Request of type {requestType.Name}");
                    execResult = CreateUnauthorizedResult(uex);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to parse access token({accessToken})");
                    execResult = CreateErrorResult("Failed to parse access token");
                }

                if (jwtPayload != null && !string.IsNullOrWhiteSpace(jwtPayload.SessionId))
                {
                    if (_sessions.TryGetValue(jwtPayload.SessionId, out var session))
                    {
                        return session;
                    }
                    else
                    {
                        _logger.Error($"Request was sent using invalid session id: {jwtPayload.SessionId}");
                        execResult = CreateRejectResult($"Session has been closed");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get session");
                execResult = CreateErrorResult("Authorization failed");
            }

            return null;
        }

        private async Task SendServerStreamResponse<TResponse>(IServerStreamWriter<TResponse> responseStream, ServerSession.Handler session, TResponse response)
            where TResponse : IMessage
        {
            _messageFormatter.LogClientResponse(session?.Logger, response);
            await responseStream.WriteAsync(response);
        }

        private TRequest GetClientStreamRequest<TRequest>(IAsyncStreamReader<TRequest> requestStream, ServerSession.Handler session)
            where TRequest : IMessage
        {
            _messageFormatter.LogClientResponse(session?.Logger, requestStream.Current);
            return requestStream.Current;
        }

        #region Request handlers

        private Task<HeartbeatResponse> HeartbeatInternal(HeartbeatRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HeartbeatResponse());
        }

        private Task<AlgoServerApi.LoginResponse> LoginInternal(AlgoServerApi.LoginRequest request, ServerCallContext context)
        {
            var res = new AlgoServerApi.LoginResponse
            {
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion,
                ExecResult = CreateSuccessResult(),
                Error = AlgoServerApi.LoginResponse.Types.LoginError.None,
                AccessToken = "",
            };

            try
            {
                if (!VersionSpec.CheckClientCompatibility(request.MajorVersion, request.MinorVersion, out var error))
                {
                    res.ExecResult = CreateRejectResult(error);
                    res.Error = AlgoServerApi.LoginResponse.Types.LoginError.VersionMismatch;
                }
                else
                {
                    var accessLevel = _algoServer.ValidateCreds(request.Login, request.Password);
                    if (accessLevel == ClientClaims.Types.AccessLevel.Anonymous)
                    {
                        res.ExecResult = CreateRejectResult();
                        res.Error = AlgoServerApi.LoginResponse.Types.LoginError.InvalidCredentials;
                    }
                    else
                    {
                        var session = new ServerSession.Handler(Guid.NewGuid().ToString(), request.Login, request.MinorVersion, _logger.Factory, _messageFormatter, accessLevel);
                        try
                        {
                            var payload = new JwtPayload
                            {
                                Username = session.Username,
                                SessionId = session.SessionId,
                                MinorVersion = session.VersionSpec.CurrentVersion,
                                AccessLevel = session.AccessManager.Level,
                            };
                            res.SessionId = session.SessionId;
                            res.AccessToken = _jwtProvider.CreateToken(payload);
                            res.AccessLevel = session.AccessManager.Level.ToApi();
                        }
                        catch (Exception ex)
                        {
                            res.ExecResult = CreateErrorResult("Failed to create access token");
                            _logger.Error(ex, $"Failed to create access token: {ex.Message}");
                        }

                        if (!string.IsNullOrWhiteSpace(res.AccessToken))
                        {
                            lock (_sessions)
                            {
                                _sessions.Add(session.SessionId, session);
                            }
                            session.Logger.Info($"Server version - {VersionSpec.LatestVersion}; Client version - {request.MajorVersion}.{request.MinorVersion}");
                            session.Logger.Info($"Current version set to {session.VersionSpec.CurrentVersionStr}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process login request");
                _logger.Error(ex, $"Failed to process login {_messageFormatter.ToJson(request)}");
            }

            return Task.FromResult(res);
        }

        private Task<AlgoServerApi.LogoutResponse> LogoutInternal(AlgoServerApi.LogoutRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.LogoutResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                lock (_sessions)
                {
                    session.CloseUpdateStream();
                    _sessions.Remove(session.SessionId);
                }
                res.Reason = AlgoServerApi.LogoutResponse.Types.LogoutReason.ClientRequest;
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process logout request");
                _logger.Error(ex, $"Failed to process logout {_messageFormatter.ToJson(request)}");
            }

            return Task.FromResult(res);
        }

        private async Task<AlgoServerApi.AccountMetadataResponse> GetAccountMetadataInternal(AlgoServerApi.AccountMetadataRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.AccountMetadataResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetAccountMetadata())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.AccountMetadata = await _algoServer.GetAccountMetadata(request.ToServer()).ContinueWith(u => u.Result.ToApi());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.PluginStatusSubscribeResponse> SubscribeToPluginStatusInternal(AlgoServerApi.PluginStatusSubscribeRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginStatusSubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginStatus())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
            {
                if (_pluginStatusTimer == null)
                {
                    _subscribedPluginsToStatus = new HashSet<string>();
                    _pluginStatusTimer = new Timer(OnPluginStatusUpdate, null, PluginStatusUpdateTimeout, -1);
                }

                if (!_subscribedPluginsToStatus.Contains(request.PluginId))
                    _subscribedPluginsToStatus.Add(request.PluginId);
            }

            return res;
        }

        private async Task<AlgoServerApi.PluginLogsSubscribeResponse> SubscribeToPluginLogsInternal(AlgoServerApi.PluginLogsSubscribeRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginLogsSubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginLogs())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
            {
                if (_pluginLogsTimer == null)
                {
                    _subscribedPluginsToLogs = new HashSet<string>();
                    _pluginLogsTimer = new Timer(OnPluginLogsUpdate, null, PluginLogsUpdateTimeout, -1);
                }

                if (!_subscribedPluginsToLogs.Contains(request.PluginId))
                    _subscribedPluginsToLogs.Add(request.PluginId);
            }

            return res;
        }

        private async Task<AlgoServerApi.PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatusInternal(AlgoServerApi.PluginStatusUnsubscribeRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginStatusUnsubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginStatus())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
            {
                if (_subscribedPluginsToStatus.Contains(request.PluginId))
                    _subscribedPluginsToStatus.Remove(request.PluginId);

                if (_subscribedPluginsToStatus.Count == 0)
                {
                    _pluginStatusTimer?.Dispose();
                    _pluginStatusTimer = null;
                }
            }

            return res;
        }

        private async Task<AlgoServerApi.PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogsInternal(AlgoServerApi.PluginLogsUnsubscribeRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginLogsUnsubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginLogs())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
            {
                if (_subscribedPluginsToLogs.Contains(request.PluginId))
                    _subscribedPluginsToLogs.Remove(request.PluginId);

                if (_subscribedPluginsToLogs.Count == 0)
                {
                    _pluginLogsTimer?.Dispose();
                    _pluginLogsTimer = null;
                }
            }

            return res;
        }

        private Task SubscribeToUpdatesInternal(AlgoServerApi.SubscribeToUpdatesRequest request, IServerStreamWriter<AlgoServerApi.UpdateInfo> responseStream, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            if (session == null)
                return Task.FromResult(this);

            if (!session.AccessManager.CanSubscribeToUpdates())
                return Task.FromResult(this);

            var task = session.SetupUpdateStream(responseStream);

            Task.Run(() => OnAlgoServerMetadataUpdate());

            return task;
        }

        private async Task<AlgoServerApi.AddPluginResponse> AddBotInternal(AlgoServerApi.AddPluginRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.AddPluginResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanAddPlugin())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.AddBot(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.RemovePluginResponse> RemoveBotInternal(AlgoServerApi.RemovePluginRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.RemovePluginResponse { ExecResult = execResult };

            if (session == null)
                return res;

            if (!session.AccessManager.CanRemovePlugin())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.RemoveBot(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;

        }

        private async Task<AlgoServerApi.StartPluginResponse> StartBotInternal(AlgoServerApi.StartPluginRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.StartPluginResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanStartPlugin())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.StartBot(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to start bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.StopPluginResponse> StopBotInternal(AlgoServerApi.StopPluginRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.StopPluginResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanStopPlugin())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.StopBot(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to stop bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.ChangePluginConfigResponse> ChangeBotConfigInternal(AlgoServerApi.ChangePluginConfigRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.ChangePluginConfigResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanChangePluginConfig())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.ChangeBotConfig(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to change bot config");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;

        }

        private async Task<AlgoServerApi.AddAccountResponse> AddAccountInternal(AlgoServerApi.AddAccountRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.AddAccountResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanAddAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.AddAccount(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.RemoveAccountResponse> RemoveAccountInternal(AlgoServerApi.RemoveAccountRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.RemoveAccountResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanRemoveAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.RemoveAccount(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.ChangeAccountResponse> ChangeAccountInternal(AlgoServerApi.ChangeAccountRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.ChangeAccountResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanChangeAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.ChangeAccount(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to change account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.TestAccountResponse> TestAccountInternal(AlgoServerApi.TestAccountRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.TestAccountResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanTestAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.ErrorInfo = (await _algoServer.TestAccount(request.ToServer())).ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.TestAccountCredsResponse> TestAccountCredsInternal(AlgoServerApi.TestAccountCredsRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.TestAccountCredsResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanTestAccountCreds())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.ErrorInfo = (await _algoServer.TestAccountCreds(request.ToServer())).ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account creds");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.UploadPackageResponse> UploadPackageInternal(IAsyncStreamReader<AlgoServerApi.FileTransferMsg> requestStream, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.UploadPackageResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanUploadPackage())
            {
                res.ExecResult = CreateNotAllowedResult(session, typeof(AlgoServerApi.UploadPackageRequest).Name);
                return res;
            }

            if (!await requestStream.MoveNext())
            {
                res.ExecResult = CreateErrorResult("Empty upload stream");
                return res;
            }

            var transferMsg = GetClientStreamRequest(requestStream, session);
            if (transferMsg == null || !transferMsg.Header.Is(AlgoServerApi.UploadPackageRequest.Descriptor))
            {
                res.ExecResult = CreateRejectResult($"Expected {nameof(AlgoServerApi.UploadPackageRequest)} header, but received '{transferMsg.Header.TypeUrl}'");
                return res;
            }
            var request = transferMsg.Header.Unpack<AlgoServerApi.UploadPackageRequest>();
            _messageFormatter.LogClientResponse(session?.Logger, request);

            var tmpPath = Path.GetTempFileName();
            try
            {
                var chunkOffset = request.TransferSettings.ChunkOffset;
                var chunkSize = request.TransferSettings.ChunkSize;

                var buffer = new byte[chunkSize];
                using (var stream = File.Open(tmpPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    for (var chunkId = 0; chunkId < chunkOffset; chunkId++)
                    {
                        stream.Write(buffer, 0, chunkSize);
                    }
                    while (await requestStream.MoveNext())
                    {
                        transferMsg = GetClientStreamRequest(requestStream, session);

                        var data = transferMsg.Data;
                        if (!data.Binary.IsEmpty)
                        {
                            data.Binary.CopyTo(buffer, 0);
                            stream.Write(buffer, 0, data.Binary.Length);
                        }
                        if (data.IsFinal)
                            break;
                    }
                }

                await _algoServer.UploadPackage(request.ToServer(), tmpPath);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to upload package");
                res.ExecResult = CreateErrorResult(ex);
            }

            if (File.Exists(tmpPath))
                File.Delete(tmpPath);

            return res;
        }

        private async Task<AlgoServerApi.RemovePackageResponse> RemovePackageInternal(AlgoServerApi.RemovePackageRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.RemovePackageResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanRemovePackage())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.RemovePackage(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task DownloadPackageInternal(AlgoServerApi.DownloadPackageRequest request, IServerStreamWriter<AlgoServerApi.FileTransferMsg> responseStream, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var response = new AlgoServerApi.DownloadPackageResponse { ExecResult = execResult };
            var transferMsg = new AlgoServerApi.FileTransferMsg { Data = AlgoServerApi.FileChunk.FinalChunk };

            if (session == null)
            {
                transferMsg.Header = Any.Pack(response);
                await SendServerStreamResponse(responseStream, session, transferMsg);
                return;
            }
            if (!session.AccessManager.CanDownloadPackage())
            {
                response.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                transferMsg.Header = Any.Pack(response);
                await SendServerStreamResponse(responseStream, session, transferMsg);
                return;
            }

            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            transferMsg.Data = new AlgoServerApi.FileChunk(chunkOffset);
            try
            {
                var buffer = new byte[chunkSize];
                var packageBinary = await _algoServer.GetPackageBinary(request.ToServer());
                using (var stream = new MemoryStream(packageBinary))
                {
                    stream.Seek((long)chunkSize * chunkOffset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        transferMsg.Data.Binary = ByteString.CopyFrom(buffer, 0, cnt);
                        await SendServerStreamResponse(responseStream, session, transferMsg);
                        transferMsg.Data.Id++;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to download Algo package");
                response.ExecResult = CreateErrorResult(ex);
            }
            transferMsg.Data = AlgoServerApi.FileChunk.FinalChunk;
            transferMsg.Header = Any.Pack(response);
            await SendServerStreamResponse(responseStream, session, transferMsg);
        }

        private async Task<AlgoServerApi.PluginFolderInfoResponse> GetBotFolderInfoInternal(AlgoServerApi.PluginFolderInfoRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginFolderInfoResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetBotFolderInfo(request.ToServer().FolderId))
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.FolderInfo = (await _algoServer.GetBotFolderInfo(request.ToServer())).ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot folder info");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.ClearPluginFolderResponse> ClearBotFolderInternal(AlgoServerApi.ClearPluginFolderRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.ClearPluginFolderResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanClearBotFolder())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.ClearBotFolder(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to clear bot folder");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.DeletePluginFileResponse> DeleteBotFileInternal(AlgoServerApi.DeletePluginFileRequest request, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.DeletePluginFileResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanDeleteBotFile())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.DeleteBotFile(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to delete bot file");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task DownloadBotFileInternal(AlgoServerApi.DownloadPluginFileRequest request, IServerStreamWriter<AlgoServerApi.FileTransferMsg> responseStream, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var response = new AlgoServerApi.DownloadPluginFileResponse { ExecResult = execResult };
            var transferMsg = new AlgoServerApi.FileTransferMsg { Data = AlgoServerApi.FileChunk.FinalChunk };

            if (session == null)
            {
                transferMsg.Header = Any.Pack(response);
                await SendServerStreamResponse(responseStream, session, transferMsg);
                return;
            }
            if (!session.AccessManager.CanDownloadBotFile(request.FolderId.ToServer()))
            {
                response.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                transferMsg.Header = Any.Pack(response);
                await SendServerStreamResponse(responseStream, session, transferMsg);
                return;
            }

            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            transferMsg.Data = new AlgoServerApi.FileChunk(chunkOffset);
            try
            {
                var buffer = new byte[chunkSize];
                var packagePath = await _algoServer.GetBotFileReadPath(request.ToServer());
                using (var stream = File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek((long)chunkSize * chunkOffset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        transferMsg.Data.Binary = ByteString.CopyFrom(buffer, 0, cnt);
                        await SendServerStreamResponse(responseStream, session, transferMsg);
                        transferMsg.Data.Id++;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to download bot file");
                response.ExecResult = CreateErrorResult(ex);
            }
            transferMsg.Data = AlgoServerApi.FileChunk.FinalChunk;
            transferMsg.Header = Any.Pack(response);
            await SendServerStreamResponse(responseStream, session, transferMsg);
        }

        private async Task<AlgoServerApi.UploadPluginFileResponse> UploadBotFileInternal(IAsyncStreamReader<AlgoServerApi.FileTransferMsg> requestStream, ServerCallContext context, ServerSession.Handler session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.UploadPluginFileResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanUploadBotFile())
            {
                res.ExecResult = CreateNotAllowedResult(session, typeof(AlgoServerApi.UploadPluginFileRequest).Name);
                return res;
            }

            if (!await requestStream.MoveNext())
            {
                res.ExecResult = CreateErrorResult("Empty upload stream");
                return res;
            }

            var transferMsg = GetClientStreamRequest(requestStream, session);
            if (transferMsg == null || !transferMsg.Header.Is(AlgoServerApi.UploadPluginFileRequest.Descriptor))
            {
                res.ExecResult = CreateRejectResult($"Expected {nameof(AlgoServerApi.UploadPluginFileRequest)} header, but received '{transferMsg.Header.TypeUrl}'");
                return res;
            }
            var request = transferMsg.Header.Unpack<AlgoServerApi.UploadPluginFileRequest>();
            _messageFormatter.LogClientResponse(session?.Logger, request);

            try
            {
                var chunkOffset = request.TransferSettings.ChunkOffset;
                var chunkSize = request.TransferSettings.ChunkSize;

                var buffer = new byte[chunkSize];
                var filePath = await _algoServer.GetBotFileWritePath(request.ToServer());
                string oldFilePath = null;
                if (File.Exists(filePath))
                {
                    oldFilePath = $"{filePath}.old";
                    File.Move(filePath, oldFilePath);
                }
                using (var stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    if (oldFilePath != null)
                    {
                        using (var oldStream = File.Open(oldFilePath, FileMode.Open, FileAccess.Read))
                        {
                            for (var chunkId = 0; chunkId < chunkOffset; chunkId++)
                            {
                                var bytesRead = oldStream.Read(buffer, 0, chunkSize);
                                for (var i = bytesRead; i < chunkSize; i++) buffer[i] = 0;
                                stream.Write(buffer, 0, chunkSize);
                            }
                        }
                        File.Delete(oldFilePath);
                    }
                    else
                    {
                        for (var chunkId = 0; chunkId < chunkOffset; chunkId++)
                        {
                            stream.Write(buffer, 0, chunkSize);
                        }
                    }
                    while (await requestStream.MoveNext())
                    {
                        transferMsg = GetClientStreamRequest(requestStream, session);

                        var data = transferMsg.Data;
                        if (!data.Binary.IsEmpty)
                        {
                            data.Binary.CopyTo(buffer, 0);
                            stream.Write(buffer, 0, data.Binary.Length);
                        }
                        if (data.IsFinal)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to upload bot file");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        #endregion Request handlers


        #region Updates

        private async Task OnAlgoServerMetadataUpdate()
        {
            var update = new AlgoServerApi.AlgoServerMetadataUpdate
            {
                ExecResult = CreateSuccessResult(),
            };

            try
            {
                update.ApiMetadata = await _algoServer.GetApiMetadata().ContinueWith(u => u.Result.ToApi());
                update.MappingsCollection = await _algoServer.GetMappingsInfo(new MappingsInfoRequest()).ContinueWith(u => u.Result.ToApi());
                update.SetupContext = await _algoServer.GetSetupContext().ContinueWith(u => u.Result.ToApi());

                var packages = await _algoServer.GetPackageList();
                update.Packages.AddRange(packages.Select(u => u.ToApi()));

                var accounts = await _algoServer.GetAccountList();
                update.Accounts.AddRange(accounts.Select(u => u.ToApi()));

                var plugins = await _algoServer.GetBotList();
                update.Plugins.AddRange(plugins.Select(u => u.ToApi()));
            }
            catch (Exception ex)
            {
                update.ExecResult = CreateErrorResult(ex);
                _logger.Error(ex);
            }
            finally
            {
                SendUpdate(new UpdateInfo<AlgoServerApi.AlgoServerMetadataUpdate>(UpdateInfo.Types.UpdateType.Added, update));
            }
        }

        private void OnPackageUpdate(UpdateInfo<PackageInfo> packageUpdate)
        {
            var apiUpdate = new AlgoServerApi.PackageUpdate
            {
                Id = packageUpdate.Value.PackageId,
                Action = packageUpdate.Type.ToApi(),
                Package = packageUpdate.Value.ToApi(),
            };

            SendUpdate(new UpdateInfo<AlgoServerApi.PackageUpdate>(packageUpdate.Type, apiUpdate));
        }

        private void OnAccountUpdate(UpdateInfo<AccountModelInfo> accountUpdate)
        {
            var apiUpdate = new AlgoServerApi.AccountModelUpdate
            {
                Id = accountUpdate.Value.AccountId,
                Action = accountUpdate.Type.ToApi(),
                Account = accountUpdate.Value.ToApi(),
            };

            SendUpdate(new UpdateInfo<AlgoServerApi.AccountModelUpdate>(accountUpdate.Type, apiUpdate));
        }

        private void OnBotUpdate(UpdateInfo<PluginModelInfo> update)
        {
            var apiUpdate = new AlgoServerApi.PluginModelUpdate
            {
                Id = update.Value.InstanceId,
                Action = update.Type.ToApi(),
                Plugin = update.Value.ToApi(),
            };

            SendUpdate(new UpdateInfo<AlgoServerApi.PluginModelUpdate>(update.Type, apiUpdate));
        }

        private void OnPackageStateUpdate(PackageStateUpdate stateUpdate)
        {
            SendUpdate(new UpdateInfo<AlgoServerApi.PackageStateUpdate>(UpdateInfo.Types.UpdateType.Replaced, stateUpdate.ToApi()));
        }

        private void OnAccountStateUpdate(AccountStateUpdate stateUpdate)
        {
            SendUpdate(new UpdateInfo<AlgoServerApi.AccountStateUpdate>(UpdateInfo.Types.UpdateType.Replaced, stateUpdate.ToApi()));
        }

        private void OnBotStateUpdate(PluginStateUpdate stateUpdate)
        {
            SendUpdate(new UpdateInfo<AlgoServerApi.PluginStateUpdate>(UpdateInfo.Types.UpdateType.Replaced, stateUpdate.ToApi()));
        }

        private async void OnAlertsUpdate(object obj)
        {
            _alertTimer?.Change(-1, -1);

            try
            {
                var update = new AlgoServerApi.AlertListUpdate();

                var alerts = await _algoServer.GetAlertsAsync(new PluginAlertsRequest
                {
                    MaxCount = 1000,
                    LastLogTimeUtc = _lastAlertTimeUtc,
                });

                update.Alerts.AddRange(alerts.Select(u => u.ToApi()));

                _lastAlertTimeUtc = alerts.Max(u => u.TimeUtc) ?? _lastAlertTimeUtc;

                SendUpdate(new UpdateInfo<AlgoServerApi.AlertListUpdate>(UpdateInfo.Types.UpdateType.Replaced, update));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            _alertTimer?.Change(AlertsUpdateTimeout, -1);
        }

        private async void OnPluginStatusUpdate(object obj)
        {
            _pluginStatusTimer?.Change(-1, -1);

            foreach (var pluginKey in _subscribedPluginsToStatus.ToList())
                try
                {
                    var update = new AlgoServerApi.PluginStatusUpdate
                    {
                        PluginId = pluginKey,
                    };

                    update.Message = await _algoServer.GetBotStatusAsync(new PluginStatusRequest { PluginId = pluginKey });

                    SendUpdate(new UpdateInfo<AlgoServerApi.PluginStatusUpdate>(UpdateInfo.Types.UpdateType.Replaced, update));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

            _pluginStatusTimer?.Change(PluginStatusUpdateTimeout, -1);
        }

        private async void OnPluginLogsUpdate(object obj)
        {
            _pluginLogsTimer?.Change(-1, -1);

            foreach (var pluginKey in _subscribedPluginsToLogs.ToList())
                try
                {
                    var update = new AlgoServerApi.PluginLogUpdate
                    {
                        PluginId = pluginKey,
                    };

                    var serverRequest = new PluginLogsRequest
                    {
                        PluginId = pluginKey,
                        MaxCount = 1000,
                        LastLogTimeUtc = _lastLogTimeUtc,
                    };

                    var records = await _algoServer.GetBotLogsAsync(serverRequest);

                    _lastLogTimeUtc = records.Max(u => u.TimeUtc) ?? _lastLogTimeUtc;

                    update.Records.AddRange(records.Select(u => u.ToApi()));

                    SendUpdate(new UpdateInfo<AlgoServerApi.PluginLogUpdate>(UpdateInfo.Types.UpdateType.Replaced, update));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

            _pluginLogsTimer?.Change(PluginLogsUpdateTimeout, -1);
        }

        private void SendUpdate(IUpdateInfo update)
        {
            lock (_sessions)
            {
                try
                {
                    var sessionsToRemove = new List<string>();
                    foreach (var session in _sessions.Values)
                    {
                        session.SendUpdate(update);
                        if (session.IsFaulted)
                            sessionsToRemove.Add(session.SessionId);
                    }

                    foreach (var sessionId in sessionsToRemove)
                    {
                        _sessions[sessionId].CancelUpdateStream();
                        _sessions.Remove(sessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to multicast update: {update}");
                }
            }
        }

        public void Dispose()
        {
            _alertTimer?.Dispose();
            _heartbeatTimer?.Dispose();

            DisconnectAllClients();
        }

        #endregion
    }
}
