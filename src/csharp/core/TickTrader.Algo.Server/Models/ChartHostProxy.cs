using System;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ChartHostProxy : IAsyncDisposable
    {
        private readonly IActorRef _actor, _parent;
        private readonly ChartInfo _info;


        public IChartInfo Info => _info;


        public ChartHostProxy(IActorRef actor, IActorRef parent, ChartInfo info)
        {
            _actor = actor;
            _parent = parent;
            _info = info;
        }


        public async ValueTask DisposeAsync()
        {
            await _parent.Ask(new IndicatorHostModel.RemoveChartCmd(_info.Id));
        }

        public Task AddIndicator(PluginConfig config) => _actor.Ask(new AddIndicatorRequest(config));

        public Task RemoveIndicator(string pluginId) => _actor.Ask(new RemoveIndicatorRequest(pluginId));


        public record AddIndicatorRequest(PluginConfig Config);

        public record RemoveIndicatorRequest(string pluginId);
    }
}
