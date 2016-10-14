using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public abstract class OutputFixture : CrossDomainObject
    {
        internal abstract void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef);
        internal abstract void Unbind();
    }

    public class OutputFixture<T> : OutputFixture
    {
        private OutputBuffer<T> buffer;
        private bool isBatch;
        private ITimeRef timeRef;

        internal void BindTo(OutputBuffer<T> buffer, ITimeRef timeRef)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (timeRef == null)
                throw new ArgumentNullException("timeRef");

            Unbind();

            this.buffer = buffer;
            this.timeRef = timeRef;

            buffer.Appended = OnAppend;
            buffer.Updated = OnUpdate;
            buffer.BeginBatchBuild = () => isBatch = true;
            buffer.EndBatchBuild = () =>
            {
                isBatch = false;
                OnAllUpdated();
            };
        }

        private void OnAppend(int index, T data)
        {
            if (!isBatch)
            {
                var timeCoordinate = timeRef.GetTimeAtIndex(index);
                Appended(new Point(timeCoordinate, index, data));
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!isBatch)
            {
                var timeCoordinate = timeRef.GetTimeAtIndex(index);
                Updated(new Point(timeCoordinate, index, data));
            }
        }

        private void OnAllUpdated()
        {
            var count = buffer.Count;

            Point[] list = new Point[count];

            for (int i = 0; i < count; i++)
            {
                var timeCoordinate = timeRef.GetTimeAtIndex(i);
                list[i] = new Point(timeCoordinate, i, buffer[i]);
            }

            AllUpdated(list);
        }

        internal override void Unbind()
        {
            if (buffer != null)
            {
                buffer.Appended = null;
                buffer.Updated = null;
                buffer.BeginBatchBuild = null;
                buffer.EndBatchBuild = null;
                buffer = null;
            }
        }

        internal override void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef)
        {
            BindTo((OutputBuffer<T>)buffer, timeRef);
        }

        public event Action<Point[]> AllUpdated = delegate { };
        public event Action<Point> Updated = delegate { };
        public event Action<Point> Appended = delegate { };

        [Serializable]
        public struct Point
        {
            public Point(DateTime? time, int index, T val)
            {
                this.TimeCoordinate = time;
                this.Index = index;
                this.Value = val;
            }

            public DateTime? TimeCoordinate { get; private set; }
            public T Value { get; private set; }
            public int Index { get; private set; }
        }
    }
}
