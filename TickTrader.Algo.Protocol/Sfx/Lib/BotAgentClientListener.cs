using NLog;
using SoftFX.Net.BotAgent;
using System;
using System.Threading.Tasks;
using SfxProtocol = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx.Lib
{
    internal class BotAgentClientListener : ClientSessionListener
    {
        private readonly ILogger _logger;
        private IBotAgentClient _client;


        public event Action Connected = delegate { };
        public event Action<string> ConnectionError = delegate { };
        public event Action Disconnected = delegate { };
        public event Action<int> Login = delegate { };
        public event Action<string> LoginReject = delegate { };
        public event Action<string> Logout = delegate { };
        public event Action Subscribed = delegate { };


        public BotAgentClientListener(IBotAgentClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }


        public override void OnConnect(ClientSession clientSession)
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

        public override void OnConnect(ClientSession clientSession, ConnectClientContext connectContext)
        {
            OnConnect(clientSession);
        }

        public override void OnConnectError(ClientSession clientSession, string text)
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

        public override void OnConnectError(ClientSession clientSession, ConnectClientContext connectContext, string text)
        {
            OnConnectError(clientSession, text);
        }

        public override void OnDisconnect(ClientSession clientSession, ClientContext[] contexts, string text)
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

        public override void OnDisconnect(ClientSession clientSession, DisconnectClientContext disconnectContext, ClientContext[] contexts, string text)
        {
            OnDisconnect(clientSession, contexts, text);
        }

        public override void OnLoginReport(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport message)
        {
            try
            {
                _logger.Info($"Successfull login, sessionId = {session.Guid}");
                Login(session.ServerMinorVersion);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnLoginReport_1(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport_1 message)
        {
            try
            {
                _logger.Info($"Successfull login, sessionId = {session.Guid}");
                Login(session.ServerMinorVersion);
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
                    case SfxProtocol.LoginRejectReason.InvalidCredentials:
                        reason = "Invalid username or password";
                        break;
                    case SfxProtocol.LoginRejectReason.VersionMismatch:
                        reason = message.Text;
                        break;
                    case SfxProtocol.LoginRejectReason.InternalServerError:
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
                    case SfxProtocol.LogoutReason.ClientRequest:
                        reason = "Client requested logout";
                        break;
                    case SfxProtocol.LogoutReason.ServerLogout:
                        reason = "Server forced logout";
                        break;
                    case SfxProtocol.LogoutReason.InternalServerError:
                        reason = $"Internal server error: {message.Text}";
                        break;
                }
                _logger.Info($"Logout, sessionId = {session.Guid}: {reason}");
                Logout(reason);
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
                    case SfxProtocol.LogoutReason.ClientRequest:
                        reason = "Client requested logout";
                        break;
                    case SfxProtocol.LogoutReason.ServerLogout:
                        reason = "Server forced logout";
                        break;
                    case SfxProtocol.LogoutReason.InternalServerError:
                        reason = $"Internal server error: {message.Text}";
                        break;
                }
                _logger.Info($"Logout, sessionId = {session.Guid}: {reason}");
                Logout(reason);
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
                var reportEntity = message.ToEntity();
                if (!ProcessReport(AccountListRequestClientContext, reportEntity))
                {
                    _client.InitAccountList(reportEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnAccountListReport_1(ClientSession session, AccountListRequestClientContext AccountListRequestClientContext, AccountListReport_1 message)
        {
            try
            {
                var reportEntity = message.ToEntity();
                if (!ProcessReport(AccountListRequestClientContext, reportEntity))
                {
                    _client.InitAccountList(reportEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnBotListReport(ClientSession session, BotListRequestClientContext BotListRequestClientContext, BotListReport message)
        {
            try
            {
                var reportEntity = message.ToEntity();
                if (!ProcessReport(BotListRequestClientContext, reportEntity))
                {
                    _client.InitBotList(reportEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnPackageListReport(ClientSession session, PackageListRequestClientContext PackageListRequestClientContext, PackageListReport message)
        {
            try
            {
                var reportEntity = message.ToEntity();
                if (!ProcessReport(PackageListRequestClientContext, reportEntity))
                {
                    _client.InitPackageList(reportEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnAccountModelUpdate(ClientSession session, AccountModelUpdate message)
        {
            try
            {
                _client.UpdateAccount(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnAccountModelUpdate_1(ClientSession session, AccountModelUpdate_1 message)
        {
            try
            {
                _client.UpdateAccount(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnBotModelUpdate(ClientSession session, BotModelUpdate message)
        {
            try
            {
                _client.UpdateBot(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnPackageModelUpdate(ClientSession session, PackageModelUpdate message)
        {
            try
            {
                _client.UpdatePackage(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnBotStateUpdate(ClientSession session, BotStateUpdate message)
        {
            try
            {
                _client.UpdateBotState(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnAccountStateUpdate(ClientSession session, AccountStateUpdate message)
        {
            try
            {
                _client.UpdateAccountState(message.ToEntity());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }


        /// <summary>
        /// Puts report result into TaskcompletionSource if one is present
        /// </summary>
        /// <returns>true if successful, false otherwise</returns>
        private static bool ProcessReport<T>(ClientContext requestContext, T reportEntity)
        {
            var tcs = requestContext?.Data as TaskCompletionSource<T>;
            if (tcs != null)
            {
                tcs.SetResult(reportEntity);
                return true;
            }
            return false;
        }
    }
}
