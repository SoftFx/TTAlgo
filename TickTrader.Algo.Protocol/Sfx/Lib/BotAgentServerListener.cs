using NLog;
using SoftFX.Net.BotAgent;
using System;
using SfxProtocol = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx.Lib
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


        public override void OnConnect(SfxProtocol.Server server, SfxProtocol.Server.Session session)
        {
            try
            {
                _logger.Info($"Connected client {session.Guid}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnDisconnect(SfxProtocol.Server server, SfxProtocol.Server.Session session, ServerContext[] contexts, string text)
        {
            try
            {
                _logger.Info($"Disconnected client {session.Guid}: {text}");
                SessionUnsubscribed(session.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
            }
        }

        public override void OnLoginRequest(SfxProtocol.Server server, SfxProtocol.Server.Session session, LoginRequest message)
        {
            try
            {
                var res = _server.ValidateCreds(message.Username, message.Password);
                if (res)
                {
                    var reason = VersionSpec.CheckClientCompatibility(session.ClientMajorVersion, session.ClientMinorVersion);
                    _logger.Info($"Client {session.Guid} version = {session.ClientMajorVersion}.{session.ClientMinorVersion}. Server version = {VersionSpec.LatestVersion}. {reason}");
                    if (reason != null)
                    {
                        session.Send(new LoginReject(0) { Reason = SfxProtocol.LoginRejectReason.VersionMismatch, Text = reason });
                    }
                    else
                    {
                        _logger.Info($"Client {session.Guid} logged in");
                        session.Send(new LoginReport(0));
                    }
                }
                else
                {
                    session.Send(new LoginReject(0) { Reason = SfxProtocol.LoginRejectReason.InvalidCredentials });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
                session.Send(new LoginReject(0) { Reason = SfxProtocol.LoginRejectReason.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnLogoutRequest(SfxProtocol.Server server, SfxProtocol.Server.Session session, LogoutRequest message)
        {
            try
            {
                _logger.Info($"Client {session.Guid} logged out");
                SessionUnsubscribed(session.Id);
                session.Send(new LogoutReport(0) { Reason = SfxProtocol.LogoutReason.ClientRequest });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
                session.Send(new LogoutReport(0) { Reason = SfxProtocol.LogoutReason.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnSubscribeRequest(SfxProtocol.Server server, SfxProtocol.Server.Session session, SubscribeRequest message)
        {
            try
            {
                SessionSubscribed(session.Id);
                session.Send(new SubscribeReport(0) { RequestId = message.Id, RequestState = SfxProtocol.RequestExecState.Completed });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
                session.Send(new SubscribeReport(0) { RequestId = message.Id, RequestState = SfxProtocol.RequestExecState.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnAccountListRequest(SfxProtocol.Server server, SfxProtocol.Server.Session session, AccountListRequest message)
        {
            try
            {
                var report = _server.GetAccountList();
                report.RequestId = message.Id;
                session.Send(report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
                session.Send(new AccountListReport(0) { RequestId = message.Id, RequestState = SfxProtocol.RequestExecState.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnBotListRequest(SfxProtocol.Server server, SfxProtocol.Server.Session session, BotListRequest message)
        {
            try
            {
                var report = _server.GetBotList();
                report.RequestId = message.Id;
                session.Send(report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
                session.Send(new BotListReport(0) { RequestId = message.Id, RequestState = SfxProtocol.RequestExecState.InternalServerError, Text = ex.Message });
            }
        }

        public override void OnPackageListRequest(SfxProtocol.Server server, SfxProtocol.Server.Session session, PackageListRequest message)
        {
            try
            {
                var report = _server.GetPackageList();
                report.RequestId = message.Id;
                session.Send(report.ToMessage());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Listener failure {session.Guid}: {ex.Message}");
                session.Send(new PackageListReport(0) { RequestId = message.Id, RequestState = SfxProtocol.RequestExecState.InternalServerError, Text = ex.Message });
            }
        }
    }
}
