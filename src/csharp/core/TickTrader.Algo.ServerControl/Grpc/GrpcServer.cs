using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.Server.Common.Grpc;
using TickTrader.Algo.ServerControl.Model;
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
            if (_impl != null)
                await _impl?.Shutdown();
            await _server.ShutdownAsync();
        }
    }


    internal class BotAgentServerImpl : AlgoServerApi.AlgoServerPublic.AlgoServerPublicBase
    {
        private readonly IActorRef _sessionsRef;

        private IAlgoServerProvider _algoServer;
        private IJwtProvider _jwtProvider;
        private ILogger _logger;
        private MessageFormatter _messageFormatter;
        private VersionSpec _version;


        public BotAgentServerImpl(IAlgoServerProvider algoServer, IJwtProvider jwtProvider, ILogger logger, bool logMessages, VersionSpec version)
        {
            _version = version;
            _algoServer = algoServer;
            _jwtProvider = jwtProvider;
            _logger = logger;

            _messageFormatter = new MessageFormatter(AlgoServerApi.AlgoServerPublicAPIReflection.Descriptor) { LogMessages = logMessages };
            _sessionsRef = SessionControlActor.Create(algoServer, logger, _messageFormatter);
        }


        public async Task Shutdown()
        {
            await SessionControl.Shutdown(_sessionsRef)
                .OnException(ex => _logger.Error(ex, "Failed to shutdown session control"));

            await ActorSystem.StopActor(_sessionsRef)
                .OnException(ex => _logger.Error(ex, "Failed to stop SessionContolActor"));
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

        public static AlgoServerApi.RequestResult CreateNotAllowedResult(SessionInfo session, string requestName)
        {
            return CreateNotAllowedResult($"{session.AccessManager.Level} is not allowed to execute {requestName}");
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
            Func<TRequest, ServerCallContext, SessionInfo, AlgoServerApi.RequestResult, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var sessionId = GetSessionId(context, typeof(TRequest), out var execResult);
                var session = await GetSession(sessionId);
                if (session != null)
                    ValidateSession(sessionId, session, out execResult);

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

        private async Task ExecuteServerStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, SessionInfo, AlgoServerApi.RequestResult, Task> requestAction,
            TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var sessionId = GetSessionId(context, typeof(TRequest), out var execResult);
                var session = await GetSession(sessionId);
                if (session != null)
                    ValidateSession(sessionId, session, out execResult);

                _messageFormatter.LogClientRequest(session?.Logger, request);
                await requestAction(request, responseStream, context, session, execResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteClientStreamingRequestAuthorized<TRequest, TResponse>(
            Func<IAsyncStreamReader<TRequest>, ServerCallContext, SessionInfo, AlgoServerApi.RequestResult, Task<TResponse>> requestAction,
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var sessionId = GetSessionId(context, typeof(TRequest), out var execResult);
                var session = await GetSession(sessionId);
                if (session != null)
                    ValidateSession(sessionId, session, out execResult);

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

        private string GetSessionId(ServerCallContext context, System.Type requestType, out AlgoServerApi.RequestResult execResult)
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
                    return jwtPayload.SessionId;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get session");
                execResult = CreateErrorResult("Authorization failed");
            }

            return null;
        }

        private async Task<SessionInfo> GetSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return null;

            return await SessionControl.GetSession(_sessionsRef, sessionId);
        }

        private void ValidateSession(string sessionId, SessionInfo session, out AlgoServerApi.RequestResult execResult)
        {
            execResult = CreateSuccessResult();

            if (session == null)
            {
                _logger.Error($"Request was sent using invalid session id: {sessionId}");
                execResult = CreateRejectResult($"Session has been closed");
            }
        }

        private async Task SendServerStreamResponse<TResponse>(IServerStreamWriter<TResponse> responseStream, SessionInfo session, TResponse response)
            where TResponse : IMessage
        {
            _messageFormatter.LogClientResponse(session?.Logger, response);
            await responseStream.WriteAsync(response);
        }

        private TRequest GetClientStreamRequest<TRequest>(IAsyncStreamReader<TRequest> requestStream, SessionInfo session)
            where TRequest : IMessage
        {
            _messageFormatter.LogClientResponse(session?.Logger, requestStream.Current);
            return requestStream.Current;
        }

        #region Request handlers

        private async Task<AlgoServerApi.LoginResponse> LoginInternal(AlgoServerApi.LoginRequest request, ServerCallContext context)
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
                    var authResult = await _algoServer.ValidateCreds(request.Login, request.Password);
                    if (!authResult.Success)
                    {
                        res.ExecResult = CreateRejectResult();
                        res.Error = authResult.TemporarilyLocked
                            ? AlgoServerApi.LoginResponse.Types.LoginError.TemporarilyLocked
                            : AlgoServerApi.LoginResponse.Types.LoginError.InvalidCredentials;
                    }
                    else
                    {
                        var authPassed = !authResult.Requires2FA;
                        if (authResult.Requires2FA)
                        {
                            if (!VersionSpec.ClientSupports2FA(request.MinorVersion))
                            {
                                res.ExecResult = CreateRejectResult("2FA is not supported on current version");
                                res.Error = AlgoServerApi.LoginResponse.Types.LoginError.None;
                            }
                            else
                            {
                                authResult = await _algoServer.Validate2FA(request.Login, request.OneTimePassword);
                                authPassed = authResult.Success;
                                if (!authResult.Success)
                                {
                                    res.ExecResult = CreateRejectResult();
                                    res.Error = authResult.TemporarilyLocked
                                        ? AlgoServerApi.LoginResponse.Types.LoginError.TemporarilyLocked
                                        : AlgoServerApi.LoginResponse.Types.LoginError.Invalid2FaCode;
                                }
                            }
                        }

                        if (authPassed)
                        {
                            var session = SessionInfo.Create(request.Login, request.MinorVersion, authResult.AccessLevel, _logger.Factory);
                            var accessToken = string.Empty;
                            try
                            {
                                accessToken = _jwtProvider.CreateToken(session.GetJwtPayload());
                            }
                            catch (Exception ex)
                            {
                                res.ExecResult = CreateErrorResult("Failed to create access token");
                                _logger.Error(ex, $"Failed to create access token: {ex.Message}");
                            }

                            if (!string.IsNullOrWhiteSpace(accessToken))
                            {
                                res.SessionId = session.Id;
                                res.AccessLevel = session.AccessManager.Level.ToApi();
                                res.AccessToken = accessToken;

                                await SessionControl.AddSession(_sessionsRef, session, _logger.Factory);
                                session.Logger.Info($"Server version - {VersionSpec.LatestVersion}; Client version - {request.MajorVersion}.{request.MinorVersion}");
                                session.Logger.Info($"Current version set to {session.VersionSpec.CurrentVersionStr}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process login request");
                _logger.Error(ex, $"Failed to process login {_messageFormatter.ToJson(request)}");
            }

            return res;
        }

        private async Task<AlgoServerApi.LogoutResponse> LogoutInternal(AlgoServerApi.LogoutRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.LogoutResponse { ExecResult = execResult };
            if (session == null)
                return res;

            try
            {
                await SessionControl.RemoveSession(_sessionsRef, session.Id);
                res.Reason = AlgoServerApi.LogoutResponse.Types.LogoutReason.ClientRequest;
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process logout request");
                _logger.Error(ex, $"Failed to process logout {_messageFormatter.ToJson(request)}");
            }

            return res;
        }

        private async Task<AlgoServerApi.AccountMetadataResponse> GetAccountMetadataInternal(AlgoServerApi.AccountMetadataRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.PluginStatusSubscribeResponse> SubscribeToPluginStatusInternal(AlgoServerApi.PluginStatusSubscribeRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginStatusSubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginStatus())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.AddPluginStatusSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task<AlgoServerApi.PluginLogsSubscribeResponse> SubscribeToPluginLogsInternal(AlgoServerApi.PluginLogsSubscribeRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginLogsSubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginLogs())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.AddPluginLogsSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task<AlgoServerApi.PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatusInternal(AlgoServerApi.PluginStatusUnsubscribeRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginStatusUnsubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginStatus())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.RemovePluginStatusSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task<AlgoServerApi.PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogsInternal(AlgoServerApi.PluginLogsUnsubscribeRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
        {
            var res = new AlgoServerApi.PluginLogsUnsubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginLogs())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.RemovePluginLogsSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task SubscribeToUpdatesInternal(AlgoServerApi.SubscribeToUpdatesRequest request, IServerStreamWriter<AlgoServerApi.UpdateInfo> responseStream, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
        {
            if (session == null)
                return;

            if (!session.AccessManager.CanSubscribeToUpdates())
                return;

            var t = await SessionControl.OpenUpdatesChannel(_sessionsRef, session.Id, responseStream);
            try
            {
                await t;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update stream internal error");
            }
        }

        private async Task<AlgoServerApi.AddPluginResponse> AddBotInternal(AlgoServerApi.AddPluginRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.RemovePluginResponse> RemoveBotInternal(AlgoServerApi.RemovePluginRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.StartPluginResponse> StartBotInternal(AlgoServerApi.StartPluginRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.StopPluginResponse> StopBotInternal(AlgoServerApi.StopPluginRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.ChangePluginConfigResponse> ChangeBotConfigInternal(AlgoServerApi.ChangePluginConfigRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.AddAccountResponse> AddAccountInternal(AlgoServerApi.AddAccountRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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
                var accId = await _algoServer.AddAccount(request.ToServer());
                res.AccountId = accId;
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<AlgoServerApi.RemoveAccountResponse> RemoveAccountInternal(AlgoServerApi.RemoveAccountRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.ChangeAccountResponse> ChangeAccountInternal(AlgoServerApi.ChangeAccountRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.TestAccountResponse> TestAccountInternal(AlgoServerApi.TestAccountRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.TestAccountCredsResponse> TestAccountCredsInternal(AlgoServerApi.TestAccountCredsRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.UploadPackageResponse> UploadPackageInternal(IAsyncStreamReader<AlgoServerApi.FileTransferMsg> requestStream, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

                var pkgId = await _algoServer.UploadPackage(request.ToServer(), tmpPath);
                res.PackageId = pkgId;
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

        private async Task<AlgoServerApi.RemovePackageResponse> RemovePackageInternal(AlgoServerApi.RemovePackageRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task DownloadPackageInternal(AlgoServerApi.DownloadPackageRequest request, IServerStreamWriter<AlgoServerApi.FileTransferMsg> responseStream, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.PluginFolderInfoResponse> GetBotFolderInfoInternal(AlgoServerApi.PluginFolderInfoRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.ClearPluginFolderResponse> ClearBotFolderInternal(AlgoServerApi.ClearPluginFolderRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.DeletePluginFileResponse> DeleteBotFileInternal(AlgoServerApi.DeletePluginFileRequest request, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task DownloadBotFileInternal(AlgoServerApi.DownloadPluginFileRequest request, IServerStreamWriter<AlgoServerApi.FileTransferMsg> responseStream, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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

        private async Task<AlgoServerApi.UploadPluginFileResponse> UploadBotFileInternal(IAsyncStreamReader<AlgoServerApi.FileTransferMsg> requestStream, ServerCallContext context, SessionInfo session, AlgoServerApi.RequestResult execResult)
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
    }
}
