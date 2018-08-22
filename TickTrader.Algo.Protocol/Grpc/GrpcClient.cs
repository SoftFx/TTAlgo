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
        private JsonFormatter _messageFormatter;
        private Channel _channel;
        private Lib.BotAgent.BotAgentClient _client;
        private string _accessToken;


        static GrpcClient()
        {
            CertificateProvider.InitClient();
        }


        public GrpcClient(IBotAgentClient agentClient) : base(agentClient)
        {
            _messageFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
        }


        protected override void StartClient()
        {
            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter(Logger));
            var creds = new SslCredentials(CertificateProvider.RootCertificate); //, new KeyCertificatePair(CertificateProvider.ClientCertificate, CertificateProvider.ClientKey));
            var options = new[] { new ChannelOption(ChannelOptions.SslTargetNameOverride, "bot-agent.soft-fx.lv"), };
            _channel = new Channel(SessionSettings.ServerAddress, SessionSettings.ProtocolSettings.ListeningPort, creds, options);
            _accessToken = "";

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
                        _accessToken = t.Result.AccessToken;
                        Logger.Info($"Server session id: {t.Result.SessionId}");
                        OnLogin(t.Result.MajorVersion, t.Result.MinorVersion);
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
            var cancelTokenSrc = new CancellationTokenSource();
            try
            {
                while (await updateStream.MoveNext(cancelTokenSrc.Token))
                {
                    var update = updateStream.Current;
                    LogResponse(Logger, update);
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
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Update stream failed");
                OnConnectionError("Update stream failed");
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

        private void LogRequest(ILogger logger, IMessage request)
        {
            if (SessionSettings.ProtocolSettings.LogMessages)
                logger?.Info($"server < {request.GetType().Name}: {_messageFormatter.Format(request)}");
        }

        private void LogResponse(ILogger logger, IMessage response)
        {
            if (SessionSettings.ProtocolSettings.LogMessages)
                logger?.Info($"server > {response.GetType().Name}: {_messageFormatter.Format(response)}");
        }

        private async Task<TResponse> ExecuteUnaryRequest<TRequest, TResponse>(
            Func<TRequest, CallOptions, Task<TResponse>> requestAction, TRequest request)
            where TRequest : IMessage
            where TResponse : IMessage
        {
            try
            {
                LogRequest(Logger, request);
                var response = await requestAction(request, GetCallOptions());
                LogResponse(Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute request of type {request.GetType().Name}");
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
                LogRequest(Logger, request);
                var response = await requestAction(request, GetCallOptions(_accessToken));
                LogResponse(Logger, response);

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute request of type {request.GetType().Name}");
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
                LogRequest(Logger, request);
                return requestAction(request, GetCallOptions(_accessToken, false));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to execute request of type {request.GetType().Name}");
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

        private Task<Lib.SnapshotResponse> GetSnapshotInternal(Lib.SnapshotRequest request, CallOptions options)
        {
            return _client.GetSnapshotAsync(request, options).ResponseAsync;
        }

        private Task<IAsyncStreamReader<Lib.UpdateInfo>> SubscribeToUpdatesInternal(Lib.SubscribeToUpdatesRequest request, CallOptions options)
        {
            var call = _client.SubscribeToUpdates(request, options);
            return Task.FromResult(call.ResponseStream);
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

        private Task<Lib.UploadPackageResponse> UploadPackageInternal(Lib.UploadPackageRequest request, CallOptions options)
        {
            return _client.UploadPackageAsync(request, options).ResponseAsync;
        }

        private Task<Lib.RemovePackageResponse> RemovePackageInternal(Lib.RemovePackageRequest request, CallOptions options)
        {
            return _client.RemovePackageAsync(request, options).ResponseAsync;
        }

        private Task<Lib.DownloadPackageResponse> DownloadPackageInternal(Lib.DownloadPackageRequest request, CallOptions options)
        {
            return _client.DownloadPackageAsync(request, options).ResponseAsync;
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

        public override async Task UploadPackage(string fileName, byte[] packageBinary)
        {
            var response = await ExecuteUnaryRequestAuthorized(UploadPackageInternal, new Lib.UploadPackageRequest { FileName = ToGrpc.Convert(fileName), PackageBinary = packageBinary.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemovePackage(PackageKey package)
        {
            var response = await ExecuteUnaryRequestAuthorized(RemovePackageInternal, new Lib.RemovePackageRequest { Package = package.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<byte[]> DownloadPackage(PackageKey package)
        {
            var response = await ExecuteUnaryRequestAuthorized(DownloadPackageInternal, new Lib.DownloadPackageRequest { Package = package.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.PackageBinary.Convert();
        }

        #endregion Requests
    }
}
