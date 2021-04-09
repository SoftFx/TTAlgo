using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl.Grpc
{
    public class GrpcClient : ProtocolClient
    {
        private const int HeartbeatTimeout = 10000;

        private MessageFormatter _messageFormatter;
        private Channel _channel;
        private AlgoServerPublic.AlgoServerPublicClient _client;
        private string _accessToken;
        private CancellationTokenSource _updateStreamCancelTokenSrc;
        private Timer _heartbeatTimer;


        static GrpcClient()
        {
            CertificateProvider.InitClient();
        }


        public GrpcClient(IAlgoServerClient algoClient) : base(algoClient)
        {
            _messageFormatter = new MessageFormatter();
        }


        protected override void StartClient()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            var creds = new SslCredentials(CertificateProvider.RootCertificate); //, new KeyCertificatePair(CertificateProvider.ClientCertificate, CertificateProvider.ClientKey));
            var options = new[] { new ChannelOption(ChannelOptions.SslTargetNameOverride, "bot-agent.soft-fx.lv"), };
            _channel = new Channel(SessionSettings.ServerAddress, SessionSettings.ServerPort, creds, options);
            _accessToken = "";
            _messageFormatter.LogMessages = SessionSettings.LogMessages;

            _client = new AlgoServerPublic.AlgoServerPublicClient(_channel);

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
            ExecuteUnaryRequest(LoginInternal, new LoginRequest
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
                        var taskResult = t.Result;
                        if (taskResult.Error == LoginResponse.Types.LoginError.None)
                        {
                            _accessToken = taskResult.AccessToken;
                            Logger.Info($"Server session id: {taskResult.SessionId}");
                            OnLogin(taskResult.MajorVersion, taskResult.MinorVersion, taskResult.AccessLevel);
                            _heartbeatTimer = new Timer(HeartbeatTimerCallback, null, HeartbeatTimeout, -1);
                        }
                        else
                        {
                            OnConnectionError(taskResult.Error.ToString());
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
            ExecuteUnaryRequestAuthorized(GetSnapshotInternal, new SnapshotRequest())
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
            ExecuteUnaryRequestAuthorized(LogoutInternal, new LogoutRequest())
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


        private bool FailForNonSuccess(RequestResult.Types.RequestStatus status)
        {
            return status != RequestResult.Types.RequestStatus.Success;
        }

        private void FailForNonSuccess(RequestResult requestResult)
        {
            if (FailForNonSuccess(requestResult.Status))
                throw requestResult.Status == RequestResult.Types.RequestStatus.Unauthorized
                    ? new UnauthorizedException(requestResult.Message)
                    : new AlgoException($"{requestResult.Status} - {requestResult.Message}");
        }

        private void ApplySnapshot(SnapshotResponse snapshot)
        {
            try
            {
                FailForNonSuccess(snapshot.ExecResult);

                var apiMetadata = snapshot.ApiMetadata;
                FailForNonSuccess(apiMetadata.ExecResult);
                AlgoClient.SetApiMetadata(apiMetadata.ApiMetadata);

                var mappings = snapshot.MappingsInfo;
                FailForNonSuccess(mappings.ExecResult);
                AlgoClient.SetMappingsInfo(mappings.Mappings);

                var setupContext = snapshot.SetupContext;
                FailForNonSuccess(setupContext.ExecResult);
                AlgoClient.SetSetupContext(setupContext.SetupContext);

                var packages = snapshot.PackageList;
                FailForNonSuccess(packages.ExecResult);
                AlgoClient.InitPackageList(packages.Packages.ToList());

                var accounts = snapshot.AccountList;
                FailForNonSuccess(accounts.ExecResult);
                AlgoClient.InitAccountList(accounts.Accounts.ToList());

                var bots = snapshot.PluginList;
                FailForNonSuccess(bots.ExecResult);
                AlgoClient.InitBotList(bots.Plugins.ToList());
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

            ExecuteServerStreamingRequestAuthorized(SubscribeToUpdatesInternal, new SubscribeToUpdatesRequest())
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

        private async void ListenToUpdates(AsyncServerStreamingCall<UpdateInfo> updateCall)
        {
            _updateStreamCancelTokenSrc = new CancellationTokenSource();
            try
            {
                var updateStream = updateCall.ResponseStream;
                while (await updateStream.MoveNext(_updateStreamCancelTokenSrc.Token))
                {
                    var update = updateStream.Current;
                    if (update.TryUnpack(out var updateInfo))
                    {
                        _messageFormatter.LogClientUpdate(Logger, updateInfo);
                        if (updateInfo is UpdateInfo<PackageInfo>)
                            AlgoClient.UpdatePackage(updateInfo.Type, ((UpdateInfo<PackageInfo>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<PackageStateUpdate>)
                            AlgoClient.UpdatePackageState(((UpdateInfo<PackageStateUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<AccountModelInfo>)
                            AlgoClient.UpdateAccount(updateInfo.Type, ((UpdateInfo<AccountModelInfo>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<AccountStateUpdate>)
                            AlgoClient.UpdateAccountState(((UpdateInfo<AccountStateUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<PluginModelInfo>)
                            AlgoClient.UpdateBot(updateInfo.Type, ((UpdateInfo<PluginModelInfo>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<PluginStateUpdate>)
                            AlgoClient.UpdateBotState(((UpdateInfo<PluginStateUpdate>)updateInfo).Value);
                        else Logger.Error($"Failed to dispatch update of type: {update.Payload.TypeUrl}");
                    }
                    else
                    {
                        Logger.Error($"Failed to unpack update of type: {update.Payload.TypeUrl}");
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
            finally
            {
                updateCall.Dispose();
            }
        }

        private async void HeartbeatTimerCallback(object state)
        {
            if (_heartbeatTimer == null)
                return;

            _heartbeatTimer?.Change(-1, -1);
            try
            {
                await ExecuteUnaryRequestAuthorized(HeartbeatInternal, new HeartbeatRequest());
            }
            catch (Exception) { }
            _heartbeatTimer?.Change(HeartbeatTimeout, -1);
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
            catch (RpcException rex)
            {
                if (rex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    Logger.Error($"Request timed out {_messageFormatter.ToJson(request)}");
                    throw new TimeoutException($"Request {nameof(TRequest)} timed out");
                }
                else if (rex.StatusCode == StatusCode.Unknown && rex.Status.Detail == "Stream removed")
                {
                    Logger.Error($"Disconnected while executing {_messageFormatter.ToJson(request)}");
                    throw new AlgoException("Connection error");
                }
                Logger.Error(rex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
        }

        private Task<AsyncServerStreamingCall<TResponse>> ExecuteServerStreamingRequestAuthorized<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<AsyncServerStreamingCall<TResponse>>> requestAction, TRequest request)
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

        private Task<LoginResponse> LoginInternal(LoginRequest request, CallOptions options)
        {
            return _client.LoginAsync(request, options).ResponseAsync;
        }

        private Task<LogoutResponse> LogoutInternal(LogoutRequest request, CallOptions options)
        {
            return _client.LogoutAsync(request, options).ResponseAsync;
        }

        private Task<HeartbeatResponse> HeartbeatInternal(HeartbeatRequest request, CallOptions options)
        {
            return _client.HeartbeatAsync(request, options).ResponseAsync;
        }

        private Task<SnapshotResponse> GetSnapshotInternal(SnapshotRequest request, CallOptions options)
        {
            return _client.GetSnapshotAsync(request, options).ResponseAsync;
        }

        private Task<AsyncServerStreamingCall<UpdateInfo>> SubscribeToUpdatesInternal(SubscribeToUpdatesRequest request, CallOptions options)
        {
            return Task.FromResult(_client.SubscribeToUpdates(request, options));
        }

        private Task<ApiMetadataResponse> GetApiMetadataInternal(ApiMetadataRequest request, CallOptions options)
        {
            return _client.GetApiMetadataAsync(request, options).ResponseAsync;
        }

        private Task<MappingsInfoResponse> GetMappingsInfoInternal(MappingsInfoRequest request, CallOptions options)
        {
            return _client.GetMappingsInfoAsync(request, options).ResponseAsync;
        }

        private Task<SetupContextResponse> GetSetupContextInternal(SetupContextRequest request, CallOptions options)
        {
            return _client.GetSetupContextAsync(request, options).ResponseAsync;
        }

        private Task<AccountMetadataResponse> GetAccountMetadataInternal(AccountMetadataRequest request, CallOptions options)
        {
            return _client.GetAccountMetadataAsync(request, options).ResponseAsync;
        }

        private Task<PluginListResponse> GetBotListInternal(PluginListRequest request, CallOptions options)
        {
            return _client.GetPluginListAsync(request, options).ResponseAsync;
        }

        private Task<AddPluginResponse> AddBotInternal(AddPluginRequest request, CallOptions options)
        {
            return _client.AddPluginAsync(request, options).ResponseAsync;
        }

        private Task<RemovePluginResponse> RemoveBotInternal(RemovePluginRequest request, CallOptions options)
        {
            return _client.RemovePluginAsync(request, options).ResponseAsync;
        }

        private Task<StartPluginResponse> StartBotInternal(StartPluginRequest request, CallOptions options)
        {
            return _client.StartPluginAsync(request, options).ResponseAsync;
        }

        private Task<StopPluginResponse> StopBotInternal(StopPluginRequest request, CallOptions options)
        {
            return _client.StopPluginAsync(request, options).ResponseAsync;
        }

        private Task<ChangePluginConfigResponse> ChangeBotConfigInternal(ChangePluginConfigRequest request, CallOptions options)
        {
            return _client.ChangePluginConfigAsync(request, options).ResponseAsync;
        }

        private Task<AccountListResponse> GetAccountListInternal(AccountListRequest request, CallOptions options)
        {
            return _client.GetAccountListAsync(request, options).ResponseAsync;
        }

        private Task<AddAccountResponse> AddAccountInternal(AddAccountRequest request, CallOptions options)
        {
            return _client.AddAccountAsync(request, options).ResponseAsync;
        }

        private Task<RemoveAccountResponse> RemoveAccountInternal(RemoveAccountRequest request, CallOptions options)
        {
            return _client.RemoveAccountAsync(request, options).ResponseAsync;
        }

        private Task<ChangeAccountResponse> ChangeAccountInternal(ChangeAccountRequest request, CallOptions options)
        {
            return _client.ChangeAccountAsync(request, options).ResponseAsync;
        }

        private Task<TestAccountResponse> TestAccountInternal(TestAccountRequest request, CallOptions options)
        {
            return _client.TestAccountAsync(request, options).ResponseAsync;
        }

        private Task<TestAccountCredsResponse> TestAccountCredsInternal(TestAccountCredsRequest request, CallOptions options)
        {
            return _client.TestAccountCredsAsync(request, options).ResponseAsync;
        }

        private Task<PackageListResponse> GetPackageListInternal(PackageListRequest request, CallOptions options)
        {
            return _client.GetPackageListAsync(request, options).ResponseAsync;
        }

        private async Task<AsyncClientStreamingCall<UploadPackageRequest, UploadPackageResponse>> UploadPackageInternal(UploadPackageRequest request, CallOptions options)
        {
            var call = _client.UploadPackage(options);
            await call.RequestStream.WriteAsync(request);
            return call;
        }

        private Task<RemovePackageResponse> RemovePackageInternal(RemovePackageRequest request, CallOptions options)
        {
            return _client.RemovePackageAsync(request, options).ResponseAsync;
        }

        private Task<AsyncServerStreamingCall<DownloadPackageResponse>> DownloadPackageInternal(DownloadPackageRequest request, CallOptions options)
        {
            return Task.FromResult(_client.DownloadPackage(request, options));
        }

        private Task<PluginStatusResponse> GetBotStatusInternal(PluginStatusRequest request, CallOptions options)
        {
            return _client.GetPluginStatusAsync(request, options).ResponseAsync;
        }

        private Task<PluginLogsResponse> GetBotLogsInternal(PluginLogsRequest request, CallOptions options)
        {
            return _client.GetPluginLogsAsync(request, options).ResponseAsync;
        }

        private Task<PluginAlertsResponse> GetAlertsInternal(PluginAlertsRequest request, CallOptions options)
        {
            return _client.GetAlertsAsync(request, options).ResponseAsync;
        }

        private Task<PluginFolderInfoResponse> GetBotFolderInfoInternal(PluginFolderInfoRequest request, CallOptions options)
        {
            return _client.GetPluginFolderInfoAsync(request, options).ResponseAsync;
        }

        private Task<ClearPluginFolderResponse> ClearBotFolderInternal(ClearPluginFolderRequest request, CallOptions options)
        {
            return _client.ClearPluginFolderAsync(request, options).ResponseAsync;
        }

        private Task<DeletePluginFileResponse> DeleteBotFileInternal(DeletePluginFileRequest request, CallOptions options)
        {
            return _client.DeletePluginFileAsync(request, options).ResponseAsync;
        }

        private Task<AsyncServerStreamingCall<DownloadPluginFileResponse>> DownloadBotFileInternal(DownloadPluginFileRequest request, CallOptions options)
        {
            return Task.FromResult(_client.DownloadPluginFile(request, options));
        }

        private async Task<AsyncClientStreamingCall<UploadPluginFileRequest, UploadPluginFileResponse>> UploadBotFileInternal(UploadPluginFileRequest request, CallOptions options)
        {
            var call = _client.UploadPluginFile(options);
            await call.RequestStream.WriteAsync(request);
            return call;
        }

        #endregion Grpc request calls


        #region Requests

        public override async Task<ApiMetadataInfo> GetApiMetadata()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetApiMetadataInternal, new ApiMetadataRequest());
            FailForNonSuccess(response.ExecResult);
            return response.ApiMetadata;
        }

        public override async Task<MappingCollectionInfo> GetMappingsInfo()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetMappingsInfoInternal, new MappingsInfoRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Mappings;
        }

        public override async Task<SetupContextInfo> GetSetupContext()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetSetupContextInternal, new SetupContextRequest());
            FailForNonSuccess(response.ExecResult);
            return response.SetupContext;
        }

        public override async Task<AccountMetadataInfo> GetAccountMetadata(string accountId)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, new AccountMetadataRequest { AccountId = accountId });
            FailForNonSuccess(response.ExecResult);
            return response.AccountMetadata;
        }

        public override async Task<List<PluginModelInfo>> GetPluginList()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotListInternal, new PluginListRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Plugins.ToList();
        }

        public override async Task AddPlugin(AddPluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(AddBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemovePlugin(RemovePluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemoveBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task StartPlugin(StartPluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(StartBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task StopPlugin(StopPluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(StopBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task ChangePluginConfig(ChangePluginConfigRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(ChangeBotConfigInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<List<AccountModelInfo>> GetAccountList()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetAccountListInternal, new AccountListRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Accounts.ToList();
        }

        public override async Task AddAccount(AddAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(AddAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemoveAccount(RemoveAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemoveAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task ChangeAccount(ChangeAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(ChangeAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(TestAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo;
        }

        public override async Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(TestAccountCredsInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo;
        }

        public override async Task<List<PackageInfo>> GetPackageList()
        {
            var response = await ExecuteUnaryRequestAuthorized(GetPackageListInternal, new PackageListRequest());
            FailForNonSuccess(response.ExecResult);
            return response.Packages.ToList();
        }

        public override async Task UploadPackage(string packageId, string srcPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            if (_client == null || _channel.State == ChannelState.Shutdown)
                throw new ConnectionFailedException("Connection failed");

            progressListener.Init((long)offset * chunkSize);

            var request = new UploadPackageRequest
            {
                Package = new PackageDetails
                {
                    PackageId = packageId,
                    ChunkSettings = new FileChunkSettings { Size = chunkSize, Offset = offset },
                }
            };

            var clientStream = await ExecuteClientStreamingRequestAuthorized(UploadPackageInternal, request);

            request.Chunk = new FileChunk { Id = offset, IsFinal = false };
            var buffer = new byte[chunkSize];
            try
            {
                using (var stream = File.Open(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)chunkSize * offset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        request.Chunk.Binary = ByteString.CopyFrom(buffer, 0, cnt);
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

        public override async Task RemovePackage(RemovePackageRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemovePackageInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task DownloadPackage(string packageId, string dstPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var buffer = new byte[chunkSize];
            string oldDstPath = null;
            if (File.Exists(dstPath))
            {
                oldDstPath = $"{dstPath}.old";
                File.Move(dstPath, oldDstPath);
            }
            using (var stream = File.Open(dstPath, FileMode.Create, FileAccess.ReadWrite))
            {
                if (oldDstPath != null)
                {
                    using (var oldStream = File.Open(oldDstPath, FileMode.Open, FileAccess.Read))
                    {
                        for (var chunkId = 0; chunkId < offset; chunkId++)
                        {
                            var bytesRead = oldStream.Read(buffer, 0, chunkSize);
                            for (var i = bytesRead; i < chunkSize; i++) buffer[i] = 0;
                            stream.Write(buffer, 0, chunkSize);
                        }
                    }
                    File.Delete(oldDstPath);
                }
                else
                {
                    for (var chunkId = 0; chunkId < offset; chunkId++)
                    {
                        stream.Write(buffer, 0, chunkSize);
                    }
                }

                var serverCall = await ExecuteServerStreamingRequestAuthorized(DownloadPackageInternal,
                new DownloadPackageRequest
                {
                    Package = new PackageDetails
                    {
                        PackageId = packageId,
                        ChunkSettings = new FileChunkSettings { Size = chunkSize, Offset = offset },
                    }
                });
                var fileReader = serverCall.ResponseStream;
                try
                {
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
                finally
                {
                    serverCall.Dispose();
                }
            }
        }

        public override async Task<string> GetPluginStatus(PluginStatusRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotStatusInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.Status;
        }

        public override async Task<LogRecordInfo[]> GetPluginLogs(PluginLogsRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotLogsInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.Logs.ToArray();
        }

        public override async Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request)
        {
            if (!VersionSpec.SupportAlerts)
                return new AlertRecordInfo[0];
            var response = await ExecuteUnaryRequestAuthorized(GetAlertsInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.Alerts.ToArray();
        }

        public override async Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotFolderInfoInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.FolderInfo;
        }

        public override async Task ClearPluginFolder(ClearPluginFolderRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(ClearBotFolderInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task DeletePluginFile(DeletePluginFileRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(DeleteBotFileInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task DownloadPluginFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var buffer = new byte[chunkSize];
            string oldDstPath = null;
            if (File.Exists(dstPath))
            {
                oldDstPath = $"{dstPath}.old";
                File.Move(dstPath, oldDstPath);
            }
            using (var stream = File.Open(dstPath, FileMode.Create, FileAccess.ReadWrite))
            {
                if (oldDstPath != null)
                {
                    using (var oldStream = File.Open(oldDstPath, FileMode.Open, FileAccess.Read))
                    {
                        for (var chunkId = 0; chunkId < offset; chunkId++)
                        {
                            var bytesRead = oldStream.Read(buffer, 0, chunkSize);
                            for (var i = bytesRead; i < chunkSize; i++) buffer[i] = 0;
                            stream.Write(buffer, 0, chunkSize);
                        }
                    }
                    File.Delete(oldDstPath);
                }
                else
                {
                    for (var chunkId = 0; chunkId < offset; chunkId++)
                    {
                        stream.Write(buffer, 0, chunkSize);
                    }
                }

                var serverCall = await ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal,
                new DownloadPluginFileRequest
                {
                    File = new PluginFileDetails
                    {
                        PluginId = botId,
                        FolderId = folderId,
                        FileName = fileName,
                        ChunkSettings = new FileChunkSettings { Size = chunkSize, Offset = offset },
                    }
                });
                var fileReader = serverCall.ResponseStream;
                try
                {
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
                finally
                {
                    serverCall.Dispose();
                }
            }
        }

        public override async Task UploadPluginFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, int chunkSize, int offset, IFileProgressListener progressListener)
        {
            progressListener.Init((long)offset * chunkSize);

            var request = new UploadPluginFileRequest
            {
                File = new PluginFileDetails
                {
                    PluginId = botId,
                    FolderId = folderId,
                    FileName = fileName,
                    ChunkSettings = new FileChunkSettings { Size = chunkSize, Offset = offset },
                }
            };

            var clientStream = await ExecuteClientStreamingRequestAuthorized(UploadBotFileInternal, request);

            request.Chunk = new FileChunk { Id = offset, IsFinal = false };
            var buffer = new byte[chunkSize];
            try
            {
                using (var stream = File.Open(srcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek((long)chunkSize * offset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        request.Chunk.Binary = ByteString.CopyFrom(buffer, 0, cnt);
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
