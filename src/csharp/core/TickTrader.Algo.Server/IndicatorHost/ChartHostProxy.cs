using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ChartHostProxy : IAsyncDisposable
    {
        private readonly IActorRef _actor, _parent;
        private readonly ChartInfo _info;
        private readonly Channel<object> _downlink;


        public IChartInfo Info => _info;


        public event Action<OutputInfo> OutputAdded;
        public event Action<OutputInfo> OutputRemoved;
        public event Action<OutputDataUpdate> OutputUpdated;


        public ChartHostProxy(IActorRef actor, IActorRef parent, ChartInfo info)
        {
            _actor = actor;
            _parent = parent;
            _info = info;
            _downlink = DefaultChannelFactory.CreateForOneToOne<object>();
        }


        public async ValueTask DisposeAsync()
        {
            _downlink.Writer.TryComplete();
            await _parent.Ask(new IndicatorHostModel.RemoveChartCmd(_info.Id));
        }

        public Task AddIndicator(PluginConfig config) => _actor.Ask(new AddIndicatorRequest(config));

        public Task RemoveIndicator(string pluginId) => _actor.Ask(new RemoveIndicatorRequest(pluginId));


        internal async Task Init()
        {
            await _actor.Ask(new AttachDownlinkCmd(_downlink.Writer));
            _ = Task.Run(() => _downlink.Consume(ProcessMsg));
        }


        private void ProcessMsg(object msg)
        {
            switch (msg)
            {
                case OutputAddedMsg addedEvent: OutputAdded?.Invoke(addedEvent.Output); break;
                case OutputRemovedMsg removedEvent: OutputRemoved?.Invoke(removedEvent.Output); break;
                case OutputUpdatedMsg updatedEvent: OutputUpdated?.Invoke(updatedEvent.Update); break;
            }
        }


        public record AddIndicatorRequest(PluginConfig Config);

        public record UpdateIndicatorRequest(PluginConfig Config);

        public record RemoveIndicatorRequest(string PluginId);

        internal record AttachDownlinkCmd(ChannelWriter<object> Sink);

        internal record OutputAddedMsg(OutputInfo Output);

        internal record OutputRemovedMsg(OutputInfo Output);

        internal record OutputUpdatedMsg(OutputDataUpdate Update);
    }
}
