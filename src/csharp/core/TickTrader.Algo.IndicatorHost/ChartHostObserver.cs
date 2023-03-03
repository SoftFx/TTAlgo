using Machinarium.Qnil;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.IndicatorHost
{
    public class ChartHostObserver : IChartHostObserver, IAsyncDisposable
    {
        private readonly VarDictionary<string, PluginOutputModel> _indicators = new();
        private readonly bool _ownsProxy;


        public ChartHostProxy Proxy { get; }

        public IChartInfo Info => Proxy.Info;

        public IVarSet<string, PluginOutputModel> Indicators => _indicators;

        public IVarList<OutputSeriesProxy> Outputs { get; }

        public IVarList<DrawableCollectionProxy> Drawables { get; }


        public ChartHostObserver(ChartHostProxy proxy, bool ownsProxy)
        {
            Proxy = proxy;
            _ownsProxy = ownsProxy;

            Outputs = _indicators.TransformToList().Chain().SelectMany(m => m.Outputs);
            Drawables = _indicators.TransformToList((k, v) => v.Drawables);
            proxy.AddObserver(this);
        }


        public async ValueTask DisposeAsync()
        {
            Outputs.Dispose();
            Drawables.Dispose();
            if (_ownsProxy)
                await Proxy.DisposeAsync();
            else
                Proxy.RemoveObserver(this);
        }

        public void OnUpdate(PluginModelUpdate update)
        {
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

        public void OnStateUpdate(PluginStateUpdate update)
        {
            if (!_indicators.TryGetValue(update.Id, out var model))
                return;

            model.UpdateState(update);
        }

        public void OnOutputUpdate(string pluginId, OutputSeriesUpdate update)
        {
            if (!_indicators.TryGetValue(pluginId, out var model))
                return;

            model.OnOutputUpdate(update);
        }

        public void OnDrawableUpdate(string pluginId, DrawableCollectionUpdate update)
        {
            if (!_indicators.TryGetValue(pluginId, out var model))
                return;

            model.OnDrawableUpdate(update);
        }
    }
}
