//using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal abstract class OutputSeriesModel : IDisposable
    {
        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public OutputDescriptor Descriptor { get; private set; }

        public IOutputConfig Config { get; private set; }

        //public abstract IXyDataSeries SeriesData { get; }

        protected void Init(IOutputConfig config, OutputDescriptor descriptor)
        {
            Config = config;
            Descriptor = descriptor;

            Id = descriptor.Id;
            DisplayName = descriptor.DisplayName;
        }

        public abstract void Dispose();
    }

    internal abstract class OutputSeriesModel<T> : OutputSeriesModel
    {
        private IPluginDataChartModel _host;
        private IOutputCollector _collector;
        private OutputSynchronizer<T> _synchronizer;

        public OutputSeriesModel(IOutputCollector collector, IPluginDataChartModel host)
        {
            _collector = collector;
            _host = host;

            if (!collector.OutputConfig.IsEnabled)
                return;

            _host.StartEvent += Start;
            _host.StopEvent += Stop;

            if (_collector.IsNotSyncrhonized)
            {
                _synchronizer = new OutputSynchronizer<T>(NanValue);
                _synchronizer.DoAppend = AppendInternal;
                _synchronizer.DoUpdate = UpdateInternal;
            }
        }

        protected void Enable()
        {
            if (_host.IsStarted)
                Start();
        }

        private void Start()
        {
            Clear();

            _synchronizer?.Start(_host.TimeSyncRef);

            if (_collector.IsNotSyncrhonized)
                _synchronizer.AppendSnapshot(_collector.Cache);

            _collector.Appended += Append;
            _collector.Updated += Update;
            _collector.SnapshotAppended += _collector_SnapshotAppended;
            _collector.Truncated += _collector_Truncated;
        }

        private Task Stop(object sender, CancellationToken cancelToken)
        {
            _synchronizer?.Stop();

            _collector.Appended -= Append;
            _collector.Updated -= Update;
            _collector.SnapshotAppended -= _collector_SnapshotAppended;
            _collector.Truncated -= _collector_Truncated;

            return Task.CompletedTask;
        }

        private void Append(OutputPoint point)
        {
            if (_synchronizer != null)
                _synchronizer.Append(point);
            else
                AppendInternal(point);
        }

        private void Update(OutputPoint point)
        {
            if (_synchronizer != null)
                _synchronizer.Update(point);
            else
                UpdateInternal(point);
        }

        private void _collector_SnapshotAppended(IEnumerable<OutputPoint> range)
        {
            if (_synchronizer != null)
                _synchronizer.AppendSnapshot(range);
            else
            {
                foreach (var p in range)
                    AppendInternal(p);
            }
        }

        private void _collector_Truncated(int size)
        {
            _synchronizer.Truncate(size);
        }

        protected abstract T NanValue { get; }

        protected abstract void AppendInternal(OutputPoint point);
        protected abstract void UpdateInternal(OutputPoint point);
        protected abstract void Clear();

        public override void Dispose()
        {
            Stop(this, CancellationToken.None);

            _host.StartEvent -= Start;
            _host.StopEvent -= Stop;
        }
    }
}
