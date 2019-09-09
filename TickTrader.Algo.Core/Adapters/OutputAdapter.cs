using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public interface IOutputListener<T>
    {
        void Append(OutputPoint<T> point);

        void Update(OutputPoint<T> point);

        void CopyAll(OutputPoint<T>[] points);

        void TruncateBy(int truncateSize);
    }

    public class OutputAdapter<T> : CrossDomainObject
    {
        private OutputFixture<T> _fixture;
        private IOutputListener<T> _listener;

        public OutputAdapter(OutputFixture<T> fixture, IOutputListener<T> listener)
        {
            _fixture = fixture;
            _listener = listener;

            _fixture.Appended += OnAppend;
            _fixture.Updated += OnUpdate;
            _fixture.RangeAppended += OnCopyAll;
            _fixture.Truncated += OnTruncate;
        }

        private void OnAppend(OutputPoint<T> point)
        {
            _listener.Append(point);
        }

        private void OnUpdate(OutputPoint<T> point)
        {
            _listener.Update(point);
        }

        private void OnCopyAll(OutputPoint<T>[] points)
        {
            _listener.CopyAll(points);
        }

        private void OnTruncate(int truncateSize)
        {
            _listener.TruncateBy(truncateSize);
        }

        public override void Dispose()
        {
            _fixture.Appended -= OnAppend;
            _fixture.Updated -= OnUpdate;
            _fixture.RangeAppended -= OnCopyAll;
            _fixture.Truncated -= OnTruncate;

            base.Dispose();
        }
    }
}
