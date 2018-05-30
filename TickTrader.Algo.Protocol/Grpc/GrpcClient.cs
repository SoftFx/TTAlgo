using Grpc.Core;
using System;
using System.Linq;

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

            _channel.ConnectAsync(DateTime.UtcNow.AddSeconds(10)).Wait();
            OnConnected();
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

            OnSubscribed();
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
            if (status != Lib.Request.Types.RequestStatus.Success)
            {
                OnConnectionError(status.ToString());
                return true;
            }
            return false;
        }
    }
}
