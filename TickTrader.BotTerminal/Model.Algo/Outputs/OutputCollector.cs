using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal interface IOutputCollector : IDisposable
    {
        bool IsNotSyncrhonized { get; }
        OutputSetupModel OutputConfig { get; }
    }

    internal interface IOutputCollector<T> : IOutputCollector
    {
        IList<OutputFixture<T>.Point> Cache { get; }

        event Action<OutputFixture<T>.Point> Appended;
        event Action<OutputFixture<T>.Point> Updated;
        event Action<OutputFixture<T>.Point[]> SnapshotAppended;
        event Action<int> Truncated;
    }

    internal class OutputCollector<T> : IOutputListener<T>, IOutputCollector<T>, IDisposable
    {
        private OutputAdapter<T> _adapter;
        private PluginExecutor _executor;

        public OutputCollector(OutputSetupModel setup, PluginExecutor executor)
        {
            OutputConfig = setup;
            _executor = executor;
            //_adapter = new OutputAdapter<T>(fixture, this);
        }

        public virtual bool IsNotSyncrhonized => false;
        public virtual IList<OutputFixture<T>.Point> Cache => null;
        public OutputSetupModel OutputConfig { get; }

        public event Action<OutputFixture<T>.Point> Appended;
        public event Action<OutputFixture<T>.Point> Updated;
        public event Action<OutputFixture<T>.Point[]> SnapshotAppended;
        public event Action<int> Truncated;

        public virtual void Dispose()
        {
            _adapter.Dispose();
        }

        protected virtual void OnAppend(OutputFixture<T>.Point point)
        {
            Appended?.Invoke(point);
        }

        protected virtual void OnSnapshot(OutputFixture<T>.Point[] points)
        {
            SnapshotAppended?.Invoke(points);
        }

        protected virtual void OnTruncate(int truncateSize)
        {
            Truncated?.Invoke(truncateSize);
        }

        protected virtual void OnUpdate(OutputFixture<T>.Point point)
        {
            Updated?.Invoke(point);
        }

        void IOutputListener<T>.Append(OutputFixture<T>.Point point)
        {
            Execute.OnUIThread(() => OnAppend(point));
        }

        void IOutputListener<T>.CopyAll(OutputFixture<T>.Point[] points)
        {
            Execute.OnUIThread(() => OnSnapshot(points));
        }

        void IOutputListener<T>.TruncateBy(int truncateSize)
        {
            Execute.OnUIThread(() => OnTruncate(truncateSize));
        }

        void IOutputListener<T>.Update(OutputFixture<T>.Point point)
        {
            Execute.OnUIThread(() => OnUpdate(point));
        }
    }

    internal class CachingOutputCollector<T> : OutputCollector<T>
    {
        private CircularList<OutputFixture<T>.Point> _cache = new CircularList<OutputFixture<T>.Point>();

        public CachingOutputCollector(OutputSetupModel setup, PluginExecutor executor) : base(setup, executor)
        {
        }

        public override bool IsNotSyncrhonized => true;
        public override IList<OutputFixture<T>.Point> Cache => _cache;

        protected override void OnAppend(OutputFixture<T>.Point point)
        {
            _cache.Add(point);
            base.OnAppend(point);
        }

        protected override void OnSnapshot(OutputFixture<T>.Point[] points)
        {
            _cache.AddRange(points);
            base.OnSnapshot(points);
        }

        protected override void OnUpdate(OutputFixture<T>.Point point)
        {
            _cache[point.Index] = point;
            base.OnUpdate(point);
        }

        protected override void OnTruncate(int truncateSize)
        {
            _cache.TruncateStart(truncateSize);
            base.OnTruncate(truncateSize);
        }
    }
}
