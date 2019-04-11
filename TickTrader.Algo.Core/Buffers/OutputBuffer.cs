using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class OutputBuffer<T> : IPluginDataBuffer<T>, IReaonlyDataBuffer, IReaonlyDataBuffer<T>, IBuffer
    {
        private CircularList<T> data = new CircularList<T>();
        private BuffersCoordinator coordinator;
        private ValueFactory<T> valueFactory;

        internal static OutputBuffer<T> Create(BuffersCoordinator coordinator, bool isHiddenEntity)
        {
            if (isHiddenEntity)
                return new EntityOutputBuffer(coordinator);
            return new OutputBuffer<T>(coordinator);
        }

        private OutputBuffer(BuffersCoordinator coordinator)
        {
            this.coordinator = coordinator;
            this.valueFactory = ValueFactory.Get<T>();

            coordinator.RegisterBuffer(this);
        }

        protected virtual T InitValue(int index)
        {
            return valueFactory.GetNewValue();
        }

        protected void OnUpdated(int index, T val)
        {
            Updated?.Invoke(index, val);
        }

        public virtual T this[int index]
        {
            get { return data[index]; }
            set
            {
                data[index] = value;
                OnUpdated(index, value);
            }
        }

        public int Count { get { return data.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }
        public Action<int, T> Appended { get; set; }
        public Action<int, T> Updated { get; set; }
        public Action<int> Truncating { get; set; }
        public Action<int> Truncated { get; set; }
        public Action Cleared { get; set; }
        //public event Action Cleared;
        public Action BeginBatchBuild { get; set; }
        public Action EndBatchBuild { get; set; }

        object IReaonlyDataBuffer.this[int index] { get { return this[index]; } }

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
            var index = data.Count;
            var initialValue = InitValue(index);
            data.Add(initialValue);
            Appended?.Invoke(index, initialValue);
        }

        void IBuffer.Truncate(int size)
        {
            Truncating?.Invoke(size);
            data.TruncateStart(size);
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

        //private void Coordinator_BuffersCleared()
        //{
        //    data.Clear();
        //    if (Cleared != null)
        //        Cleared();
        //}

        internal class EntityOutputBuffer : OutputBuffer<T>
        {
            public EntityOutputBuffer(BuffersCoordinator coordinator)
                : base(coordinator)
            {
            }

            protected override T InitValue(int index)
            {
                var newVal = base.InitValue(index);
                ((IFixedEntry<T>)newVal).Changed = v => OnUpdated(index, v);
                return newVal;
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
