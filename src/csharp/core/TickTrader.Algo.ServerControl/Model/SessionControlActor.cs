using Google.Protobuf;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;

using AlgoServerApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl.Model
{
    internal sealed class SessionControlActor : Actor
    {
        private const int HeartbeatTimeout = 10000;

        private static readonly AlgoServerApi.UpdateInfo _heartbeat = new AlgoServerApi.UpdateInfo { Type = AlgoServerApi.UpdateInfo.Types.PayloadType.Heartbeat, Payload = ByteString.Empty };

        private readonly IAlgoServerProvider _server;
        private readonly ILogger _logger;
        private readonly MessageFormatter _msgFormatter;
        private readonly Dictionary<string, SessionHandler> _sessions = new Dictionary<string, SessionHandler>();

        private ServerUpdateDistributor _updateDistributor;


        private SessionControlActor(IAlgoServerProvider server, ILogger logger, MessageFormatter msgFormatter)
        {
            _server = server;
            _logger = logger;
            _msgFormatter = msgFormatter;

            Receive<SessionControl.ShutdownCmd>(Shutdown);
            Receive<SessionControl.AddSessionRequest>(AddSession);
            Receive<SessionControl.SessionInfoRequest, SessionInfo>(GetSessionInfo);
            Receive<SessionControl.RemoveSessionCmd>(RemoveSession);
            Receive<SessionControl.OpenUpdatesChannelCmd, Task>(OpenUpdatesChannel);

            Receive<HeartbeatMsg>(OnHeartbeat);
            Receive<CredsChangedMsg>(OnCredsChanged);
        }


        public static IActorRef Create(IAlgoServerProvider server, ILogger logger, MessageFormatter msgFormatter)
        {
            return ActorSystem.SpawnLocal(() => new SessionControlActor(server, logger, msgFormatter), nameof(SessionControlActor));
        }


        protected override void ActorInit(object initMsg)
        {
            _updateDistributor = new ServerUpdateDistributor(_server, _logger, _msgFormatter);

            _server.AdminCredsChanged += OnAdminCredsChanged;
            _server.DealerCredsChanged += OnDealerCredsChanged;
            _server.ViewerCredsChanged += OnViewerCredsChanged;

            Self.Tell(HeartbeatMsg.Instance);
        }


        private async Task Shutdown(SessionControl.ShutdownCmd cmd)
        {
            foreach (var session in _sessions.Values)
            {
                session.Close("Server shutdown");
            }
            _sessions.Clear();

            _server.AdminCredsChanged -= OnAdminCredsChanged;
            _server.DealerCredsChanged -= OnDealerCredsChanged;
            _server.ViewerCredsChanged -= OnViewerCredsChanged;

            await _updateDistributor.Shudown();
        }

        private void AddSession(SessionControl.AddSessionRequest request)
        {
            var session = request.Session;
            var id = session.Id;
            var handler = new SessionHandler(session);
            _sessions.Add(id, handler);

            _logger.Info($"Added session {id}");
        }

        private SessionInfo GetSessionInfo(SessionControl.SessionInfoRequest r)
        {
            if (!_sessions.TryGetValue(r.Id, out var session))
                return null;

            return session.IsClosed ? null : session.Info;
        }

        private void RemoveSession(SessionControl.RemoveSessionCmd cmd)
        {
            var id = cmd.Id;
            if (!_sessions.TryGetValue(id, out var session))
                return;

            _sessions.Remove(id);
            session.Close("Client request");
        }

        private async Task<Task> OpenUpdatesChannel(SessionControl.OpenUpdatesChannelCmd cmd)
        {
            var id = cmd.SessionId;
            if (!_sessions.TryGetValue(id, out var session))
                throw new Domain.AlgoException("Session not found");

            var res = session.Open(cmd.NetworkStream);
            await _updateDistributor.AttachSession(session);
            return res;
        }


        private void OnHeartbeat(HeartbeatMsg msg)
        {
            try
            {
                var sessionsToRemove = _sessions.Where(s => !s.Value.TryWrite(_heartbeat)).Select(s => s.Value).ToList();
                if (sessionsToRemove.Count > 0)
                {
                    try
                    {
                        foreach (var session in sessionsToRemove)
                        {
                            session.Close("Heartbeat error");
                            _sessions.Remove(session.Info.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to cleanup invalid sessions");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send heartbeat");
            }

            TaskExt.Schedule(HeartbeatTimeout, () => Self.Tell(HeartbeatMsg.Instance), StopToken);
        }


        private class HeartbeatMsg : Singleton<HeartbeatMsg> { }


        #region Credentials handlers

        private void OnCredsChanged(CredsChangedMsg msg)
        {
            var level = msg.AccessLevel;
            var sessionsToRemove = _sessions.Values.Where(s => s.Info.AccessManager.Level == level).ToList();
            foreach (var session in sessionsToRemove)
            {
                session.Close("Creds changed");
                _sessions.Remove(session.Info.Id);
            }
        }

        private void OnAdminCredsChanged() => Self.Tell(new CredsChangedMsg(ClientClaims.Types.AccessLevel.Admin));

        private void OnDealerCredsChanged() => Self.Tell(new CredsChangedMsg(ClientClaims.Types.AccessLevel.Dealer));

        private void OnViewerCredsChanged() => Self.Tell(new CredsChangedMsg(ClientClaims.Types.AccessLevel.Viewer));


        private class CredsChangedMsg
        {
            public ClientClaims.Types.AccessLevel AccessLevel { get; }


            public CredsChangedMsg(ClientClaims.Types.AccessLevel accessLevel)
            {
                AccessLevel = accessLevel;
            }
        }

        #endregion
    }
}
