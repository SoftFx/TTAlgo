using NLog;
using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol.Lib
{
    internal class BotAgentServerListener : ServerListener
    {
        private readonly ILogger _logger;
        private IBotAgentServer _server;


        public BotAgentServerListener(IBotAgentServer server, ILogger logger)
        {
            _server = server;
            _logger = logger;
        }


        public override void OnConnect(Server server, Server.Session session)
        {
            try
            {
                _logger.Info($"Connected client {session.Id}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }

        public override void OnDisconnect(Server server, Server.Session session, ServerContext[] contexts, string text)
        {
            try
            {
                _logger.Info($"Disconnected client {session.Id}: {text}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }

        public override void OnAccountListRequest(Server server, Server.Session session, AccountListRequest message)
        {
            try
            {
                var report = _server.GetAccountList(message.Id);
                server.Send(session.Id, report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }
    }
}
