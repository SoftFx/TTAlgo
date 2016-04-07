using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class InputBuffer<T> : IPluginDataBuffer<T>, IDataBuffer, IDataBuffer<T>
    {
        private List<T> data = new List<T>();
        private BuffersCoordinator coordinator;

        internal InputBuffer(BuffersCoordinator coordinator)
        {
            this.coordinator = coordinator;

            coordinator.BuffersCleared += () => data.Clear();
        }

        public void Append(T rec)
        {
            data.Add(rec);
        }

        public void Append(IEnumerable<T> recRange)
        {
            data.AddRange(recRange);
        }

        public T this[int index]
        {
            get { return data[index]; }
            set
            {
                data[index] = value;
                ItemUpdated(index, value);
            }
        }
        public int Count { get { return data.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }
        internal BuffersCoordinator Coordinator { get { return coordinator; } }
        public event Action<T> ItemAppended = delegate { };
        public event Action<int, T> ItemUpdated = delegate { };

        public T Last
        {
            get { return this[Count - 1]; }
            set { this[Count - 1] = value; }
        }

        object IDataBuffer.this[int index] { get { return data[index]; } set { data[index] = (T)value; } }

        void IDataBuffer.Append(object item)
        {
            Append((T)item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}
