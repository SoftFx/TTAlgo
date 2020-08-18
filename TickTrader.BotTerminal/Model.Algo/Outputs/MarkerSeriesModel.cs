using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using SciChart.Charting.Model.DataSeries;
using Caliburn.Micro;
using TickTrader.Algo.Domain;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.BotTerminal
{
    internal class MarkerSeriesModel : OutputSeriesModel<MarkerInfo>
    {
        private static readonly MarkerInfo NanMarker = new MarkerInfo { ColorRgb = -1, Y = double.NaN };

        private XyDataSeries<DateTime, double> _seriesData;

        public override IXyDataSeries SeriesData => _seriesData;
        protected override MarkerInfo NanValue => NanMarker;

        public MarkerSeriesModel(IPluginModel plugin, IPluginDataChartModel outputHost, IOutputCollector collector, MarkerSeriesOutputSetupModel setup)
            : base(collector, outputHost, setup.IsEnabled)
        {
            _seriesData = new XyDataSeries<DateTime, double>();
            Init(plugin, setup);

            if (setup.IsEnabled)
            {
                _seriesData.SeriesName = DisplayName;
                Enable();
            }
        }

        protected override void AppendInternal(DateTime time, Any data)
        {
            var marker = UnpackValue(data);
            _seriesData.Append(time, marker.Y, GetMetadata(marker));
        }

        protected override void UpdateInternal(int index, DateTime time, Any data)
        {
            var marker = UnpackValue(data);
            _seriesData.Metadata[index] = GetMetadata(marker);
            _seriesData.YValues[index] = marker.Y;
        }

        protected override void Clear()
        {
            _seriesData.Clear();
            _seriesData.Metadata.Clear();
        }

        private AlgoMarkerMetadata GetMetadata(MarkerInfo marker)
        {
            if (double.IsNaN(marker.Y))
                return null;
            return new AlgoMarkerMetadata(marker);
        }

        private MarkerInfo UnpackValue(Any data)
        {
            var y = NanValue;
            if (data != null)
                y = data.Unpack<MarkerInfo>();
            return y;
        }
    }
}
