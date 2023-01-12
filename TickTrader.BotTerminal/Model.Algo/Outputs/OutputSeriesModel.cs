using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed record OutputSeriesModel
    {
        public IList<OutputPoint> Values { get; init; }

        public OutputDescriptor Descriptor { get; init; }

        public IOutputConfig Config { get; init; }
    }

    internal sealed class OutputModel : IDisposable
    {
        public Dictionary<string, OutputSeriesModel> Series { get; } = new();

        public string Id { get; }

        public string DisplayName { get; }

        public PluginConfig Config { get; }


        internal OutputModel(PluginConfig config, PluginDescriptor descriptor, Dictionary<string, List<OutputPoint>> points)
        {
            Id = descriptor.Id;
            DisplayName = descriptor.UiDisplayName;
            Config = config;

            var outputMap = descriptor.Outputs.ToDictionary(o => o.Id);

            foreach (var property in config.UnpackProperties())
                if (property is IOutputConfig outputConfig)
                {
                    var id = outputConfig.PropertyId;

                    if (outputMap.TryGetValue(id, out var output))
                    {
                        points.TryGetValue(id, out var values);

                        var series = new OutputSeriesModel
                        {
                            Config = outputConfig,
                            Descriptor = output,
                            Values = values,
                        };

                        Series.Add(id, series);
                    }
                }
        }


        public void Dispose() { }
    }

    //internal abstract class OutputSeriesModel<T> : OutputSeriesModel
    //{
    //    //private readonly IPluginDataChartModel _host;
    //    private readonly IOutputCollector _collector;
    //    private readonly OutputSynchronizer<T> _synchronizer;

    //    public OutputSeriesModel(IOutputCollector collector)
    //    {
    //        _collector = collector;
    //        //_host = host;

    //        if (!collector.OutputConfig.IsEnabled)
    //            return;

    //        //_host.StartEvent += Start;
    //        //_host.StopEvent += Stop;

    //        //if (_collector.IsNotSyncrhonized)
    //        //{
    //        //    _synchronizer = new OutputSynchronizer<T>(NanValue);
    //        //    _synchronizer.DoAppend = AppendInternal;
    //        //    _synchronizer.DoUpdate = UpdateInternal;
    //        //}
    //    }

    //    protected void Enable()
    //    {
    //        if (_host.IsStarted)
    //            Start();
    //    }

    //    private void Start()
    //    {
    //        Clear();

    //        _synchronizer?.Start(_host.TimeSyncRef);

    //        if (_collector.IsNotSyncrhonized)
    //            _synchronizer.AppendSnapshot(_collector.Cache);

    //        _collector.Appended += Append;
    //        _collector.Updated += Update;
    //        _collector.SnapshotAppended += _collector_SnapshotAppended;
    //        _collector.Truncated += _collector_Truncated;
    //    }

    //    private Task Stop(object sender, CancellationToken cancelToken)
    //    {
    //        _synchronizer?.Stop();

    //        _collector.Appended -= Append;
    //        _collector.Updated -= Update;
    //        _collector.SnapshotAppended -= _collector_SnapshotAppended;
    //        _collector.Truncated -= _collector_Truncated;

    //        return Task.CompletedTask;
    //    }

    //    private void Append(OutputPoint point)
    //    {
    //        if (_synchronizer != null)
    //            _synchronizer.Append(point);
    //        else
    //            AppendInternal(point);
    //    }

    //    private void Update(OutputPoint point)
    //    {
    //        if (_synchronizer != null)
    //            _synchronizer.Update(point);
    //        else
    //            UpdateInternal(point);
    //    }

    //    private void _collector_SnapshotAppended(IEnumerable<OutputPoint> range)
    //    {
    //        if (_synchronizer != null)
    //            _synchronizer.AppendSnapshot(range);
    //        else
    //        {
    //            foreach (var p in range)
    //                AppendInternal(p);
    //        }
    //    }

    //    private void _collector_Truncated(int size)
    //    {
    //        _synchronizer.Truncate(size);
    //    }

    //    protected abstract T NanValue { get; }

    //    protected abstract void AppendInternal(OutputPoint point);
    //    protected abstract void UpdateInternal(OutputPoint point);
    //    protected abstract void Clear();

    //    public override void Dispose()
    //    {
    //        Stop(this, CancellationToken.None);

    //        _host.StartEvent -= Start;
    //        _host.StopEvent -= Stop;
    //    }
    //}
}
