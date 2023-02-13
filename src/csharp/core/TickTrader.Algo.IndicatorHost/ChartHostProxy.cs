using Machinarium.Qnil;
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
        private readonly VarDictionary<string, PluginOutputModel> _indicators = new();
        private readonly SubList<IChartHostObserver> _eventObservers = new();


        public IChartInfo Info { get; }

        public IVarSet<string, PluginOutputModel> Indicators => _indicators;

        public IVarList<OutputSeriesProxy> Outputs { get; }

        public IVarList<DrawableCollectionProxy> Drawables { get; }


        public ChartHostProxy(IActorRef actor, IActorRef parent, IChartInfo info)
        {
            _actor = actor;
            _parent = parent;
            Info = info;
            _downlink = DefaultChannelFactory.CreateForOneToOne<object>();

            Outputs = _indicators.TransformToList().Chain().SelectMany(m => m.Outputs);
            Drawables = _indicators.TransformToList((k, v) => v.Drawables);
        }


        public async ValueTask DisposeAsync()
        {
            _downlink.Writer.TryComplete();
            _eventObservers.Clear();
            Outputs.Dispose();
            Drawables.Dispose();
            _indicators.Clear();
            await _parent.Ask(new IndicatorHostModel.RemoveChartCmd(Info.Id));
        }

        public void AddObserver(IChartHostObserver observer) => _eventObservers.AddSub(observer);

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

            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    var newModel = new PluginOutputModel(update.Id);
                    newModel.Update(update.Plugin);
                    _indicators.Add(update.Id, newModel);
                    break;
                case Update.Types.Action.Updated:
                    if (!_indicators.TryGetValue(update.Id, out var model))
                        return;
                    model.Update(update.Plugin);
                    break;
                case Update.Types.Action.Removed:
                    _indicators.Remove(update.Id);
                    break;
            }
        }

        private void OnPluginStateUpdate(PluginStateUpdate update)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnStateUpdate(p), update);

            if (!_indicators.TryGetValue(update.Id, out var model))
                return;

            model.UpdateState(update);
        }

        private void OnOutputDataUpdate(OutputDataUpdatedMsg msg)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnOutputUpdate(p.Item1, p.Item2), (msg.PluginId, msg.Update));

            if (!_indicators.TryGetValue(msg.PluginId, out var model))
                return;

            model.OnOutputUpdate(msg.Update);
        }

        private void OnDrawableObjectUpdate(DrawableObjectUpdatedMsg msg)
        {
            _eventObservers.Dispatch(static (o, p) => o.OnDrawableUpdate(p.Item1, p.Item2), (msg.PluginId, msg.Update));

            if (!_indicators.TryGetValue(msg.PluginId, out var model))
                return;

            model.OnDrawableUpdate(msg.Update);
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
