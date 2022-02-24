using System;
using System.Collections.Generic;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal interface IOutputCollector
    {
        List<OutputPoint> Snapshot { get; }

        void Stop();
    }

    internal class OutputCollector<T> : IOutputCollector
    {
        private string _outputId;
        private OutputFixture<T> _fixture;
        private TestDataSeriesFlags _flags;
        private Action<object> _onRealtimeUpdate;
        private Action<object> _onTruncateUpdate;

        public OutputCollector(string outputId, OutputFixture<T> fixture, Action<object> onUpdate, TestDataSeriesFlags flags)
        {
            _outputId = outputId;
            _fixture = fixture;
            _flags = flags;

            bool snapshotEnabled = flags.HasFlag(TestDataSeriesFlags.Snapshot);
            bool streamEnabled = flags.HasFlag(TestDataSeriesFlags.Stream);
            bool isRealtimeStream = flags.HasFlag(TestDataSeriesFlags.Realtime);

            if (snapshotEnabled || streamEnabled)
            {
                _fixture.Truncating += OnTruncate;

                if (snapshotEnabled)
                    Snapshot = new List<OutputPoint>();

                if (streamEnabled)
                {
                    if (isRealtimeStream)
                    {
                        _onRealtimeUpdate = onUpdate;

                        fixture.Appended += Fixture_Appended;
                        fixture.Updated += Fixture_Updated;
                        fixture.ResetAll += Fixture_AllUpdated;
                    }
                    else
                    {
                        _onTruncateUpdate = onUpdate;
                    }
                }
            }
        }

        public List<OutputPoint> Snapshot { get; }

        public void Stop()
        {
            bool snapshotEnabled = _flags.HasFlag(TestDataSeriesFlags.Snapshot);
            bool streamEnabled = _flags.HasFlag(TestDataSeriesFlags.Stream);
            bool isRealtimeStream = _flags.HasFlag(TestDataSeriesFlags.Realtime);

            _fixture.Truncating -= OnTruncate;

            var buffer = _fixture.Buffer;

            if (snapshotEnabled)
            {
                // copy all data from buffer to snapshot
                for (int i = 0; i < buffer.Count; i++)
                    Snapshot.Add(_fixture[i]);
            }

            if (streamEnabled && !isRealtimeStream && _fixture.Buffer != null)
            {
                // copy all data from buffer to update
                for (int i = 0; i < _fixture.Count; i++)
                {
                    var point = _fixture[i];
                    var update = new OutputSeriesUpdate(_outputId, DataSeriesUpdate.Types.Action.Append, new OutputPointWire(point));
                    _onTruncateUpdate(update);
                }
            }
        }

        private void Fixture_Updated(OutputPoint point)
        {
            var update = new OutputSeriesUpdate(_outputId, DataSeriesUpdate.Types.Action.Update, new OutputPointWire(point));
            _onRealtimeUpdate(update);
        }

        private void Fixture_Appended(OutputPoint point)
        {
            var update = new OutputSeriesUpdate(_outputId, DataSeriesUpdate.Types.Action.Append, new OutputPointWire(point));
            _onRealtimeUpdate(update);
        }

        private void Fixture_AllUpdated()
        {
            var cnt = _fixture.Count;
            var update = new OutputSeriesUpdate(_outputId, DataSeriesUpdate.Types.Action.Reset);
            update.Points.Capacity = cnt;
            for (var i = 0; i < cnt; i++)
            {
                update.Points.Add(new OutputPointWire(_fixture[i]));
            }
            _onRealtimeUpdate(update);
        }

        private void OnTruncate(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var point = _fixture[i];
                var update = new OutputSeriesUpdate(_outputId, DataSeriesUpdate.Types.Action.Append, new OutputPointWire(point));
                _onTruncateUpdate?.Invoke(update);
                Snapshot?.Add(point);
            }
        }
    }
}
