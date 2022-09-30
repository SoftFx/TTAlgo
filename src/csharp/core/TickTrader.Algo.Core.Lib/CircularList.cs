using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TickTrader.Algo.Core.Lib
{
    public class CircularList<T> : IReadOnlyList<T>, IList<T>
    {
        private int _begin = 0, _end = -1, _mask;
        private T[] _buffer;


        public T this[int index]
        {
            get => _buffer[CalculateBufferIndex(index)];
            set => _buffer[CalculateBufferIndex(index)] = value;
        }

        public int Count { get; private set; }

        public int Capacity => _buffer.Length;


        public CircularList()
        {
            _buffer = new T[16];
            _mask = _buffer.Length - 1;
        }

        public CircularList(int capacity)
        {
            _buffer = new T[RoundUpCapacity(capacity)];
            _mask = _buffer.Length - 1;
        }


        public void Enqueue(T item)
        {
            Add(item);
        }

        public virtual void Add(T item)
        {
            if (Count == Capacity)
                Expand();

            _end = (_end + 1) & _mask;

            _buffer[_end] = item;
            Count++;
        }

        public void AddRange(IEnumerable<T> recRange)
        {
            // TO DO : optimization in case recRange is IList or ICollection

            foreach (T rec in recRange)
                Add(rec);
        }

        public virtual T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("List is empty!");

            T result = _buffer[_begin];
            _buffer[_begin] = default(T);

            Count--;

            _begin = (_begin + 1) & _mask;

            return result;
        }

        public virtual void Clear()
        {
            if (Count > 0)
                DoTruncateStart(Count);
        }

        public void TruncateStart(int tSize)
        {
            if (tSize == 0)
                return;

            if (tSize < 0 || tSize > Count)
                throw new ArgumentOutOfRangeException();

            DoTruncateStart(tSize);
        }


        protected virtual void DoTruncateStart(int tSize)
        {
            if (_begin <= _end)
            {
                Array.Clear(_buffer, _begin, tSize);
                _begin += tSize;
            }
            else
            {
                var firstPartLen = Capacity - _begin;
                if (tSize < firstPartLen)
                {
                    Array.Clear(_buffer, _begin, tSize);
                    _begin += tSize;
                }
                else if (tSize == firstPartLen)
                {
                    Array.Clear(_buffer, _begin, tSize);
                    _begin = 0;
                }
                else
                {
                    Array.Clear(_buffer, _begin, firstPartLen);
                    _begin = tSize - firstPartLen;
                    Array.Clear(_buffer, 0, _begin);
                }
            }

            Count -= tSize;

            if (Count == 0)
            {
                _begin = 0;
                _end = -1;
            }
        }


        private void Expand()
        {
            var oldBuffer = _buffer;
            _buffer = new T[2 * Capacity];
            _mask = _buffer.Length - 1;

            CopyUnsafe(oldBuffer, _begin, _end, Count, _buffer.AsSpan());

            _begin = 0;
            _end = Count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateBufferIndex(int queueIndex)
        {
            if (queueIndex < 0 || queueIndex >= Count)
                throw new ArgumentOutOfRangeException();

            return (_begin + queueIndex) & _mask;
            // This code is equivalent to one below
            // Because buffer size guaranteed to be a power of 2 
            //int realIndex = begin + queueIndex;
            //if (realIndex >= buffer.Length)
            //    realIndex -= buffer.Length;
            //return realIndex;
        }


        // Rounds up provided value to power of 2
        private static int RoundUpCapacity(int capacity)
        {
            if (capacity <= 16)
                return 16;

            var v = capacity - 1; // workaround for case when already power of 2
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            return v + 1;
        }

        private static void CopyUnsafe(T[] src, int begin, int end, int cnt, Span<T> dst)
        {
            if (cnt == 0)
                return;

            if (begin <= end)
            {
                src.AsSpan(begin, cnt).CopyTo(dst);
            }
            else
            {
                var part1 = src.AsSpan(begin);
                part1.CopyTo(dst);
                dst.Slice(part1.Length);
                src.AsSpan(0, end + 1).CopyTo(dst);
            }
        }


        #region IList support

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            if (Count != 0)
            {
                if (_begin <= _end)
                {
                    for (int i = _begin; i <= _end; i++)
                        yield return _buffer[i];
                }
                else
                {
                    for (int i = _begin; i < _buffer.Length; i++)
                        yield return _buffer[i];

                    for (int i = 0; i <= _end; i++)
                        yield return _buffer[i];
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item)
        {
            int index = 0;

            foreach (var r in this)
            {
                if (item.Equals(r))
                    return index;

                index++;
            }

            return -1;
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("Array not large enough", nameof(array));

            CopyUnsafe(_buffer, _begin, _end, Count, array.AsSpan(arrayIndex));
        }

        public void Insert(int index, T item) => throw new NotSupportedException("FIFO modication only");

        public void RemoveAt(int index) => throw new NotSupportedException("FIFO modication only");

        public bool Remove(T item) => throw new NotSupportedException("FIFO modification only");

        #endregion IList support
    }
}
