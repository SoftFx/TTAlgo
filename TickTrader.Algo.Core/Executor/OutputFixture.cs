using System;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface IOutputFixture
    {
        void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef);
        void Unbind();
    }

    public class OutputFixture<T> : CrossDomainObject, IOutputFixture
    {
        private OutputBuffer<T> _buffer;
        private bool _isBatch;
        private ITimeRef _timeRef;
        private readonly OutputPointFactory<T> _pointFactory = OutputPointFactory.Get<T>();

        internal void BindTo(OutputBuffer<T> buffer, ITimeRef timeRef)
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

        private void OnAppend(int index, T data)
        {
            if (!_isBatch)
            {
                var timeCoordinate = _timeRef[index];
                Appended(new OutputPoint(timeCoordinate, index, _pointFactory.PackValue(data)));
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!_isBatch)
            {
                var timeCoordinate = _timeRef[index];
                Updated(new OutputPoint(timeCoordinate, index, _pointFactory.PackValue(data)));
            }
        }

        private void OnRangeAppend()
        {
            var count = _buffer.Count;

            var range = new OutputPointRange();
            range.Points.Capacity = count;

            for (var i = 0; i < count; i++)
            {
                var timeCoordinate = _timeRef[i];
                range.Points.Add(new OutputPoint(timeCoordinate, i, _pointFactory.PackValue(_buffer[i])));
            }

            RangeAppended(range);
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

        public void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef)
        {
            BindTo((OutputBuffer<T>)buffer, timeRef);
        }

        internal int Count => _buffer.Count;
        internal OutputPoint this[int index] => new OutputPoint(_timeRef[index], index, _pointFactory.PackValue(_buffer[index]));
        internal OutputBuffer<T> Buffer => _buffer;

        public event Action<OutputPointRange> RangeAppended = delegate { };
        public event Action<OutputPoint> Updated = delegate { };
        public event Action<OutputPoint> Appended = delegate { };
        public event Action<int> Truncating;
        public event Action<int> Truncated;
    }

    
}
