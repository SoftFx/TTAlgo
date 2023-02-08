using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.IndicatorHost
{
    public class DrawableCollectionProxy
    {
        private const int DefaultUpdateCount = 16;

        private readonly object _syncObj = new();

        private List<DrawableObjectUpdate> _pendingUpdates = new(DefaultUpdateCount);


        public string PluginId { get; }


        public DrawableCollectionProxy(string pluginId)
        {
            PluginId = pluginId;
        }


        public IEnumerable<DrawableObjectUpdate> TakePendingUpdates()
        {
            lock (_syncObj)
            {
                if (_pendingUpdates.Count == 0)
                    return Enumerable.Empty<DrawableObjectUpdate>();

                var res = _pendingUpdates;
                _pendingUpdates = new List<DrawableObjectUpdate>(DefaultUpdateCount);
                return res;
            }
        }

        internal void AddUpdate(DrawableObjectUpdate update)
        {
            lock (_syncObj)
            {
                _pendingUpdates.Add(update);
            }
        }
    }
}
