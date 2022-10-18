using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class InputBuffer<T> : IPluginDataBuffer<T>, IDataBuffer, IDataBuffer<T>, IBuffer
    {
        private CircularList<T> _data = new CircularList<T>();
        private BuffersCoordinator _coordinator;

        public InputBuffer(BuffersCoordinator coordinator)
        {
            _coordinator = coordinator;

            coordinator.RegisterBuffer(this);
        }

        public void Append(T rec)
        {
            _data.Add(rec);
            ItemAppended?.Invoke(rec);
        }

        public void AppendRange(IEnumerable<T> recRange)
        {
            _data.AddRange(recRange);
            ItemsAppended?.Invoke(recRange);
        }

        public T this[int index]
        {
            get { return _data[index]; }
            set
            {
                _data[index] = value;
                ItemUpdated(index, value);
            }
        }
        public int Count { get { return _data.Count; } }
        public int VirtualPos { get { return _coordinator.VirtualPos; } }
        internal BuffersCoordinator Coordinator { get { return _coordinator; } }
        public event Action<T> ItemAppended = delegate { };
        public event Action<IEnumerable<T>> ItemsAppended = delegate { };
        public event Action<int, T> ItemUpdated = delegate { };

        public T Last
        {
            get { return this[Count - 1]; }
            set { this[Count - 1] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        #region IBuffer

        void IBuffer.Extend()
        {

        }

        void IBuffer.Truncate(int size)
        {
            _data.TruncateStart(size);
        }

        void IBuffer.Clear()
        {
            _data.Clear();
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
