using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.Server.Common.Grpc;

namespace TickTrader.Algo.Server.PublicAPI
{
    public sealed class AlgoServerClient : ProtocolClient, IAlgoServerClient
    {
        //private const int HeartbeatTimeout = 10000;
        private const int DefaultRequestTimeout = 10;

        public ClientStates State => _stateMachine.Current;

        //public string LastError => throw new System.NotImplementedException();

        //public IVersionSpec VersionSpec => throw new System.NotImplementedException();

        //public IAccessManager AccessManager => throw new System.NotImplementedException();


        private readonly MessageFormatter _messageFormatter;
        private Channel _channel;
        private AlgoServerPublic.AlgoServerPublicClient _client;
        private string _accessToken;
        private CancellationTokenSource _updateStreamCancelTokenSrc;


        static AlgoServerClient()
        {
            CertificateProvider.InitClient(Assembly.GetExecutingAssembly(), "TickTrader.Algo.Server.PublicAPI.certs.algo-ca.crt");
        }

        private AlgoServerClient(IAlgoServerEventHandler handler) : base(handler)
        {
            _messageFormatter = new MessageFormatter(AlgoServerPublicAPIReflection.Descriptor);

            VersionSpec = new ApiVersionSpec();
            AccessManager = new ApiAccessManager(ClientClaims.Types.AccessLevel.Anonymous);
        }

        public static IAlgoServerClient Create(IAlgoServerEventHandler handler) => new AlgoServerClient(handler);


        public override void StartClient()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));

            var creds = new SslCredentials(CertificateProvider.RootCertificate);
            var options = new[] { new ChannelOption(ChannelOptions.SslTargetNameOverride, "bot-agent.soft-fx.lv"), };

            _channel = new Channel(SessionSettings.ServerAddress, SessionSettings.ServerPort, creds, options);
            _client = new AlgoServerPublic.AlgoServerPublicClient(_channel);

            _messageFormatter.LogMessages = SessionSettings.LogMessages;
            _accessToken = string.Empty;


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

        public override void StopClient()
        {
            _messageFormatter.LogMessages = false;

            _updateStreamCancelTokenSrc?.Cancel();
            _updateStreamCancelTokenSrc = null;

            _channel.ShutdownAsync().Wait();
        }

        public override void SendLogin()
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
                        {
                            var taskResult = t.Result;

                            if (taskResult.Error == LoginResponse.Types.LoginError.None)
                            {
                                _accessToken = taskResult.AccessToken;
                                Logger.Info($"Server session id: {taskResult.SessionId}");
                                OnLogin(taskResult.MajorVersion, taskResult.MinorVersion, taskResult.AccessLevel);
                                //_heartbeatTimer = new Timer(HeartbeatTimerCallback, null, HeartbeatTimeout, -1);
                            }
                            else
                                OnConnectionError(taskResult.Error.ToString());

                            break;
                        }
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

        public override void Init()
        {
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

        public override void SendLogout()
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

        private void ApplyAlgoServerMetadata(AlgoServerMetadataUpdate snapshot)
        {
            try
            {
                FailForNonSuccess(snapshot.ExecResult);

                _serverHandler.SetApiMetadata(snapshot.ApiMetadata);
                _serverHandler.SetMappingsInfo(snapshot.MappingsCollection);
                _serverHandler.SetSetupContext(snapshot.SetupContext);

                _serverHandler.InitPackageList(snapshot.Packages.ToList());
                _serverHandler.InitAccountList(snapshot.Accounts.ToList());
                _serverHandler.InitBotList(snapshot.Plugins.ToList());
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
        }


        public override void SendDisconnect()
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
                    : new AlgoServerException($"{requestResult.Status} - {requestResult.Message}");
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
                        //_messageFormatter.LogClientUpdate(Logger, updateInfo);
                        if (updateInfo is UpdateInfo<AlgoServerMetadataUpdate>)
                            ApplyAlgoServerMetadata(((UpdateInfo<AlgoServerMetadataUpdate>)updateInfo).Value);

                        else if (updateInfo is UpdateInfo<PackageUpdate>)
                            _serverHandler.OnPackageUpdate(((UpdateInfo<PackageUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<AccountModelUpdate>)
                            _serverHandler.OnAccountUpdate(((UpdateInfo<AccountModelUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<PluginModelUpdate>)
                            _serverHandler.OnPluginModelUpdate(((UpdateInfo<PluginModelUpdate>)updateInfo).Value);

                        else if (updateInfo is UpdateInfo<PackageStateUpdate>)
                            _serverHandler.OnPackageStateUpdate(((UpdateInfo<PackageStateUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<AccountStateUpdate>)
                            _serverHandler.OnAccountStateUpdate(((UpdateInfo<AccountStateUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<PluginStateUpdate>)
                            _serverHandler.OnPluginStateUpdate(((UpdateInfo<PluginStateUpdate>)updateInfo).Value);

                        else if (updateInfo is UpdateInfo<PluginStatusUpdate>)
                            _serverHandler.OnPluginStatusUpdate(((UpdateInfo<PluginStatusUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<PluginLogUpdate>)
                            _serverHandler.OnPluginLogUpdate(((UpdateInfo<PluginLogUpdate>)updateInfo).Value);
                        else if (updateInfo is UpdateInfo<AlertListUpdate>)
                            _serverHandler.OnAlertListUpdate(((UpdateInfo<AlertListUpdate>)updateInfo).Value);

                        else if (updateInfo is UpdateInfo<HeartbeatUpdate>)
                            break;
                        else
                            Logger.Error($"Failed to dispatch update of type: {update.Payload.TypeUrl}");
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
                    throw new Common.TimeoutException($"Request {nameof(TRequest)} timed out");
                }
                else if (rex.StatusCode == StatusCode.Unknown && rex.Status.Detail == "Stream removed")
                {
                    Logger.Error($"Disconnected while executing {_messageFormatter.ToJson(request)}");
                    throw new AlgoServerException("Connection error");
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

        private Task<AccountMetadataResponse> GetAccountMetadataInternal(AccountMetadataRequest request, CallOptions options)
        {
            return _client.GetAccountMetadataAsync(request, options).ResponseAsync;
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

        private async Task<AsyncClientStreamingCall<FileTransferMsg, UploadPackageResponse>> UploadPackageInternal(FileTransferMsg request, CallOptions options)
        {
            var call = _client.UploadPackage(options);
            await call.RequestStream.WriteAsync(request);
            return call;
        }

        private Task<RemovePackageResponse> RemovePackageInternal(RemovePackageRequest request, CallOptions options)
        {
            return _client.RemovePackageAsync(request, options).ResponseAsync;
        }

        private Task<AsyncServerStreamingCall<FileTransferMsg>> DownloadPackageInternal(DownloadPackageRequest request, CallOptions options)
        {
            return Task.FromResult(_client.DownloadPackage(request, options));
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

        private Task<AsyncServerStreamingCall<FileTransferMsg>> DownloadBotFileInternal(DownloadPluginFileRequest request, CallOptions options)
        {
            return Task.FromResult(_client.DownloadPluginFile(request, options));
        }

        private async Task<AsyncClientStreamingCall<FileTransferMsg, UploadPluginFileResponse>> UploadBotFileInternal(FileTransferMsg request, CallOptions options)
        {
            var call = _client.UploadPluginFile(options);
            await call.RequestStream.WriteAsync(request);
            return call;
        }

        private Task<AlertListSubscribeResponse> SubscribeToAlertListInternal(AlertListSubscribeRequest request, CallOptions options)
        {
            return _client.SubscribeToAlertListAsync(request, options).ResponseAsync;
        }

        private Task<PluginStatusSubscribeResponse> SubscribeToPluginStatusInternal(PluginStatusSubscribeRequest request, CallOptions options)
        {
            return _client.SubscribeToPluginStatusAsync(request, options).ResponseAsync;
        }

        private Task<PluginLogsSubscribeResponse> SubscribeToPluginLogsInternal(PluginLogsSubscribeRequest request, CallOptions options)
        {
            return _client.SubscribeToPluginLogsAsync(request, options).ResponseAsync;
        }

        private Task<AlertListUnsubscribeResponse> UnsubscribeToAlertListInternal(AlertListUnsubscribeRequest request, CallOptions options)
        {
            return _client.UnsubscribeToAlertListAsync(request, options).ResponseAsync;
        }

        private Task<PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatusInternal(PluginStatusUnsubscribeRequest request, CallOptions options)
        {
            return _client.UnsubscribeToPluginStatusAsync(request, options).ResponseAsync;
        }

        private Task<PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogsInternal(PluginLogsUnsubscribeRequest request, CallOptions options)
        {
            return _client.UnsubscribeToPluginLogsAsync(request, options).ResponseAsync;
        }

        private Task<AsyncServerStreamingCall<UpdateInfo>> SubscribeToUpdatesInternal(SubscribeToUpdatesRequest request, CallOptions options)
        {
            return Task.FromResult(_client.SubscribeToUpdates(request, options));
        }
        #endregion Grpc request calls


        #region Requests

        public async Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetAccountMetadataInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.AccountMetadata;
        }

        public async Task SubscribeToAlertList(AlertListSubscribeRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(SubscribeToAlertListInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task SubscribeToPluginStatus(PluginStatusSubscribeRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(SubscribeToPluginStatusInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task SubscribeToPluginLogs(PluginLogsSubscribeRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(SubscribeToPluginLogsInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task UnsubscribeToAlertList(AlertListUnsubscribeRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(UnsubscribeToAlertListInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task UnsubscribeToPluginStatus(PluginStatusUnsubscribeRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(UnsubscribeToPluginStatusInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task UnsubscribeToPluginLogs(PluginLogsUnsubscribeRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(UnsubscribeToPluginLogsInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task AddPlugin(AddPluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(AddBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task RemovePlugin(RemovePluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemoveBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task StartPlugin(StartPluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(StartBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task StopPlugin(StopPluginRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(StopBotInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task ChangePluginConfig(ChangePluginConfigRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(ChangeBotConfigInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task AddAccount(AddAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(AddAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task RemoveAccount(RemoveAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemoveAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task ChangeAccount(ChangeAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(ChangeAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(TestAccountInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo;
        }

        public async Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(TestAccountCredsInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo;
        }

        public async Task UploadPackage(UploadPackageRequest request, string srcPath, IFileProgressListener progressListener)
        {
            if (_client == null || _channel.State == ChannelState.Shutdown)
                throw new ConnectionFailedException("Connection failed");

            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            progressListener.Init((long)chunkOffset * chunkSize);

            var transferMsg = new FileTransferMsg { Header = Any.Pack(request) };
            var clientStream = await ExecuteClientStreamingRequestAuthorized(UploadPackageInternal, transferMsg);

            transferMsg.Header = null;
            transferMsg.Data = new FileChunk(chunkOffset);
            var buffer = new byte[chunkSize];
            try
            {
                using (var stream = File.Open(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)chunkSize * chunkOffset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        transferMsg.Data.Binary = ByteString.CopyFrom(buffer, 0, cnt);
                        _messageFormatter.LogClientRequest(Logger, transferMsg);
                        await clientStream.RequestStream.WriteAsync(transferMsg);
                        progressListener.IncrementProgress(cnt);
                        transferMsg.Data.Id++;
                    }
                }
            }
            finally
            {
                transferMsg.Data = FileChunk.FinalChunk;
                _messageFormatter.LogClientRequest(Logger, transferMsg);
                await clientStream.RequestStream.WriteAsync(transferMsg);
            }

            var response = await clientStream.ResponseAsync;
            FailForNonSuccess(response.ExecResult);
        }

        public async Task RemovePackage(RemovePackageRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemovePackageInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task DownloadPackage(DownloadPackageRequest request, string dstPath, IFileProgressListener progressListener)
        {
            var offset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

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

                var serverCall = await ExecuteServerStreamingRequestAuthorized(DownloadPackageInternal, request);
                var fileReader = serverCall.ResponseStream;
                try
                {
                    if (!await fileReader.MoveNext())
                        throw new AlgoServerException("Empty download response");

                    do
                    {
                        var transferMsg = fileReader.Current;
                        _messageFormatter.LogServerResponse(Logger, transferMsg);

                        if (transferMsg.Header != null)
                        {
                            var response = transferMsg.Header.Unpack<DownloadPackageResponse>();
                            _messageFormatter.LogServerResponse(Logger, response);
                            FailForNonSuccess(response.ExecResult);
                        }

                        var data = transferMsg.Data;
                        if (!data.Binary.IsEmpty)
                        {
                            data.Binary.CopyTo(buffer, 0);
                            progressListener.IncrementProgress(data.Binary.Length);
                            stream.Write(buffer, 0, data.Binary.Length);
                        }
                        if (data.IsFinal)
                            break;
                    }
                    while (await fileReader.MoveNext());
                }
                finally
                {
                    serverCall.Dispose();
                }
            }
        }

        public async Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(GetBotFolderInfoInternal, request);
            FailForNonSuccess(response.ExecResult);
            return response.FolderInfo;
        }

        public async Task ClearPluginFolder(ClearPluginFolderRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(ClearBotFolderInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task DeletePluginFile(DeletePluginFileRequest request)
        {
            var response = await ExecuteUnaryRequestAuthorized(DeleteBotFileInternal, request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task DownloadPluginFile(DownloadPluginFileRequest request, string dstPath, IFileProgressListener progressListener)
        {
            var offset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

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

                var serverCall = await ExecuteServerStreamingRequestAuthorized(DownloadBotFileInternal, request);
                var fileReader = serverCall.ResponseStream;
                try
                {
                    if (!await fileReader.MoveNext())
                        throw new AlgoServerException("Empty download response");

                    do
                    {
                        var transferMsg = fileReader.Current;
                        _messageFormatter.LogServerResponse(Logger, transferMsg);

                        if (transferMsg.Header != null)
                        {
                            var response = transferMsg.Header.Unpack<DownloadPluginFileResponse>();
                            _messageFormatter.LogServerResponse(Logger, response);
                            FailForNonSuccess(response.ExecResult);
                        }

                        var data = transferMsg.Data;
                        if (!data.Binary.IsEmpty)
                        {
                            data.Binary.CopyTo(buffer, 0);
                            progressListener.IncrementProgress(data.Binary.Length);
                            stream.Write(buffer, 0, data.Binary.Length);
                        }
                        if (data.IsFinal)
                            break;
                    }
                    while (await fileReader.MoveNext());
                }
                finally
                {
                    serverCall.Dispose();
                }
            }
        }

        public async Task UploadPluginFile(UploadPluginFileRequest request, string srcPath, IFileProgressListener progressListener)
        {
            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            progressListener.Init((long)chunkOffset * chunkSize);

            var transferMsg = new FileTransferMsg { Header = Any.Pack(request) };
            var clientStream = await ExecuteClientStreamingRequestAuthorized(UploadBotFileInternal, transferMsg);

            transferMsg.Header = null;
            transferMsg.Data = new FileChunk(chunkOffset);
            var buffer = new byte[chunkSize];
            try
            {
                using (var stream = File.Open(srcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek((long)chunkSize * chunkOffset, SeekOrigin.Begin);
                    for (var cnt = stream.Read(buffer, 0, chunkSize); cnt > 0; cnt = stream.Read(buffer, 0, chunkSize))
                    {
                        transferMsg.Data.Binary = ByteString.CopyFrom(buffer, 0, cnt);
                        _messageFormatter.LogClientRequest(Logger, transferMsg);
                        await clientStream.RequestStream.WriteAsync(transferMsg);
                        progressListener.IncrementProgress(cnt);
                        transferMsg.Data.Id++;
                    }
                }
            }
            finally
            {
                transferMsg.Data = FileChunk.FinalChunk;
                _messageFormatter.LogClientRequest(Logger, request);
                await clientStream.RequestStream.WriteAsync(transferMsg);
            }

            var response = await clientStream.ResponseAsync;
            FailForNonSuccess(response.ExecResult);
        }

        #endregion Requests
    }
}
