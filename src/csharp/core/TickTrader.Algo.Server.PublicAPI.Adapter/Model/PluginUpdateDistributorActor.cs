using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.Server.PublicAPI.Converters;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    internal sealed class PluginUpdateDistributorActor : Actor
    {
        private readonly AlgoServerAdapter _server;
        private readonly ILogger _logger;
        private readonly MessageFormatter _msgFormatter;
        private readonly Dictionary<string, PluginSubNode> _logSubs = new Dictionary<string, PluginSubNode>();
        private readonly Dictionary<string, PluginSubNode> _statusSubs = new Dictionary<string, PluginSubNode>();


        private PluginUpdateDistributorActor(AlgoServerAdapter server, ILogger logger, MessageFormatter msgFormatter)
        {
            _server = server;
            _logger = logger;
            _msgFormatter = msgFormatter;

            Receive<PluginUpdateDistributor.AddPluginLogsSubRequest>(r => AddPluginLogSub(r.PluginId, r.Session));
            Receive<PluginUpdateDistributor.RemovePluginLogsSubRequest>(r => RemoveSession(_logSubs, r.PluginId, r.SessionId));
            Receive<PluginUpdateDistributor.AddPluginStatusSubRequest>(r => AddSession(_statusSubs, r.PluginId, r.Session));
            Receive<PluginUpdateDistributor.RemovePluginStatusSubRequest>(r => RemoveSession(_statusSubs, r.PluginId, r.SessionId));
        }


        public static IActorRef Create(AlgoServerAdapter server, ILogger logger, MessageFormatter msgFormatter)
        {
            return ActorSystem.SpawnLocal(() => new PluginUpdateDistributorActor(server, logger, msgFormatter), nameof(PluginUpdateDistributorActor));
        }


        protected override void ActorInit(object initMsg)
        {
            var _ = DispatchLoop();
        }


        private static PluginSubNode AddSession(Dictionary<string, PluginSubNode> map, string pluginId, SessionHandler session)
        {
            if (!map.TryGetValue(pluginId, out var subNode))
            {
                subNode = new PluginSubNode(pluginId);
                map.Add(pluginId, subNode);
            }
            subNode.AddSession(session);
            return subNode;
        }

        private void AddPluginLogSub(string pluginId, SessionHandler session)
        {
            var node = AddSession(_logSubs, pluginId, session);
            var snapshot = node.GetLogsCacheSnapshot();
            if (TryPackUpdate(snapshot, out var packedSnapshot, true))
            {
                var logMsg = _msgFormatter.FormatUpdateToClient(snapshot, packedSnapshot.Payload.Length, packedSnapshot.Compressed);
                session.TryWrite(packedSnapshot, logMsg);
            }
        }

        private static void RemoveSession(Dictionary<string, PluginSubNode> map, string pluginId, string sessionId)
        {
            if (!map.TryGetValue(pluginId, out var subNode))
                return;

            subNode.RemoveSession(sessionId);
            if (subNode.IsEmpty)
                map.Remove(pluginId);
        }


        private async Task DispatchLoop()
        {
            while (!StopToken.IsCancellationRequested)
            {
                try
                {
                    var t1 = Task.WhenAll(_logSubs.Values.Select(sub => DispatchLogUpdate(sub)));
                    var t2 = Task.WhenAll(_statusSubs.Values.Select(sub => DispatchStatusUpdate(sub)));

                    await Task.WhenAll(t1, t2);

                    _logSubs.Where(node => node.Value.IsEmpty).ToArray().ForEach(node => _logSubs.Remove(node.Key));
                    _statusSubs.Where(node => node.Value.IsEmpty).ToArray().ForEach(node => _statusSubs.Remove(node.Key));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Dispatch loop iteration failed");
                }

                await Task.Delay(1000, StopToken);
            }
        }

        private async Task DispatchStatusUpdate(PluginSubNode node)
        {
            var id = node.PluginId;
            try
            {
                var statusRes = await _server.GetBotStatusAsync(new Domain.ServerControl.PluginStatusRequest { PluginId = id });

                if (string.IsNullOrEmpty(statusRes.PluginId))
                {
                    node.RemoveAllSessions();
                    return;
                }

                if (node.IsEmpty || StopToken.IsCancellationRequested)
                    return;

                var status = statusRes.Status;
                if (!string.IsNullOrEmpty(status))
                {
                    var update = new PluginStatusUpdate { PluginId = id, Message = status };
                    if (TryPackUpdate(update, out var packedUpdate, true))
                    {
                        var logMsg = _msgFormatter.FormatUpdateToClient(update, packedUpdate.Payload.Length, packedUpdate.Compressed);
                        node.DispatchUpdate(packedUpdate, logMsg);
                    }
                }
                else
                {
                    node.CleanupSessions();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to dispatch status update for plugin {id}");
            }
        }

        private async Task DispatchLogUpdate(PluginSubNode node)
        {
            var id = node.PluginId;
            try
            {
                var logsRes = await _server.GetBotLogsAsync(new Domain.ServerControl.PluginLogsRequest { PluginId = id, MaxCount = PluginSubNode.MaxLogsPerMessage, LastLogTimeUtc = node.LastRequestTime });

                if (string.IsNullOrEmpty(logsRes.PluginId))
                {
                    node.RemoveAllSessions();
                    return;
                }

                if (node.IsEmpty || StopToken.IsCancellationRequested)
                    return;

                var logs = logsRes.Logs;
                if (logs.Count > 0)
                {
                    var update = node.UpdateLogsCache(logs);
                    if (TryPackUpdate(update, out var packedUpdate, true))
                    {
                        var logMsg = _msgFormatter.FormatUpdateToClient(update, packedUpdate.Payload.Length, packedUpdate.Compressed);
                        node.DispatchUpdate(packedUpdate, logMsg);
                    }
                }
                else
                {
                    node.CleanupSessions();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to dispatch log update for plugin {id}");
            }
        }

        private bool TryPackUpdate(IMessage update, out UpdateInfo packedUpdate, bool compress = false)
        {
            packedUpdate = null;

            try
            {
                if (!UpdateInfo.TryPack(update, out packedUpdate, compress))
                {
                    _logger.Error($"Failed to pack msg '{update.Descriptor.Name}'");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to pack msg '{update.Descriptor.Name}'");
                return false;
            }
        }


        private class PluginSubNode
        {
            internal const int MaxLogsPerMessage = 100;

            private readonly LinkedList<SessionHandler> _sessions = new LinkedList<SessionHandler>();
            private readonly CircularItemCache<LogRecordInfo> _logsCache = new CircularItemCache<LogRecordInfo>(MaxLogsPerMessage);
            private readonly List<LogRecordInfo> _newLogsBuffer = new List<LogRecordInfo>(MaxLogsPerMessage);


            public string PluginId { get; }

            public Timestamp LastRequestTime { get; set; }

            public bool IsEmpty => _sessions.Count == 0;


            public PluginSubNode(string pluginId)
            {
                PluginId = pluginId;
                LastRequestTime = new Timestamp();
            }


            public void AddSession(SessionHandler session)
            {
                _sessions.AddLast(session);
            }

            public void RemoveSession(string sessionId)
            {
                var node = _sessions.First;
                while (node != null)
                {
                    if (node.Value.Id == sessionId)
                        break;

                    node = node.Next;
                }

                if (node != null)
                {
                    _sessions.Remove(node);
                }
            }

            public void DispatchUpdate(UpdateInfo update, string logMsg)
            {
                var node = _sessions.First;
                while (node != null)
                {
                    var session = node.Value;
                    var nextNode = node.Next;
                    if (!session.TryWrite(update, logMsg))
                        _sessions.Remove(node);

                    node = nextNode;
                }
            }

            public void CleanupSessions()
            {
                var node = _sessions.First;
                while (node != null)
                {
                    var session = node.Value;
                    var nextNode = node.Next;
                    if (session.IsClosed)
                        _sessions.Remove(node);

                    node = nextNode;
                }
            }

            public void RemoveAllSessions() => _sessions.Clear();


            internal PluginLogUpdate UpdateLogsCache(IList<Domain.LogRecordInfo> logs)
            {
                _newLogsBuffer.Clear();
                _newLogsBuffer.AddRange(logs.Select(lr => lr.ToApi()));

                _logsCache.AddRange(_newLogsBuffer);

                LastRequestTime = logs[logs.Count - 1].TimeUtc;
                var update = new PluginLogUpdate { PluginId = PluginId };
                update.Records.AddRange(_newLogsBuffer);

                return update;
            }

            internal PluginLogUpdate GetLogsCacheSnapshot()
            {
                var snapshot = new PluginLogUpdate { PluginId = PluginId };
                snapshot.Records.AddRange(_logsCache);
                return snapshot;
            }
        }
    }
}
