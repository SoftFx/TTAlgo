﻿using Google.Protobuf;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    internal sealed class SessionControlActor : Actor
    {
        private const int HeartbeatTimeout = 10000;

        private static readonly UpdateInfo _heartbeat = new UpdateInfo { Type = UpdateInfo.Types.PayloadType.Heartbeat, Payload = ByteString.Empty };

        private readonly AlgoServerAdapter _server;
        private readonly ILogger _logger;
        private readonly MessageFormatter _msgFormatter;
        private readonly Dictionary<string, SessionHandler> _sessions = new Dictionary<string, SessionHandler>();
        private readonly string _heartbeatLogMsg;

        private ServerUpdateDistributor _updateDistributor;
        private IActorRef _pluginUpdateDistributorRef;
        private IDisposable _credsChangedSub;


        private SessionControlActor(AlgoServerAdapter server, ILogger logger, MessageFormatter msgFormatter)
        {
            _server = server;
            _logger = logger;
            _msgFormatter = msgFormatter;

            _heartbeatLogMsg = _msgFormatter.LogMessages ? "server > hearbeat" : null;

            Receive<SessionControl.ShutdownCmd>(Shutdown);
            Receive<SessionControl.AddSessionRequest>(AddSession);
            Receive<SessionControl.SessionInfoRequest, SessionInfo>(GetSessionInfo);
            Receive<SessionControl.RemoveSessionCmd>(RemoveSession);
            Receive<SessionControl.OpenUpdatesChannelCmd, Task>(OpenUpdatesChannel);

            Receive<SessionControl.AddPluginLogsSubRequest>(r => PluginUpdateDistributor.AddPluginLogsSub(_pluginUpdateDistributorRef, GetSessionOrThrow(r.SessionId), r.PluginId));
            Receive<SessionControl.RemovePluginLogsSubRequest>(r => PluginUpdateDistributor.RemovePluginLogsSub(_pluginUpdateDistributorRef, r.SessionId, r.PluginId));
            Receive<SessionControl.AddPluginStatusSubRequest>(r => PluginUpdateDistributor.AddPluginStatusSub(_pluginUpdateDistributorRef, GetSessionOrThrow(r.SessionId), r.PluginId));
            Receive<SessionControl.RemovePluginStatusSubRequest>(r => PluginUpdateDistributor.RemovePluginStatusSub(_pluginUpdateDistributorRef, r.SessionId, r.PluginId));

            Receive<HeartbeatMsg>(OnHeartbeat);
            Receive<CredsChangedEvent>(OnCredsChanged);
        }


        public static IActorRef Create(AlgoServerAdapter server, ILogger logger, MessageFormatter msgFormatter)
        {
            return ActorSystem.SpawnLocal(() => new SessionControlActor(server, logger, msgFormatter), nameof(SessionControlActor));
        }


        protected override void ActorInit(object initMsg)
        {
            _updateDistributor = new ServerUpdateDistributor(_server, _logger, _msgFormatter);
            _pluginUpdateDistributorRef = PluginUpdateDistributorActor.Create(_server, _logger, _msgFormatter);

            _credsChangedSub = _server.CredsChanged.Subscribe(Self);

            Self.Tell(HeartbeatMsg.Instance);
        }


        private async Task Shutdown(SessionControl.ShutdownCmd cmd)
        {
            foreach (var session in _sessions.Values)
            {
                session.Close("Server shutdown");
            }
            _sessions.Clear();

            _credsChangedSub.Dispose();

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
            var session = GetSessionOrThrow(cmd.SessionId);

            var res = session.Open(cmd.NetworkStream);
            await _updateDistributor.AttachSession(session);
            return res;
        }

        private SessionHandler GetSessionOrThrow(string id)
        {
            if (!_sessions.TryGetValue(id, out var session))
                throw new Domain.AlgoException("Session not found");

            return session;
        }


        private void OnHeartbeat(HeartbeatMsg msg)
        {
            try
            {
                var sessionsToRemove = _sessions.Where(s => !s.Value.TryWrite(_heartbeat, _heartbeatLogMsg)).Select(s => s.Value).ToList();
                if (sessionsToRemove.Count > 0)
                {
                    try
                    {
                        foreach (var session in sessionsToRemove)
                        {
                            session.Close("Heartbeat error");
                            _sessions.Remove(session.Id);
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

        private void OnCredsChanged(CredsChangedEvent msg)
        {
            var level = msg.AccessLevel;
            var sessionsToRemove = _sessions.Values.Where(s => s.Info.AccessManager.Level == level).ToList();
            foreach (var session in sessionsToRemove)
            {
                session.Close("Creds changed");
                _sessions.Remove(session.Id);
            }
        }


        private class HeartbeatMsg : Singleton<HeartbeatMsg> { }
    }
}
