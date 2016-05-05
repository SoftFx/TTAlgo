using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Utility
{
    public class RevertableDataSeries<T>
    {
        private DataSeries<T> _series;
        private Dictionary<int, T> _cache;

        public RevertableDataSeries(DataSeries<T> series)
        {
            _series = series;

            Initialize();
        }

        private void Initialize()
        {
            _cache = new Dictionary<int, T>();
        }

        public T this[int index]
        {
            get { return _series[index]; }
            set
            {
                if (!_cache.ContainsKey(index))
                {
                    _cache[index] = _series[index];
                }
                _series[index] = value;
            }
        }

        protected void ClearCache()
        {
            _cache.Clear();
        }

        public void ApplyChanges()
        {
            ClearCache();
        }

        public void RevertChanges(bool isNextStep = true)
        {
            var shift = isNextStep ? 1 : 0;
            foreach (var pair in _cache)
            {
                _series[pair.Key + shift] = pair.Value;
            }
            ClearCache();
        }
    }
}
