using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Entities;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using SciChart.Charting.Model.DataSeries;
using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    internal class MarkerSeriesModel : OutputSeriesModel<Marker>
    {
        private static readonly Marker NanMarker = new MarkerEntity();

        private XyDataSeries<DateTime, double> _seriesData;

        public override IXyDataSeries SeriesData => _seriesData;
        protected override Marker NanValue => NanMarker;

        public MarkerSeriesModel(IPluginModel plugin, IPluginDataChartModel outputHost, IOutputCollector collector, MarkerSeriesOutputSetupModel setup)
            : base(collector, outputHost, setup.IsEnabled)
        {
            _seriesData = new XyDataSeries<DateTime, double>();
            Init(plugin, setup);

            if (setup.IsEnabled)
            {
                _seriesData.SeriesName = DisplayName;
                Enable();

                //buffer.Updated = Update;
                //buffer.Appended = Append;

                //buffer.BeginBatchBuild = () => isBatchBuild = true;

                //buffer.EndBatchBuild = () =>
                //{
                //    isBatchBuild = false;
                //    CopyAll();
                //};

                //seriesData.SeriesName = setup.Descriptor.DisplayName;
                //FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
                //chartSeries.DataSeries = seriesData;
                //chartSeries.StrokeThickness = 0;
                //var markerTool = new AlgoPointMarker()
                //{
                //    Stroke = setup.LineColor,
                //    StrokeThickness = setup.LineThickness
                //};
                //SetSize(markerTool, setup.MarkerSize);
                //chartSeries.PointMarker = markerTool;
                //context.AddSeries(chartSeries);
            }
        }

        protected override void AppendInternal(DateTime time, Marker marker)
        {
            _seriesData.Append(time, marker.Y, GetMetadata(marker));
        }

        protected override void UpdateInternal(int index, DateTime time, Marker marker)
        {
            _seriesData.Metadata[index] = GetMetadata(marker);
            _seriesData.YValues[index] = marker.Y;
        }

        protected override void Clear()
        {
            _seriesData.Clear();
            _seriesData.Metadata.Clear();
        }

        //protected override void CopyAllInternal(OutputFixture<Marker>.Point[] points)
        //{
        //    Execute.OnUIThread(() =>
        //    {
        //        _seriesData.Clear();
        //        _seriesData.Metadata.Clear();
        //        _seriesData.Append(
        //            points.Select(p => p.TimeCoordinate.Value),
        //            points.Select(p => p.Value.Y),
        //            points.Select(p => GetMetadata(p.Value)));
        //    });
        //}


        //private void SetSize(AlgoPointMarker markerTool, MarkerSizes size)
        //{
        //    switch (size)
        //    {
        //        case MarkerSizes.Large: markerTool.Width = 16; markerTool.Height = 16; break;
        //        case MarkerSizes.Small: markerTool.Width = 4; markerTool.Height = 4; break;
        //        default: markerTool.Width = 8; markerTool.Height = 8; break;
        //    }
        //}

        private AlgoMarkerMetadata GetMetadata(Marker marker)
        {
            if (double.IsNaN(marker.Y))
                return null;
            return new AlgoMarkerMetadata(marker);
        }
    }
}
