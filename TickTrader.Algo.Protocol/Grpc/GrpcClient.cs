using Google.Protobuf;
using Grpc.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Protocol.Grpc
{
    public class GrpcClient : ProtocolClient
    {
        private const int HeartbeatTimeout = 10000;

        private MessageFormatter _messageFormatter;
        private Channel _channel;
        private Lib.BotAgent.BotAgentClient _client;
        private string _accessToken;
        private CancellationTokenSource _updateStreamCancelTokenSrc;
        private Timer _heartbeatTimer;


        static GrpcClient()
        {
            CertificateProvider.InitClient();
        }


        public GrpcClient(IBotAgentClient agentClient) : base(agentClient)
        {
            _messageFormatter = new MessageFormatter();
        }


        protected override void StartClient()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            var creds = new SslCredentials(CertificateProvider.RootCertificate); //, new KeyCertificatePair(CertificateProvider.ClientCertificate, CertificateProvider.ClientKey));
            var options = new[] { new ChannelOption(ChannelOptions.SslTargetNameOverride, "bot-agent.soft-fx.lv"), };
            _channel = new Channel(SessionSettings.ServerAddress, SessionSettings.ProtocolSettings.ListeningPort, creds, options);
            _accessToken = "";
            _messageFormatter.LogMessages = SessionSettings.ProtocolSettings.LogMessages;

            _client = new Lib.BotAgent.BotAgentClient(_channel);

            _channel.ConnectAsync(DateTime.UtcNow.AddSeconds(DefaultRequestTimeout))
                .ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            OnConnected();
                            break;
                        case TaskStatus.Canceled:
                            Logger.Info("Connect timed out");
                            OnConnectionError("Connection timeout");
                            break;
                        case TaskStatus.Faulted:
                            Logger.Error(t.Exception, "Connection failed");
                            OnConnectionError("Connection failed");
                            break;
                    }
                });
        }

        protected override void StopClient()
        {
            _messageFormatter.LogMessages = false;
            _updateStreamCancelTokenSrc?.Cancel();
            _updateStreamCancelTokenSrc = null;
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            _channel.ShutdownAsync().Wait();
        }

        protected override void SendLogin()
        {
            ExecuteUnaryRequest(LoginInternal, new Lib.LoginRequest
            {
                Login = SessionSettings.Login,
                Password = SessionSettings.Password,
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion
            }).ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        if (t.Result.Error == Lib.LoginResponse.Types.LoginError.None)
                        {
                            _accessToken = t.Result.AccessToken;
                            Logger.Info($"Server session id: {t.Result.SessionId}");
                            OnLogin(t.Result.MajorVersion, t.Result.MinorVersion, t.Result.AccessLevel.Convert());
                            _heartbeatTimer = new Timer(HeartbeatTimerCallback, null, HeartbeatTimeout, -1);
                        }
                        else
                        {
                            OnConnectionError(t.Result.Error.ToString());
                        }
                        break;
                    case TaskStatus.Canceled:
                        Logger.Error("Login request timed out");
                        OnConnectionError("Login timeout");
                        break;
                    case TaskStatus.Faulted:
                        Logger.Error(t.Exception, "Login failed");
                        OnConnectionError("Login failed");
                        break;
                }
            });
        }

        protected override void Init()
        {
            ExecuteUnaryRequestAuthorized(GetSnapshotInternal, new Lib.SnapshotRequest())
                .ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            ApplySnapshot(t.Result);
                            break;
                        case TaskStatus.Canceled:
                            Logger.Error("Get snapshot request timed out");
                            OnConnectionError("Request timeout during init");
                            break;
                        case TaskStatus.Faulted:
                            Logger.Error(t.Exception, "Get snapshot request failed");
                            OnConnectionError("Init failed");
                            break;
                    }
                });
        }

        protected override void SendLogout()
        {
            ExecuteUnaryRequestAuthorized(LogoutInternal, new Lib.LogoutRequest())
                .ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            OnLogout(t.Result.Reason.ToString());
                            break;
                        case TaskStatus.Canceled:
                            Logger.Error("Logout request timed out");
                            OnConnectionError("Logout timeout");
                            break;
                        case TaskStatus.Faulted:
                            Logger.Error(t.Exception, "Logout failed");
                            OnConnectionError("Logout failed");
                            break;
                    }
                });
        }

        protected override void SendDisconnect()
        {
            OnDisconnected();
        }


        private bool FailForNonSuccess(Lib.RequestResult.Types.RequestStatus status)
        {
            return status != Lib.RequestResult.Types.RequestStatus.Success;
        }

        private void FailForNonSuccess(Lib.RequestResult requestResult)
        {
            if (FailForNonSuccess(requestResult.Status))
                throw requestResult.Status == Lib.RequestResult.Types.RequestStatus.Unauthorized
                    ? new UnauthorizedException(requestResult.Message)
                    : new BAException($"{requestResult.Status} - {requestResult.Message}");
        }

        private void ApplySnapshot(Lib.SnapshotResponse snapshot)
        {
            try
            {
                FailForNonSuccess(snapshot.ExecResult);

                var apiMetadata = snapshot.ApiMetadata;
                FailForNonSuccess(apiMetadata.ExecResult);
                AgentClient.SetApiMetadata(apiMetadata.ApiMetadata.Convert());

                var mappings = snapshot.MappingsInfo;
                FailForNonSuccess(mappings.ExecResult);
                AgentClient.SetMappingsInfo(mappings.Mappings.Convert());

                var setupContext = snapshot.SetupContext;
                FailForNonSuccess(setupContext.ExecResult);
                AgentClient.SetSetupContext(setupContext.SetupContext.Convert());

                var packages = snapshot.PackageList;
                FailForNonSuccess(packages.ExecResult);
                AgentClient.InitPackageList(packages.Packages.Select(ToAlgo.Convert).ToList());

                var accounts = snapshot.AccountList;
                FailForNonSuccess(accounts.ExecResult);
                AgentClient.InitAccountList(accounts.Accounts.Select(ToAlgo.Convert).ToList());

                var bots = snapshot.BotList;
                FailForNonSuccess(bots.ExecResult);
                AgentClient.InitBotList(bots.Bots.Select(ToAlgo.Convert).ToList());
            }
            catch (UnauthorizedException uex)
            {
                Logger.Error(uex, "Init failed: Bad access token");
                OnConnectionError("Bad access token");
                return;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Init failed: Can't apply snapshot");
                OnConnectionError("Init failed");
                return;
            }

            ExecuteServerStreamingRequestAuthorized(SubscribeToUpdatesInternal, new Lib.SubscribeToUpdatesRequest())
                .ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            ListenToUpdates(t.Result);
                            OnSubscribed();
                            break;
                        case TaskStatus.Canceled:
                            Logger.Error("Subscribe to updates request timed out");
                            OnConnectionError("Request timeout during init");
                            break;
                        case TaskStatus.Faulted:
                            Logger.Error(t.Exception, "Init failed: Can't subscribe to updates");
                            OnConnectionError("Init failed");
                            break;
                    }
                });
        }

        private async void ListenToUpdates(IAsyncStreamReader<Lib.UpdateInfo> updateStream)
        {
            _updateStreamCancelTokenSrc = new CancellationTokenSource();
            try
            {
                while (await updateStream.MoveNext(_updateStreamCancelTokenSrc.Token))
                {
                    var update = updateStream.Current;
                    _messageFormatter.LogServerResponse(Logger, update);
                    switch (update.UpdateInfoCase)
                    {
                        case Lib.UpdateInfo.UpdateInfoOneofCase.Package:
                            AgentClient.UpdatePackage((UpdateInfo<PackageInfo>)update.Convert());
                            break;
                        case Lib.UpdateInfo.UpdateInfoOneofCase.PackageState:
                            AgentClient.UpdatePackageState((UpdateInfo<PackageInfo>)update.Convert());
                            break;
                        case Lib.UpdateInfo.UpdateInfoOneofCase.Account:
                            AgentClient.UpdateAccount((UpdateInfo<AccountModelInfo>)update.Convert());
                            break;
                        case Lib.UpdateInfo.UpdateInfoOneofCase.AccountState:
                            AgentClient.UpdateAccountState((UpdateInfo<AccountModelInfo>)update.Convert());
                            break;
                        case Lib.UpdateInfo.UpdateInfoOneofCase.Bot:
                            AgentClient.UpdateBot((UpdateInfo<BotModelInfo>)update.Convert());
                            break;
                        case Lib.UpdateInfo.UpdateInfoOneofCase.BotState:
                            AgentClient.UpdateBotState((UpdateInfo<BotModelInfo>)update.Convert());
                            break;
                        default:
                            throw new ArgumentException();
                    }
                }
                if (State != ClientStates.LoggingOut)
                {
                    Logger.Info("Update stream stopped by server");
                    OnConnectionError("Update stream stopped by server");
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Logger.Error(ex, "Update stream failed");
                OnConnectionError("Update stream failed");
            }
        }

        private async void HeartbeatTimerCallback(object state)
        {
            if (_heartbeatTimer == null)
                return;

            _heartbeatTimer.Change(-1, -1);
            try
            {
                await ExecuteUnaryRequestAuthorized(HeartbeatInternal, new Lib.HeartbeatRequest());
            }
            catch (Exception) { }
            _heartbeatTimer.Change(HeartbeatTimeout, -1);
        }

        private CallOptions GetCallOptions(bool setDeadline = true)
        {
            return !setDeadline ? new CallOptions()
                 : new CallOptions(deadline: DateTime.UtcNow.AddSeconds(DefaultRequestTimeout));
        }

        private CallOptions GetCallOptions(string accessToken, bool setDeadline = true)
        {
            return !setDeadline ? new CallOptions(credentials: AlgoGrpcCredentials.FromAccessToken(accessToken))
                 : new CallOptions(deadline: DateTime.UtcNow.AddSeconds(DefaultRequestTimeout), credentials: AlgoGrpcCredentials.FromAccessToken(accessToken));
        }

        private async Task<TResponse> ExecuteUnaryRequest<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<TResponse>> requestAction, TRequest request)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogServerRequest(Logger, request);
                var response = await requestAction(request, GetCallOptions());
                _messageFormatter.LogServerResponse(Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private async Task<TResponse> ExecuteUnaryRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<TResponse>> requestAction, TRequest request)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogServerRequest(Logger, request);
                var response = await requestAction(request, GetCallOptions(_accessToken));
                _messageFormatter.LogServerResponse(Logger, response);

                return response;
            }
            catch (UnauthorizedException uex)
            {
                Logger.Error(uex, $"Bad access token for {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private Task<IAsyncStreamReader<TResponse>> ExecuteServerStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<IAsyncStreamReader<TResponse>>> requestAction, TRequest request)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogServerRequest(Logger, request);
                return requestAction(request, GetCallOptions(_accessToken, false));
            }
            catch (UnauthorizedException uex)
            {
                Logger.Error(uex, $"Bad access token for {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private Task<AsyncClientStreamingCall<TRequest, TResponse>> ExecuteClientStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<AsyncClientStreamingCall<TRequest, TResponse>>> requestAction, TRequest request)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogServerRequest(Logger, request);
                return requestAction(request, GetCallOptions(_accessToken, false));
            }
            catch (UnauthorizedException uex)
            {
                Logger.Error(uex, $"Bad access token for {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }


        #region Grpc request calls

        private Task<Lib.LoginResponse> LoginInternal(Lib.LoginRequest request, CallOptions options)
        {
            return _client.LoginAsync(request, options).ResponseAsync;
        }

        private Task<Lib.LogoutResponse> LogoutInternal(Lib.LogoutRequest request, CallOptions options)
        {
            return _client.LogoutAsync(request, options).ResponseAsync;
        }

        private Task<Lib.HeartbeatResponse> HeartbeatInternal(Lib.HeartbeatRequest request, CallOptions options)
        {
            return _client.HeartbeatAsync(request, options).ResponseAsync;
        }

        private Task<Lib.SnapshotResponse> GetSnapshotInternal(Lib.SnapshotRequest request, CallOptions options)
        {
            return _client.GetSnapshotAsync(request, options).ResponseAsync;
        }

        private Task<IAsyncStreamReader<Lib.UpdateInfo>> SubscribeToUpdatesInternal(Lib.SubscribeToUpdatesRequest request, CallOptions options)
        {
            return Task.FromResult(_client.SubscribeToUpdates(request, options).ResponseStream);
        }

        private Task<Lib.ApiMetadataResponse> GetApiMetadataInternal(Lib.ApiMetadataRequest request, CallOptions options)
        {
            return _client.GetApiMetadataAsync(request, options).ResponseAsync;
        }

        private Task<Lib.MappingsInfoResponse> GetMappingsInfoInternal(Lib.MappingsInfoRequest request, CallOptions options)
        {
            return _client.GetMappingsInfoAsync(request, options).ResponseAsync;
        }

        private Task<Lib.SetupContextResponse> GetSetupContextInternal(Lib.SetupContextRequest request, CallOptions options)
        {
            return _client.GetSetupContextAsync(request, options).ResponseAsync;
        }

        private Task<Lib.AccountMetadataResponse> GetAccountMetadataInternal(Lib.AccountMetadataRequest request, CallOptions options)
        {
            return _client.GetAccountMetadataAsync(request, options).ResponseAsync;
        }

        private Task<Lib.BotListResponse> GetBotListInternal(Lib.BotListRequest request, CallOptions options)
        {
            return _client.GetBotListAsync(request, options).ResponseAsync;
        }

        private Task<Lib.AddBotResponse> AddBotInternal(Lib.AddBotRequest request, CallOptions options)
        {
            return _client.AddBotAsync(request, options).ResponseAsync;
        }

        private Task<Lib.RemoveBotResponse> RemoveBotInternal(Lib.RemoveBotRequest request, CallOptions options)
        {
            return _client.RemoveBotAsync(request, options).ResponseAsync;
        }

        private Task<Lib.StartBotResponse> StartBotInternal(Lib.StartBotRequest request, CallOptions options)
        {
            return _client.StartBotAsync(request, options).ResponseAsync;
        }

        private Task<Lib.StopBotResponse> StopBotInternal(Lib.StopBotRequest request, CallOptions options)
        {
            return _client.StopBotAsync(request, options).ResponseAsync;
        }

        private Task<Lib.ChangeBotConfigResponse> ChangeBotConfigInternal(Lib.ChangeBotConfigRequest request, CallOptions options)
        {
            return _client.ChangeBotConfigAsync(request, options).ResponseAsync;
        }

        private Task<Lib.AccountListResponse> GetAccountListInternal(Lib.AccountListRequest request, CallOptions options)
        {
            return _client.GetAccountListAsync(request, options).ResponseAsync;
        }

        private Task<Lib.AddAccountResponse> AddAccountInternal(Lib.AddAccountRequest request, CallOptions options)
        {
            return _client.AddAccountAsync(request, options).ResponseAsync;
        }

        private Task<Lib.RemoveAccountResponse> RemoveAccountInternal(Lib.RemoveAccountRequest request, CallOptions options)
        {
            return _client.RemoveAccountAsync(request, options).ResponseAsync;
        }

        private Task<Lib.ChangeAccountResponse> ChangeAccountInternal(Lib.ChangeAccountRequest request, CallOptions options)
        {
            return _client.ChangeAccountAsync(request, options).ResponseAsync;
        }

        private Task<Lib.TestAccountResponse> TestAccountInternal(Lib.TestAccountRequest request, CallOptions options)
        {
            return _client.TestAccountAsync(request, options).ResponseAsync;
        }

        private Task<Lib.TestAccountCredsResponse> TestAccountCredsInternal(Lib.TestAccountCredsRequest request, CallOptions options)
        {
            return _client.TestAccountCredsAsync(request, options).ResponseAsync;
        }

        private Task<Lib.PackageListResponse> GetPackageListInternal(Lib.PackageListRequest request, CallOptions options)
        {
            return _client.GetPackageListAsync(request, options).ResponseAsync;
        }

        private async Task<AsyncClientStreamingCall<Lib.UploadPackageRequest, Lib.UploadPackageResponse>> UploadPackageInternal(Lib.UploadPackageRequest request, CallOptions options)
        {
            var call = _client.UploadPackage(options);
            await call.RequestStream.WriteAsync(request);
            return call;
        }

        private Task<Lib.RemovePackageResponse> RemovePackageInternal(Lib.RemovePackageRequest request, CallOptions options)
        {
            return _client.RemovePackageAsync(request, options).ResponseAsync;
        }

        private Task<IAsyncStreamReader<Lib.DownloadPackageResponse>> DownloadPackageInternal(Lib.DownloadPackageRequest request, CallOptions options)
        {
            return Task.FromResult(_client.DownloadPackage(request, options).ResponseStream);
        }

        private Task<Lib.BotStatusResponse> GetBotStatusInternal(Lib.BotStatusRequest request, CallOptions options)
        {
            return _client.GetBotStatusAsync(request, options).ResponseAsync;
        }

        private Task<Lib.BotLogsResponse> GetBotLogsInternal(Lib.BotLogsRequest request, CallOptions options)
        {
            return _client.GetBotLogsAsync(request, options).ResponseAsync;
        }

        private Task<Lib.BotFolderInfoResponse> GetBotFolderInfoInternal(Lib.BotFolderInfoRequest request, CallOptions options)
        {
            return _client.GetBotFolderInfoAsync(request, options).ResponseAsync;
        }

        private Task<Lib.ClearBotFolderResponse> ClearBotFolderInternal(Lib.ClearBotFolderRequest request, CallOptions options)
        {
            return _client.ClearBotFolderAsync(request, options).ResponseAsync;
        }

        private Task<Lib.DeleteBotFileResponse> DeleteBotFileInternal(Lib.DeleteBotFileRequest request, CallOptions options)
        {
            return _client.DeleteBotFileAsync(request, options).ResponseAsync;
        }

        private Task<IAsyncStreamReader<Lib.DownloadBotFileResponse>> DownloadBotFileInternal(Lib.DownloadBotFileRequest request, CallOptions options)
        {
            return Task.FromResult(_client.DownloadBotFile(request, options).ResponseStream);
        }

        private async Task<AsyncClientStreamingCall<Lib.UploadBotFileRequest, Lib.UploadBotFileResponse>> UploadBotFileInternal(Lib.UploadBotFileRequest request, CallOptions options)
        {
            var call = _client.UploadBotFile(options);
            await call.RequestStream.WriteAsync(request);
            return call;
        }

        #endregion Grpc request calls


        #region Requests

        public override async Task<ApiMetadataInfo> GetApiMetadata()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetApiMetadataInternal, new Lib.ApiMetadataRequest());
            FailForNonSuccess(response.ExecResult);
            return response.ApiMetadata.Convert();
        }

        public override async Task<MappingCollectionInfo> GetMappingsInfo()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetMappingsInfoInternal, new Lib.MappingsInfoRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Mappings.Convert();
        }

        public override async Task<SetupContextInfo> GetSetupContext()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetSetupContextInternal, new Lib.SetupContextRequest());
            FailForNonSuccess(response.ExecResult);
            return response.SetupContext.Convert();
        }

        public override async Task<AccountMetadataInfo> GetAccountMetadata(AccountKey account)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, new Lib.AccountMetadataRequest { Account = account.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.AccountMetadata.Convert();
        }

        public override async Task<List<BotModelInfo>> GetBotList()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotListInternal, new Lib.BotListRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Bots.Select(ToAlgo.Convert).ToList();
        }

        public override async Task AddBot(AccountKey account, PluginConfig config)
        {
            var response = await ExecuteUnaryRequestAuthorized(AddBotInternal, new Lib.AddBotRequest { Account = account.Convert(), Config = config.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemoveBotInternal, new Lib.RemoveBotRequest { BotId = ToGrpc.Convert(botId), CleanLog = cleanLog, CleanAlgoData = cleanAlgoData });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task StartBot(string botId)
        {
            var response = await ExecuteUnaryRequestAuthorized(StartBotInternal, new Lib.StartBotRequest { BotId = botId });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task StopBot(string botId)
        {
            var response = await ExecuteUnaryRequestAuthorized(StopBotInternal, new Lib.StopBotRequest { BotId = botId });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task ChangeBotConfig(string botId, PluginConfig newConfig)
        {
            var response = await ExecuteUnaryRequestAuthorized(ChangeBotConfigInternal, new Lib.ChangeBotConfigRequest { BotId = ToGrpc.Convert(botId), NewConfig = newConfig.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<List<AccountModelInfo>> GetAccountList()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetAccountListInternal, new Lib.AccountListRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Accounts.Select(ToAlgo.Convert).ToList();
        }

        public override async Task AddAccount(AccountKey account, string password, bool useNewProtocol)
        {
            var response = await ExecuteUnaryRequestAuthorized(AddAccountInternal, new Lib.AddAccountRequest { Account = account.Convert(), Password = ToGrpc.Convert(password), UseNewProtocol = useNewProtocol });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemoveAccount(AccountKey account)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemoveAccountInternal, new Lib.RemoveAccountRequest { Account = account.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task ChangeAccount(AccountKey account, string password, bool useNewProtocol)
        {
            var response = await ExecuteUnaryRequestAuthorized(ChangeAccountInternal, new Lib.ChangeAccountRequest { Account = account.Convert(), Password = ToGrpc.Convert(password), UseNewProtocol = useNewProtocol });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            var response = await ExecuteUnaryRequestAuthorized(TestAccountInternal, new Lib.TestAccountRequest { Account = account.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo.Convert();
        }

        public override async Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password, bool useNewProtocol)
        {
            var response = await ExecuteUnaryRequestAuthorized(TestAccountCredsInternal, new Lib.TestAccountCredsRequest { Account = account.Convert(), Password = ToGrpc.Convert(password), UseNewProtocol = useNewProtocol });
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo.Convert();
        }

        public override async Task<List<PackageInfo>> GetPackageList()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetPackageListInternal, new Lib.PackageListRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Packages.Select(ToAlgo.Convert).ToList();
        }

        public override async Task UploadPackage(PackageKey package, string srcPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var request = new Lib.UploadPackageRequest
            {
                Package = new Lib.PackageDetails
                {
                    Key = package.Convert(),
                    ChunkSettings = new Lib.FileChunkSettings { Size = chunkSize, Offset = offset },
                }
            };

            var clientStream = await ExecuteClientStreamingRequestAuthorized(UploadPackageInternal, request);

            request.Chunk = new Lib.FileChunk { Id = 0, IsFinal = false };
            var buffer = new byte[chunkSize];
            try
            {
                using (var stream = System.IO.File.OpenRead(srcPath))
                {
                    stream.Seek((long)chunkSize * offset, System.IO.SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        request.Chunk.Binary = buffer.Convert(0, cnt);
                        _messageFormatter.LogClientRequest(Logger, request);
                        await clientStream.RequestStream.WriteAsync(request);
                        progressListener.IncrementProgress(cnt);
                        request.Chunk.Id++;
                    }
                }
            }
            finally
            {
                request.Chunk.Binary = ByteString.Empty;
                request.Chunk.IsFinal = true;
                request.Chunk.Id = -1;
                _messageFormatter.LogClientRequest(Logger, request);
                await clientStream.RequestStream.WriteAsync(request);
            }

            var response = await clientStream.ResponseAsync;
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemovePackage(PackageKey package)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemovePackageInternal, new Lib.RemovePackageRequest { Package = package.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task DownloadPackage(PackageKey package, string dstPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var fileReader = await ExecuteServerStreamingRequestAuthorized(DownloadPackageInternal,
                new Lib.DownloadPackageRequest
                {
                    Package = new Lib.PackageDetails
                    {
                        Key = package.Convert(),
                        ChunkSettings = new Lib.FileChunkSettings { Size = chunkSize, Offset = offset },
                    }
                });

            var buffer = new byte[chunkSize];
            using (var stream = System.IO.File.OpenWrite(dstPath))
            {
                stream.Seek((long)chunkSize * offset, System.IO.SeekOrigin.Begin);
                while (await fileReader.MoveNext())
                {
                    var response = fileReader.Current;
                    _messageFormatter.LogServerResponse(Logger, response);
                    FailForNonSuccess(response.ExecResult);
                    if (!response.Chunk.Binary.IsEmpty)
                    {
                        response.Chunk.Binary.CopyTo(buffer, 0);
                        progressListener.IncrementProgress(response.Chunk.Binary.Length);
                        stream.Write(buffer, 0, response.Chunk.Binary.Length);
                    }
                    if (response.Chunk.IsFinal)
                        break;
                }
            }
        }

        public override async Task<string> GetBotStatus(string botId)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotStatusInternal, new Lib.BotStatusRequest { BotId = ToGrpc.Convert(botId) });
            FailForNonSuccess(response.ExecResult);
            return response.Status;
        }

        public override async Task<LogRecordInfo[]> GetBotLogs(string botId, DateTime lastLogTimeUtc, int maxCount)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotLogsInternal, new Lib.BotLogsRequest { BotId = ToGrpc.Convert(botId), LastLogTimeUtc = lastLogTimeUtc.Convert(), MaxCount = maxCount });
            FailForNonSuccess(response.ExecResult);
            return response.Logs.Select(ToAlgo.Convert).ToArray();
        }

        public override async Task<BotFolderInfo> GetBotFolderInfo(string botId, BotFolderId folderId)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotFolderInfoInternal, new Lib.BotFolderInfoRequest { BotId = ToGrpc.Convert(botId), FolderId = folderId.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.FolderInfo.Convert();
        }

        public override async Task ClearBotFolder(string botId, BotFolderId folderId)
        {
            var response = await ExecuteUnaryRequestAuthorized(ClearBotFolderInternal, new Lib.ClearBotFolderRequest { BotId = ToGrpc.Convert(botId), FolderId = folderId.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task DeleteBotFile(string botId, BotFolderId folderId, string fileName)
        {
            var response = await ExecuteUnaryRequestAuthorized(DeleteBotFileInternal, new Lib.DeleteBotFileRequest { BotId = ToGrpc.Convert(botId), FolderId = folderId.Convert(), FileName = ToGrpc.Convert(fileName) });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task DownloadBotFile(string botId, BotFolderId folderId, string fileName, string dstPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var fileReader = await ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal,
                new Lib.DownloadBotFileRequest
                {
                    File = new Lib.BotFileDetails
                    {
                        BotId = ToGrpc.Convert(botId),
                        FolderId = folderId.Convert(),
                        FileName = ToGrpc.Convert(fileName),
                        ChunkSettings = new Lib.FileChunkSettings { Size = chunkSize, Offset = offset },
                    }
                });

            var buffer = new byte[chunkSize];
            using (var stream = System.IO.File.OpenWrite(dstPath))
            {
                stream.Seek((long)chunkSize * offset, System.IO.SeekOrigin.Begin);
                while (await fileReader.MoveNext())
                {
                    var response = fileReader.Current;
                    _messageFormatter.LogServerResponse(Logger, response);
                    FailForNonSuccess(response.ExecResult);
                    if (!response.Chunk.Binary.IsEmpty)
                    {
                        response.Chunk.Binary.CopyTo(buffer, 0);
                        progressListener.IncrementProgress(response.Chunk.Binary.Length);
                        stream.Write(buffer, 0, response.Chunk.Binary.Length);
                    }
                    if (response.Chunk.IsFinal)
                        break;
                }
            }
        }

        public override async Task UploadBotFile(string botId, BotFolderId folderId, string fileName, string srcPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var request = new Lib.UploadBotFileRequest
            {
                File = new Lib.BotFileDetails
                {
                    BotId = ToGrpc.Convert(botId),
                    FolderId = folderId.Convert(),
                    FileName = ToGrpc.Convert(fileName),
                    ChunkSettings = new Lib.FileChunkSettings { Size = chunkSize, Offset = offset },
                }
            };

            var clientStream = await ExecuteClientStreamingRequestAuthorized(UploadBotFileInternal, request);

            request.Chunk = new Lib.FileChunk { Id = 0, IsFinal = false };
            var buffer = new byte[chunkSize];
            try
            {
                using (var stream = System.IO.File.OpenRead(srcPath))
                {
                    stream.Seek((long)chunkSize * offset, System.IO.SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        request.Chunk.Binary = buffer.Convert(0, cnt);
                        _messageFormatter.LogClientRequest(Logger, request);
                        await clientStream.RequestStream.WriteAsync(request);
                        progressListener.IncrementProgress(cnt);
                        request.Chunk.Id++;
                    }
                }
            }
            finally
            {
                request.Chunk.Binary = ByteString.Empty;
                request.Chunk.IsFinal = true;
                request.Chunk.Id = -1;
                _messageFormatter.LogClientRequest(Logger, request);
                await clientStream.RequestStream.WriteAsync(request);
            }

            var response = await clientStream.ResponseAsync;
            FailForNonSuccess(response.ExecResult);
        }

        #endregion Requests
    }
}
