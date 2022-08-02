using System;
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

        private TaskCompletionSource<bool> _initTaskSrc;


        public Action<PluginStatusUpdate> StatusUpdateCallback { get; set; }

        public Action<PluginLogRecord> LogUpdateCallback { get; set; }


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
            StatusUpdateCallback?.Invoke(update);
        }

        private void OnLogUpdate(PluginLogRecord logRecord)
        {
            LogUpdateCallback?.Invoke(logRecord);
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
