using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.PaletteProviders;
using SciChart.Data.Numerics;
using System.ComponentModel;
using System.Windows.Media;
using TickTrader.Algo.Core;
using Caliburn.Micro;
using TickTrader.Algo.GuiModel;
using SciChart.Charting.Visuals.RenderableSeries;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal static class SeriesViewModel
    {
        public static IRenderableSeriesViewModel Create(ChartModelBase chart, IDataSeries data)
        {
            if (chart is BarChartModel && data is OhlcDataSeries<DateTime, double>)
            {
                switch (chart.SelectedChartType)
                {
                    case SelectableChartTypes.OHLC:
                        return new OhlcRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_OhlcStyle" };
                    case SelectableChartTypes.Candle:
                        return new CandlestickRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_CandlestickStyle" };
                    case SelectableChartTypes.Line:
                        return new LineRenderableSeriesViewModel() { DataSeries = data, StyleKey= "BarChart_LineStyle" };
                    case SelectableChartTypes.Mountain:
                        return new MountainRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_MountainStyle" };
                }
            }

            return null;
        }

        public static IRenderableSeriesViewModel Create(IIndicatorAdapterContext context, OutputSetup outputSetup)
        {
            if (outputSetup is ColoredLineOutputSetup)
                return Create(context, (ColoredLineOutputSetup)outputSetup);
            else if (outputSetup is MarkerSeriesOutputSetup)
                return Create(context, (MarkerSeriesOutputSetup)outputSetup);

            return null;
        }

        private static IRenderableSeriesViewModel Create(IIndicatorAdapterContext context, ColoredLineOutputSetup outputSetup)
        {
            DoubleSeriesAdapter adapter = new DoubleSeriesAdapter(context, outputSetup.Id);

            var viewModel = new LineRenderableSeriesViewModel();
            viewModel.DataSeries = adapter.SeriesData;
            viewModel.DrawNaNAs = outputSetup.Descriptor.PlotType == Algo.Api.PlotType.DiscontinuousLine ?
                 LineDrawMode.Gaps : LineDrawMode.ClosedLines;
            viewModel.Stroke = outputSetup.LineColor;
            viewModel.StrokeThickness = outputSetup.LineThickness;
            viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
            viewModel.StrokeDashArray = outputSetup.LineStyle.ToStrokeDashArray();
            viewModel.StyleKey = "DoubleSeries_Style";

            return viewModel;
        }

        private static IRenderableSeriesViewModel Create(IIndicatorAdapterContext context, MarkerSeriesOutputSetup outputSetup)
        {
            MarkerSeriesAdapter adapter = new MarkerSeriesAdapter(context, outputSetup.Id);

            var viewModel = new LineRenderableSeriesViewModel();
            viewModel.DataSeries = adapter.SeriesData;
            viewModel.DrawNaNAs = LineDrawMode.Gaps;
            viewModel.StrokeThickness = 0;
            viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
            viewModel.StyleKey = "MarkerSeries_Style";
            var markerTool = new AlgoPointMarker()
            {
                Stroke = outputSetup.LineColor,
                StrokeThickness = outputSetup.LineThickness
            };
            switch (outputSetup.MarkerSize)
            {
                case MarkerSizes.Large: markerTool.Width = 10; markerTool.Height = 20; break;
                case MarkerSizes.Small: markerTool.Width = 6; markerTool.Height = 12; break;
                default: markerTool.Width = 8; markerTool.Height = 16; break;
            }
            viewModel.PointMarker = markerTool;

            return viewModel;
        }
    }

    internal class DoubleSeriesAdapter
    {
        private IIndicatorAdapterContext context;
        private OutputBuffer<double> buffer;

        private bool isBatchBuild;

        public DoubleSeriesAdapter(IIndicatorAdapterContext context, string outputId)
        {
            this.context = context;
            this.buffer = context.GetOutput<double>(outputId);
            this.SeriesData = new XyDataSeries<DateTime, double>();

            buffer.Updated = Update;
            buffer.Appended = Append;

            buffer.BeginBatchBuild = () => isBatchBuild = true;

            buffer.EndBatchBuild = () =>
            {
                isBatchBuild = false;
                CopyAll();
            };
        }

        public XyDataSeries<DateTime, double> SeriesData { get; private set; }

        private void Append(int index, double data)
        {
            if (!isBatchBuild)
            {
                DateTime x = context.GetTimeCoordinate(index);
                Execute.OnUIThread(() => SeriesData.Append(x, data));
            }
        }

        private void Update(int index, double data)
        {
            if (!isBatchBuild)
            {
                DateTime x = context.GetTimeCoordinate(index);
                Execute.OnUIThread(() => SeriesData.YValues[index] = data);
            }
        }

        private void CopyAll()
        {
            Execute.OnUIThread(() =>
            {
                SeriesData.Clear();
                SeriesData.Append(EnumerateDateTimeCoordinate(), buffer);
            });
        }

        private IEnumerable<DateTime> EnumerateDateTimeCoordinate()
        {
            for (int i = 0; i < buffer.Count; i++)
                yield return context.GetTimeCoordinate(i);
        }
    }

    internal class MarkerSeriesAdapter
    {
        private IIndicatorAdapterContext context;
        private OutputBuffer<Marker> buffer;

        private bool isBatchBuild;

        public MarkerSeriesAdapter(IIndicatorAdapterContext context, string outputId)
        {
            this.context = context;
            this.buffer = context.GetOutput<Marker>(outputId);

            SeriesData = new XyDataSeries<DateTime, double>();

            buffer.Updated = Update;
            buffer.Appended = Append;

            buffer.BeginBatchBuild = () => isBatchBuild = true;

            buffer.EndBatchBuild = () =>
            {
                isBatchBuild = false;
                CopyAll();
            };
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

        private void Append(int index, Marker marker)
        {
            if (!isBatchBuild)
            {
                DateTime x = context.GetTimeCoordinate(index);
                Execute.OnUIThread(() => SeriesData.Append(x, marker.Y, new AlgoMarkerMetadata(marker)));
            }
        }

        private void Update(int index, Marker marker)
        {
            if (!isBatchBuild)
            {
                DateTime x = context.GetTimeCoordinate(index);
                Execute.OnUIThread(() =>
                {
                    SeriesData.Metadata[index] = new AlgoMarkerMetadata(marker);
                    SeriesData.YValues[index] = marker.Y;
                });
            }
        }

        private void CopyAll()
        {
            Execute.OnUIThread(() =>
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    var marker = buffer[i];
                    var x = context.GetTimeCoordinate(i);
                    var y = marker.Y;
                    SeriesData.Append(x, y, new AlgoMarkerMetadata(marker));
                }
            });
        }

        private IEnumerable<DateTime> EnumerateDateTimeCoordinate()
        {
            for (int i = 0; i < buffer.Count; i++)
                yield return context.GetTimeCoordinate(i);
        }
    }
}
