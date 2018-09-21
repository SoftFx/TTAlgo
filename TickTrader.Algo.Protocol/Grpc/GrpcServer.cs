using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using NLog;
using TickTrader.Algo.Common.Info;
using GrpcCore = Grpc.Core;

namespace TickTrader.Algo.Protocol.Grpc
{
    public class GrpcServer : ProtocolServer
    {
        private IJwtProvider _jwtProvider;
        private BotAgentServerImpl _impl;
        private GrpcCore.Server _server;


        static GrpcServer()
        {
            CertificateProvider.InitServer();
        }


        public GrpcServer(IBotAgentServer agentServer, IServerSettings settings, IJwtProvider jwtProvider) : base(agentServer, settings)
        {
            _jwtProvider = jwtProvider;
        }


        protected override void StartServer()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            _impl = new BotAgentServerImpl(AgentServer, _jwtProvider, Logger, Settings.ProtocolSettings.LogMessages);
            var creds = new SslServerCredentials(new[] { new KeyCertificatePair(CertificateProvider.ServerCertificate, CertificateProvider.ServerKey), }); //,CertificateProvider.RootCertificate, true);
            _server = new GrpcCore.Server
            {
                Services = { Lib.BotAgent.BindService(_impl) },
                Ports = { new ServerPort("0.0.0.0", Settings.ProtocolSettings.ListeningPort, creds) },
            };
            _server.Start();
        }

        protected override void StopServer()
        {
            _impl.DisconnectAllClients();
            _server.ShutdownAsync().Wait();
        }
    }


    internal class BotAgentServerImpl : Lib.BotAgent.BotAgentBase
    {
        private IBotAgentServer _botAgent;
        private IJwtProvider _jwtProvider;
        private ILogger _logger;
        private MessageFormatter _messageFormatter;
        private Dictionary<string, ServerSession.Handler> _sessions;


        public BotAgentServerImpl(IBotAgentServer botAgent, IJwtProvider jwtProvider, ILogger logger, bool logMessages)
        {
            _botAgent = botAgent;
            _jwtProvider = jwtProvider;
            _logger = logger;

            _messageFormatter = new MessageFormatter { LogMessages = logMessages };
            _sessions = new Dictionary<string, ServerSession.Handler>();

            _botAgent.PackageUpdated += OnPackageUpdate;
            _botAgent.PackageStateUpdated += OnPackageStateUpdate;
            _botAgent.AccountUpdated += OnAccountUpdate;
            _botAgent.AccountStateUpdated += OnAccountStateUpdate;
            _botAgent.BotUpdated += OnBotUpdate;
            _botAgent.BotStateUpdated += OnBotStateUpdate;
        }


        public static Lib.RequestResult CreateSuccessResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.Success, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateErrorResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.InternalServerError, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateErrorResult(Exception ex)
        {
            return CreateErrorResult(ex.Flatten().Message);
        }

        public static Lib.RequestResult CreateUnauthorizedResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.Unauthorized, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateUnauthorizedResult(Exception ex)
        {
            return CreateUnauthorizedResult(ex.Flatten().Message);
        }

        public static Lib.RequestResult CreateRejectResult(string message = "")
        {
            return new Lib.RequestResult { Status = Lib.RequestResult.Types.RequestStatus.Reject, Message = message ?? "" };
        }

        public static Lib.RequestResult CreateRejectResult(Exception ex)
        {
            return CreateRejectResult(ex.Flatten().Message);
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

        public override Task<Lib.LoginResponse> Login(Lib.LoginRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequest(LoginInternal, request, context);
        }

        public override Task<Lib.LogoutResponse> Logout(Lib.LogoutRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(LogoutInternal, request, context);
        }

        public override Task<Lib.SnapshotResponse> GetSnapshot(Lib.SnapshotRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetSnapshotInternal, request, context);
        }

        public override Task SubscribeToUpdates(Lib.SubscribeToUpdatesRequest request, IServerStreamWriter<Lib.UpdateInfo> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(SubscribeToUpdatesInternal, request, responseStream, context);
        }

        public override Task<Lib.ApiMetadataResponse> GetApiMetadata(Lib.ApiMetadataRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetApiMetadataInternal, request, context);
        }

        public override Task<Lib.MappingsInfoResponse> GetMappingsInfo(Lib.MappingsInfoRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetMappingsInfoInternal, request, context);
        }

        public override Task<Lib.SetupContextResponse> GetSetupContext(Lib.SetupContextRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetSetupContextInternal, request, context);
        }

        public override Task<Lib.AccountMetadataResponse> GetAccountMetadata(Lib.AccountMetadataRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, request, context);
        }

        public override Task<Lib.BotListResponse> GetBotList(Lib.BotListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotListInternal, request, context);
        }

        public override Task<Lib.AddBotResponse> AddBot(Lib.AddBotRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(AddBotInternal, request, context);

        }

        public override Task<Lib.RemoveBotResponse> RemoveBot(Lib.RemoveBotRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemoveBotInternal, request, context);
        }

        public override Task<Lib.StartBotResponse> StartBot(Lib.StartBotRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StartBotInternal, request, context);
        }

        public override Task<Lib.StopBotResponse> StopBot(Lib.StopBotRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(StopBotInternal, request, context);
        }

        public override Task<Lib.ChangeBotConfigResponse> ChangeBotConfig(Lib.ChangeBotConfigRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ChangeBotConfigInternal, request, context);
        }

        public override Task<Lib.AccountListResponse> GetAccountList(Lib.AccountListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetAccountListInternal, request, context);
        }

        public override Task<Lib.AddAccountResponse> AddAccount(Lib.AddAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(AddAccountInternal, request, context);
        }

        public override Task<Lib.RemoveAccountResponse> RemoveAccount(Lib.RemoveAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemoveAccountInternal, request, context);
        }

        public override Task<Lib.ChangeAccountResponse> ChangeAccount(Lib.ChangeAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ChangeAccountInternal, request, context);
        }

        public override Task<Lib.TestAccountResponse> TestAccount(Lib.TestAccountRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(TestAccountInternal, request, context);
        }

        public override Task<Lib.TestAccountCredsResponse> TestAccountCreds(Lib.TestAccountCredsRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(TestAccountCredsInternal, request, context);
        }

        public override Task<Lib.PackageListResponse> GetPackageList(Lib.PackageListRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetPackageListInternal, request, context);
        }

        public override Task<Lib.UploadPackageResponse> UploadPackage(Lib.UploadPackageRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(UploadPackageInternal, request, context);
        }

        public override Task<Lib.RemovePackageResponse> RemovePackage(Lib.RemovePackageRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(RemovePackageInternal, request, context);
        }

        public override Task<Lib.DownloadPackageResponse> DownloadPackage(Lib.DownloadPackageRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(DownloadPackageInternal, request, context);
        }

        public override Task<Lib.BotStatusResponse> GetBotStatus(Lib.BotStatusRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotStatusInternal, request, context);
        }

        public override Task<Lib.BotLogsResponse> GetBotLogs(Lib.BotLogsRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotLogsInternal, request, context);
        }

        public override Task<Lib.BotFolderInfoResponse> GetBotFolderInfo(Lib.BotFolderInfoRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(GetBotFolderInfoInternal, request, context);
        }

        public override Task<Lib.ClearBotFolderResponse> ClearBotFolder(Lib.ClearBotFolderRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(ClearBotFolderInternal, request, context);
        }

        public override Task<Lib.DeleteBotFileResponse> DeleteBotFile(Lib.DeleteBotFileRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(DeleteBotFileInternal, request, context);
        }

        public override Task DownloadBotFile(Lib.DownloadBotFileRequest request, IServerStreamWriter<Lib.FileChunk> responseStream, ServerCallContext context)
        {
            return ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal, request, responseStream, context);
        }

        public override Task<Lib.UploadBotFileResponse> UploadBotFile(Lib.UploadBotFileRequest request, ServerCallContext context)
        {
            return ExecuteUnaryRequestAuthorized(UploadBotFileInternal, request, context);
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
                _logger.Error(ex, $"Failed to execute request: {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteUnaryRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, ServerCallContext, ServerSession.Handler, Lib.RequestResult, Task<TResponse>> requestAction, TRequest request, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var session = GetSession(context, request, out var execResult);

                _messageFormatter.LogClientRequest(session?.Logger, request);
                var response = await requestAction(request, context, session, execResult);
                _messageFormatter.LogClientResponse(session?.Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute request: {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private Task ExecuteServerStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, ServerSession.Handler, Lib.RequestResult, Task> requestAction,
            TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                var session = GetSession(context, request, out var execResult);

                _messageFormatter.LogClientRequest(session?.Logger, request);
                return requestAction(request, responseStream, context, session, execResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute request: {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private ServerSession.Handler GetSession<TRequest>(ServerCallContext context, TRequest request, out Lib.RequestResult execResult)
        {
            execResult = CreateSuccessResult();

            try
            {
                var entry = context.RequestHeaders.LastOrDefault(e => e.Key == "authorization");
                if (entry == null)
                {
                    _logger.Error($"Missing authorization header for request of type {request.GetType().Name}");
                    execResult = CreateUnauthorizedResult("Missing authorization header");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(entry.Value) || !entry.Value.StartsWith("Bearer "))
                {
                    _logger.Error($"Authorization header value({entry.Value}) doesn't match Bearer authentication scheme for request of type {request.GetType().Name}");
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
                    _logger.Error($"{uex.Message}. Request of type {request.GetType().Name}");
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


        #region Request handlers

        private Task<Lib.LoginResponse> LoginInternal(Lib.LoginRequest request, ServerCallContext context)
        {
            var res = new Lib.LoginResponse
            {
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion,
                ExecResult = CreateSuccessResult(),
                Error = Lib.LoginResponse.Types.LoginError.None,
                AccessToken = "",
            };

            try
            {
                if (!VersionSpec.CheckClientCompatibility(request.MajorVersion, request.MinorVersion, out var error))
                {
                    res.ExecResult = CreateRejectResult(error);
                    res.Error = Lib.LoginResponse.Types.LoginError.VersionMismatch;
                }
                else if (!_botAgent.ValidateCreds(request.Login, request.Password))
                {
                    res.ExecResult = CreateRejectResult();
                    res.Error = Lib.LoginResponse.Types.LoginError.InvalidCredentials;
                }
                else
                {
                    var session = new ServerSession.Handler(Guid.NewGuid().ToString(), request.Login, request.MinorVersion, _logger.Factory, _messageFormatter);
                    try
                    {
                        var payload = new JwtPayload
                        {
                            Username = session.Username,
                            SessionId = session.SessionId,
                            MinorVersion = session.VersionSpec.CurrentVersion,
                        };
                        res.SessionId = session.SessionId;
                        res.AccessToken = _jwtProvider.CreateToken(payload);
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
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process login request");
                _logger.Error(ex, $"Failed to process login request: {_messageFormatter.ToJson(request)}");
            }

            return Task.FromResult(res);
        }

        private Task<Lib.LogoutResponse> LogoutInternal(Lib.LogoutRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.LogoutResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                lock (_sessions)
                {
                    session.CloseUpdateStream();
                    _sessions.Remove(session.SessionId);
                }
                res.Reason = Lib.LogoutResponse.Types.LogoutReason.ClientRequest;
            }
            catch (Exception ex)
            {
                res.ExecResult = CreateErrorResult("Failed to process logout request");
                _logger.Error(ex, $"Failed to process logout request: {_messageFormatter.ToJson(request)}");
            }

            return Task.FromResult(res);
        }

        private async Task<Lib.SnapshotResponse> GetSnapshotInternal(Lib.SnapshotRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.SnapshotResponse { ExecResult = execResult };
            if (session == null)
                return res;

            try
            {
                res.ApiMetadata = await GetApiMetadataInternal(new Lib.ApiMetadataRequest(), context, session, execResult);
                res.MappingsInfo = await GetMappingsInfoInternal(new Lib.MappingsInfoRequest(), context, session, execResult);
                res.SetupContext = await GetSetupContextInternal(new Lib.SetupContextRequest(), context, session, execResult);
                res.PackageList = await GetPackageListInternal(new Lib.PackageListRequest(), context, session, execResult);
                res.AccountList = await GetAccountListInternal(new Lib.AccountListRequest(), context, session, execResult);
                res.BotList = await GetBotListInternal(new Lib.BotListRequest(), context, session, execResult);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get snapshot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return res;
        }

        private Task SubscribeToUpdatesInternal(Lib.SubscribeToUpdatesRequest request, IServerStreamWriter<Lib.UpdateInfo> responseStream, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            if (session == null)
                return Task.FromResult(this);

            return session.SetupUpdateStream(responseStream);
        }

        private Task<Lib.ApiMetadataResponse> GetApiMetadataInternal(Lib.ApiMetadataRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.ApiMetadataResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.ApiMetadata = _botAgent.GetApiMetadata().Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get api metadata");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.MappingsInfoResponse> GetMappingsInfoInternal(Lib.MappingsInfoRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.MappingsInfoResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.Mappings = _botAgent.GetMappingsInfo().Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get mappings collection");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.SetupContextResponse> GetSetupContextInternal(Lib.SetupContextRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.SetupContextResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.SetupContext = _botAgent.GetSetupContext().Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get setup context");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.AccountMetadataResponse> GetAccountMetadataInternal(Lib.AccountMetadataRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.AccountMetadataResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.AccountMetadata = _botAgent.GetAccountMetadata(request.Account.Convert()).Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get account metadata");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.BotListResponse> GetBotListInternal(Lib.BotListRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.BotListResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.Bots.AddRange(_botAgent.GetBotList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.AddBotResponse> AddBotInternal(Lib.AddBotRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.AddBotResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.AddBot(request.Account.Convert(), request.Config.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        private Task<Lib.RemoveBotResponse> RemoveBotInternal(Lib.RemoveBotRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.RemoveBotResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.RemoveBot(request.BotId, request.CleanLog, request.CleanAlgoData);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        private Task<Lib.StartBotResponse> StartBotInternal(Lib.StartBotRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.StartBotResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.StartBot(request.BotId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to start bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.StopBotResponse> StopBotInternal(Lib.StopBotRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.StopBotResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.StopBot(request.BotId);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to stop bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.ChangeBotConfigResponse> ChangeBotConfigInternal(Lib.ChangeBotConfigRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.ChangeBotConfigResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.ChangeBotConfig(request.BotId, request.NewConfig.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add bot");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);

        }

        private Task<Lib.AccountListResponse> GetAccountListInternal(Lib.AccountListRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.AccountListResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.Accounts.AddRange(_botAgent.GetAccountList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get account list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.AddAccountResponse> AddAccountInternal(Lib.AddAccountRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.AddAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.AddAccount(request.Account.Convert(), request.Password, request.UseNewProtocol);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to add account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.RemoveAccountResponse> RemoveAccountInternal(Lib.RemoveAccountRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.RemoveAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.RemoveAccount(request.Account.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.ChangeAccountResponse> ChangeAccountInternal(Lib.ChangeAccountRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.ChangeAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.ChangeAccount(request.Account.Convert(), request.Password, request.UseNewProtocol);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to change account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.TestAccountResponse> TestAccountInternal(Lib.TestAccountRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.TestAccountResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.ErrorInfo = _botAgent.TestAccount(request.Account.Convert()).Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.TestAccountCredsResponse> TestAccountCredsInternal(Lib.TestAccountCredsRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.TestAccountCredsResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.ErrorInfo = _botAgent.TestAccountCreds(request.Account.Convert(), request.Password, request.UseNewProtocol).Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to test account creds");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.PackageListResponse> GetPackageListInternal(Lib.PackageListRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.PackageListResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.Packages.AddRange(_botAgent.GetPackageList().Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get packages list");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.UploadPackageResponse> UploadPackageInternal(Lib.UploadPackageRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.UploadPackageResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.UploadPackage(request.FileName, request.PackageBinary.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to upload package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.RemovePackageResponse> RemovePackageInternal(Lib.RemovePackageRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.RemovePackageResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.RemovePackage(request.Package.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to remove package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.DownloadPackageResponse> DownloadPackageInternal(Lib.DownloadPackageRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.DownloadPackageResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.PackageBinary = _botAgent.DownloadPackage(request.Package.Convert()).Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to download package");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.BotStatusResponse> GetBotStatusInternal(Lib.BotStatusRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.BotStatusResponse { ExecResult = execResult, BotId = request.BotId };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.Status = ToGrpc.Convert(_botAgent.GetBotStatus(request.BotId));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot status");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.BotLogsResponse> GetBotLogsInternal(Lib.BotLogsRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.BotLogsResponse { ExecResult = execResult, BotId = request.BotId };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.Logs.AddRange(_botAgent.GetBotLogs(request.BotId, request.LastLogTimeUtc.ToDateTime(), request.MaxCount).Select(ToGrpc.Convert));
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot logs");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.BotFolderInfoResponse> GetBotFolderInfoInternal(Lib.BotFolderInfoRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.BotFolderInfoResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                res.FolderInfo = _botAgent.GetBotFolderInfo(request.BotId, request.FolderId.Convert()).Convert();
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to get bot folder info");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.ClearBotFolderResponse> ClearBotFolderInternal(Lib.ClearBotFolderRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.ClearBotFolderResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.ClearBotFolder(request.BotId, request.FolderId.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to clear bot folder");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private Task<Lib.DeleteBotFileResponse> DeleteBotFileInternal(Lib.DeleteBotFileRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.DeleteBotFileResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.DeleteBotFile(request.BotId, request.FolderId.Convert(), request.FileName);
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to delete bot file");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        private async Task DownloadBotFileInternal(Lib.DownloadBotFileRequest request, IServerStreamWriter<Lib.FileChunk> responseStream, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var chunk = new Lib.FileChunk { ExecResult = execResult };
            if (session == null)
            {
                chunk.IsFinal = true;
                await responseStream.WriteAsync(chunk);
                return;
            }
            try
            {
                const int bufferSize = 512 * 1024;
                using (var stream = _botAgent.GetBotFile(request.BotId, request.FolderId.Convert(), request.FileName))
                using (var binaryReader = new BinaryReader(stream))
                {
                    chunk.ChunkBinary = binaryReader.ReadBytes(bufferSize).Convert();
                    await responseStream.WriteAsync(chunk);
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to clear bot folder");
                chunk.ExecResult = CreateErrorResult(ex);
                chunk.ChunkBinary = ByteString.Empty;
                chunk.IsFinal = true;
                await responseStream.WriteAsync(chunk);
                return;
            }
            chunk.ChunkBinary = ByteString.Empty;
            chunk.IsFinal = true;
            await responseStream.WriteAsync(chunk);
        }

        private Task<Lib.UploadBotFileResponse> UploadBotFileInternal(Lib.UploadBotFileRequest request, ServerCallContext context, ServerSession.Handler session, Lib.RequestResult execResult)
        {
            var res = new Lib.UploadBotFileResponse { ExecResult = execResult };
            if (session == null)
                return Task.FromResult(res);

            try
            {
                _botAgent.UploadBotFile(request.BotId, request.FolderId.Convert(), request.FileName, request.FileBinary.Convert());
            }
            catch (Exception ex)
            {
                session.Logger.Error(ex, "Failed to upload bot file");
                res.ExecResult = CreateErrorResult(ex);
            }
            return Task.FromResult(res);
        }

        #endregion Request handlers


        #region Updates

        private void OnPackageUpdate(UpdateInfo<PackageInfo> update)
        {
            SendUpdate(update.Convert());
        }

        private void OnPackageStateUpdate(PackageInfo package)
        {
            SendUpdate(new UpdateInfo<PackageInfo> { Type = UpdateType.Replaced, Value = package }.ConvertStateUpdate());
        }

        private void OnAccountUpdate(UpdateInfo<AccountModelInfo> update)
        {
            SendUpdate(update.Convert());
        }

        private void OnAccountStateUpdate(AccountModelInfo account)
        {
            SendUpdate(new UpdateInfo<AccountModelInfo> { Type = UpdateType.Replaced, Value = account }.ConvertStateUpdate());
        }

        private void OnBotUpdate(UpdateInfo<BotModelInfo> update)
        {
            SendUpdate(update.Convert());
        }

        private void OnBotStateUpdate(BotModelInfo bot)
        {
            SendUpdate(new UpdateInfo<BotModelInfo> { Type = UpdateType.Replaced, Value = bot }.ConvertStateUpdate());
        }

        private void SendUpdate(Lib.UpdateInfo update)
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
                    _logger.Error(ex, $"Failed to multicast update: {_messageFormatter.ToJson(update)}");
                }
            }
        }

        #endregion
    }
}
