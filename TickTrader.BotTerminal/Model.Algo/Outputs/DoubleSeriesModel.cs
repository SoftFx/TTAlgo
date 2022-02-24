using SciChart.Charting.Model.DataSeries;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class DoubleSeriesModel : OutputSeriesModel<double>
    {
        private XyDataSeries<DateTime, double> _seriesData;

        public override IXyDataSeries SeriesData => _seriesData;
        protected override double NanValue => double.NaN;

        public DoubleSeriesModel(IPluginModel plugin, IPluginDataChartModel outputHost, IOutputCollector collector)
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
            _seriesData.Append(TimeMs.ToUtc(p.Time), p.Value);
        }

        protected override void UpdateInternal(OutputPoint p)
        {
            var index = _seriesData.FindIndex(TimeMs.ToUtc(p.Time));
            _seriesData.Update(index, p.Value);
        }

        protected override void Clear()
        {
            _seriesData.Clear();
        }
    }
}
