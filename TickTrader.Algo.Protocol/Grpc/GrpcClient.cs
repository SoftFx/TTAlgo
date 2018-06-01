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
            _channel = new Channel($"{SessionSettings.ServerAddress}:{SessionSettings.ProtocolSettings.ListeningPort}", ChannelCredentials.Insecure);

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
            var packages = _client.GetPackageList(new Lib.PackageListRequest());
            if (FailConnectionForNonSuccess(packages.Status))
                return;
            AgentClient.InitPackageList(packages.Packages.Select(ToAlgo.Convert).ToList());

            var accounts = _client.GetAccountList(new Lib.AccountListRequest());
            if (FailConnectionForNonSuccess(accounts.Status))
                return;
            AgentClient.InitAccountList(accounts.Accounts.Select(ToAlgo.Convert).ToList());

            var bots = _client.GetBotList(new Lib.BotListRequest());
            if (FailConnectionForNonSuccess(bots.Status))
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


        private bool FailConnectionForNonSuccess(Lib.Request.Types.RequestStatus status)
        {
            if (FailForNonSuccess(status))
            {
                OnConnectionError(status.ToString());
                return true;
            }
            return false;
        }

        private bool FailForNonSuccess(Lib.Request.Types.RequestStatus status)
        {
            return status != Lib.Request.Types.RequestStatus.Success;
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

        public override async Task StartBot(string botId)
        {
            var response = await _client.StartBotAsync(new Lib.StartBotRequest { BotId = botId });
            if (FailForNonSuccess(response.Status))
                throw new Exception(response.Status.ToString());
        }

        public override async Task StopBot(string botId)
        {
            var response = await _client.StopBotAsync(new Lib.StopBotRequest { BotId = botId });
            if (FailForNonSuccess(response.Status))
                throw new Exception(response.Status.ToString());
        }

        public override async Task AddBot(AccountKey account, PluginConfig config)
        {
            var response = await _client.AddBotAsync(new Lib.AddBotRequest { Account = account.Convert(), Config = config.Convert() });
            if (FailForNonSuccess(response.Status))
                throw new Exception(response.Status.ToString());
        }

        public override async Task RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            var response = await _client.RemoveBotAsync(new Lib.RemoveBotRequest { BotId = ToGrpc.Convert(botId), CleanLog = cleanLog, CleanAlgoData = cleanAlgoData });
            if (FailForNonSuccess(response.Status))
                throw new Exception(response.Status.ToString());
        }

        public override async Task ChangeBotConfig(string botId, PluginConfig newConfig)
        {
            var response = await _client.ChangeBotConfigAsync(new Lib.ChangeBotConfigRequest { BotId = ToGrpc.Convert(botId), NewConfig = newConfig.Convert() });
            if (FailForNonSuccess(response.Status))
                throw new Exception(response.Status.ToString());
        }

        #endregion Requests
    }
}
