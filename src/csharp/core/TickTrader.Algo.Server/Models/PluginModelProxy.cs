using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class PluginModelProxy : IDisposable
    {
        private readonly IActorRef _ref;
        private readonly Channel<object> _downlink;
        private readonly MessageCache<PluginLogRecord> _logs = new MessageCache<PluginLogRecord>(1000);
        private readonly SubList<Listener<PluginLogRecord>> _logSubs = new SubList<Listener<PluginLogRecord>>();

        private TaskCompletionSource<bool> _initTaskSrc;


        public PluginModelInfo Info { get; private set; }

        public string Status { get; private set; }


        private PluginModelProxy(IActorRef actor)
        {
            _ref = actor;
            _downlink = DefaultChannelFactory.CreateForOneToOne<object>();
        }


        public void Dispose()
        {
            _downlink.Writer.Complete();
        }


        public static async Task<PluginModelProxy> Create(IActorRef actor)
        {
            var proxy = new PluginModelProxy(actor);
            await proxy.Init();
            return proxy;
        }


        public Task Start() => _ref.Ask(PluginActor.StartCmd.Instance);

        public Task Stop() => _ref.Ask(PluginActor.StopCmd.Instance);

        public Task UpdateConfig(PluginConfig newConfig) => _ref.Ask(new PluginActor.UpdateConfigCmd(newConfig));


        private async Task Init()
        {
            _initTaskSrc = new TaskCompletionSource<bool>();
            await _ref.Ask(new AttachProxyDownlinkCmd(_downlink));
            _ = _downlink.Consume(ProcessMsg);
            await _initTaskSrc.Task;
        }

        private void ProcessMsg(object msg)
        {
            switch (msg)
            {
                case EndProxyInitMsg _: _initTaskSrc.TrySetResult(true); break;
                case PluginModelInfo pluginModelInfo: OnUpdate(pluginModelInfo); break;
                case PluginStateUpdate stateUpdate: OnStateUpdate(stateUpdate); break;
                case PluginStatusUpdate statusUpdate: OnStatusUpdate(statusUpdate); break;
                case PluginLogRecord logUpdate: OnLogUpdate(logUpdate); break;
            }
        }

        private void OnUpdate(PluginModelInfo info)
        {
            Info = info;
        }

        private void OnStateUpdate(PluginStateUpdate update)
        {
            Info.State = update.State;
            Info.FaultMessage = update.FaultMessage;
        }

        private void OnStatusUpdate(PluginStatusUpdate update)
        {
            Status = update.Message;
        }

        private void OnLogUpdate(PluginLogRecord logRecord)
        {
            lock (_logs)
            {
                _logs.Add(logRecord);
            }

            var subs = _logSubs.Items;
            var n = subs.Length;
            for (var i = 0; i < n; i++)
            {
                subs[i].Handler?.Invoke(logRecord);
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


        private class Listener<T> : IDisposable
            where T : class
        {
            private readonly SubList<Listener<T>> _subList;

            public Action<T> Handler { get; }

            public Listener(Action<T> handler, SubList<Listener<T>> subList)
            {
                Handler = handler;
                _subList = subList;

                subList.AddSub(this);
            }


            public void Dispose()
            {
                _subList.RemoveSub(this);
            }
        }
    }
}
