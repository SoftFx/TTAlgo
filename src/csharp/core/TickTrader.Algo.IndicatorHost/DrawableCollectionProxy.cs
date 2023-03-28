using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.IndicatorHost
{
    public class DrawableCollectionProxy
    {
        private const int DefaultUpdateCount = 16;

        private readonly object _syncObj = new();

        private List<DrawableCollectionUpdate> _pendingUpdates = new(DefaultUpdateCount);


        public string PluginId { get; }


        public DrawableCollectionProxy(string pluginId)
        {
            PluginId = pluginId;
        }


        public IEnumerable<DrawableCollectionUpdate> TakePendingUpdates()
        {
            lock (_syncObj)
            {
                if (_pendingUpdates.Count == 0)
                    return Enumerable.Empty<DrawableCollectionUpdate>();

                var res = _pendingUpdates;
                _pendingUpdates = new List<DrawableCollectionUpdate>(DefaultUpdateCount);
                return res;
            }
        }

        internal void AddUpdate(DrawableCollectionUpdate update)
        {
            lock (_syncObj)
            {
                _pendingUpdates.Add(update);
            }
        }
    }
}
