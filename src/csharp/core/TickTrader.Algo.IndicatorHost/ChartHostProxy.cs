using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.IndicatorHost
{
    public interface IChartHostObserver
    {
        void OnUpdate(PluginModelUpdate update);

        void OnStateUpdate(PluginStateUpdate update);

        void OnOutputUpdate(string pluginId, OutputSeriesUpdate update);

        void OnDrawableUpdate(string pluginId, DrawableCollectionUpdate update);
    }


    public class ChartHostProxy : IAsyncDisposable
    {
        private readonly IActorRef _actor, _parent;
        private readonly Channel<object> _downlink;
        private readonly SubList<IChartHostObserver> _eventObservers = new();


        public IChartInfo Info { get; }


        public ChartHostProxy(IActorRef actor, IActorRef parent, IChartInfo info)
        {
            _actor = actor;
            _parent = parent;
            Info = info;
            _downlink = DefaultChannelFactory.CreateForOneToOne<object>();
        }


        public async ValueTask DisposeAsync()
        {
            _downlink.Writer.TryComplete();
            _eventObservers.Clear();
            await _parent.Ask(new IndicatorHostModel.RemoveChartCmd(Info.Id));
        }

        public void AddObserver(IChartHostObserver observer) => _eventObservers.AddSub(observer);

        public void RemoveObserver(IChartHostObserver observer) => _eventObservers.RemoveSub(observer);

        public Task AddIndicator(PluginConfig config) => _actor.Ask(new AddIndicatorRequest(config));

        public Task UpdateIndicator(PluginConfig config) => _actor.Ask(new UpdateIndicatorRequest(config));

        public Task RemoveIndicator(string pluginId) => _actor.Ask(new RemoveIndicatorRequest(pluginId));

        public Task ChangeTimeframe(Feed.Types.Timeframe timeframe) => _actor.Ask(new ChangeTimeframeCmd(timeframe));

        public Task ChangeBoundaries(int barsCount) => _actor.Ask(new ChangeBoundariesCmd(barsCount));


        internal async Task Init()
        {
            await _actor.Ask(new AttachDownlinkCmd(_downlink.Writer));
            _ = Task.Run(() => _downlink.Consume(ProcessMsg));
        }


        private void ProcessMsg(object msg)
        {
            switch (msg)
            {
                case PluginModelUpdate pluginUpdate: OnPluginUpdate(pluginUpdate); break;
                case PluginStateUpdate stateUpdate: OnPluginStateUpdate(stateUpdate); break;
                case OutputDataUpdatedMsg outputUpdate: OnOutputDataUpdate(outputUpdate); break;
                case DrawableObjectUpdatedMsg drawableUpdate: OnDrawableObjectUpdate(drawableUpdate); break;
            }
        }

        private void OnPluginUpdate(PluginModelUpdate update)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnUpdate(p), update);
        }

        private void OnPluginStateUpdate(PluginStateUpdate update)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnStateUpdate(p), update);
        }

        private void OnOutputDataUpdate(OutputDataUpdatedMsg msg)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnOutputUpdate(p.Item1, p.Item2), (msg.PluginId, msg.Update));
        }

        private void OnDrawableObjectUpdate(DrawableObjectUpdatedMsg msg)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnDrawableUpdate(p.Item1, p.Item2), (msg.PluginId, msg.Update));
        }


        public record AddIndicatorRequest(PluginConfig Config);

        public record UpdateIndicatorRequest(PluginConfig Config);

        public record RemoveIndicatorRequest(string PluginId);

        public record ChangeTimeframeCmd(Feed.Types.Timeframe Timeframe);

        public record ChangeBoundariesCmd(int? BarsCount);

        internal record AttachDownlinkCmd(ChannelWriter<object> Sink);

        internal record OutputDataUpdatedMsg(string PluginId, OutputSeriesUpdate Update);

        internal record DrawableObjectUpdatedMsg(string PluginId, DrawableCollectionUpdate Update);
    }
}
