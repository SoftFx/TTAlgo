using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Lib
{
    internal class BotAgentServerListener : ServerListener
    {
        private IBotAgentServer _server;


        public BotAgentServerListener(IBotAgentServer server)
        {
            _server = server;
        }


        public override void OnConnect(Server server, Server.Session session)
        {
            _server.Connected(session.Id);
        }

        public override void OnDisconnect(Server server, Server.Session session, ServerContext[] contexts, string text)
        {
            _server.Disconnected(session.Id, text);
        }

        public override void OnAccountListRequest(Server server, Server.Session session, AccountListRequest message)
        {
            var report = _server.GetAccountList();
            server.Send(session.Id, report.ToMessage());
        }
    }
}
