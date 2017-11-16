using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Lib
{
    internal class BotAgentClientListener : ClientSessionListener
    {
        private IBotAgentClient _client;


        public BotAgentClientListener(IBotAgentClient client)
        {
            _client = client;
        }


        public override void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
            _client.Connected();
        }

        public override void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext)
        {
            _client.ConnectionError();
        }

        public override void OnDisconnect(ClientSession clientSession, DisconnectClientContext disconnectContext, ClientContext[] contexts, string text)
        {
            _client.Disconnected();
        }

        public override void OnAccountListReport(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport message)
        {
            _client.SetAccountList(message.ToEntity());
        }
    }
}
