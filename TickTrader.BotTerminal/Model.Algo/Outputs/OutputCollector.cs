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
        IList<OutputPoint<T>> Cache { get; }

        event Action<OutputPoint<T>> Appended;
        event Action<OutputPoint<T>> Updated;
        event Action<OutputPoint<T>[]> SnapshotAppended;
        event Action<int> Truncated;
    }

    internal class OutputCollector<T> : IOutputCollector<T>, IDisposable
    {
        private readonly PluginExecutor _executor;
        private readonly string _outputId;

        public OutputCollector(OutputSetupModel setup, PluginExecutor executor)
        {
            OutputConfig = setup;
            _executor = executor;
            _outputId = setup.Id;

            executor.OutputUpdate += Executor_OutputUpdate;
        }

        public virtual bool IsNotSyncrhonized => false;
        public virtual IList<OutputPoint<T>> Cache => null;
        public OutputSetupModel OutputConfig { get; }

        public event Action<OutputPoint<T>> Appended;
        public event Action<OutputPoint<T>> Updated;
        public event Action<OutputPoint<T>[]> SnapshotAppended;
        public event Action<int> Truncated;

        public virtual void Dispose()
        {
            _executor.OutputUpdate -= Executor_OutputUpdate;
        }

        private void Executor_OutputUpdate(IDataSeriesUpdate update)
        {
            if (update.SeriesId == _outputId)
            {
                if (update.BufferTrucatedBy > 0)
                    OnTruncate(update.BufferTrucatedBy);

                var batchUpdate = update as DataSeriesUpdate<OutputPoint<T>[]>;
                if (batchUpdate != null)
                    OnSnapshot(batchUpdate.Value);
                else
                {
                    var pointUpdate = update as DataSeriesUpdate<OutputPoint<T>>;
                    if (pointUpdate != null)
                    {
                        if (pointUpdate.Action == SeriesUpdateActions.Append)
                            OnAppend(pointUpdate.Value);
                        else if (pointUpdate.Action == SeriesUpdateActions.Update)
                            OnUpdate(pointUpdate.Value);
                    }
                }
            }
        }

        protected virtual void OnAppend(OutputPoint<T> point)
        {
            Appended?.Invoke(point);
        }

        protected virtual void OnSnapshot(OutputPoint<T>[] points)
        {
            SnapshotAppended?.Invoke(points);
        }

        protected virtual void OnTruncate(int truncateSize)
        {
            Truncated?.Invoke(truncateSize);
        }

        protected virtual void OnUpdate(OutputPoint<T> point)
        {
            Updated?.Invoke(point);
        }
    }

    internal class CachingOutputCollector<T> : OutputCollector<T>
    {
        private CircularList<OutputPoint<T>> _cache = new CircularList<OutputPoint<T>>();

        public CachingOutputCollector(OutputSetupModel setup, PluginExecutor executor) : base(setup, executor)
        {
        }

        public override bool IsNotSyncrhonized => true;
        public override IList<OutputPoint<T>> Cache => _cache;

        protected override void OnAppend(OutputPoint<T> point)
        {
            _cache.Add(point);
            base.OnAppend(point);
        }

        protected override void OnSnapshot(OutputPoint<T>[] points)
        {
            _cache.AddRange(points);
            base.OnSnapshot(points);
        }

        protected override void OnUpdate(OutputPoint<T> point)
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
