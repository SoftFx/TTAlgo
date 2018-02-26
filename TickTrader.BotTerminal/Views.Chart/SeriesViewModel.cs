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
using TickTrader.Algo.Common.Model.Setup;
using SciChart.Charting.Visuals.RenderableSeries;
using TickTrader.Algo.Api;
using SciChart.Charting.Visuals.PointMarkers;

namespace TickTrader.BotTerminal
{
    internal static class SeriesViewModel
    {
        //public static IRenderableSeriesViewModel Create(ChartModelBase chart, IDataSeries data)
        //{
        //    if (chart is BarChartModel && data is OhlcDataSeries<DateTime, double>)
        //    {
        //        switch (chart.SelectedChartType)
        //        {
        //            case SelectableChartTypes.OHLC:
        //                return new OhlcRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_OhlcStyle" };
        //            case SelectableChartTypes.Candle:
        //                return new CandlestickRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_CandlestickStyle" };
        //            case SelectableChartTypes.Line:
        //                return new LineRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_LineStyle" };
        //            case SelectableChartTypes.Mountain:
        //                return new MountainRenderableSeriesViewModel() { DataSeries = data, StyleKey = "BarChart_MountainStyle" };
        //        }
        //    }
        //    else if (chart is TickChartModel && data is XyDataSeries<DateTime, double>)
        //    {
        //        switch (chart.SelectedChartType)
        //        {
        //            case SelectableChartTypes.Line:
        //                return new LineRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_LineStyle" };
        //            case SelectableChartTypes.Mountain:
        //                return new MountainRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_MountainStyle" };
        //            case SelectableChartTypes.DigitalLine:
        //                return new LineRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_DigitalLineStyle", IsDigitalLine = true };
        //            case SelectableChartTypes.Scatter:
        //                return new XyScatterRenderableSeriesViewModel() { DataSeries = data, StyleKey = "TickChart_ScatterStyle" };
        //        }
        //    }

        //    return null;
        //}

        public static IRenderableSeriesViewModel CreateIndicatorSeries(IndicatorModel model, OutputSetupModel outputSetup)
        {
            var seriesData = model.GetOutputSeries(outputSetup.Id);

            if (outputSetup is ColoredLineOutputSetupModel)
                return CreateIndicatorSeries(seriesData, (ColoredLineOutputSetupModel)outputSetup);
            else if (outputSetup is MarkerSeriesOutputSetupModel)
                return CreateIndicatorSeries(seriesData, (MarkerSeriesOutputSetupModel)outputSetup);

            return null;
        }

        private static IRenderableSeriesViewModel CreateIndicatorSeries(IXyDataSeries seriesData, ColoredLineOutputSetupModel outputSetup)
        {
            var plotType = outputSetup.Descriptor.PlotType;

            if (plotType == PlotType.Line || plotType == PlotType.DiscontinuousLine)
            {
                var viewModel = new LineRenderableSeriesViewModel();
                viewModel.DataSeries = seriesData;
                viewModel.DrawNaNAs = outputSetup.Descriptor.PlotType == Algo.Api.PlotType.DiscontinuousLine ?
                     LineDrawMode.Gaps : LineDrawMode.ClosedLines;
                viewModel.Stroke = outputSetup.LineColor;
                viewModel.StrokeThickness = outputSetup.LineThickness;
                viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
                viewModel.StrokeDashArray = outputSetup.LineStyle.ToStrokeDashArray();
                viewModel.StyleKey = "IndicatorSeriesStyle_Line";

                return viewModel;
            }
            else if (plotType == PlotType.Points)
            {
                var viewModel = new LineRenderableSeriesViewModel();
                viewModel.DataSeries = seriesData;
                viewModel.Stroke = outputSetup.LineColor;
                viewModel.StrokeThickness = 0;
                viewModel.PointMarker = new EllipsePointMarker()
                {
                    Height = 4,
                    Width = 4,
                    Fill = outputSetup.LineColor,
                    StrokeThickness = outputSetup.LineThickness / 2
                };
                viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
                viewModel.StyleKey = "IndicatorSeriesStyle_Dots";

                return viewModel;
            }
            else if (plotType == PlotType.Histogram)
            {
                var viewModel = new ColumnRenderableSeriesViewModel();
                viewModel.DataSeries = seriesData;
                viewModel.DrawNaNAs = outputSetup.Descriptor.PlotType == Algo.Api.PlotType.DiscontinuousLine ?
                     LineDrawMode.Gaps : LineDrawMode.ClosedLines;
                viewModel.Stroke = outputSetup.LineColor;
                viewModel.Fill = new SolidColorBrush(outputSetup.LineColor);
                viewModel.StrokeThickness = outputSetup.LineThickness;
                viewModel.ZeroLineY = outputSetup.Descriptor.ZeroLine;
                viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
                viewModel.StyleKey = "IndicatorSeriesStyle_Histogram";

                return viewModel;
            }

            throw new NotImplementedException("Unsupported plot type: " + plotType);
        }

        private static IRenderableSeriesViewModel CreateIndicatorSeries(IXyDataSeries seriesData, MarkerSeriesOutputSetupModel outputSetup)
        {
            var viewModel = new LineRenderableSeriesViewModel();
            viewModel.DataSeries = seriesData;
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
}
