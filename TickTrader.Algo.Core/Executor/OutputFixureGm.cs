using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal interface IOutputFixtureFactory
    {
        string OutputId { get; }
        IOutputFixture Create(IFixtureContext context);
    }

    [Serializable]
    internal class OutputFixtureFactory<T> : IOutputFixtureFactory
    {
        public OutputFixtureFactory(string outputId)
        {
            OutputId = outputId;
        }

        public string OutputId { get; }

        public IOutputFixture Create(IFixtureContext context)
        {
            return new OutputFixureGm<T>(OutputId, context);
        }
    }

    internal class OutputFixureGm<T> : IOutputFixture
    {
        private OutputBuffer<T> buffer;
        private bool isBatch;
        private ITimeRef timeRef;
        private IFixtureContext _context;
        private readonly string _outputId;
        private int _truncatedBy;

        internal OutputFixureGm(string outputId, IFixtureContext context)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _outputId = outputId;
        }

        public void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef)
        {
            BindTo((OutputBuffer<T>)buffer, timeRef);
        }

        private void BindTo(OutputBuffer<T> buffer, ITimeRef timeRef)
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

        private void SendUpdate<TUpdate>(TUpdate data, SeriesUpdateActions action)
        {
            var update = new DataSeriesUpdate<TUpdate>(DataSeriesTypes.Output, _outputId, action, data);
            update.BufferTrucatedBy = _truncatedBy;
            _truncatedBy = 0;
            _context.SendExtUpdate(update);
        }

        #region Buffer events

        private void OnAppend(int index, T data)
        {
            if (!isBatch)
            {
                var timeCoordinate = timeRef[index];
                SendUpdate(new OutputPoint<T>(timeCoordinate, index, data), SeriesUpdateActions.Append);
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!isBatch)
            {
                var timeCoordinate = timeRef[index];
                SendUpdate(new OutputPoint<T>(timeCoordinate, index, data), SeriesUpdateActions.Update);
            }
        }

        private void OnRangeAppend()
        {
            var count = buffer.Count;

            OutputPoint<T>[] list = new OutputPoint<T>[count];

            for (int i = 0; i < count; i++)
            {
                var timeCoordinate = timeRef[i];
                list[i] = new OutputPoint<T>(timeCoordinate, i, buffer[i]);
            }

            SendUpdate(list, SeriesUpdateActions.Append);
        }

        private void OnTruncate(int truncateSize)
        {
            _truncatedBy += truncateSize;
            //Truncated?.Invoke(truncateSize);
        }

        private void OnTruncating(int truncateSize)
        {
            //Truncating?.Invoke(truncateSize);
        }

        #endregion
    }
}
