using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class CircularList<T> : IReadOnlyList<T>
    {
        private static readonly T[] emptyBuffer = new T[0];

        private int begin = 0;
        private int end = -1;
        private T[] innerBuffer;

        public CircularList()
        {
            innerBuffer = emptyBuffer;
        }

        public CircularList(int capacity)
        {
            innerBuffer = new T[capacity];
        }

        public void Enqueue(T item)
        {
            if (Count == Capacity)
                Expand();

            if (++end >= Capacity)
                end = 0;

            innerBuffer[end] = item;
            Count++;
        }

        public int Capacity { get { return innerBuffer.Length; } }

        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("List is empty!");

            T result = innerBuffer[begin];
            innerBuffer[begin] = default(T);

            Count--;

            if (++begin == Capacity)
                begin = 0;

            return result;
        }

        private void Expand()
        {
            int expandBy = Capacity > 0 ? Capacity : 4;

            T[] oldBuffer = innerBuffer;
            innerBuffer = new T[Capacity + expandBy];

            if (begin < end || Count == 0)
                Array.Copy(oldBuffer, begin, innerBuffer, begin, Count);
            else
            {
                // copy first part
                Array.Copy(oldBuffer, begin, innerBuffer, begin + expandBy, oldBuffer.Length - begin);
                // copy second part
                Array.Copy(oldBuffer, innerBuffer, end + 1);

                begin += expandBy;
            }
        }

        private int CalculateBufferIndex(int queueIndex)
        {
            if (queueIndex < 0 || queueIndex >= Count)
                throw new ArgumentOutOfRangeException();

            int realIndex = begin + queueIndex;
            if (realIndex >= innerBuffer.Length)
                realIndex -= innerBuffer.Length;
            return realIndex;
        }

        public T this[int index]
        {
            get { return innerBuffer[CalculateBufferIndex(index)]; }
            set { innerBuffer[CalculateBufferIndex(index)] = value; }
        }

        public int Count { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return new QueueEnumarator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new QueueEnumarator(this);
        }

        private struct QueueEnumarator : IEnumerator<T>
        {
            private CircularList<T> list;
            private int listIndex;
            private int bufferIndex;

            public T Current { get; private set; }

            public QueueEnumarator(CircularList<T> list) : this()
            {
                this.list = list;
                Reset();
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (listIndex >= list.Count)
                    return false;

                Current = list.innerBuffer[bufferIndex];

                listIndex++;
                bufferIndex++;

                if (bufferIndex >= list.innerBuffer.Length)
                    bufferIndex = 0;

                return true;
            }

            public void Reset()
            {
                listIndex = 0;
                bufferIndex = list.begin;
                Current = default(T);
            }
        }
    }
}
