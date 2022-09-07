using Machinarium.Qnil;
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
        private readonly VarDictionary<string, PluginOutputModel> _indicators = new();


        public IChartInfo Info => _info;

        public IVarSet<string, PluginOutputModel> Indicators => _indicators;


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
            _indicators.Clear();
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
                case PluginModelUpdate pluginUpdate: OnPluginUpdate(pluginUpdate); break;
                case PluginStateUpdate stateUpdate: OnPluginStateUpdate(stateUpdate); break;
                case OutputDataUpdatedMsg updatedEvent: OnOutputDataUpdate(updatedEvent); break;
            }
        }

        private void OnPluginUpdate(PluginModelUpdate update)
        {
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    var newModel = new PluginOutputModel();
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
            if (!_indicators.TryGetValue(update.Id, out var model))
                return;

            model.UpdateState(update);
        }

        private void OnOutputDataUpdate(OutputDataUpdatedMsg msg)
        {
            if (!_indicators.TryGetValue(msg.PluginId, out var model))
                return;

            model.OnOutputUpdate(msg.Update);
        }


        public record AddIndicatorRequest(PluginConfig Config);

        public record UpdateIndicatorRequest(PluginConfig Config);

        public record RemoveIndicatorRequest(string PluginId);

        internal record AttachDownlinkCmd(ChannelWriter<object> Sink);

        internal record OutputDataUpdatedMsg(string PluginId, OutputSeriesUpdate Update);
    }
}
