using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public interface IOutputListener<T>
    {
        void Append(OutputFixture<T>.Point point);

        void Update(OutputFixture<T>.Point point);

        void CopyAll(OutputFixture<T>.Point[] points);

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
            _fixture.AllUpdated += OnCopyAll;
            _fixture.Truncated += OnTruncate;
        }

        private void OnAppend(OutputFixture<T>.Point point)
        {
            _listener.Append(point);
        }

        private void OnUpdate(OutputFixture<T>.Point point)
        {
            _listener.Update(point);
        }

        private void OnCopyAll(OutputFixture<T>.Point[] points)
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
            _fixture.AllUpdated -= OnCopyAll;
            _fixture.Truncated -= OnTruncate;

            base.Dispose();
        }
    }
}
