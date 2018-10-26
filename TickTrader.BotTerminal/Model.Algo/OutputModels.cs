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
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal abstract class OutputSeriesModel
    {
        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public OutputDescriptor Descriptor { get; private set; }

        public string PluginId { get; private set; }

        public OutputSetupModel Setup { get; private set; }

        public abstract IXyDataSeries SeriesData { get; }


        protected void Init(PluginExecutor executor, OutputSetupModel setup)
        {
            Id = setup.Metadata.Id;
            DisplayName = setup.Metadata.DisplayName;
            Descriptor = setup.Metadata.Descriptor;
            PluginId = executor.InstanceId;
            Setup = setup;
        }
    }


    internal abstract class OutputSeriesModel<T> : OutputSeriesModel, IOutputListener<T>
    {
        protected OutputAdapter<T> CreateOutputAdapter(PluginExecutor executor, OutputSetupModel setup)
        {
            var fixture = executor.GetOutput<T>(setup.Id);
            var adapter = new OutputAdapter<T>(fixture, this);
            return adapter;
        }


        protected abstract void AppendInternal(OutputFixture<T>.Point point);

        protected abstract void UpdateInternal(OutputFixture<T>.Point point);

        protected abstract void CopyAllInternal(OutputFixture<T>.Point[] points);


        void IOutputListener<T>.Append(OutputFixture<T>.Point point)
        {
            AppendInternal(point);
        }

        void IOutputListener<T>.Update(OutputFixture<T>.Point point)
        {
            UpdateInternal(point);
        }

        void IOutputListener<T>.CopyAll(OutputFixture<T>.Point[] points)
        {
            CopyAllInternal(points);
        }
    }


    internal class DoubleSeriesModel : OutputSeriesModel<double>
    {
        private XyDataSeries<DateTime, double> _seriesData;
        private OutputAdapter<double> _adapter;


        public override IXyDataSeries SeriesData => _seriesData;


        public DoubleSeriesModel(PluginExecutor executor, ColoredLineOutputSetupModel setup)
        {
            _seriesData = new XyDataSeries<DateTime, double>();
            Init(executor, setup);

            if (setup.IsEnabled)
            {
                _adapter = CreateOutputAdapter(executor, setup);

                _seriesData.SeriesName = DisplayName;
            }
        }


        protected override void AppendInternal(OutputFixture<double>.Point point)
        {
            if (point.TimeCoordinate != null)
                Execute.OnUIThread(() => _seriesData.Append(point.TimeCoordinate.Value, point.Value));
        }

        protected override void UpdateInternal(OutputFixture<double>.Point point)
        {
            if (point.TimeCoordinate != null)
                Execute.OnUIThread(() => _seriesData.Update(point.Index, point.Value));
        }

        protected override void CopyAllInternal(OutputFixture<double>.Point[] points)
        {
            Execute.OnUIThread(() =>
            {
                _seriesData.Clear();
                _seriesData.Append(points.Select(p => p.TimeCoordinate.Value), points.Select(p => p.Value));
            });
        }
    }


    internal class MarkerSeriesModel : OutputSeriesModel<Marker>
    {
        private XyDataSeries<DateTime, double> _seriesData;
        private OutputAdapter<Marker> _adapter;


        public override IXyDataSeries SeriesData => _seriesData;


        public MarkerSeriesModel(PluginExecutor executor, MarkerSeriesOutputSetupModel setup)
        {
            _seriesData = new XyDataSeries<DateTime, double>();
            Init(executor, setup);

            if (setup.IsEnabled)
            {
                _adapter = CreateOutputAdapter(executor, setup);

                _seriesData.SeriesName = DisplayName;

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


        protected override void AppendInternal(OutputFixture<Marker>.Point point)
        {
            var x = point.TimeCoordinate;
            var marker = point.Value;

            if (x != null)
                Execute.OnUIThread(() => _seriesData.Append(x.Value, marker.Y, GetMetadata(marker)));
        }

        protected override void UpdateInternal(OutputFixture<Marker>.Point point)
        {
            var x = point.TimeCoordinate;
            var marker = point.Value;

            if (x != null)
            {
                Execute.OnUIThread(() =>
                {
                    _seriesData.Metadata[point.Index] = GetMetadata(marker);
                    _seriesData.YValues[point.Index] = marker.Y;
                });
            }
        }

        protected override void CopyAllInternal(OutputFixture<Marker>.Point[] points)
        {
            Execute.OnUIThread(() =>
            {
                _seriesData.Clear();
                _seriesData.Metadata.Clear();
                _seriesData.Append(
                    points.Select(p => p.TimeCoordinate.Value),
                    points.Select(p => p.Value.Y),
                    points.Select(p => GetMetadata(p.Value)));
            });
        }


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
