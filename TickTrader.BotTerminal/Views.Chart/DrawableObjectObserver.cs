using Machinarium.Qnil;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.IndicatorHost;

namespace TickTrader.BotTerminal
{
    internal class DrawableObjectObserver
    {
        private readonly VarDictionary<(string, string), DrawableObjectViewModel> _drawables = new();

        private readonly IVarList<DrawableCollectionProxy> _drawableCollections;

        private bool _disposed;


        public ChartHostProxy ChartHost { get; }

        public IObservableList<DrawableObjectViewModel> DrawableObjects { get; }


        public DrawableObjectObserver(ChartHostProxy chartHost)
        {
            ChartHost = chartHost;

            DrawableObjects = _drawables.TransformToList().AsObservable();

            // Sync collection updates on GUI thread
            _drawableCollections = chartHost.Drawables.UseSyncContext().ToList();
            _drawableCollections.Updated += DrawableCollectionsUpdated;

            _ = UpdateLoop();
        }


        public void Dispose()
        {
            _disposed = true;

            DrawableObjects.Dispose();
            _drawableCollections.Updated -= DrawableCollectionsUpdated;
            _drawableCollections.Dispose();
        }

        private async Task UpdateLoop()
        {
            while (!_disposed)
            {
                UpdateDrawables();
                await Task.Delay(250);
            }
        }

        private void UpdateDrawables()
        {
            foreach (var collection in _drawableCollections.Snapshot)
            {
                var pluginId = collection.PluginId;
                var updates = collection.TakePendingUpdates();
                foreach (var update in updates)
                {
                    var key = (pluginId, update.Name);
                    switch (update.Action)
                    {
                        case Update.Types.Action.Added:
                        case Update.Types.Action.Updated:
                            if (_drawables.TryGetValue(key, out var objectVM))
                                objectVM.Update(update.Info);
                            else
                                _drawables[key] = new DrawableObjectViewModel(pluginId, update.Info);
                            break;
                        case Update.Types.Action.Removed:
                            _drawables.Remove(key);
                            break;
                    }
                }
            }
        }

        private void DrawableCollectionsUpdated(ListUpdateArgs<DrawableCollectionProxy> args)
        {
            if (args.Action == DLinqAction.Remove)
            {
                var pluginId = args.OldItem.PluginId;

                var toDeleteKeys = _drawables.Snapshot.Where(p => p.Key.Item1 == pluginId).Select(p => p.Key).ToArray();
                foreach (var key in toDeleteKeys)
                    _drawables.Remove(key);
            }
        }
    }
}
