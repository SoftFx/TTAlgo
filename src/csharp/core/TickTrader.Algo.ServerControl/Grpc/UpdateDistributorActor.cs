﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using AlgoServerApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl.Grpc
{
    internal sealed class UpdateDistributorActor : Actor
    {
        private readonly IAlgoServerProvider _server;
        private readonly ILogger _logger;
        private readonly Dictionary<string, PluginSubNode> _logSubs = new Dictionary<string, PluginSubNode>();
        private readonly Dictionary<string, PluginSubNode> _statusSubs = new Dictionary<string, PluginSubNode>();


        private UpdateDistributorActor(IAlgoServerProvider server, ILogger logger)
        {
            _server = server;
            _logger = logger;

            Receive<UpdateDistributorController.AddPluginLogsSubRequest>(r => AddSession(_logSubs, r.PluginId, r.Session));
            Receive<UpdateDistributorController.RemovePluginLogsSubRequest>(r => RemoveSession(_logSubs, r.PluginId, r.SessionId));
            Receive<UpdateDistributorController.AddPluginStatusSubRequest>(r => AddSession(_statusSubs, r.PluginId, r.Session));
            Receive<UpdateDistributorController.RemovePluginStatusSubRequest>(r => RemoveSession(_statusSubs, r.PluginId, r.SessionId));
        }


        public static IActorRef Create(IAlgoServerProvider server, ILogger logger)
        {
            return ActorSystem.SpawnLocal(() => new UpdateDistributorActor(server, logger), nameof(UpdateDistributorActor));
        }


        protected override void ActorInit(object initMsg)
        {
            var _ = DispatchLoop();
        }


        private static void AddSession(Dictionary<string, PluginSubNode> map, string pluginId, ServerSession.Handler session)
        {
            if (!map.TryGetValue(pluginId, out var subNode))
            {
                subNode = new PluginSubNode(pluginId);
                map.Add(pluginId, subNode);
            }
            subNode.AddSession(session);
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
                var status = await _server.GetBotStatusAsync(new Domain.ServerControl.PluginStatusRequest { PluginId = id });

                if (node.IsEmpty || StopToken.IsCancellationRequested)
                    return;

                if (!string.IsNullOrEmpty(status))
                {
                    var update = new AlgoServerApi.PluginStatusUpdate { PluginId = id, Message = status };
                    if (TryPackUpdate(update, out var packedUpdate, true))
                        node.DispatchMessage(packedUpdate);
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
                var logs = await _server.GetBotLogsAsync(new Domain.ServerControl.PluginLogsRequest { PluginId = id, MaxCount = 100, LastLogTimeUtc = node.LastRequestTime });

                if (node.IsEmpty || StopToken.IsCancellationRequested)
                    return;

                if (logs.Length > 0)
                {
                    node.LastRequestTime = logs[logs.Length - 1].TimeUtc;
                    var update = new AlgoServerApi.PluginLogUpdate { PluginId = id };
                    update.Records.AddRange(logs.Select(lr => lr.ToApi()));
                    if (TryPackUpdate(update, out var packedUpdate, true))
                        node.DispatchMessage(packedUpdate);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to dispatch log update for plugin {id}");
            }
        }

        private bool TryPackUpdate(IMessage update, out AlgoServerApi.UpdateInfo packedUpdate, bool compress = false)
        {
            packedUpdate = null;

            try
            {
                if (!AlgoServerApi.UpdateInfo.TryPack(update, out packedUpdate, compress))
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
            private readonly LinkedList<ServerSession.Handler> _sessions = new LinkedList<ServerSession.Handler>();


            public string PluginId { get; }

            public Timestamp LastRequestTime { get; set; }

            public bool IsEmpty => _sessions.Count == 0;


            public PluginSubNode(string pluginId)
            {
                PluginId = pluginId;
                LastRequestTime = new Timestamp();
            }


            public void AddSession(ServerSession.Handler session)
            {
                _sessions.AddLast(session);
            }

            public void RemoveSession(string sessionId)
            {
                var node = _sessions.First;
                while (node != null)
                {
                    if (node.Value.SessionId == sessionId)
                        break;

                    node = node.Next;
                }

                if (node != null)
                {
                    _sessions.Remove(node);
                }
            }

            public void DispatchMessage(IMessage msg)
            {
                var node = _sessions.First;
                while (node != null)
                {
                    var session = node.Value;
                    var nextNode = node.Next;
                    if (!session.IsFaulted)
                    {
                        session.SendUpdate(msg);
                    }
                    else
                    {
                        _sessions.Remove(node);
                    }
                    node = nextNode;
                }
            }
        }
    }
}