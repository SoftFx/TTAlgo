using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using NLog;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl.Grpc
{
    public class GrpcServer : ProtocolServer
    {
        private IJwtProvider _jwtProvider;
        private BotAgentServerImpl _impl;
        private Server _server;


        public GrpcServer(IAlgoServerProvider agentServer, ServerSettings settings, IJwtProvider jwtProvider) : base(agentServer, settings)
        {
            _jwtProvider = jwtProvider;
        }


        protected override Task StartServer()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            _impl = new BotAgentServerImpl(AlgoSrv, _jwtProvider, Logger, Settings.LogMessages, VersionSpec);
            var creds = new SslServerCredentials(new[] { new KeyCertificatePair(CertificateProvider.ServerCertificate, CertificateProvider.ServerKey), }); //,CertificateProvider.RootCertificate, true);
            _server = new Server
            {
                Services = { AlgoServerPublic.BindService(_impl) },
                Ports = { new ServerPort("0.0.0.0", Settings.ServerPort, creds) },
            };
            _server.Start();

            return Task.FromResult(this);
        }

        protected override async Task StopServer()
        {
            _impl.DisconnectAllClients();
            await _server.ShutdownAsync();
        }
    }


    internal class BotAgentServerImpl : AlgoServerPublic.AlgoServerPublicBase
    {
        private IAlgoServerProvider _algoServer;
        private IJwtProvider _jwtProvider;
        private ILogger _logger;
        private MessageFormatter _messageFormatter;
        private Dictionary<string, ServerSession.Handler> _sessions;
        private VersionSpec _version;

        public BotAgentServerImpl(IAlgoServerProvider algoServer, IJwtProvider jwtProvider, ILogger logger, bool logMessages, VersionSpec version)
        {
            _version = version;
            _algoServer = algoServer;
            _jwtProvider = jwtProvider;
            _logger = logger;

            _messageFormatter = new MessageFormatter { LogMessages = logMessages };
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
        }


        public static RequestResult CreateSuccessResult(string message = "")
        {
            return new RequestResult { Status = RequestResult.Types.RequestStatus.Success, Message = message ?? "" };
        }

        public static RequestResult CreateErrorResult(string message = "")
        {
            return new RequestResult { Status = RequestResult.Types.RequestStatus.InternalServerError, Message = message ?? "" };
        }

        public static RequestResult CreateErrorResult(Exception ex)
        {
            return CreateErrorResult(ex.Flatten().Message);
        }

        public static RequestResult CreateUnauthorizedResult(string message = "")
        {
            return new RequestResult { Status = RequestResult.Types.RequestStatus.Unauthorized, Message = message ?? "" };
        }

        public static RequestResult CreateUnauthorizedResult(Exception ex)
        {
            return CreateUnauthorizedResult(ex.Flatten().Message);
        }

        public static RequestResult CreateRejectResult(string message = "")
        {
            return new RequestResult { Status = RequestResult.Types.RequestStatus.Reject, Message = message ?? "" };
        }

        public static RequestResult CreateRejectResult(Exception ex)
        {
            return CreateRejectResult(ex.Flatten().Message);
        }

        public static RequestResult CreateNotAllowedResult(string message = "")
        {
            return new RequestResult { Status = RequestResult.Types.RequestStatus.NotAllowed, Message = message ?? "" };
        }

        public static RequestResult CreateNotAllowedResult(Exception ex)
        {
            return CreateNotAllowedResult(ex.Flatten().Message);
        }

        public static RequestResult CreateNotAllowedResult(ServerSession.Handler session, string requestName)
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

        public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequest(LoginInternal, request, context);
        }

        public override Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(LogoutInternal, request, context);
        }

        public override Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(HeartbeatInternal, request, context);
        }

        public override Task<SnapshotResponse> GetSnapshot(SnapshotRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetSnapshotInternal, request, context);
        }

        public override Task SubscribeToUpdates(SubscribeToUpdatesRequest request, IServerStreamWriter<UpdateInfo> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(SubscribeToUpdatesInternal, request, responseStream, context);
        }

        public override Task<ApiMetadataResponse> GetApiMetadata(ApiMetadataRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetApiMetadataInternal, request, context);
        }

        public override Task<MappingsInfoResponse> GetMappingsInfo(MappingsInfoRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetMappingsInfoInternal, request, context);
        }

        public override Task<SetupContextResponse> GetSetupContext(SetupContextRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetSetupContextInternal, request, context);
        }

        public override Task<AccountMetadataResponse> GetAccountMetadata(AccountMetadataRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, request, context);
        }

        public override Task<PluginListResponse> GetPluginList(PluginListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotListInternal, request, context);
        }

        public override Task<AddPluginResponse> AddPlugin(AddPluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(AddBotInternal, request, context);

        }

        public override Task<RemovePluginResponse> RemovePlugin(RemovePluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemoveBotInternal, request, context);
        }

        public override Task<StartPluginResponse> StartPlugin(StartPluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StartBotInternal, request, context);
        }

        public override Task<StopPluginResponse> StopPlugin(StopPluginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StopBotInternal, request, context);
        }

        public override Task<ChangePluginConfigResponse> ChangePluginConfig(ChangePluginConfigRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ChangeBotConfigInternal, request, context);
        }

        public override Task<AccountListResponse> GetAccountList(AccountListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAccountListInternal, request, context);
        }

        public override Task<AddAccountResponse> AddAccount(AddAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(AddAccountInternal, request, context);
        }

        public override Task<RemoveAccountResponse> RemoveAccount(RemoveAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemoveAccountInternal, request, context);
        }

        public override Task<ChangeAccountResponse> ChangeAccount(ChangeAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ChangeAccountInternal, request, context);
        }

        public override Task<TestAccountResponse> TestAccount(TestAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(TestAccountInternal, request, context);
        }

        public override Task<TestAccountCredsResponse> TestAccountCreds(TestAccountCredsRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(TestAccountCredsInternal, request, context);
        }

        public override Task<PackageListResponse> GetPackageList(PackageListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetPackageListInternal, request, context);
        }

        public override Task<UploadPackageResponse> UploadPackage(IAsyncStreamReader<UploadPackageRequest> requestStream, ServerCallContext context)
        {
            return ExecuteClientStreamingRequestAuthorized(UploadPackageInternal, requestStream, context);
        }

        public override Task<RemovePackageResponse> RemovePackage(RemovePackageRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemovePackageInternal, request, context);
        }

        public override Task DownloadPackage(DownloadPackageRequest request, IServerStreamWriter<DownloadPackageResponse> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadPackageInternal, request, responseStream, context);
        }

        public override Task<PluginStatusResponse> GetPluginStatus(PluginStatusRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotStatusInternal, request, context);
        }

        public override Task<PluginLogsResponse> GetPluginLogs(PluginLogsRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotLogsInternal, request, context);
        }

        public override Task<PluginAlertsResponse> GetAlerts(PluginAlertsRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAlertsInternal, request, context);
        }

        public override Task<PluginFolderInfoResponse> GetPluginFolderInfo(PluginFolderInfoRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotFolderInfoInternal, request, context);
        }

        public override Task<ClearPluginFolderResponse> ClearPluginFolder(ClearPluginFolderRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ClearBotFolderInternal, request, context);
        }

        public override Task<DeletePluginFileResponse> DeletePluginFile(DeletePluginFileRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(DeleteBotFileInternal, request, context);
        }

        public override Task DownloadPluginFile(DownloadPluginFileRequest request, IServerStreamWriter<DownloadPluginFileResponse> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal, request, responseStream, context);
        }

        public override Task<UploadPluginFileResponse> UploadPluginFile(IAsyncStreamReader<UploadPluginFileRequest> requestStream, ServerCallContext context)
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
            Func<TRequest, ServerCallContext, ServerSession.Handler, RequestResult, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
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
            Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, ServerSession.Handler, RequestResult, Task> requestAction,
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
            Func<IAsyncStreamReader<TRequest>, ServerCallContext, ServerSession.Handler, RequestResult, Task<TResponse>> requestAction,
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

        private ServerSession.Handler GetSession(ServerCallContext context, Type requestType, out RequestResult execResult)
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

        private Task<LoginResponse> LoginInternal(LoginRequest request, ServerCallContext context)
        {
            var res = new LoginResponse
            {
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion,
                ExecResult = CreateSuccessResult(),
                Error = LoginResponse.Types.LoginError.None,
                AccessToken = "",
            };

            try
            {
                if (!VersionSpec.CheckClientCompatibility(request.MajorVersion, request.MinorVersion, out var error))
                {
                    res.ExecResult = CreateRejectResult(error);
                    res.Error = LoginResponse.Types.LoginError.VersionMismatch;
                }
                else
                {
                    var accessLevel = _algoServer.ValidateCreds(request.Login, request.Password);
                    if (accessLevel == ClientClaims.Types.AccessLevel.Anonymous)
                    {
                        res.ExecResult = CreateRejectResult();
                        res.Error = LoginResponse.Types.LoginError.InvalidCredentials;
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
                            res.AccessLevel = session.AccessManager.Level;
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

        private Task<LogoutResponse> LogoutInternal(LogoutRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new LogoutResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                lock (_sessions)
                {
                    session.CloseUpdateStream();
                    _sessions.Remove(session.SessionId);
                }
                res.Reason = LogoutResponse.Types.LogoutReason.ClientRequest;
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process logout request");
                _logger.Error(ex, $"Failed to process logout {_messageFormatter.ToJson(request)}");
            }

            return Task.FromResult(res);
        }

        private Task<HeartbeatResponse> HeartbeatInternal(HeartbeatRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new HeartbeatResponse { ExecResult = execResult };
            //if (session == null)
            //    return Task.FromResult(res);

            return Task.FromResult(res);
        }

        private async Task<SnapshotResponse> GetSnapshotInternal(SnapshotRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new SnapshotResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetSnapshot())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.ApiMetadata = await GetApiMetadataInternal(new ApiMetadataRequest(), context, session, execResult);
                res.MappingsInfo = await GetMappingsInfoInternal(new MappingsInfoRequest(), context, session, execResult);
                res.SetupContext = await GetSetupContextInternal(new SetupContextRequest(), context, session, execResult);
                res.PackageList = await GetPackageListInternal(new PackageListRequest(), context, session, execResult);
                res.AccountList = await GetAccountListInternal(new AccountListRequest(), context, session, execResult);
                res.PluginList = await GetBotListInternal(new PluginListRequest(), context, session, execResult);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get snapshot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private Task SubscribeToUpdatesInternal(SubscribeToUpdatesRequest request, IServerStreamWriter<UpdateInfo> responseStream, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            if (session == null)
                return Task.FromResult(this);
            if (!session.AccessManager.CanSubscribeToUpdates())
                return Task.FromResult(this);

            return session.SetupUpdateStream(responseStream);
        }

        private async Task<ApiMetadataResponse> GetApiMetadataInternal(ApiMetadataRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new ApiMetadataResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetApiMetadata())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.ApiMetadata = await _algoServer.GetApiMetadata();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get api metadata");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<MappingsInfoResponse> GetMappingsInfoInternal(MappingsInfoRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new MappingsInfoResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetMappingsInfo())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.Mappings = await _algoServer.GetMappingsInfo();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get mappings collection");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<SetupContextResponse> GetSetupContextInternal(SetupContextRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new SetupContextResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetSetupContext())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.SetupContext = await _algoServer.GetSetupContext();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get setup context");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AccountMetadataResponse> GetAccountMetadataInternal(AccountMetadataRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new AccountMetadataResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetAccountMetadata())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.AccountMetadata = await _algoServer.GetAccountMetadata(request.Account);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get account metadata");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<PluginListResponse> GetBotListInternal(PluginListRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new PluginListResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetBotList())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                var bots = await _algoServer.GetBotList();
                res.Plugins.AddRange(bots);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private Task<AddPluginResponse> AddBotInternal(AddPluginRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new AddPluginResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanAddBot())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.AddBot(request.Account, request.Config);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        private Task<RemovePluginResponse> RemoveBotInternal(RemovePluginRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new RemovePluginResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanRemoveBot())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.RemoveBot(request.PluginId, request.CleanLog, request.CleanAlgoData);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        private Task<StartPluginResponse> StartBotInternal(StartPluginRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new StartPluginResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanStartBot())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.StartBot(request.PluginId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to start bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<StopPluginResponse> StopBotInternal(StopPluginRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new StopPluginResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanStopBot())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.StopBot(request.PluginId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to stop bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<ChangePluginConfigResponse> ChangeBotConfigInternal(ChangePluginConfigRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new ChangePluginConfigResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanChangeBotConfig())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.ChangeBotConfig(request.PluginId, request.NewConfig);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        private async Task<AccountListResponse> GetAccountListInternal(AccountListRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new AccountListResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetAccountList())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                var accounts = await _algoServer.GetAccountList();
                res.Accounts.AddRange(accounts);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get account list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private Task<AddAccountResponse> AddAccountInternal(AddAccountRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new AddAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanAddAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.AddAccount(request.Account, request.Password);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<RemoveAccountResponse> RemoveAccountInternal(RemoveAccountRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new RemoveAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanRemoveAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.RemoveAccount(request.Account);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<ChangeAccountResponse> ChangeAccountInternal(ChangeAccountRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new ChangeAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanChangeAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.ChangeAccount(request.Account, request.Password);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to change account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private async Task<TestAccountResponse> TestAccountInternal(TestAccountRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new TestAccountResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanTestAccount())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.ErrorInfo = await _algoServer.TestAccount(request.Account);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<TestAccountCredsResponse> TestAccountCredsInternal(TestAccountCredsRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new TestAccountCredsResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanTestAccountCreds())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.ErrorInfo = await _algoServer.TestAccountCreds(request.Account, request.Password);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account creds");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<PackageListResponse> GetPackageListInternal(PackageListRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new PackageListResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetPackageList())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                var packages = await _algoServer.GetPackageList();
                res.Packages.AddRange(packages);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get packages list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<UploadPackageResponse> UploadPackageInternal(IAsyncStreamReader<UploadPackageRequest> requestStream, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new UploadPackageResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanUploadPackage())
            {
                res.ExecResult = CreateNotAllowedResult(session, typeof(UploadPackageRequest).Name);
                return res;
            }

            await requestStream.MoveNext();
            var request = GetClientStreamRequest(requestStream, session);
            if (request == null || request.ValueCase != UploadPackageRequest.ValueOneofCase.Package)
            {
                res.ExecResult = CreateRejectResult("First message should specify Algo package details");
                return res;
            }

            try
            {
                var chunkSize = request.Package.ChunkSettings.Size;
                var buffer = new byte[chunkSize];
                var packagePath = await _algoServer.GetPackageWritePath(request.Package.PackageId);
                string oldPackagePath = null;
                if (File.Exists(packagePath))
                {
                    oldPackagePath = $"{packagePath}.old";
                    File.Move(packagePath, oldPackagePath);
                }
                using (var stream = File.Open(packagePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    if (oldPackagePath != null)
                    {
                        using (var oldStream = File.Open(oldPackagePath, FileMode.Open, FileAccess.Read))
                        {
                            for (var chunkId = 0; chunkId < request.Package.ChunkSettings.Offset; chunkId++)
                            {
                                var bytesRead = oldStream.Read(buffer, 0, chunkSize);
                                for (var i = bytesRead; i < chunkSize; i++) buffer[i] = 0;
                                stream.Write(buffer, 0, chunkSize);
                            }
                        }
                        File.Delete(oldPackagePath);
                    }
                    else
                    {
                        for (var chunkId = 0; chunkId < request.Package.ChunkSettings.Offset; chunkId++)
                        {
                            stream.Write(buffer, 0, chunkSize);
                        }
                    }
                    while (await requestStream.MoveNext())
                    {
                        request = GetClientStreamRequest(requestStream, session);
                        if (request.ValueCase != UploadPackageRequest.ValueOneofCase.Chunk)
                            continue;
                        if (!request.Chunk.Binary.IsEmpty)
                        {
                            request.Chunk.Binary.CopyTo(buffer, 0);
                            stream.Write(buffer, 0, request.Chunk.Binary.Length);
                        }
                        if (request.Chunk.IsFinal)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to upload package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<RemovePackageResponse> RemovePackageInternal(RemovePackageRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new RemovePackageResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanRemovePackage())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                await _algoServer.RemovePackage(request.PackageId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task DownloadPackageInternal(DownloadPackageRequest request, IServerStreamWriter<DownloadPackageResponse> responseStream, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new DownloadPackageResponse { ExecResult = execResult, Chunk = new FileChunk { Id = request.Package.ChunkSettings.Offset, IsFinal = false, } };
            if (session == null)
            {
                res.Chunk.IsFinal = true;
                res.Chunk.Id = -1;
                await SendServerStreamResponse(responseStream, session, res);
                return;
            }
            if (!session.AccessManager.CanDownloadPackage())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                res.Chunk.IsFinal = true;
                res.Chunk.Id = -1;
                await SendServerStreamResponse(responseStream, session, res);
                return;
            }

            try
            {
                var chunkSize = request.Package.ChunkSettings.Size;
                var buffer = new byte[chunkSize];
                var packagePath = await _algoServer.GetPackageReadPath(request.Package.PackageId);
                using (var stream = File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)chunkSize * request.Package.ChunkSettings.Offset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        res.Chunk.Binary = ByteString.CopyFrom(buffer, 0, cnt);
                        await SendServerStreamResponse(responseStream, session, res);
                        res.Chunk.Id++;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to download Algo package");
                res.ExecResult = CreateErrorResult(ex);
            }
            res.Chunk.Binary = ByteString.Empty;
            res.Chunk.Id = -1;
            res.Chunk.IsFinal = true;
            await SendServerStreamResponse(responseStream, session, res);
        }

        private async Task<PluginStatusResponse> GetBotStatusInternal(PluginStatusRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new PluginStatusResponse { ExecResult = execResult, PluginId = request.PluginId };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetBotStatus())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.Status = await _algoServer.GetBotStatusAsync(request.PluginId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot status");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<PluginLogsResponse> GetBotLogsInternal(PluginLogsRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new PluginLogsResponse { ExecResult = execResult, PluginId = request.PluginId };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetBotLogs())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                var entries = await _algoServer.GetBotLogsAsync(request.PluginId, request.LastLogTimeUtc, request.MaxCount);
                res.Logs.AddRange(entries);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot logs");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<PluginAlertsResponse> GetAlertsInternal(PluginAlertsRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new PluginAlertsResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetAlerts())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                var entries = await _algoServer.GetAlertsAsync(request.LastLogTimeUtc, request.MaxCount);
                res.Alerts.AddRange(entries);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get alerts");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<PluginFolderInfoResponse> GetBotFolderInfoInternal(PluginFolderInfoRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new PluginFolderInfoResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetBotFolderInfo(request.FolderId))
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.FolderInfo = (await _algoServer.GetBotFolderInfo(request.PluginId, request.FolderId));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot folder info");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private Task<ClearPluginFolderResponse> ClearBotFolderInternal(ClearPluginFolderRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new ClearPluginFolderResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanClearBotFolder())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.ClearBotFolder(request.PluginId, request.FolderId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to clear bot folder");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<DeletePluginFileResponse> DeleteBotFileInternal(DeletePluginFileRequest request, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new DeletePluginFileResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);
            if (!session.AccessManager.CanDeleteBotFile())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return Task.FromResult(res);
            }

            try
            {
                _algoServer.DeleteBotFile(request.PluginId, request.FolderId, request.FileName);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to delete bot file");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private async Task DownloadBotFileInternal(DownloadPluginFileRequest request, IServerStreamWriter<DownloadPluginFileResponse> responseStream, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new DownloadPluginFileResponse { ExecResult = execResult, Chunk = new FileChunk { Id = request.File.ChunkSettings.Offset, IsFinal = false, } };
            if (session == null)
            {
                res.Chunk.IsFinal = true;
                res.Chunk.Id = -1;
                await SendServerStreamResponse(responseStream, session, res);
                return;
            }
            if (!session.AccessManager.CanDownloadBotFile(request.File.FolderId))
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                res.Chunk.IsFinal = true;
                res.Chunk.Id = -1;
                await SendServerStreamResponse(responseStream, session, res);
                return;
            }

            try
            {
                var chunkSize = request.File.ChunkSettings.Size;
                var buffer = new byte[chunkSize];
                var packagePath = await _algoServer.GetBotFileReadPath(request.File.PluginId, request.File.FolderId, request.File.FileName);
                using (var stream = File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek((long)chunkSize * request.File.ChunkSettings.Offset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        res.Chunk.Binary = ByteString.CopyFrom(buffer, 0, cnt);
                        await SendServerStreamResponse(responseStream, session, res);
                        res.Chunk.Id++;
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to download bot file");
                res.ExecResult = CreateErrorResult(ex);
            }
            res.Chunk.Binary = ByteString.Empty;
            res.Chunk.Id = -1;
            res.Chunk.IsFinal = true;
            await SendServerStreamResponse(responseStream, session, res);
        }

        private async Task<UploadPluginFileResponse> UploadBotFileInternal(IAsyncStreamReader<UploadPluginFileRequest> requestStream, ServerCallContext context, ServerSession.Handler session, RequestResult execResult)
        {
            var res = new UploadPluginFileResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanUploadBotFile())
            {
                res.ExecResult = CreateNotAllowedResult(session, typeof(UploadPluginFileRequest).Name);
                return res;
            }

            await requestStream.MoveNext();
            var request = GetClientStreamRequest(requestStream, session);
            if (request == null || request.ValueCase != UploadPluginFileRequest.ValueOneofCase.File)
            {
                res.ExecResult = CreateRejectResult("First message should specify bot file details");
                return res;
            }

            try
            {
                var chunkSize = request.File.ChunkSettings.Size;
                var buffer = new byte[chunkSize];
                var filePath = await _algoServer.GetBotFileWritePath(request.File.PluginId, request.File.FolderId, request.File.FileName);
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
                            for (var chunkId = 0; chunkId < request.File.ChunkSettings.Offset; chunkId++)
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
                        for (var chunkId = 0; chunkId < request.File.ChunkSettings.Offset; chunkId++)
                        {
                            stream.Write(buffer, 0, chunkSize);
                        }
                    }
                    while (await requestStream.MoveNext())
                    {
                        request = GetClientStreamRequest(requestStream, session);
                        if (request.ValueCase != UploadPluginFileRequest.ValueOneofCase.Chunk)
                            continue;
                        if (!request.Chunk.Binary.IsEmpty)
                        {
                            request.Chunk.Binary.CopyTo(buffer, 0);
                            stream.Write(buffer, 0, request.Chunk.Binary.Length);
                        }
                        if (request.Chunk.IsFinal)
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

        private void OnPackageUpdate(UpdateInfo<PackageInfo> packageUpdate)
        {
            SendUpdate(packageUpdate);
        }

        private void OnPackageStateUpdate(PackageStateUpdate stateUpdate)
        {
            SendUpdate(new UpdateInfo<PackageStateUpdate>(UpdateInfo.Types.UpdateType.Replaced, stateUpdate));
        }

        private void OnAccountUpdate(UpdateInfo<AccountModelInfo> accountUpdate)
        {
            SendUpdate(accountUpdate);
        }

        private void OnAccountStateUpdate(AccountStateUpdate stateUpdate)
        {
            SendUpdate(new UpdateInfo<AccountStateUpdate>(UpdateInfo.Types.UpdateType.Replaced, stateUpdate));
        }

        private void OnBotUpdate(UpdateInfo<PluginModelInfo> update)
        {
            SendUpdate(update);
        }

        private void OnBotStateUpdate(PluginStateUpdate stateUpdate)
        {
            SendUpdate(new UpdateInfo<PluginStateUpdate>(UpdateInfo.Types.UpdateType.Replaced, stateUpdate));
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

        #endregion
    }
}
