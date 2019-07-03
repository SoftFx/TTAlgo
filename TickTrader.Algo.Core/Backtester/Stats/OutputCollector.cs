using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal interface IOutputCollector
    {
        void Stop();
    }

    internal class OutputCollector<T> : IOutputCollector
    {
        private string _outputId;
        private OutputFixture<T> _fixture;
        private TestDataSeriesFlags _flags;
        private Action<object> _onRealtimeUpdate;
        private Action<object> _onTruncateUpdate;
        private int _indexShift;

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
                    Snapshot = new List<T>();

                if (streamEnabled)
                {
                    if (isRealtimeStream)
                    {
                        _onRealtimeUpdate = onUpdate;

                        fixture.Appended += Fixture_Appended;
                        fixture.Updated += Fixture_Updated;
                        fixture.RangeAppended += Fixture_AllUpdated;
                    }
                    else
                    {
                        _onTruncateUpdate = onUpdate;
                    }
                }
            }
        }

        public List<T> Snapshot { get; }

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
                    Snapshot.Add(buffer[i]);
            }

            if (streamEnabled && !isRealtimeStream && _fixture.Buffer != null)
            {
                // copy all data from buffer to update
                for (int i = 0; i < _fixture.Count; i++)
                {
                    var point = _fixture[i].ChangeIndex(-1);
                    var update = new DataSeriesUpdate<OutputFixture<T>.Point>(DataSeriesTypes.Output, _outputId, SeriesUpdateActions.Append, point);
                    _onTruncateUpdate(update);
                }
            }
        }

        private void Fixture_Updated(OutputFixture<T>.Point point)
        {
            var adjustedPoint = point.ChangeIndex(point.Index + _indexShift);
            var update = new DataSeriesUpdate<OutputFixture<T>.Point>(DataSeriesTypes.Output, _outputId, SeriesUpdateActions.Update, adjustedPoint);
            _onRealtimeUpdate(update);
        }

        private void Fixture_Appended(OutputFixture<T>.Point point)
        {
            var adjustedPoint = point.ChangeIndex(point.Index + _indexShift);
            var update = new DataSeriesUpdate<OutputFixture<T>.Point>(DataSeriesTypes.Output, _outputId, SeriesUpdateActions.Append, adjustedPoint);
            _onRealtimeUpdate(update);
        }

        private void Fixture_AllUpdated(OutputFixture<T>.Point[] snapshot)
        {
            foreach (var point in snapshot)
                Fixture_Appended(point);
        }

        private void OnTruncate(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var point = _fixture[i].ChangeIndex(-1);
                var update = new DataSeriesUpdate<OutputFixture<T>.Point>(DataSeriesTypes.Output, _outputId, SeriesUpdateActions.Append, point);
                _onTruncateUpdate?.Invoke(update);
                Snapshot?.Add(point.Value);
            }

            _indexShift += size;
        }
    }
}
