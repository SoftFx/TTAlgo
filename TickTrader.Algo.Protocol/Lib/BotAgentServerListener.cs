using NLog;
using SoftFX.Net.BotAgent;
using System;
using Sfx = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Lib
{
    internal class BotAgentServerListener : ServerListener
    {
        private readonly ILogger _logger;
        private IBotAgentServer _server;


        public event Action<int> SessionSubscribed = delegate { };
        public event Action<int> SessionUnsubscribed = delegate { };


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
                SessionUnsubscribed(session.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }

        public override void OnLoginRequest(Server server, Server.Session session, LoginRequest message)
        {
            try
            {
                var res = _server.ValidateCreds(message.Username, message.Password);
                if (res)
                {
                    var currentVersion = VersionSpec.ResolveVersion(session.ClientMajorVersion, session.ClientMinorVersion, out var reason);
                    if (currentVersion == -1)
                    {
                        session.Send(new LoginReject(0) { Reason = Sfx.LoginRejectReason.VersionMismatch, Text = reason });
                    }
                    else
                    {
                        _logger.Info($"Client {session.Id} logged in");
                        session.Send(new LoginReport(0) { CurrentVersion = currentVersion });
                    }
                }
                else
                {
                    session.Send(new LoginReject(0) { Reason = Sfx.LoginRejectReason.InvalidCredentials });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
                session.Send(new LoginReject(0) { Reason = Sfx.LoginRejectReason.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnLogoutRequest(Server server, Server.Session session, LogoutRequest message)
        {
            try
            {
                _logger.Info($"Client {session.Id} logged out");
                SessionUnsubscribed(session.Id);
                session.Send(new LogoutReport(0) { Reason = Sfx.LogoutReason.ClientRequest });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
                session.Send(new LogoutReport(0) { Reason = Sfx.LogoutReason.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnSubscribeRequest(Server server, Server.Session session, SubscribeRequest message)
        {
            try
            {
                SessionSubscribed(session.Id);
                session.Send(new SubscribeReport(0) { RequestId = message.Id });
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
                var report = _server.GetAccountList();
                report.RequestId = message.Id;
                session.Send(report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }

        public override void OnBotListRequest(Server server, Server.Session session, BotListRequest message)
        {
            try
            {
                var report = _server.GetBotList();
                report.RequestId = message.Id;
                session.Send(report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }

        public override void OnPackageListRequest(Server server, Server.Session session, PackageListRequest message)
        {
            try
            {
                var report = _server.GetPackageList();
                report.RequestId = message.Id;
                session.Send(report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Id}: {ex.Message}");
            }
        }
    }
}
