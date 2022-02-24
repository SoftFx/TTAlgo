using System;
using SciChart.Charting.Model.DataSeries;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class MarkerSeriesModel : OutputSeriesModel<MarkerInfo>
    {
        private static readonly MarkerInfo NanMarker = new MarkerInfo { ColorArgb = uint.MaxValue };

        private XyDataSeries<DateTime, double> _seriesData;

        public override IXyDataSeries SeriesData => _seriesData;
        protected override MarkerInfo NanValue => NanMarker;

        public MarkerSeriesModel(IPluginModel plugin, IPluginDataChartModel outputHost, IOutputCollector collector)
            : base(collector, outputHost)
        {
            _seriesData = new XyDataSeries<DateTime, double>();

            var config = collector.OutputConfig;
            Init(plugin, config, collector.OutputDescriptor);

            if (config.IsEnabled)
            {
                _seriesData.SeriesName = DisplayName;
                Enable();
            }
        }

        protected override void AppendInternal(OutputPoint p)
        {
            _seriesData.Append(TimeMs.ToUtc(p.Time), p.Value, GetMetadata(p.Value, p.Metadata as MarkerInfo));
        }

        protected override void UpdateInternal(OutputPoint p)
        {
            var index = _seriesData.FindIndex(TimeMs.ToUtc(p.Time));
            _seriesData.Metadata[index] = GetMetadata(p.Value, p.Metadata as MarkerInfo);
            _seriesData.YValues[index] = p.Value;
        }

        protected override void Clear()
        {
            _seriesData.Clear();
            _seriesData.Metadata.Clear();
        }

        private AlgoMarkerMetadata GetMetadata(double y, MarkerInfo marker)
        {
            if (marker == null || double.IsNaN(y))
                return null;
            return new AlgoMarkerMetadata(marker);
        }
    }
}
