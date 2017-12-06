using NLog;
using SoftFX.Net.BotAgent;
using System;
using Sfx = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Lib
{
    internal class BotAgentClientListener : ClientSessionListener
    {
        private readonly ILogger _logger;
        private IBotAgentClient _client;


        public event Action Connected = delegate { };
        public event Action<string> ConnectionError = delegate { };
        public event Action Disconnected = delegate { };
        public event Action Login = delegate { };
        public event Action<string> LoginReject = delegate { };
        public event Action<string> Logout = delegate { };
        public event Action Subscribed = delegate { };


        public BotAgentClientListener(IBotAgentClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }


        public override void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
            try
            {
                _logger.Info($"Connected to server, sessionId = {clientSession.Guid}");
                Connected();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {clientSession.Guid}: {ex.Message}");
            }
        }

        public override void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext, string text)
        {
            try
            {
                _logger.Info($"Connection error: {text}, sessionId = {clientSession.Guid}");
                ConnectionError(text);
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
                _logger.Info($"Disconnected from server: {text}, sessionId = {clientSession.Guid}");
                Disconnected();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {clientSession.Guid}: {ex.Message}");
            }
        }

        public override void OnLoginReport(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport message)
        {
            try
            {
                _logger.Info($"Successfull login, sessionId = {session.Guid}");
                Login();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnLoginReject(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReject message)
        {
            try
            {
                var reason = "";
                switch (message.Reason)
                {
                    case Sfx.LoginRejectReason.InvalidCredentials:
                        reason = "Invalid username or password";
                        break;
                    case Sfx.LoginRejectReason.InternalServerError:
                        reason = $"Internal server error: {message.Text}";
                        break;
                }
                _logger.Info($"Server rejected login, sessionId = {session.Guid}: {reason}");
                LoginReject(reason);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnLogoutReport(ClientSession session, LogoutReport message)
        {
            try
            {
                var reason = "";
                switch (message.Reason)
                {
                    case Sfx.LogoutReason.ClientRequest:
                        reason = "Client requested logout";
                        break;
                    case Sfx.LogoutReason.ServerLogout:
                        reason = "Server forced logout";
                        break;
                    case Sfx.LogoutReason.InternalServerError:
                        reason = $"Internal server error: {message.Text}";
                        break;
                }
                _logger.Info($"Logout, sessionId = {session.Guid}: {reason}");
                LoginReject(reason);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnLogoutReport(ClientSession session, LogoutRequestClientContext LogoutRequestClientContext, LogoutReport message)
        {
            try
            {
                var reason = "";
                switch (message.Reason)
                {
                    case Sfx.LogoutReason.ClientRequest:
                        reason = "Client requested logout";
                        break;
                    case Sfx.LogoutReason.ServerLogout:
                        reason = "Server forced logout";
                        break;
                    case Sfx.LogoutReason.InternalServerError:
                        reason = $"Internal server error: {message.Text}";
                        break;
                }
                _logger.Info($"Logout, sessionId = {session.Guid}: {reason}");
                LoginReject(reason);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnSubscribeReport(ClientSession session, SubscribeRequestClientContext SubscribeRequestClientContext, SubscribeReport message)
        {
            try
            {
                _logger.Info($"Client subscribed, sessionId = {session.Guid}");
                Subscribed();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
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
