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
        private const int DefaultRequestTimeout = 10;

        private readonly PluginsSubscriptionsManager _subscriptions;
        private readonly ApiMessageFormatter _messageFormatter;

        private AlgoServerPublic.AlgoServerPublicClient _client;
        private CancellationTokenSource _updateStreamCancelTokenSrc;
        private Channel _channel;
        private string _accessToken;


        static AlgoServerClient()
        {
            CertificateProvider.InitClient(Assembly.GetExecutingAssembly(), "TickTrader.Algo.Server.PublicAPI.certs.algo-ca.crt");
        }

        private AlgoServerClient(IAlgoServerEventHandler handler) : base(handler)
        {
            _messageFormatter = new ApiMessageFormatter(AlgoServerPublicAPIReflection.Descriptor);
            _subscriptions = new PluginsSubscriptionsManager();

            VersionSpec = new ApiVersionSpec();
            AccessManager = new ApiAccessManager(ClientClaims.Types.AccessLevel.Anonymous);
        }

        public static IAlgoServerClient Create(IAlgoServerEventHandler handler) => new AlgoServerClient(handler);


        public override void StartClient()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(_logger));

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
                                OnConnectionError("Connection timeout");
                                break;
                            case TaskStatus.Faulted:
                                OnConnectionError("Connection failed");
                                break;
                        }
                    });
        }

        public override void StopClient()
        {
            _messageFormatter.LogMessages = false;

            StopConnectionTimer();
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
                            var loginResponse = t.Result;
                            if (loginResponse.Error != LoginResponse.Types.LoginError.None)
                                OnConnectionError(loginResponse.Error.ToString());
                            else if (loginResponse.ExecResult.Status != RequestResult.Types.RequestStatus.Success)
                            {
                                var res = loginResponse.ExecResult;
                                OnConnectionError($"{res.Status}{(string.IsNullOrEmpty(res.Message) ? "" : $" ({res.Message})")}");
                            }
                            else if (taskResult.Error == LoginResponse.Types.LoginError.Invalid2FaCode)
                            {
                                On2FALogin();
                            }
                            else
                            {
                                _accessToken = loginResponse.AccessToken;
                                _logger.Info($"Server session id: {loginResponse.SessionId}");

                                OnLogin(loginResponse.MajorVersion, loginResponse.MinorVersion, loginResponse.AccessLevel);
                            }

                            break;
                        }
                    case TaskStatus.Canceled:
                        OnConnectionError("Login request time out");
                        break;
                    case TaskStatus.Faulted:
                        OnConnectionError("Login failed");
                        break;
                }
            });
        }

        public override void Send2FALogin()
        {
            Login2FAAsync().ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion: HandleLoginResponse(t.Result); break;
                    case TaskStatus.Canceled: OnConnectionError("Login request time out"); break;
                    case TaskStatus.Faulted: OnConnectionError("Login failed"); break;
                }
            });
        }

        private async Task<LoginResponse> LoginAsync()
        {
            return await ExecuteUnaryRequest(LoginInternal, GetLoginRequest());
        }

        private async Task<LoginResponse> Login2FAAsync()
        {
            var otp = await _serverHandler.Get2FACode();

            var request = GetLoginRequest();
            request.OneTimePassword = otp;

            return await ExecuteUnaryRequest(LoginInternal, request);
        }

        private LoginRequest GetLoginRequest()
        {
            return new LoginRequest
            {
                Login = SessionSettings.Login,
                Password = SessionSettings.Password,
                MajorVersion = VersionSpec.MajorVersion,
                MinorVersion = VersionSpec.MinorVersion,
            };
        }

        private void HandleLoginResponse(LoginResponse response)
        {
            if (response.Error == LoginResponse.Types.LoginError.None)
            {
                _accessToken = response.AccessToken;
                _logger.Info($"Server session id: {response.SessionId}");

                OnLogin(response.MajorVersion, response.MinorVersion, response.AccessLevel);
            }
            else
                OnConnectionError(response.Error.ToString());
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
                            RestoreSubscribeToPluginStatusInternal();
                            RestoreSubscribeToPluginLogsInternal();
                            OnSubscribed();
                            break;
                        case TaskStatus.Canceled:
                            OnConnectionError("Request timeout during init");
                            break;
                        case TaskStatus.Faulted:
                            OnConnectionError("Init failed: Can't subscribe to updates");
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
                            OnLogout(t.Result.Reason);
                            break;
                        case TaskStatus.Canceled:
                            OnConnectionError("Logout timeout");
                            break;
                        case TaskStatus.Faulted:
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
                _serverHandler.InitAccountModelList(snapshot.Accounts.ToList());
                _serverHandler.InitPluginModelList(snapshot.Plugins.ToList());
            }
            catch (UnauthorizedException uex)
            {
                OnConnectionError("Bad access token", uex);
            }
            catch (Exception ex)
            {
                OnConnectionError("Init failed: Can't apply server metadata", ex);
            }
        }


        public override void SendDisconnect()
        {
            OnDisconnected();
        }

        private static void FailForNonSuccess(RequestResult requestResult)
        {
            if (requestResult.Status != RequestResult.Types.RequestStatus.Success)
                throw requestResult.Status == RequestResult.Types.RequestStatus.Unauthorized
                    ? new UnauthorizedException(requestResult.Message)
                    : new AlgoServerException($"{requestResult.Status} - {requestResult.Message}");
        }

        private async void ListenToUpdates(AsyncServerStreamingCall<UpdateInfo> updateCall)
        {
            try
            {
                var updateStream = updateCall.ResponseStream;
                _updateStreamCancelTokenSrc = new CancellationTokenSource();
                var cancelToken = _updateStreamCancelTokenSrc.Token;

                while (await updateStream.MoveNext(cancelToken))
                {
                    var updateInfo = updateStream.Current;
                    if (updateInfo.Type == UpdateInfo.Types.PayloadType.Heartbeat)
                    {
                        _messageFormatter.LogHeartbeat(_logger);
                        RefreshConnectionTimer();
                    }
                    else if (updateInfo.TryUnpack(out var update))
                    {
                        _messageFormatter.LogClientUpdate(_logger, update);
                        DispatchUpdate(update);
                    }
                    else
                    {
                        _logger.Error($"Failed to unpack update of type: {updateInfo.Type}");
                    }
                }
                if (State != ClientStates.LoggingOut)
                    OnConnectionError("Update stream stopped by server");
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                OnConnectionError("Update stream failed", ex);
            }
            finally
            {
                updateCall.Dispose();
            }
        }

        public void DispatchUpdate(IMessage update)
        {
            try
            {
                switch (update)
                {
                    case AlertListUpdate alertUpdate: _serverHandler.OnAlertListUpdate(alertUpdate); break;
                    case PluginStatusUpdate pluginStatusUpdate: _serverHandler.OnPluginStatusUpdate(pluginStatusUpdate); break;
                    case PluginLogUpdate logUpdate: _serverHandler.OnPluginLogUpdate(logUpdate); break;

                    case PackageUpdate packageUpdate: _serverHandler.OnPackageUpdate(packageUpdate); break;
                    case AccountModelUpdate accountUpdate: _serverHandler.OnAccountModelUpdate(accountUpdate); break;
                    case PluginModelUpdate pluginUpdate: _serverHandler.OnPluginModelUpdate(pluginUpdate); break;

                    case PackageStateUpdate packageStateUpdate: _serverHandler.OnPackageStateUpdate(packageStateUpdate); break;
                    case AccountStateUpdate accountStateUpdate: _serverHandler.OnAccountStateUpdate(accountStateUpdate); break;
                    case PluginStateUpdate pluginStateUpdate: _serverHandler.OnPluginStateUpdate(pluginStateUpdate); break;

                    case AlgoServerMetadataUpdate serverMetadata: ApplyAlgoServerMetadata(serverMetadata); break;

                    default: _logger.Error($"Can't dispatch update of type: {update.Descriptor.Name}"); break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to dispatch update of type: {update.Descriptor.Name}");
            }
        }

        private static CallOptions BuildCallOptions(string accessToken = null, bool setDeadline = true)
        {
            var deadline = setDeadline ? (DateTime?)DateTime.UtcNow.AddSeconds(DefaultRequestTimeout) : null;
            var credentials = string.IsNullOrEmpty(accessToken) ? null : AlgoGrpcCredentials.FromAccessToken(accessToken);

            return new CallOptions(deadline: deadline, credentials: credentials);
        }

        private async Task<TResponse> ExecuteUnaryRequest<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<TResponse>> requestAction, TRequest request)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                _messageFormatter.LogServerRequest(_logger, request);
                var response = await requestAction(request, BuildCallOptions());
                _messageFormatter.LogServerResponse(_logger, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
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
                _messageFormatter.LogServerRequest(_logger, request);
                var response = await requestAction(request, BuildCallOptions(_accessToken));
                _messageFormatter.LogServerResponse(_logger, response);

                return response;
            }
            catch (UnauthorizedException uex)
            {
                _logger.Error(uex, $"Bad access token for {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (RpcException rex)
            {
                if (rex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    _logger.Error($"Request timed out {_messageFormatter.ToJson(request)}");
                    throw new Common.TimeoutException($"Request {nameof(TRequest)} timed out");
                }
                else if (rex.StatusCode == StatusCode.Unknown && rex.Status.Detail == "Stream removed")
                {
                    _logger.Error($"Disconnected while executing {_messageFormatter.ToJson(request)}");
                    throw new AlgoServerException("Connection error");
                }
                _logger.Error(rex, $"Failed to execute {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
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
                _messageFormatter.LogServerRequest(_logger, request);

                return requestAction(request, BuildCallOptions(_accessToken, false));
            }
            catch (UnauthorizedException uex)
            {
                _logger.Error(uex, $"Bad access token for {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
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
                _messageFormatter.LogServerRequest(_logger, request);

                return requestAction(request, BuildCallOptions(_accessToken, false));
            }
            catch (UnauthorizedException uex)
            {
                _logger.Error(uex, $"Bad access token for {_messageFormatter.ToJson(request)}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to execute {_messageFormatter.ToJson(request)}");
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

        private Task<PluginStatusSubscribeResponse> SubscribeToPluginStatusInternal(PluginStatusSubscribeRequest request, CallOptions options)
        {
            return _client.SubscribeToPluginStatusAsync(request, options).ResponseAsync;
        }

        private Task<PluginStatusUnsubscribeResponse> UnsubscribeToPluginStatusInternal(PluginStatusUnsubscribeRequest request, CallOptions options)
        {
            return _client.UnsubscribeToPluginStatusAsync(request, options).ResponseAsync;
        }

        private void RestoreSubscribeToPluginStatusInternal()
        {
            foreach (var request in _subscriptions.BuildStatusSubscriptionRequests())
                Task.Run(() => SubscribeToPluginStatus(request));
        }


        private Task<PluginLogsUnsubscribeResponse> UnsubscribeToPluginLogsInternal(PluginLogsUnsubscribeRequest request, CallOptions options)
        {
            return _client.UnsubscribeToPluginLogsAsync(request, options).ResponseAsync;
        }

        private Task<PluginLogsSubscribeResponse> SubscribeToPluginLogsInternal(PluginLogsSubscribeRequest request, CallOptions options)
        {
            return _client.SubscribeToPluginLogsAsync(request, options).ResponseAsync;
        }

        private void RestoreSubscribeToPluginLogsInternal()
        {
            foreach (var request in _subscriptions.BuildLogsSubscriptionRequests())
                Task.Run(() => SubscribeToPluginLogs(request));
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


        public async Task SubscribeToPluginStatus(PluginStatusSubscribeRequest request)
        {
            if (_subscriptions.TryAddStatusSubscription(request.PluginId))
            {
                var response = await ExecuteUnaryRequestAuthorized(SubscribeToPluginStatusInternal, request);
                FailForNonSuccess(response.ExecResult);
            }
        }

        public async Task UnsubscribeToPluginStatus(PluginStatusUnsubscribeRequest request)
        {
            if (_subscriptions.TryRemoveStatusSubscription(request.PluginId))
            {
                var response = await ExecuteUnaryRequestAuthorized(UnsubscribeToPluginStatusInternal, request);
                FailForNonSuccess(response.ExecResult);
            }
        }


        public async Task SubscribeToPluginLogs(PluginLogsSubscribeRequest request)
        {
            if (_subscriptions.TryAddLogsSubscription(request.PluginId))
            {
                var response = await ExecuteUnaryRequestAuthorized(SubscribeToPluginLogsInternal, request);
                FailForNonSuccess(response.ExecResult);
            }
        }

        public async Task UnsubscribeToPluginLogs(PluginLogsUnsubscribeRequest request)
        {
            if (_subscriptions.TryRemoveLogsSubscription(request.PluginId))
            {
                var response = await ExecuteUnaryRequestAuthorized(UnsubscribeToPluginLogsInternal, request);
                FailForNonSuccess(response.ExecResult);
            }
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
                        _messageFormatter.LogClientRequest(_logger, transferMsg);
                        await clientStream.RequestStream.WriteAsync(transferMsg);
                        progressListener.IncrementProgress(cnt);
                        transferMsg.Data.Id++;
                    }
                }
            }
            finally
            {
                transferMsg.Data = FileChunk.FinalChunk;
                _messageFormatter.LogClientRequest(_logger, transferMsg);
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
                        _messageFormatter.LogServerResponse(_logger, transferMsg);

                        if (transferMsg.Header != null)
                        {
                            var response = transferMsg.Header.Unpack<DownloadPackageResponse>();
                            _messageFormatter.LogServerResponse(_logger, response);
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
                        _messageFormatter.LogServerResponse(_logger, transferMsg);

                        if (transferMsg.Header != null)
                        {
                            var response = transferMsg.Header.Unpack<DownloadPluginFileResponse>();
                            _messageFormatter.LogServerResponse(_logger, response);
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
                        _messageFormatter.LogClientRequest(_logger, transferMsg);
                        await clientStream.RequestStream.WriteAsync(transferMsg);
                        progressListener.IncrementProgress(cnt);
                        transferMsg.Data.Id++;
                    }
                }
            }
            finally
            {
                transferMsg.Data = FileChunk.FinalChunk;
                _messageFormatter.LogClientRequest(_logger, request);
                await clientStream.RequestStream.WriteAsync(transferMsg);
            }

            var response = await clientStream.ResponseAsync;
            FailForNonSuccess(response.ExecResult);
        }

        #endregion Requests
    }
}
