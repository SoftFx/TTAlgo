using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class OutputBuffer<T> : IPluginDataBuffer<T>, IReaonlyDataBuffer, IReaonlyDataBuffer<T>
    {
        private List<T> data = new List<T>();
        private BuffersCoordinator coordinator;

        internal OutputBuffer(BuffersCoordinator coordinator)
        {
            this.coordinator = coordinator;

            //coordinator.BuffersCleared += Coordinator_BuffersCleared;
            coordinator.BuffersExtended += () =>
            {
                data.Add(default(T));
                if (Appended != null)
                    Appended(data.Count - 1, default(T));
            };

            coordinator.BeginBatch += () =>
            {
                if (BeginBatchBuild != null)
                    BeginBatchBuild();
            };

            coordinator.EndBatch += () =>
            {
                if (EndBatchBuild != null)
                    EndBatchBuild();
            };
        }

        public T this[int index]
        {
            get { return data[index]; }
            set
            {
                data[index] = value;
                if (Updated != null)
                    Updated(index, value);
            }
        }

        public int Count { get { return data.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }
        public Action<int, T> Appended { get; set; }
        public Action<int, T> Updated { get; set; }
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

        //private void Coordinator_BuffersCleared()
        //{
        //    data.Clear();
        //    if (Cleared != null)
        //        Cleared();
        //}
    }
}
