using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Channels;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class AlertManager : Actor
    {
        private const int MaxCachedAlerts = 10_000;

        private readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlertManager>();

        private readonly AlgoServer _server;
        private readonly MessageCache<AlertRecordInfo> _cache = new MessageCache<AlertRecordInfo>(MaxCachedAlerts);
        private readonly ActorEventSource<AlertRecordInfo> _alertEventSrc = new ActorEventSource<AlertRecordInfo>();
        private readonly TimeKeyGenerator _timeGen = new TimeKeyGenerator();


        private AlertManager(AlgoServer server)
        {
            _server = server;

            Receive<PluginAlertMsg>(OnPluginAlert);
            Receive<ServerAlertMsg>(OnServerAlert);

            Receive<AttachAlertChannelCmd>(AttachAlertChannel);
        }


        public static void Create(AlgoServer server)
        {
            ActorSystem.SpawnLocal(() => new AlertManager(server), $"{nameof(AlertManager)}");
        }


        private void OnPluginAlert(PluginAlertMsg msg)
        {
            var log = msg.LogRecord;
            if (log == null)
                return;

            var pluginId = msg.Id;
            var severity = msg.LogRecord.Severity;
            if (severity != PluginLogRecord.Types.LogSeverity.Alert)
            {
                _logger.Error($"Received log with severity '{severity}' from plugin '{pluginId}'");
                return;
            }

            var alert = new AlertRecordInfo
            {
                PluginId = pluginId,
                Message = log.Message,
                TimeUtc = log.TimeUtc,
            };

            AddAlert(alert);
        }

        private void OnServerAlert(ServerAlertMsg msg)
        {
            var alert = new AlertRecordInfo
            {
                PluginId = "<AlgoServer>",
                Message = msg.Message,
                // In concurrent scenarios we can't guarantee time sequence to be ascending when we receive messages from many thread.
                // Therefore we have to assign our own time. Plugin alerts time is assigned on plugin thread within log time sequence
                TimeUtc = _timeGen.NextKey(DateTime.UtcNow),
            };

            AddAlert(alert);
        }

        private void AttachAlertChannel(AttachAlertChannelCmd cmd)
        {
            var sink = cmd.AlertSink;
            if (sink == null)
                return;

            var from = cmd.From;
            if (from != null)
            {
                foreach (var alert in _cache)
                    if (alert.TimeUtc >= from)
                        sink.TryWrite(alert);
            }

            _alertEventSrc.Subscribe(sink);
        }


        private void AddAlert(AlertRecordInfo alert)
        {
            _cache.Add(alert);
            _alertEventSrc.DispatchEvent(alert);
        }


        internal class PluginAlertMsg
        {
            public string Id { get; }

            public PluginLogRecord LogRecord { get; }

            public PluginAlertMsg(string id, PluginLogRecord logRecord)
            {
                Id = id;
                LogRecord = logRecord;
            }
        }

        internal class ServerAlertMsg
        {
            public string Message { get; }

            public ServerAlertMsg(string message)
            {
                Message = message;
            }
        }

        internal class AttachAlertChannelCmd
        {
            public ChannelWriter<AlertRecordInfo> AlertSink { get; }

            public Timestamp From { get; }

            public AttachAlertChannelCmd(ChannelWriter<AlertRecordInfo> alertSink, Timestamp from)
            {
                AlertSink = alertSink;
                From = from;
            }
        }
    }
}
