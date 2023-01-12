using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal interface IOutputCollector : IDisposable
    {
        bool IsNotSyncrhonized { get; }
        IOutputConfig OutputConfig { get; }
        OutputDescriptor OutputDescriptor { get; }

        IList<OutputPoint> Cache { get; }

        event Action<OutputPoint> Appended;
        event Action<OutputPoint> Updated;
        event Action<IEnumerable<OutputPoint>> SnapshotAppended;
        event Action<int> Truncated;

        void SendSnapshot(IEnumerable<OutputPoint> points);
    }


    internal class OutputCollector<T> : IOutputCollector, IDisposable
    {
        private readonly string _outputId;
        private IDisposable _outputSub;

        public OutputCollector(IOutputConfig config, OutputDescriptor descriptor)
        {
            _outputId = config.PropertyId;
            OutputConfig = config;
            OutputDescriptor = descriptor;
        }

        public virtual bool IsNotSyncrhonized => false;
        public virtual IList<OutputPoint> Cache => null;
        public IOutputConfig OutputConfig { get; }
        public OutputDescriptor OutputDescriptor { get; }

        public event Action<OutputPoint> Appended;
        public event Action<OutputPoint> Updated;
        public event Action<IEnumerable<OutputPoint>> SnapshotAppended;
        public event Action<int> Truncated;


        public virtual void Dispose()
        {
            _outputSub.Dispose();
        }

        public void Subscribe(IEventSource<OutputSeriesUpdate> eventSrc)
        {
            _outputSub = eventSrc.Subscribe(Executor_OutputUpdate);
        }

        public void SendSnapshot(IEnumerable<OutputPoint> points)
        {
            OnSnapshot(points);
        }


        private void Executor_OutputUpdate(OutputSeriesUpdate update)
        {
            if (update.SeriesId == _outputId)
            {
                if (update.BufferTruncatedBy > 0)
                    OnTruncate(update.BufferTruncatedBy);

                switch (update.UpdateAction)
                {
                    case DataSeriesUpdate.Types.Action.Append: OnAppend(update.Points[0].Unpack()); break;
                    case DataSeriesUpdate.Types.Action.Update: OnUpdate(update.Points[0].Unpack()); break;
                    case DataSeriesUpdate.Types.Action.Reset: OnSnapshot(update.Points.Select(p => p.Unpack())); break;
                }
            }
        }

        protected virtual void OnAppend(OutputPoint point)
        {
            Appended?.Invoke(point);
        }

        protected virtual void OnSnapshot(IEnumerable<OutputPoint> points)
        {
            SnapshotAppended?.Invoke(points);
        }

        protected virtual void OnTruncate(int truncateSize)
        {
            Truncated?.Invoke(truncateSize);
        }

        protected virtual void OnUpdate(OutputPoint point)
        {
            Updated?.Invoke(point);
        }
    }

    internal class CachingOutputCollector<T> : OutputCollector<T>
    {
        private CircularList<OutputPoint> _cache = new CircularList<OutputPoint>();

        public CachingOutputCollector(IOutputConfig config, OutputDescriptor descriptor) : base(config, descriptor)
        {
        }

        public override bool IsNotSyncrhonized => true;
        public override IList<OutputPoint> Cache => _cache;

        protected override void OnAppend(OutputPoint point)
        {
            _cache.Add(point);
            base.OnAppend(point);
        }

        protected override void OnSnapshot(IEnumerable<OutputPoint> range)
        {
            _cache.AddRange(range);
            base.OnSnapshot(range);
        }

        protected override void OnUpdate(OutputPoint point)
        {
            var index = _cache.BinarySearchBy(p => p.Time, point.Time);
            _cache[index] = point;
            base.OnUpdate(point);
        }

        protected override void OnTruncate(int truncateSize)
        {
            _cache.TruncateStart(truncateSize);
            base.OnTruncate(truncateSize);
        }
    }
}
