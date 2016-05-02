using System.Collections.Generic;

namespace TickTrader.Algo.Indicators.Utility
{
    public class RevertableList<T> : List<T>
    {
        private Dictionary<int, T> _cache;
        private int _addedInCache;

        public RevertableList()
        {
            Initialize();
        }

        public RevertableList(int capacity) : base(capacity)
        {
            Initialize();
        }

        public RevertableList(IEnumerable<T> collection) : base(collection)
        {
            Initialize();
        }

        private void Initialize()
        {
            _cache = new Dictionary<int, T>();
            _addedInCache = 0;
        }

        public new T this[int index]
        {
            get { return _cache.ContainsKey(index) ? _cache[index] : base[index]; }
            set { _cache[index] = value; }
        }

        public new void Add(T item)
        {
            _addedInCache++;
            _cache[Count - 1 + _addedInCache] = item;
        }

        protected void ClearCache()
        {
            _cache.Clear();
            _addedInCache = 0;
        }

        public void RevertChanges()
        {
            ClearCache();
        }

        public void ApplyChanges()
        {
            var cnt = Count - 1;
            foreach (var pair in _cache)
            {
                if (pair.Key > cnt)
                {
                    base.Add(pair.Value);
                }
                else
                {
                    base[pair.Key] = pair.Value;
                }
            }
            ClearCache();
        }
    }
}
