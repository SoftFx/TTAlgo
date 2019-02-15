using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal abstract class OutputSeriesModel : IDisposable
    {
        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public OutputDescriptor Descriptor { get; private set; }

        public string PluginId { get; private set; }

        public OutputSetupModel Setup { get; private set; }

        public abstract IXyDataSeries SeriesData { get; }

        protected void Init(PluginModel plugin, OutputSetupModel setup)
        {
            Id = setup.Metadata.Id;
            DisplayName = setup.Metadata.DisplayName;
            Descriptor = setup.Metadata.Descriptor;
            PluginId = plugin.InstanceId;
            Setup = setup;
        }

        public abstract void Dispose();
    }

    internal abstract class OutputSeriesModel<T> : OutputSeriesModel
    {
        private IAlgoPluginHost _host;
        private OutputCollector<T> _collector;
        private OutputSynchronizer<T> _synchronizer;

        public OutputSeriesModel(IOutputCollector collector, IAlgoPluginHost host, bool isEnabled)
        {
            _collector = (OutputCollector<T>)collector;
            _host = host;

            if (!isEnabled)
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

            return CompletedTask.Default;
        }

        private void Append(OutputFixture<T>.Point point)
        {
            if (_synchronizer != null)
                _synchronizer.Append(point);
            else
                AppendInternal(point.TimeCoordinate.Value, point.Value);
        }

        private void Update(OutputFixture<T>.Point point)
        {
            if (_synchronizer != null)
                _synchronizer.Update(point);
            else
                UpdateInternal(point.Index, point.TimeCoordinate.Value, point.Value);
        }

        private void _collector_SnapshotAppended(OutputFixture<T>.Point[] points)
        {
            if (_synchronizer != null)
                _synchronizer.AppendSnapshot(points);
            else
            {
                foreach (var p in points)
                    AppendInternal(p.TimeCoordinate.Value, p.Value);
            }
        }

        private void _collector_Truncated(int size)
        {
            _synchronizer.Truncate(size);
        }

        protected abstract T NanValue { get; }

        protected abstract void AppendInternal(DateTime time, T data);
        protected abstract void UpdateInternal(int index, DateTime time, T data);
        protected abstract void Clear();

        public override void Dispose()
        {
            Stop(this, CancellationToken.None);

            _host.StartEvent -= Start;
            _host.StopEvent -= Stop;
        }
    }
}
