using Google.Protobuf;
using System;
using TickTrader.Algo.Domain;

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
        private OutputBuffer<T> _buffer;
        private bool _isBatch;
        private ITimeRef _timeRef;
        private IFixtureContext _context;
        private readonly string _outputId;
        private readonly OutputPointFactory<T> _pointFactory;
        private int _truncatedBy;

        internal OutputFixureGm(string outputId, IFixtureContext context)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _outputId = outputId;
            _pointFactory = OutputPointFactory.Get<T>();
        }

        public void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef)
        {
            BindTo((OutputBuffer<T>)buffer, timeRef);
        }

        private void BindTo(OutputBuffer<T> buffer, ITimeRef timeRef)
        {
            Unbind();

            _buffer = buffer ?? throw new ArgumentNullException("buffer");
            _timeRef = timeRef ?? throw new ArgumentNullException("timeRef");

            buffer.Appended = OnAppend;
            buffer.Updated = OnUpdate;
            buffer.BeginBatchBuild = () => _isBatch = true;
            buffer.EndBatchBuild = () =>
            {
                _isBatch = false;
                OnRangeAppend();
            };
            buffer.Truncated = OnTruncate;
            buffer.Truncating = OnTruncating;
        }

        public void Unbind()
        {
            if (_buffer != null)
            {
                _buffer.Appended = null;
                _buffer.Updated = null;
                _buffer.BeginBatchBuild = null;
                _buffer.EndBatchBuild = null;
                _buffer.Truncated = null;
                _buffer.Truncating = null;
                _buffer = null;
            }
        }

        private void SendUpdate<TUpdate>(TUpdate data, DataSeriesUpdate.Types.UpdateAction action)
            where TUpdate : IMessage
        {
            var update = new DataSeriesUpdate(DataSeriesUpdate.Types.Type.Output, _outputId, action, data);
            update.BufferTruncatedBy = _truncatedBy;
            _truncatedBy = 0;
            _context.SendExtUpdate(update);
        }

        #region Buffer events

        private void OnAppend(int index, T data)
        {
            if (!_isBatch)
            {
                var timeCoordinate = _timeRef[index];
                SendUpdate(new OutputPoint(timeCoordinate, index, _pointFactory.PackValue(data)), DataSeriesUpdate.Types.UpdateAction.Append);
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!_isBatch)
            {
                var timeCoordinate = _timeRef[index];
                SendUpdate(new OutputPoint(timeCoordinate, index, _pointFactory.PackValue(data)), DataSeriesUpdate.Types.UpdateAction.Update);
            }
        }

        private void OnRangeAppend()
        {
            var count = _buffer.Count;

            var range = new OutputPointRange();
            range.Points.Capacity = count;

            for (var i = 0; i < count; i++)
            {
                range.Points.Add(new OutputPoint(_timeRef[i], i, _pointFactory.PackValue(_buffer[i])));
            }

            SendUpdate(range, DataSeriesUpdate.Types.UpdateAction.Append);
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
