using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public class QuoteSubTracker
    {
        private readonly object _syncObj = new object();
        private readonly ConcurrentDictionary<string, int> _bySymbol = new ConcurrentDictionary<string, int>();


        public object SyncObj => _syncObj;


        public bool HasSymbolSubs(string symbol) => _bySymbol.ContainsKey(symbol);

        public bool TryGetDepth(string symbol, out int depth) => _bySymbol.TryGetValue(symbol, out depth);

        public bool ApplyUpdate(QuoteSubUpdate update)
        {
            lock (_syncObj)
            {
                var smb = update.Symbol;

                if (update.IsUpsertAction)
                {
                    var hasValue = _bySymbol.TryGetValue(smb, out var currentDepth);
                    if (!hasValue || (hasValue && currentDepth != update.Depth))
                    {
                        _bySymbol[smb] = update.Depth;
                        return true;
                    }
                }
                else if (update.IsRemoveAction)
                {
                    if (_bySymbol.TryRemove(update.Symbol, out _))
                        return true;
                }

                return false;
            }
        }

        public void ApplyUpdates(IEnumerable<QuoteSubUpdate> updates)
        {
            lock (_syncObj)
            {
                foreach (var update in updates)
                    ApplyUpdate(update);
            }
        }

        public List<QuoteSubUpdate> GetRemoveList()
        {
            lock (_syncObj)
            {
                return _bySymbol.Select(p => QuoteSubUpdate.Remove(p.Key)).ToList();
            }
        }
    }
}
