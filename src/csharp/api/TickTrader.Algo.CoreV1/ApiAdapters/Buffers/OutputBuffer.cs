using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.CoreV1
{
    public class OutputBuffer<T> : IPluginDataBuffer<T>, IReadOnlyDataBuffer, IReadOnlyDataBuffer<T>, IBuffer
    {
        private CircularList<T> _data = new CircularList<T>();
        private BuffersCoordinator _coordinator;
        private ValueFactory<T> _valueFactory;

        internal static OutputBuffer<T> Create(BuffersCoordinator coordinator, bool isHiddenEntity)
        {
            if (isHiddenEntity)
                return new EntityOutputBuffer(coordinator);
            return new OutputBuffer<T>(coordinator);
        }

        private OutputBuffer(BuffersCoordinator coordinator)
        {
            _coordinator = coordinator;
            _valueFactory = ValueFactory.Get<T>();

            coordinator.RegisterBuffer(this);
        }

        protected virtual T InitValue(int index)
        {
            return _valueFactory.GetNewValue();
        }

        protected void OnUpdated(int index, T val)
        {
            Updated?.Invoke(index, val);
        }

        protected virtual void OnBeforeTruncate(int size) { }

        public virtual T this[int index]
        {
            get { return _data[index]; }
            set
            {
                _data[index] = value;
                OnUpdated(index, value);
            }
        }

        public int Count { get { return _data.Count; } }
        public int VirtualPos { get { return _coordinator.VirtualPos; } }
        public Action<int, T> Appended { get; set; }
        public Action<int, T> Updated { get; set; }
        public Action<int> Truncating { get; set; }
        public Action<int> Truncated { get; set; }
        public Action Cleared { get; set; }
        public Action BeginBatchBuild { get; set; }
        public Action EndBatchBuild { get; set; }

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
            var index = _data.Count;
            var initialValue = InitValue(index);
            _data.Add(initialValue);
            Appended?.Invoke(index, initialValue);
        }

        void IBuffer.Truncate(int size)
        {
            Truncating?.Invoke(size);
            OnBeforeTruncate(size);
            _data.TruncateStart(size);
            Truncated?.Invoke(size);
        }

        void IBuffer.Clear()
        {
            Cleared?.Invoke();
        }

        void IBuffer.BeginBatch()
        {
            BeginBatchBuild?.Invoke();
        }

        void IBuffer.EndBatch()
        {
            EndBatchBuild?.Invoke();
        }

        #endregion

        internal class EntityOutputBuffer : OutputBuffer<T>
        {
            public EntityOutputBuffer(BuffersCoordinator coordinator)
                : base(coordinator)
            {
            }

            protected override T InitValue(int index)
            {
                var newVal = base.InitValue(index);
                var entry = (IFixedEntry<T>)newVal;
                entry.UpdateIndex(index); // set initial value
                entry.Changed = OnChanged;
                return newVal;
            }

            protected override void OnBeforeTruncate(int size)
            {
                foreach (var entry in _data)
                {
                    var fixedEntry = (IFixedEntry<T>)entry;
                    fixedEntry.UpdateIndex(-size);
                }
            }

            private void OnChanged(int index, T val)
            {
                if (index < 0)
                    return; // if user hold entity reference too long he can have out of view marker

                OnUpdated(index, val);
            }

            public override T this[int index]
            {
                get { return base[index]; }
                set
                {
                    ((IFixedEntry<T>)base[index]).CopyFrom(value);
                }
            }
        }
    }
}
