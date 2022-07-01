using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface ITimeVectorRef : IReadOnlyList<UtcTicks>
    {
        event Action Extended;
    }

    public abstract class TimeVectorBase<T> : IReadOnlyList<T>, IDisposable
    {
        private ISynchroniser _sync;
        private RefImpl _ref;

        protected TimeVectorBase()
        {
            _ref = new RefImpl(this);
        }

        public abstract int Count { get; }
        public abstract T this[int index] { get; }
        public int WaitingSyncCount => _sync?.CachedItemsCount ?? 0;
        public bool IsEmpty => Count + WaitingSyncCount == 0;
        public ITimeVectorRef Ref => _ref;

        public virtual void Clear()
        {
            _sync?.Clear();
            ClearInternalCollection();
        }

        protected abstract void ClearInternalCollection();

        protected abstract UtcTicks GetItemTimeCoordinate(T item);

        public void InitSynchronization(ITimeVectorRef masterVector, Func<int, T> fillItemFunc)
        {
            _sync?.Dispose();

            _sync = new Synchroniser(masterVector, this, fillItemFunc);
        }

        protected virtual T Append(T newItem)
        {
            if (_sync != null)
                return _sync.Append(newItem);

            return AppendNoSync(newItem);
        }

        private T AppendNoSync(T newItem)
        {
            AddToInternalCollection(newItem);
            _ref.OnExtended();
            return newItem;
        }

        protected abstract void AddToInternalCollection(T item);

        protected T GetLastItem()
        {
            return GetLastItem(out _);
        }

        protected T GetLastItem(out int index)
        {
            if (_sync != null)
                return _sync.GetLastItem(out index);

            index = Count - 1;
            return LastOrDefault();
        }

        private T LastOrDefault()
        {
            return Count == 0 ? default : this[Count - 1];
        }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Dispose()
        {
            _sync?.Dispose();
        }

        private class RefImpl : ITimeVectorRef
        {
            private TimeVectorBase<T> _vector;

            public RefImpl(TimeVectorBase<T> vector)
            {
                _vector = vector;
            }

            public int Count => _vector.Count;
            public UtcTicks this[int index] => _vector.GetItemTimeCoordinate(_vector[index]);

            public event Action Extended;

            public void OnExtended()
            {
                Extended?.Invoke();
            }

            public IEnumerator<UtcTicks> GetEnumerator()
            {
                return _vector.Select(i => _vector.GetItemTimeCoordinate(i)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private interface ISynchroniser : IDisposable
        {
            int CachedItemsCount { get; }

            void Clear();
            T Append(T newItem);
            T GetLastItem(out int index);
        }

        private class Synchroniser : ISynchroniser
        {
            private ITimeVectorRef _master;
            private TimeVectorBase<T> _slave;
            private int _syncIndex = 0;
            private CircularList<T> _futureItems = new CircularList<T>();
            private Func<int, T> _fillItemFunc;

            public Synchroniser(ITimeVectorRef masterVector, TimeVectorBase<T> slaveVector, Func<int, T> fillItemFunc)
            {
                _master = masterVector;
                _slave = slaveVector;
                _fillItemFunc = fillItemFunc;

                _master.Extended += _master_Extended;
            }

            public int CachedItemsCount => _futureItems.Count;

            public void Clear()
            {
                _syncIndex = 0;
                _futureItems.Clear();
            }

            public T GetLastItem(out int index)
            {
                if (_futureItems.Count > 0)
                {
                    index = -1;
                    return _futureItems[_futureItems.Count - 1];
                }

                index = _slave.Count - 1;
                return _slave.LastOrDefault();
            }

            private void _master_Extended()
            {
                //var masterLastItem = 
                var masterItemTime = _master[_master.Count - 1];

                while (_futureItems.Count > 0)
                {
                    var cachedItem = _futureItems[0];
                    var cachedItemTime = _slave.GetItemTimeCoordinate(cachedItem);

                    if (cachedItemTime < masterItemTime)
                    {
                        // skip
                        _futureItems.Dequeue();
                    }
                    else if (cachedItemTime == masterItemTime)
                    {
                        // take
                        var newItem = _futureItems.Dequeue();
                        SyncTo(newItem);
                        return;
                    }
                    else
                        return;
                }
            }

            public T Append(T newItem)
            {
                if (_master.Count == 0)
                {
                    // no sync possible, cache
                    _futureItems.Enqueue(newItem);
                    return newItem;
                }

                return SyncTo(newItem);
            }

            private T SyncTo(T newItem)
            {
                var newItemTime = _slave.GetItemTimeCoordinate(newItem);

                while (_syncIndex < _master.Count)
                {
                    var masterItemTime = _master[_syncIndex];

                    if (masterItemTime == newItemTime)
                    {
                        // hit
                        _slave.AppendNoSync(newItem);
                        _syncIndex++;
                        return newItem;
                    }
                    else if (newItemTime < masterItemTime)
                    {
                        // too old, skip
                        return default(T);
                    }
                    else
                    {
                        var fillItem = _fillItemFunc(_syncIndex);
                        _slave.AppendNoSync(fillItem);
                        _syncIndex++;
                    }
                }

                // no corresponding bar yet
                _futureItems.Enqueue(newItem);
                return newItem;
            }

            public void Dispose()
            {
                _master.Extended -= _master_Extended;
            }
        }
    }
}