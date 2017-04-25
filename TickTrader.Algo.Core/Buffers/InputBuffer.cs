using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class InputBuffer<T> : IPluginDataBuffer<T>, IDataBuffer, IDataBuffer<T>, IBuffer
    {
        private CircularList<T> data = new CircularList<T>();
        private BuffersCoordinator coordinator;

        internal InputBuffer(BuffersCoordinator coordinator)
        {
            this.coordinator = coordinator;

            coordinator.RegisterBuffer(this);
        }

        public void Append(T rec)
        {
            data.Add(rec);
            ItemAppended?.Invoke(rec);
        }

        public void AppendRange(IEnumerable<T> recRange)
        {
            data.AddRange(recRange);
            ItemsAppended?.Invoke(recRange);
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
        public event Action<IEnumerable<T>> ItemsAppended = delegate { };
        public event Action<int, T> ItemUpdated = delegate { };

        public T Last
        {
            get { return this[Count - 1]; }
            set { this[Count - 1] = value; }
        }

        object IDataBuffer.this[int index] { get { return data[index]; } set { data[index] = (T)value; } }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        #region IBuffer

        void IBuffer.Extend()
        {

        }

        void IBuffer.Truncate(int size)
        {
            data.TruncateStart(size);
        }

        void IBuffer.Clear()
        {
            data.Clear();
        }

        void IBuffer.BeginBatch()
        {
        }

        void IBuffer.EndBatch()
        {
        }

        #endregion
    }
}
