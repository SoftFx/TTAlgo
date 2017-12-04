using NLog;
using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol.Lib
{
    internal class BotAgentClientListener : ClientSessionListener
    {
        private readonly ILogger _logger;
        private IBotAgentClient _client;


        public event Action Connected = delegate { };
        public event Action ConnectionError = delegate { };
        public event Action Disconnected = delegate { };


        public BotAgentClientListener(IBotAgentClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }


        public override void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
            try
            {
                Connected();
                _logger.Info($"Connected to server, sessionId = {clientSession.Guid}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {clientSession.Guid}: {ex.Message}");
            }
        }

        public override void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext)
        {
            try
            {
                ConnectionError();
                _logger.Info($"Failed to connect to server, sessionId = {clientSession.Guid}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {clientSession.Guid}: {ex.Message}");
            }
        }

        public override void OnDisconnect(ClientSession clientSession, DisconnectClientContext disconnectContext, ClientContext[] contexts, string text)
        {
            try
            {
                Disconnected();
                _logger.Info($"Disconnected from server, sessionId = {clientSession.Guid}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {clientSession.Guid}: {ex.Message}");
            }
        }

        public override void OnAccountListReport(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport message)
        {
            try
            {
                _client.SetAccountList(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }
    }
}
