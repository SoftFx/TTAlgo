using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal interface IOutputCollector : IDisposable
    {
        bool IsNotSyncrhonized { get; }
        OutputSetupModel OutputConfig { get; }

        IList<OutputPoint> Cache { get; }

        event Action<OutputPoint> Appended;
        event Action<OutputPoint> Updated;
        event Action<OutputPointRange> SnapshotAppended;
        event Action<int> Truncated;
    }

    internal class OutputCollector<T> : IOutputCollector, IDisposable
    {
        private readonly ExecutorModel _executor;
        private readonly string _outputId;

        public OutputCollector(OutputSetupModel setup, ExecutorModel executor)
        {
            OutputConfig = setup;
            _executor = executor;
            _outputId = setup.Id;

            executor.OutputUpdate += Executor_OutputUpdate;
        }

        public virtual bool IsNotSyncrhonized => false;
        public virtual IList<OutputPoint> Cache => null;
        public OutputSetupModel OutputConfig { get; }

        public event Action<OutputPoint> Appended;
        public event Action<OutputPoint> Updated;
        public event Action<OutputPointRange> SnapshotAppended;
        public event Action<int> Truncated;

        public virtual void Dispose()
        {
            _executor.OutputUpdate -= Executor_OutputUpdate;
        }

        private void Executor_OutputUpdate(DataSeriesUpdate update)
        {
            if (update.SeriesId == _outputId)
            {
                if (update.BufferTruncatedBy > 0)
                    OnTruncate(update.BufferTruncatedBy);

                if (update.Value.Is(OutputPointRange.Descriptor))
                    OnSnapshot(update.Value.Unpack<OutputPointRange>());
                else
                {
                    if (update.Value.Is(OutputPoint.Descriptor))
                    {
                        var point = update.Value.Unpack<OutputPoint>();
                        if (update.UpdateAction == DataSeriesUpdate.Types.UpdateAction.Append)
                            OnAppend(point);
                        else if (update.UpdateAction == DataSeriesUpdate.Types.UpdateAction.Update)
                            OnUpdate(point);
                    }
                }
            }
        }

        protected virtual void OnAppend(OutputPoint point)
        {
            Appended?.Invoke(point);
        }

        protected virtual void OnSnapshot(OutputPointRange points)
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

        public CachingOutputCollector(OutputSetupModel setup, ExecutorModel executor) : base(setup, executor)
        {
        }

        public override bool IsNotSyncrhonized => true;
        public override IList<OutputPoint> Cache => _cache;

        protected override void OnAppend(OutputPoint point)
        {
            _cache.Add(point);
            base.OnAppend(point);
        }

        protected override void OnSnapshot(OutputPointRange range)
        {
            _cache.AddRange(range.Points);
            base.OnSnapshot(range);
        }

        protected override void OnUpdate(OutputPoint point)
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
