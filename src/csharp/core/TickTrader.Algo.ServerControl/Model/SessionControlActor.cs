using NLog;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.ServerControl.Model
{
    internal sealed class SessionControlActor : Actor
    {
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

            Receive<SessionControl.CreateSessionRequest, SessionInfo>(CreateSession);
            Receive<SessionControl.SessionInfoRequest, SessionInfo>(GetSessionInfo);
        }


        public static IActorRef Create(IAlgoServerProvider server, ILogger logger, MessageFormatter msgFormatter)
        {
            return ActorSystem.SpawnLocal(() => new SessionControlActor(server, logger, msgFormatter), nameof(SessionControlActor));
        }


        protected override void ActorInit(object initMsg)
        {
            _updateDistributor = new ServerUpdateDistributor(_server, _logger, _msgFormatter);
        }


        private SessionInfo CreateSession(SessionControl.CreateSessionRequest request)
        {
            try
            {
                var id = Guid.NewGuid().ToString("N");
                var session = new SessionInfo(id, request.UserId, request.MinorVersion, request.AccessLevel, LoggerHelper.GetSessionLogger(request.LogFactory, id));

                var handler = new SessionHandler(session);
                _sessions.Add(id, handler);

                return session;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to create session");
                return null;
            }
        }

        private SessionInfo GetSessionInfo(SessionControl.SessionInfoRequest r)
        {
            if (!_sessions.TryGetValue(r.Id, out var session))
                return null;

            return session.Info;
        }
    }
}
