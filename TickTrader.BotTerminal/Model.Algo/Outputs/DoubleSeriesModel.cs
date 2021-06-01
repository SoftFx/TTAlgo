using Google.Protobuf.WellKnownTypes;
using SciChart.Charting.Model.DataSeries;
using System;

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

        protected override void AppendInternal(DateTime time, Any data)
        {
            _seriesData.Append(time, UnpackValue(data));
        }

        protected override void UpdateInternal(int index, DateTime time, Any data)
        {
            _seriesData.Update(index, UnpackValue(data));
        }

        protected override void Clear()
        {
            _seriesData.Clear();
        }

        private double UnpackValue(Any data)
        {
            var y = NanValue;
            if (data != null)
                y = data.Unpack<DoubleValue>().Value;
            return y;
        }
    }
}
