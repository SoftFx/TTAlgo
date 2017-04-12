using Caliburn.Micro;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Entities;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class DoubleSeriesAdapter : CrossDomainObject
    {
        private OutputFixture<double> buffer;

        public DoubleSeriesAdapter(OutputFixture<double> buffer, ColoredLineOutputSetup setup)
        {
            this.buffer = buffer;
            this.SeriesData = new XyDataSeries<DateTime, double>();

            if (setup.IsEnabled)
            {
                buffer.Updated += Update;
                buffer.Appended += Append;
                buffer.AllUpdated += CopyAll;

                SeriesData.SeriesName = setup.Descriptor.Id;
            }
        }

        public XyDataSeries<DateTime, double> SeriesData { get; private set; }

        private void Append(OutputFixture<double>.Point point)
        {
            if (point.TimeCoordinate != null)
                Execute.OnUIThread(() => SeriesData.Append(point.TimeCoordinate.Value, point.Value));
        }

        private void Update(OutputFixture<double>.Point point)
        {
            if (point.TimeCoordinate != null)
                Execute.OnUIThread(() => SeriesData.Update(point.Index, point.Value));
        }

        private void CopyAll(OutputFixture<double>.Point[] points)
        {
            Execute.OnUIThread(() =>
            {
                SeriesData.Clear();
                SeriesData.Append(points.Select(p => p.TimeCoordinate.Value), points.Select(p => p.Value));
            });
        }
    }

    internal class MarkerSeriesAdapter : CrossDomainObject
    {
        private OutputFixture<Marker> buffer;

        public MarkerSeriesAdapter(OutputFixture<Marker> buffer, MarkerSeriesOutputSetup setup)
        {
            this.buffer = buffer;

            SeriesData = new XyDataSeries<DateTime, double>();

            if (setup.IsEnabled)
            {
                buffer.Updated += Update;
                buffer.Appended += Append;
                buffer.AllUpdated += CopyAll;

                SeriesData.SeriesName = setup.Descriptor.Id;

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

        public XyDataSeries<DateTime, double> SeriesData { get; private set; }

        private void SetSize(AlgoPointMarker markerTool, MarkerSizes size)
        {
            switch (size)
            {
                case MarkerSizes.Large: markerTool.Width = 16; markerTool.Height = 16; break;
                case MarkerSizes.Small: markerTool.Width = 4; markerTool.Height = 4; break;
                default: markerTool.Width = 8; markerTool.Height = 8; break;
            }
        }

        private void Append(OutputFixture<Marker>.Point point)
        {
            var x = point.TimeCoordinate;
            var marker = point.Value;

            if (x != null)
                Execute.OnUIThread(() => SeriesData.Append(x.Value, marker.Y, new AlgoMarkerMetadata(marker)));
        }

        private void Update(OutputFixture<Marker>.Point point)
        {
            var x = point.TimeCoordinate;
            var marker = point.Value;

            if (x != null)
            {
                Execute.OnUIThread(() =>
                {
                    SeriesData.Metadata[point.Index] = new AlgoMarkerMetadata(marker);
                    SeriesData.YValues[point.Index] = marker.Y;
                });
            }
        }

        private void CopyAll(OutputFixture<Marker>.Point[] points)
        {
            Execute.OnUIThread(() =>
            {
                SeriesData.Clear();
                SeriesData.Append(
                    points.Select(p => p.TimeCoordinate.Value),
                    points.Select(p => p.Value.Y),
                    points.Select(p => GetMetadata(p.Value)));
            });
        }

        private AlgoMarkerMetadata GetMetadata(Marker marker)
        {
            if (double.IsNaN(marker.Y))
                return null;
            return new AlgoMarkerMetadata(marker);
        }
    }
}
