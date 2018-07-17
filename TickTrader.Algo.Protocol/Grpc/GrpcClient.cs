using Grpc.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Protocol.Grpc
{
    public class GrpcClient : ProtocolClient
    {
        private Channel _channel;
        private Lib.BotAgent.BotAgentClient _client;


        public GrpcClient(IBotAgentClient agentClient) : base(agentClient)
        {
        }


        protected override void StartClient()
        {
            _channel = new Channel(SessionSettings.ServerAddress, SessionSettings.ProtocolSettings.ListeningPort, ChannelCredentials.Insecure);

            _client = new Lib.BotAgent.BotAgentClient(_channel);

            _channel.ConnectAsync(DateTime.UtcNow.AddSeconds(10))
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
            var response = _client.Login(new Lib.LoginRequest { MajorVersion = VersionSpec.MajorVersion, MinorVersion = VersionSpec.MinorVersion });
            OnLogin(response.MajorVersion, response.MinorVersion);
        }

        protected override void Init()
        {
            var apiMetadata = _client.GetApiMetadata(new Lib.ApiMetadataRequest());
            if (FailConnectionForNonSuccess(apiMetadata.ExecResult))
                return;
            AgentClient.SetApiMetadata(apiMetadata.ApiMetadata.Convert());

            var mappings = _client.GetMappingsInfo(new Lib.MappingsInfoRequest());
            if (FailConnectionForNonSuccess(mappings.ExecResult))
                return;
            AgentClient.SetMappingsInfo(mappings.Mappings.Convert());

            var setupContext = _client.GetSetupContext(new Lib.SetupContextRequest());
            if (FailConnectionForNonSuccess(setupContext.ExecResult))
                return;
            AgentClient.SetSetupContext(setupContext.SetupContext.Convert());

            var packages = _client.GetPackageList(new Lib.PackageListRequest());
            if (FailConnectionForNonSuccess(packages.ExecResult))
                return;
            AgentClient.InitPackageList(packages.Packages.Select(ToAlgo.Convert).ToList());

            var accounts = _client.GetAccountList(new Lib.AccountListRequest());
            if (FailConnectionForNonSuccess(accounts.ExecResult))
                return;
            AgentClient.InitAccountList(accounts.Accounts.Select(ToAlgo.Convert).ToList());

            var bots = _client.GetBotList(new Lib.BotListRequest());
            if (FailConnectionForNonSuccess(bots.ExecResult))
                return;
            AgentClient.InitBotList(bots.Bots.Select(ToAlgo.Convert).ToList());

            ListenToUpdates();
        }

        protected override void SendLogout()
        {
            OnLogout("because");
            //throw new System.NotImplementedException();
        }

        protected override void SendDisconnect()
        {
            OnDisconnected();
            //throw new System.NotImplementedException();
        }


        private bool FailForNonSuccess(Lib.RequestResult.Types.RequestStatus status)
        {
            return status != Lib.RequestResult.Types.RequestStatus.Success;
        }

        private bool FailConnectionForNonSuccess(Lib.RequestResult requestResult)
        {
            if (FailForNonSuccess(requestResult.Status))
            {
                OnConnectionError($"{requestResult.Status} - {requestResult.Message}");
                return true;
            }
            return false;
        }

        private void FailForNonSuccess(Lib.RequestResult requestResult)
        {
            if (FailForNonSuccess(requestResult.Status))
                throw new BAException($"{requestResult.Status} - {requestResult.Message}");
        }

        private async void ListenToUpdates()
        {
            AsyncServerStreamingCall<Lib.UpdateInfo> updateStream;
            try
            {
                updateStream = _client.SubscribeToUpdates(new Lib.SubscribeToUpdatesRequest());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Init failed: Can't subscribe to updates");
                OnConnectionError("Init failed");
                return;
            }

            OnSubscribed();

            var cancelTokenSrc = new CancellationTokenSource();
            try
            {
                while (await updateStream.ResponseStream.MoveNext(cancelTokenSrc.Token))
                {
                    var update = updateStream.ResponseStream.Current;
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


        #region Requests

        public override async Task<AccountMetadataInfo> GetAccountMetadata(AccountKey account)
        {
            var response = await _client.GetAccountMetadataAsync(new Lib.AccountMetadataRequest { Account = account.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.AccountMetadata.Convert();
        }

        public override async Task StartBot(string botId)
        {
            var response = await _client.StartBotAsync(new Lib.StartBotRequest { BotId = botId });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task StopBot(string botId)
        {
            var response = await _client.StopBotAsync(new Lib.StopBotRequest { BotId = botId });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task AddBot(AccountKey account, PluginConfig config)
        {
            var response = await _client.AddBotAsync(new Lib.AddBotRequest { Account = account.Convert(), Config = config.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            var response = await _client.RemoveBotAsync(new Lib.RemoveBotRequest { BotId = ToGrpc.Convert(botId), CleanLog = cleanLog, CleanAlgoData = cleanAlgoData });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task ChangeBotConfig(string botId, PluginConfig newConfig)
        {
            var response = await _client.ChangeBotConfigAsync(new Lib.ChangeBotConfigRequest { BotId = ToGrpc.Convert(botId), NewConfig = newConfig.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task AddAccount(AccountKey account, string password, bool useNewProtocol)
        {
            var response = await _client.AddAccountAsync(new Lib.AddAccountRequest { Account = account.Convert(), Password = ToGrpc.Convert(password), UseNewProtocol = useNewProtocol });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemoveAccount(AccountKey account)
        {
            var response = await _client.RemoveAccountAsync(new Lib.RemoveAccountRequest { Account = account.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task ChangeAccount(AccountKey account, string password, bool useNewProtocol)
        {
            var response = await _client.ChangeAccountAsync(new Lib.ChangeAccountRequest { Account = account.Convert(), Password = ToGrpc.Convert(password), UseNewProtocol = useNewProtocol });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            var response = await _client.TestAccountAsync(new Lib.TestAccountRequest { Account = account.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo.Convert();
        }

        public override async Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password, bool useNewProtocol)
        {
            var response = await _client.TestAccountCredsAsync(new Lib.TestAccountCredsRequest { Account = account.Convert(), Password = ToGrpc.Convert(password), UseNewProtocol = useNewProtocol });
            FailForNonSuccess(response.ExecResult);
            return response.ErrorInfo.Convert();
        }

        public override async Task UploadPackage(string fileName, byte[] packageBinary)
        {
            var response = await _client.UploadPackageAsync(new Lib.UploadPackageRequest { FileName = ToGrpc.Convert(fileName), PackageBinary = packageBinary.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task RemovePackage(PackageKey package)
        {
            var response = await _client.RemovePackageAsync(new Lib.RemovePackageRequest { Package = package.Convert() });
            FailForNonSuccess(response.ExecResult);
        }

        public override async Task<byte[]> DownloadPackage(PackageKey package)
        {
            var response = await _client.DownloadPackageAsync(new Lib.DownloadPackageRequest { Package = package.Convert() });
            FailForNonSuccess(response.ExecResult);
            return response.PackageBinary.Convert();
        }

        #endregion Requests
    }
}
