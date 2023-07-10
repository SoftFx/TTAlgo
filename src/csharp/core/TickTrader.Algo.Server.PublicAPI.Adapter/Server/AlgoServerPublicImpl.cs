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
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    public class AlgoServerPublicImpl : AlgoServerPublic.AlgoServerPublicBase
    {
        private readonly AlgoServerAdapter _algoServer;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger _logger;
        private readonly MessageFormatter _messageFormatter;
        private readonly VersionSpec _version;

        private IActorRef _sessionsRef;


        public AlgoServerPublicImpl(IAlgoServerApi serverApi, IAuthManager authManager, IJwtProvider jwtProvider, ILogger logger, bool logMessages, VersionSpec version)
        {
            _version = version;
            _algoServer = new AlgoServerAdapter(serverApi, authManager);
            _jwtProvider = jwtProvider;
            _logger = logger;

            _messageFormatter = new MessageFormatter(AlgoServerPublicAPIReflection.Descriptor) { LogMessages = logMessages };
        }


        public void Start()
        {
            _sessionsRef = SessionControlActor.Create(_algoServer, _logger, _messageFormatter);
        }

        public async Task Shutdown()
        {
            await SessionControl.Shutdown(_sessionsRef)
                .OnException(ex => _logger.Error(ex, "Failed to shutdown session control"));

            await ActorSystem.StopActor(_sessionsRef)
                .OnException(ex => _logger.Error(ex, "Failed to stop SessionContolActor"));
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

        internal static RequestResult CreateNotAllowedResult(SessionInfo session, string requestName)
        {
            return CreateNotAllowedResult($"{session.AccessManager.Level} is not allowed to execute {requestName}");
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

        public override Task<PluginStatusSubscribeResponse> SubscribeToPluginStatus(PluginStatusSubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(SubscribeToPluginStatusInternal, request, context);
        }

        public override Task<PluginLogsSubscribeResponse> SubscribeToPluginLogs(PluginLogsSubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(SubscribeToPluginLogsInternal, request, context);
        }

        public override Task<PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatus(PluginStatusUnsubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(UnsubscribeToPluginStatusInternal, request, context);
        }

        public override Task<PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogs(PluginLogsUnsubscribeRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(UnsubscribeToPluginLogsInternal, request, context);
        }

        public override Task<AccountMetadataResponse> GetAccountMetadata(AccountMetadataRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, request, context);
        }

        public override Task SubscribeToUpdates(SubscribeToUpdatesRequest request, IServerStreamWriter<UpdateInfo> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(SubscribeToUpdatesInternal, request, responseStream, context);
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

        public override Task<UploadPackageResponse> UploadPackage(IAsyncStreamReader<FileTransferMsg> requestStream, ServerCallContext context)
        {
            return ExecuteClientStreamingRequestAuthorized(UploadPackageInternal, requestStream, context);
        }

        public override Task<RemovePackageResponse> RemovePackage(RemovePackageRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemovePackageInternal, request, context);
        }

        public override Task DownloadPackage(DownloadPackageRequest request, IServerStreamWriter<FileTransferMsg> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadPackageInternal, request, responseStream, context);
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

        public override Task DownloadPluginFile(DownloadPluginFileRequest request, IServerStreamWriter<FileTransferMsg> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal, request, responseStream, context);
        }

        public override Task<UploadPluginFileResponse> UploadPluginFile(IAsyncStreamReader<FileTransferMsg> requestStream, ServerCallContext context)
        {
            return ExecuteClientStreamingRequestAuthorized(UploadBotFileInternal, requestStream, context);
        }

        public override Task<ServerVersionResponse> GetServerVersion(ServerVersionRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetServerVersionInternal, request, context);
        }

        public override Task<ServerUpdateListResponse> GetServerUpdateList(ServerUpdateListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetServerUpdateListInternal, request, context);
        }

        public override Task<StartServerUpdateResponse> StartServerUpdate(StartServerUpdateRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StartServerUpdateInternal, request, context);
        }

        #endregion Grpc request handlers overrides


        private async Task<TResponse> ExecuteUnaryRequest<TRequest, TResponse>(
            Func<TRequest, ServerCallContext, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogMsgFromClient(_logger, request);
                var response = await requestAction(request, context);
                _messageFormatter.LogMsgToClient(_logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteUnaryRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, ServerCallContext, SessionInfo, RequestResult, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var sessionId = GetSessionId(context, typeof(TRequest), out var execResult);
                var session = await GetSession(sessionId);
                if (session != null)
                    ValidateSession(sessionId, session, out execResult);

                _messageFormatter.LogMsgFromClient(session?.Logger, request);
                var response = await requestAction(request, context, session, execResult);
                _messageFormatter.LogMsgToClient(session?.Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task ExecuteServerStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, SessionInfo, RequestResult, Task> requestAction,
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

                _messageFormatter.LogMsgFromClient(session?.Logger, request);
                await requestAction(request, responseStream, context, session, execResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteClientStreamingRequestAuthorized<TRequest, TResponse>(
            Func<IAsyncStreamReader<TRequest>, ServerCallContext, SessionInfo, RequestResult, Task<TResponse>> requestAction,
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
                _messageFormatter.LogMsgToClient(session?.Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute request of type {typeof(TRequest).Name}");
                throw;
            }
        }

        private string GetSessionId(ServerCallContext context, System.Type requestType, out RequestResult execResult)
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

        private void ValidateSession(string sessionId, SessionInfo session, out RequestResult execResult)
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
            _messageFormatter.LogMsgToClient(session?.Logger, response);
            await responseStream.WriteAsync(response);
        }

        private TRequest GetClientStreamRequest<TRequest>(IAsyncStreamReader<TRequest> requestStream, SessionInfo session)
            where TRequest : IMessage
        {
            _messageFormatter.LogMsgToClient(session?.Logger, requestStream.Current);
            return requestStream.Current;
        }

        #region Request handlers

        private async Task<LoginResponse> LoginInternal(LoginRequest request, ServerCallContext context)
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
                    var authResult = await _algoServer.ValidateCreds(request.Login, request.Password);
                    if (!authResult.Success)
                    {
                        res.ExecResult = CreateRejectResult();
                        res.Error = authResult.TemporarilyLocked
                            ? LoginResponse.Types.LoginError.TemporarilyLocked
                            : LoginResponse.Types.LoginError.InvalidCredentials;
                    }
                    else
                    {
                        var authPassed = !authResult.Requires2FA;
                        if (authResult.Requires2FA)
                        {
                            if (!VersionSpec.ClientSupports2FA(request.MinorVersion))
                            {
                                res.ExecResult = CreateRejectResult("2FA is not supported on current version");
                                res.Error = LoginResponse.Types.LoginError.None;
                            }
                            else
                            {
                                authResult = await _algoServer.Validate2FA(request.Login, request.OneTimePassword);
                                authPassed = authResult.Success;
                                if (!authResult.Success)
                                {
                                    res.ExecResult = CreateRejectResult();
                                    res.Error = authResult.TemporarilyLocked
                                        ? LoginResponse.Types.LoginError.TemporarilyLocked
                                        : LoginResponse.Types.LoginError.Invalid2FaCode;
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
                                res.AccessLevel = session.AccessManager.Level;
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

        private async Task<LogoutResponse> LogoutInternal(LogoutRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new LogoutResponse { ExecResult = execResult };
            if (session == null)
                return res;

            try
            {
                await SessionControl.RemoveSession(_sessionsRef, session.Id);
                res.Reason = LogoutResponse.Types.LogoutReason.ClientRequest;
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process logout request");
                _logger.Error(ex, $"Failed to process logout {_messageFormatter.ToJson(request)}");
            }

            return res;
        }

        private async Task<AccountMetadataResponse> GetAccountMetadataInternal(AccountMetadataRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
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
                res.AccountMetadata = await _algoServer.GetAccountMetadata(request.ToServer()).ContinueWith(u => u.Result.ToApi());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<PluginStatusSubscribeResponse> SubscribeToPluginStatusInternal(PluginStatusSubscribeRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new PluginStatusSubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginStatus())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.AddPluginStatusSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task<PluginLogsSubscribeResponse> SubscribeToPluginLogsInternal(PluginLogsSubscribeRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new PluginLogsSubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginLogs())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.AddPluginLogsSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task<PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatusInternal(PluginStatusUnsubscribeRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new PluginStatusUnsubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginStatus())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.RemovePluginStatusSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task<PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogsInternal(PluginLogsUnsubscribeRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new PluginLogsUnsubscribeResponse { ExecResult = execResult };

            if (!session.AccessManager.CanGetPluginLogs())
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
            else
                await SessionControl.RemovePluginLogsSub(_sessionsRef, session.Id, request.PluginId);

            return res;
        }

        private async Task SubscribeToUpdatesInternal(SubscribeToUpdatesRequest request, IServerStreamWriter<UpdateInfo> responseStream, ServerCallContext context, SessionInfo session, RequestResult execResult)
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

        private async Task<AddPluginResponse> AddBotInternal(AddPluginRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new AddPluginResponse { ExecResult = execResult };
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

        private async Task<RemovePluginResponse> RemoveBotInternal(RemovePluginRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new RemovePluginResponse { ExecResult = execResult };

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

        private async Task<StartPluginResponse> StartBotInternal(StartPluginRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new StartPluginResponse { ExecResult = execResult };
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

        private async Task<StopPluginResponse> StopBotInternal(StopPluginRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new StopPluginResponse { ExecResult = execResult };
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

        private async Task<ChangePluginConfigResponse> ChangeBotConfigInternal(ChangePluginConfigRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new ChangePluginConfigResponse { ExecResult = execResult };
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

        private async Task<AddAccountResponse> AddAccountInternal(AddAccountRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new AddAccountResponse { ExecResult = execResult };
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

        private async Task<RemoveAccountResponse> RemoveAccountInternal(RemoveAccountRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new RemoveAccountResponse { ExecResult = execResult };
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

        private async Task<ChangeAccountResponse> ChangeAccountInternal(ChangeAccountRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new ChangeAccountResponse { ExecResult = execResult };
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

        private async Task<TestAccountResponse> TestAccountInternal(TestAccountRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
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
                res.ErrorInfo = (await _algoServer.TestAccount(request.ToServer())).ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<TestAccountCredsResponse> TestAccountCredsInternal(TestAccountCredsRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
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
                res.ErrorInfo = (await _algoServer.TestAccountCreds(request.ToServer())).ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account creds");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<UploadPackageResponse> UploadPackageInternal(IAsyncStreamReader<FileTransferMsg> requestStream, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new UploadPackageResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanUploadPackage())
            {
                res.ExecResult = CreateNotAllowedResult(session, typeof(UploadPackageRequest).Name);
                return res;
            }

            if (!await requestStream.MoveNext())
            {
                res.ExecResult = CreateErrorResult("Empty upload stream");
                return res;
            }

            var transferMsg = GetClientStreamRequest(requestStream, session);
            if (transferMsg == null || !transferMsg.Header.Is(UploadPackageRequest.Descriptor))
            {
                res.ExecResult = CreateRejectResult($"Expected {nameof(UploadPackageRequest)} header, but received '{transferMsg.Header.TypeUrl}'");
                return res;
            }
            var request = transferMsg.Header.Unpack<UploadPackageRequest>();
            _messageFormatter.LogMsgToClient(session?.Logger, request);

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

                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
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

        private async Task<RemovePackageResponse> RemovePackageInternal(RemovePackageRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
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
                await _algoServer.RemovePackage(request.ToServer());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task DownloadPackageInternal(DownloadPackageRequest request, IServerStreamWriter<FileTransferMsg> responseStream, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var response = new DownloadPackageResponse { ExecResult = execResult };
            var transferMsg = new FileTransferMsg { Data = FileChunk.FinalChunk };

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

            transferMsg.Data = new FileChunk(chunkOffset);
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
            transferMsg.Data = FileChunk.FinalChunk;
            transferMsg.Header = Any.Pack(response);
            await SendServerStreamResponse(responseStream, session, transferMsg);
        }

        private async Task<PluginFolderInfoResponse> GetBotFolderInfoInternal(PluginFolderInfoRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
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
                res.FolderInfo = (await _algoServer.GetBotFolderInfo(request.ToServer())).ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot folder info");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<ClearPluginFolderResponse> ClearBotFolderInternal(ClearPluginFolderRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new ClearPluginFolderResponse { ExecResult = execResult };
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

        private async Task<DeletePluginFileResponse> DeleteBotFileInternal(DeletePluginFileRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new DeletePluginFileResponse { ExecResult = execResult };
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

        private async Task DownloadBotFileInternal(DownloadPluginFileRequest request, IServerStreamWriter<FileTransferMsg> responseStream, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var response = new DownloadPluginFileResponse { ExecResult = execResult };
            var transferMsg = new FileTransferMsg { Data = FileChunk.FinalChunk };

            if (session == null)
            {
                transferMsg.Header = Any.Pack(response);
                await SendServerStreamResponse(responseStream, session, transferMsg);
                return;
            }
            if (!session.AccessManager.CanDownloadBotFile(request.FolderId))
            {
                response.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                transferMsg.Header = Any.Pack(response);
                await SendServerStreamResponse(responseStream, session, transferMsg);
                return;
            }

            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            transferMsg.Data = new FileChunk(chunkOffset);
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
            transferMsg.Data = FileChunk.FinalChunk;
            transferMsg.Header = Any.Pack(response);
            await SendServerStreamResponse(responseStream, session, transferMsg);
        }

        private async Task<UploadPluginFileResponse> UploadBotFileInternal(IAsyncStreamReader<FileTransferMsg> requestStream, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new UploadPluginFileResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanUploadBotFile())
            {
                res.ExecResult = CreateNotAllowedResult(session, typeof(UploadPluginFileRequest).Name);
                return res;
            }

            if (!await requestStream.MoveNext())
            {
                res.ExecResult = CreateErrorResult("Empty upload stream");
                return res;
            }

            var transferMsg = GetClientStreamRequest(requestStream, session);
            if (transferMsg == null || !transferMsg.Header.Is(UploadPluginFileRequest.Descriptor))
            {
                res.ExecResult = CreateRejectResult($"Expected {nameof(UploadPluginFileRequest)} header, but received '{transferMsg.Header.TypeUrl}'");
                return res;
            }
            var request = transferMsg.Header.Unpack<UploadPluginFileRequest>();
            _messageFormatter.LogMsgToClient(session?.Logger, request);

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

        private async Task<ServerVersionResponse> GetServerVersionInternal(ServerVersionRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new ServerVersionResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetServerVersion())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.Version = await _algoServer.GetServerVersion();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get server version");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<ServerUpdateListResponse> GetServerUpdateListInternal(ServerUpdateListRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new ServerUpdateListResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanGetServerUpdates())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                res.Updates.AddRange((await _algoServer.GetServerUpdates(request.ToServer())).Updates.Select(u => u.ToApi()));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get server updates list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private async Task<StartServerUpdateResponse> StartServerUpdateInternal(StartServerUpdateRequest request, ServerCallContext context, SessionInfo session, RequestResult execResult)
        {
            var res = new StartServerUpdateResponse { ExecResult = execResult };
            if (session == null)
                return res;
            if (!session.AccessManager.CanStartServerUpdate())
            {
                res.ExecResult = CreateNotAllowedResult(session, request.GetType().Name);
                return res;
            }

            try
            {
                var serverRes = await _algoServer.StartServerUpdate(request.ToServer());
                res.Status = serverRes.Status.ToApi();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get server version");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        #endregion Request handlers
    }
}
