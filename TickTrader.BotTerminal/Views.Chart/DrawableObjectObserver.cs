using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.IndicatorHost;

namespace TickTrader.BotTerminal
{
    internal class DrawableObjectObserver
    {
        private readonly VarDictionary<string, DrawableCollectionViewModel> _collections = new();

        private bool _disposed;


        public ChartHostObserver ChartHost { get; }

        public IObservableList<DrawableObjectViewModel> DrawableObjects { get; }


        public DrawableObjectObserver(ChartHostObserver chartHost)
        {
            ChartHost = chartHost;

            ChartHost.Drawables.Updated += DrawableCollectionsUpdated;
            DrawableObjects = _collections.TransformToList().Chain().SelectMany(c => c.Objects).Chain().AsObservable();

            _ = UpdateLoop();
        }


        public void Dispose()
        {
            _disposed = true;

            ChartHost.Drawables.Updated -= DrawableCollectionsUpdated;
            DrawableObjects.Dispose();
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
            foreach (var collection in _collections.Snapshot.Values)
            {
                collection.ApplyUpdates();
            }
        }

        private void DrawableCollectionsUpdated(ListUpdateArgs<DrawableCollectionProxy> args)
        {
            if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Remove)
                Execute.OnUIThread(() => UpdateCollections(args));
        }

        private void UpdateCollections(ListUpdateArgs<DrawableCollectionProxy> args)
        {
            if (args.Action == DLinqAction.Insert)
            {
                var proxy = args.NewItem;

                var collectionVM = new DrawableCollectionViewModel(proxy);
                _collections.Add(proxy.PluginId, collectionVM);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var pluginId = args.OldItem.PluginId;

                if (_collections.TryGetValue(pluginId, out var collectionVM))
                {
                    _collections.Remove(pluginId);
                    collectionVM.Dispose();
                }
            }
        }


        private class DrawableCollectionViewModel : IDisposable
        {
            private readonly VarDictionary<string, DrawableObjectViewModel> _objects = new();


            public DrawableCollectionProxy Proxy { get; }

            public IVarList<DrawableObjectViewModel> Objects { get; }


            public DrawableCollectionViewModel(DrawableCollectionProxy proxy)
            {
                Proxy = proxy;

                Objects = _objects.TransformToList();
            }


            public void Dispose()
            {
                Objects.Dispose();
            }


            public void ApplyUpdates()
            {
                var pluginId = Proxy.PluginId;
                var updates = Proxy.TakePendingUpdates();

                foreach (var update in updates)
                {
                    var key = update.ObjName;

                    switch (update.Action)
                    {
                        case CollectionUpdate.Types.Action.Added:
                        case CollectionUpdate.Types.Action.Updated:
                            if (_objects.TryGetValue(key, out var objectVM))
                                objectVM.Update(update.ObjInfo);
                            else
                                _objects[key] = new DrawableObjectViewModel(pluginId, update.ObjInfo);
                            break;
                        case CollectionUpdate.Types.Action.Removed:
                            _objects.Remove(key);
                            break;
                        case CollectionUpdate.Types.Action.Cleared:
                            _objects.Clear();
                            break;
                    }
                }
            }
        }
    }
}
