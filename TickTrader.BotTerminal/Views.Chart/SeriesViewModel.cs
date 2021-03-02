﻿using SciChart.Charting.Model.ChartSeries;
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
using TickTrader.Algo.Domain;

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

        public static IRenderableSeriesViewModel FromOutputSeries(OutputSeriesModel outputModel)
        {
            if (outputModel.Setup is ColoredLineOutputSetupModel)
                return CreateOutputSeries(outputModel.SeriesData, (ColoredLineOutputSetupModel)outputModel.Setup);
            else if (outputModel.Setup is MarkerSeriesOutputSetupModel)
                return CreateOutputSeries(outputModel.SeriesData, (MarkerSeriesOutputSetupModel)outputModel.Setup);

            return null;
        }

        private static IRenderableSeriesViewModel CreateOutputSeries(IXyDataSeries seriesData, ColoredLineOutputSetupModel outputSetup)
        {
            var plotType = outputSetup.Metadata.Descriptor.PlotType;

            if (plotType == Metadata.Types.PlotType.Line || plotType == Metadata.Types.PlotType.DiscontinuousLine)
            {
                var viewModel = new LineRenderableSeriesViewModel();
                viewModel.DataSeries = seriesData;
                viewModel.DrawNaNAs = outputSetup.Metadata.Descriptor.PlotType == Metadata.Types.PlotType.DiscontinuousLine ?
                     LineDrawMode.Gaps : LineDrawMode.ClosedLines;
                viewModel.Stroke = outputSetup.LineColorArgb.ToWindowsColor();
                viewModel.StrokeThickness = outputSetup.LineThickness;
                viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
                viewModel.StrokeDashArray = outputSetup.LineStyle.ToStrokeDashArray();
                viewModel.StyleKey = "IndicatorSeriesStyle_Line";

                return viewModel;
            }
            else if (plotType == Metadata.Types.PlotType.Points)
            {
                var viewModel = new LineRenderableSeriesViewModel();
                viewModel.DataSeries = seriesData;
                viewModel.Stroke = outputSetup.LineColorArgb.ToWindowsColor();
                viewModel.StrokeThickness = 0;
                viewModel.PointMarker = new EllipsePointMarker()
                {
                    Height = 4,
                    Width = 4,
                    Fill = outputSetup.LineColorArgb.ToWindowsColor(),
                    StrokeThickness = outputSetup.LineThickness / 2
                };
                viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
                viewModel.StyleKey = "IndicatorSeriesStyle_Dots";

                return viewModel;
            }
            else if (plotType == Metadata.Types.PlotType.Histogram)
            {
                var viewModel = new ColumnRenderableSeriesViewModel();
                viewModel.DataSeries = seriesData;
                viewModel.DrawNaNAs = outputSetup.Metadata.Descriptor.PlotType == Metadata.Types.PlotType.DiscontinuousLine ?
                     LineDrawMode.Gaps : LineDrawMode.ClosedLines;
                viewModel.Stroke = outputSetup.LineColorArgb.ToWindowsColor();
                viewModel.Fill = new SolidColorBrush(outputSetup.LineColorArgb.ToWindowsColor());
                viewModel.StrokeThickness = outputSetup.LineThickness;
                viewModel.ZeroLineY = outputSetup.Metadata.Descriptor.ZeroLine;
                viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
                viewModel.StyleKey = "IndicatorSeriesStyle_Histogram";

                return viewModel;
            }

            throw new NotImplementedException("Unsupported plot type: " + plotType);
        }

        private static IRenderableSeriesViewModel CreateOutputSeries(IXyDataSeries seriesData, MarkerSeriesOutputSetupModel outputSetup)
        {
            var viewModel = new LineRenderableSeriesViewModel();
            viewModel.DataSeries = seriesData;
            viewModel.DrawNaNAs = LineDrawMode.Gaps;
            viewModel.StrokeThickness = 0;
            viewModel.IsVisible = outputSetup.IsEnabled && outputSetup.IsValid;
            viewModel.StyleKey = "MarkerSeries_Style";
            var markerTool = new AlgoPointMarker()
            {
                Stroke = outputSetup.LineColorArgb.ToWindowsColor(),
                StrokeThickness = outputSetup.LineThickness
            };
            switch (outputSetup.MarkerSize)
            {
                case Metadata.Types.MarkerSize.Large: markerTool.Width = 10; markerTool.Height = 20; break;
                case Metadata.Types.MarkerSize.Small: markerTool.Width = 6; markerTool.Height = 12; break;
                default: markerTool.Width = 8; markerTool.Height = 16; break;
            }
            viewModel.PointMarker = markerTool;

            return viewModel;
        }
    }
}
