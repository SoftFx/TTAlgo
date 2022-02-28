using Google.Protobuf;
using System;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal interface IOutputFixtureFactory
    {
        string OutputId { get; }
        IOutputFixture Create(IFixtureContext context, bool sendUpdates);
    }


    internal class OutputFixtureFactory<T> : IOutputFixtureFactory
    {
        public OutputFixtureFactory(string outputId)
        {
            OutputId = outputId;
        }

        public string OutputId { get; }

        public IOutputFixture Create(IFixtureContext context, bool sendUpdates)
        {
            return new OutputFixureGm<T>(OutputId, context, sendUpdates);
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
        private readonly bool _sendUpdates;

        internal OutputFixureGm(string outputId, IFixtureContext context, bool sendUpdates)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _outputId = outputId;
            _pointFactory = OutputPointFactory.Get<T>();
            _sendUpdates = sendUpdates;
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
                OnResetAll();
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

        private void SendUpdate(OutputPointWire point, DataSeriesUpdate.Types.Action action)
        {
            var update = new OutputSeriesUpdate(_outputId, action, point);
            update.BufferTruncatedBy = _truncatedBy;
            _truncatedBy = 0;
            _context.SendNotification(update);
        }

        #region Buffer events

        private void OnAppend(int index, T data)
        {
            if (!_isBatch && _sendUpdates)
            {
                var timeCoordinate = _timeRef[index];
                SendUpdate(new OutputPointWire(_pointFactory(timeCoordinate, data)), DataSeriesUpdate.Types.Action.Append);
            }
        }

        private void OnUpdate(int index, T data)
        {
            if (!_isBatch && _sendUpdates)
            {
                var timeCoordinate = _timeRef[index];
                SendUpdate(new OutputPointWire(_pointFactory(timeCoordinate, data)), DataSeriesUpdate.Types.Action.Update);
            }
        }

        private void OnResetAll()
        {
            if (_sendUpdates)
            {
                var count = _buffer.Count;
                var update = new OutputSeriesUpdate(_outputId, DataSeriesUpdate.Types.Action.Reset);
                update.BufferTruncatedBy = _truncatedBy;
                _truncatedBy = 0;

                update.Points.Capacity = count;

                for (var i = 0; i < count; i++)
                {
                    var timeCoordinate = _timeRef[i];
                    update.Points.Add(new OutputPointWire(_pointFactory(timeCoordinate, _buffer[i])));
                }

                _context.SendNotification(update);
            }
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
