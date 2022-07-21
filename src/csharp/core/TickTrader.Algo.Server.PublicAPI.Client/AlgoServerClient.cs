using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.IO;
using System.Linq;
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
        private IGrpcChannelProxy _channel;
        private string _accessToken;


        static AlgoServerClient()
        {
            CertificateProvider.InitClient(typeof(IAlgoServerClient).Assembly, "TickTrader.Algo.Server.PublicAPI.certs.algo-ca.crt");
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
            _channel = new GrpcChannelProxy(SessionSettings, _logger);

            _client = new AlgoServerPublic.AlgoServerPublicClient(
                _channel.GetCallInvoker()
                .Intercept(headers =>
                {
                    if (!string.IsNullOrEmpty(_accessToken))
                    {
                        headers.Add(AlgoGrpcCredentials.GetBearerGrpcHeader(_accessToken));
                    }
                    return headers;
                })
                .Intercept(new CallOptionsInterceptor())
                .Intercept(new MessageLoggerInterceptor(_logger, _messageFormatter, SessionSettings.LogMessages))
                .Intercept(new ErrorLoggerInterceptor(_logger, _messageFormatter)));

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

            ChangeAccessLevel(ClientClaims.Types.AccessLevel.Anonymous);

            StopConnectionTimer();
            _updateStreamCancelTokenSrc?.Cancel();
            _updateStreamCancelTokenSrc = null;

            _channel.ShutdownAsync().Wait();
        }

        public override void SendLogin()
        {
            LoginAsync().ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        {
                            var loginResponse = t.Result;
                            if (loginResponse.Error == LoginResponse.Types.LoginError.Invalid2FaCode)
                                On2FALogin();
                            else
                                HandleLoginResponse(loginResponse);

                            break;
                        }
                    case TaskStatus.Canceled: OnConnectionError("Login request time out"); break;
                    case TaskStatus.Faulted: OnConnectionError("Login failed"); break;
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
            return await _client.LoginAsync(GetLoginRequest());
        }

        private async Task<LoginResponse> Login2FAAsync()
        {
            var otp = await _serverHandler.Get2FACode();

            if (string.IsNullOrEmpty(otp))
            {
                _logger.Info("2FA challenge cancelled");
                OnDisconnected();
                return null;
            }

            var request = GetLoginRequest();
            request.OneTimePassword = otp;

            return await _client.LoginAsync(request);
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
            if (response == null)
                return;

            if (response.Error != LoginResponse.Types.LoginError.None)
            {
                Only2FAFailed = response.Error == LoginResponse.Types.LoginError.Invalid2FaCode;
                OnConnectionError(response.Error.ToString());
            }
            else if (response.ExecResult.Status != RequestResult.Types.RequestStatus.Success)
            {
                var res = response.ExecResult;
                OnConnectionError($"{res.Status}{(string.IsNullOrEmpty(res.Message) ? "" : $" ({res.Message})")}");
            }
            else
            {
                _accessToken = response.AccessToken;
                _logger.Info($"Server session id: {response.SessionId}");

                OnLogin(response.MajorVersion, response.MinorVersion, response.AccessLevel);
            }
        }

        public override void Init()
        {
            try
            {
                var call = _client.SubscribeToUpdates(new SubscribeToUpdatesRequest());
                ListenToUpdates(call);
                RestoreSubscribeToPluginStatusInternal();
                RestoreSubscribeToPluginLogsInternal();
                OnSubscribed();
            }
            catch (OperationCanceledException) { OnConnectionError("Request timeout during init"); }
            catch (Exception ex)
            {
                OnConnectionError("Init failed: Can't subscribe to updates", ex);
            }
        }

        public override void SendLogout()
        {
            _client.LogoutAsync(new LogoutRequest()).ResponseAsync
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
                throw new AlgoServerException($"{requestResult.Status} - {requestResult.Message}");
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
                        _messageFormatter.LogUpdateFromServer(_logger, update, updateInfo.Payload.Length, updateInfo.Compressed);
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


        private void RestoreSubscribeToPluginStatusInternal()
        {
            foreach (var request in _subscriptions.BuildStatusSubscriptionRequests())
                Task.Run(() => SubscribeToPluginStatus(request));
        }

        private void RestoreSubscribeToPluginLogsInternal()
        {
            foreach (var request in _subscriptions.BuildLogsSubscriptionRequests())
                Task.Run(() => SubscribeToPluginLogs(request));
        }


        #region Requests

        public async Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request)
        {
            var response = await _client.GetAccountMetadataAsync(request);
            FailForNonSuccess(response.ExecResult);
            return response.AccountMetadata;
        }


        public async Task SubscribeToPluginStatus(PluginStatusSubscribeRequest request)
        {
            if (_subscriptions.TryAddStatusSubscription(request.PluginId))
            {
                if (!CanSendPluginSubRequests())
                    return;

                var response = await _client.SubscribeToPluginStatusAsync(request);
                FailForNonSuccess(response.ExecResult);
            }
        }

        public async Task UnsubscribeToPluginStatus(PluginStatusUnsubscribeRequest request)
        {
            if (_subscriptions.TryRemoveStatusSubscription(request.PluginId))
            {
                if (!CanSendPluginSubRequests())
                    return;

                var response = await _client.UnsubscribeToPluginStatusAsync(request);
                FailForNonSuccess(response.ExecResult);
            }
        }


        public async Task SubscribeToPluginLogs(PluginLogsSubscribeRequest request)
        {
            if (_subscriptions.TryAddLogsSubscription(request.PluginId))
            {
                if (!CanSendPluginSubRequests())
                    return;

                var response = await _client.SubscribeToPluginLogsAsync(request);
                FailForNonSuccess(response.ExecResult);
            }
        }

        public async Task UnsubscribeToPluginLogs(PluginLogsUnsubscribeRequest request)
        {
            if (_subscriptions.TryRemoveLogsSubscription(request.PluginId))
            {
                if (!CanSendPluginSubRequests())
                    return;

                var response = await _client.UnsubscribeToPluginLogsAsync(request);
                FailForNonSuccess(response.ExecResult);
            }
        }


        public async Task AddPlugin(AddPluginRequest request)
        {
            var response = await _client.AddPluginAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task RemovePlugin(RemovePluginRequest request)
        {
            var response = await _client.RemovePluginAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task StartPlugin(StartPluginRequest request)
        {
            var response = await _client.StartPluginAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task StopPlugin(StopPluginRequest request)
        {
            var response = await _client.StopPluginAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task ChangePluginConfig(ChangePluginConfigRequest request)
        {
            var response = await _client.ChangePluginConfigAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task AddAccount(AddAccountRequest request)
        {
            var response = await _client.AddAccountAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task RemoveAccount(RemoveAccountRequest request)
        {
            var response = await _client.RemoveAccountAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task ChangeAccount(ChangeAccountRequest request)
        {
            var response = await _client.ChangeAccountAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            var response = await _client.TestAccountAsync(request);
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo;
        }

        public async Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            var response = await _client.TestAccountCredsAsync(request);
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo;
        }

        public async Task UploadPackage(UploadPackageRequest request, string srcPath, IFileProgressListener progressListener)
        {
            if (_client == null || _channel.IsShutdownState)
                throw new ConnectionFailedException("Connection failed");

            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            progressListener.Init((long)chunkOffset * chunkSize);

            var transferMsg = new FileTransferMsg { Header = Any.Pack(request) };
            using (var call = _client.UploadPackage())
            {
                await call.RequestStream.WriteAsync(transferMsg);

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
                            await call.RequestStream.WriteAsync(transferMsg);
                            progressListener.IncrementProgress(cnt);
                            transferMsg.Data.Id++;
                        }
                    }
                }
                finally
                {
                    transferMsg.Data = FileChunk.FinalChunk;
                    await call.RequestStream.WriteAsync(transferMsg);
                }

                var response = await call.ResponseAsync;
                FailForNonSuccess(response.ExecResult);
            }
        }

        public async Task RemovePackage(RemovePackageRequest request)
        {
            var response = await _client.RemovePackageAsync(request);
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

                using (var serverCall = _client.DownloadPackage(request))
                {
                    var fileReader = serverCall.ResponseStream;

                    if (!await fileReader.MoveNext())
                        throw new AlgoServerException("Empty download response");

                    do
                    {
                        var transferMsg = fileReader.Current;

                        if (transferMsg.Header != null)
                        {
                            var response = transferMsg.Header.Unpack<DownloadPackageResponse>();
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
            }
        }

        public async Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request)
        {
            var response = await _client.GetPluginFolderInfoAsync(request);
            FailForNonSuccess(response.ExecResult);
            return response.FolderInfo;
        }

        public async Task ClearPluginFolder(ClearPluginFolderRequest request)
        {
            var response = await _client.ClearPluginFolderAsync(request);
            FailForNonSuccess(response.ExecResult);
        }

        public async Task DeletePluginFile(DeletePluginFileRequest request)
        {
            var response = await _client.DeletePluginFileAsync(request);
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

                using (var serverCall = _client.DownloadPluginFile(request))
                {
                    var fileReader = serverCall.ResponseStream;

                    if (!await fileReader.MoveNext())
                        throw new AlgoServerException("Empty download response");

                    do
                    {
                        var transferMsg = fileReader.Current;

                        if (transferMsg.Header != null)
                        {
                            var response = transferMsg.Header.Unpack<DownloadPluginFileResponse>();
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
            }
        }

        public async Task UploadPluginFile(UploadPluginFileRequest request, string srcPath, IFileProgressListener progressListener)
        {
            var chunkOffset = request.TransferSettings.ChunkOffset;
            var chunkSize = request.TransferSettings.ChunkSize;

            progressListener.Init((long)chunkOffset * chunkSize);

            var transferMsg = new FileTransferMsg { Header = Any.Pack(request) };
            using (var call = _client.UploadPluginFile()) 
            {
                await call.RequestStream.WriteAsync(transferMsg);

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
                            await call.RequestStream.WriteAsync(transferMsg);
                            progressListener.IncrementProgress(cnt);
                            transferMsg.Data.Id++;
                        }
                    }
                }
                finally
                {
                    transferMsg.Data = FileChunk.FinalChunk;
                    await call.RequestStream.WriteAsync(transferMsg);
                }

                var response = await call.ResponseAsync;
                FailForNonSuccess(response.ExecResult);
            }
        }

        #endregion Requests
    }
}
