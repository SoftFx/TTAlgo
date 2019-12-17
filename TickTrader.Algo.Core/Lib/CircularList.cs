using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class CircularList<T> : IReadOnlyList<T>, IList<T>
    {
        private static readonly T[] emptyBuffer = new T[0];

        private int begin = 0;
        private int end = -1;
        private T[] buffer;

        public CircularList()
        {
            buffer = emptyBuffer;
        }

        public CircularList(int capacity)
        {
            buffer = new T[capacity];
        }

        public int Capacity { get { return buffer.Length; } }

        public void Enqueue(T item)
        {
            Add(item);
        }

        public virtual void Add(T item)
        {
            if (Count == Capacity)
                Expand();

            if (++end >= Capacity)
                end = 0;

            buffer[end] = item;
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

            T result = buffer[begin];
            buffer[begin] = default(T);

            Count--;

            if (++begin == Capacity)
                begin = 0;

            return result;
        }

        public virtual void Clear()
        {
            if (Count > 0)
                DoTruncateStart(Count);
        }

        public virtual void TruncateStart(int tSize)
        {
            if (tSize == 0)
                return;

            if (tSize < 0 || tSize > Count)
                throw new ArgumentOutOfRangeException();

            DoTruncateStart(tSize);
        }

        protected virtual void DoTruncateStart(int tSize)
        {
            if (begin <= end)
            {
                Array.Clear(buffer, begin, tSize);
                begin += tSize;
            }
            else
            {
                var firstPartLen = Capacity - begin;
                if (tSize < firstPartLen)
                {
                    Array.Clear(buffer, begin, tSize);
                    begin += tSize;
                }
                else if (tSize == firstPartLen)
                {
                    Array.Clear(buffer, begin, tSize);
                    begin = 0;
                }
                else
                {
                    Array.Clear(buffer, begin, firstPartLen);
                    begin = tSize - firstPartLen;
                    Array.Clear(buffer, 0, begin);
                }
            }

            Count -= tSize;

            if (Count == 0)
            {
                begin = 0;
                end = -1;
            }
        }

        public void TruncateEnd(int count)
        {
            throw new NotImplementedException();
        }

        private void Expand()
        {
            int expandBy = Capacity > 0 ? Capacity : 4;

            var oldBuffer = buffer;
            buffer = new T[Capacity + expandBy];

            if (Count != 0)
            {
                if (begin <= end)
                    Array.Copy(oldBuffer, begin, buffer, 0, Count);
                else
                {
                    var firstPartLength = oldBuffer.Length - begin;
                    // copy first part
                    Array.Copy(oldBuffer, begin, buffer, 0, firstPartLength);
                    // copy second part
                    Array.Copy(oldBuffer, 0, buffer, firstPartLength, end + 1);
                }
            }

            begin = 0;
            end = Count - 1;
        }

        private int CalculateBufferIndex(int queueIndex)
        {
            if (queueIndex < 0 || queueIndex >= Count)
                throw new ArgumentOutOfRangeException();

            int realIndex = begin + queueIndex;
            if (realIndex >= buffer.Length)
                realIndex -= buffer.Length;
            return realIndex;
        }

        public T this[int index]
        {
            get { return buffer[CalculateBufferIndex(index)]; }
            set { buffer[CalculateBufferIndex(index)] = value; }
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            if (Count != 0)
            {
                if (begin <= end)
                {
                    for (int i = begin; i <= end; i++)
                        yield return buffer[i];
                }
                else
                {
                    for (int i = begin; i < buffer.Length; i++)
                        yield return buffer[i];

                    for (int i = 0; i <= end; i++)
                        yield return buffer[i];
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this.buffer, 0, array, arrayIndex, this.Count);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
    }
}
