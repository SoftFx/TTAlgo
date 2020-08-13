using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public interface IOutputFixture
    {
        void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef);
        void Unbind();
    }

    public class OutputFixture<T> : CrossDomainObject, IOutputFixture
    {
        private OutputBuffer<T> buffer;
        private bool isBatch;
        private ITimeRef timeRef;

        internal void BindTo(OutputBuffer<T> buffer, ITimeRef timeRef)
        {
            Unbind();

            this.buffer = buffer ?? throw new ArgumentNullException("buffer");
            this.timeRef = timeRef ?? throw new ArgumentNullException("timeRef");

            buffer.Appended = OnAppend;
            buffer.Updated = OnUpdate;
            buffer.BeginBatchBuild = () => isBatch = true;
            buffer.EndBatchBuild = () =>
            {
                isBatch = false;
                OnRangeAppend();
            };
            buffer.Truncated = OnTruncate;
            buffer.Truncating = OnTruncating;
        }

        private void OnAppend(int index, T data)
        {
            if (!isBatch)
            {
                var timeCoordinate = timeRef[index];
                Appended(new OutputPoint<T>(timeCoordinate?.ToDateTime(), index, data));
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!isBatch)
            {
                var timeCoordinate = timeRef[index];
                Updated(new OutputPoint<T>(timeCoordinate?.ToDateTime(), index, data));
            }
        }

        private void OnRangeAppend()
        {
            var count = buffer.Count;

            OutputPoint<T>[] list = new OutputPoint<T>[count];

            for (int i = 0; i < count; i++)
            {
                var timeCoordinate = timeRef[i];
                list[i] = new OutputPoint<T>(timeCoordinate?.ToDateTime(), i, buffer[i]);
            }

            RangeAppended(list);
        }

        private void OnTruncate(int truncateSize)
        {
            Truncated?.Invoke(truncateSize);
        }

        private void OnTruncating(int truncateSize)
        {
            Truncating?.Invoke(truncateSize);
        }

        public void Unbind()
        {
            if (buffer != null)
            {
                buffer.Appended = null;
                buffer.Updated = null;
                buffer.BeginBatchBuild = null;
                buffer.EndBatchBuild = null;
                buffer.Truncated = null;
                buffer.Truncating = null;
                buffer = null;
            }
        }

        public void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef)
        {
            BindTo((OutputBuffer<T>)buffer, timeRef);
        }

        internal int Count => buffer.Count;
        internal OutputPoint<T> this[int index] => new OutputPoint<T>(timeRef[index]?.ToDateTime(), index, buffer[index]);
        internal OutputBuffer<T> Buffer => buffer;

        public event Action<OutputPoint<T>[]> RangeAppended = delegate { };
        public event Action<OutputPoint<T>> Updated = delegate { };
        public event Action<OutputPoint<T>> Appended = delegate { };
        public event Action<int> Truncating;
        public event Action<int> Truncated;
    }

    
}
