using System;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public interface IOutputFixture
    {
        void BindTo(IReaonlyDataBuffer buffer, ITimeRef timeRef);
        void Unbind();
    }

    public class OutputFixture<T> : IOutputFixture
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
                ResetAll();
            };
            buffer.Truncated = OnTruncate;
            buffer.Truncating = OnTruncating;
        }

        private void OnAppend(int index, T data)
        {
            if (!_isBatch)
            {
                var timeCoordinate = TimeMs.FromTimestamp(_timeRef[index]);
                Appended(_pointFactory(timeCoordinate, data));
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!_isBatch)
            {
                var timeCoordinate = TimeMs.FromTimestamp(_timeRef[index]);
                Updated(_pointFactory(timeCoordinate, data));
            }
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
        internal OutputPoint this[int index] => _pointFactory(TimeMs.FromTimestamp(_timeRef[index]), _buffer[index]);
        internal OutputBuffer<T> Buffer => _buffer;

        public event Action ResetAll = delegate { };
        public event Action<OutputPoint> Updated = delegate { };
        public event Action<OutputPoint> Appended = delegate { };
        public event Action<int> Truncating;
        public event Action<int> Truncated;
    }

    
}
