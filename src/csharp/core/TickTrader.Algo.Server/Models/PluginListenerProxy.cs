using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class PluginListenerProxy : IDisposable
    {
        private readonly IActorRef _server;
        private readonly string _pluginId;
        private readonly Channel<object> _downlink;
        private readonly object _logsLock = new object();

        private TaskCompletionSource<bool> _initTaskSrc;
        private PluginStatusUpdate _lastStatus;
        private CircularList<PluginLogRecord> _lastLogs = new CircularList<PluginLogRecord>(200);


        public int MaxLastLogs { get; set; } = 200;


        internal PluginListenerProxy(IActorRef server, string pluginId)
        {
            _server = server;
            _pluginId = pluginId;
            _downlink = DefaultChannelFactory.CreateForOneToOne<object>();
        }


        public void Dispose()
        {
            _downlink.Writer.TryComplete();
        }


        public async Task Init()
        {
            _initTaskSrc = new TaskCompletionSource<bool>();
            await _server.Ask(new LocalAlgoServer.ExecPluginCmd(_pluginId, new AttachProxyDownlinkCmd(_downlink)));
            _ = Task.Run(() => _downlink.Consume(ProcessMsg));
            await _initTaskSrc.Task;
        }

        public PluginStatusUpdate GetLastStatus() => _lastStatus;

        public PluginLogRecord[] GetLastLogs()
        {
            lock (_logsLock)
            {
                var res = _lastLogs.ToArray();
                _lastLogs.Clear();
                return res;
            }
        }


        private void ProcessMsg(object msg)
        {
            switch (msg)
            {
                case EndProxyInitMsg _: _initTaskSrc.TrySetResult(true); break;
                case PluginStatusUpdate statusUpdate: OnStatusUpdate(statusUpdate); break;
                case PluginLogRecord logUpdate: OnLogUpdate(logUpdate); break;
            }
        }

        private void OnStatusUpdate(PluginStatusUpdate update)
        {
            _lastStatus = update;
        }

        private void OnLogUpdate(PluginLogRecord logRecord)
        {
            lock (_logsLock)
            {
                while (_lastLogs.Count >= MaxLastLogs)
                    _lastLogs.Dequeue();

                _lastLogs.Enqueue(logRecord);
            }
        }


        internal class AttachProxyDownlinkCmd
        {
            public ChannelWriter<object> Sink { get; set; }

            public AttachProxyDownlinkCmd(ChannelWriter<object> sink)
            {
                Sink = sink;
            }
        }

        internal class EndProxyInitMsg : Singleton<EndProxyInitMsg> { }
    }
}
