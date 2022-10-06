﻿using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System;
using TickTrader.Algo.Domain;
using static TickTrader.Algo.Domain.Metadata.Types;
using Machinarium.Qnil;
using TickTrader.Algo.Server;
using System.Linq;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public interface IOutputSeriesViewModel : IDisposable
    {
        public ISeries Series { get; }


        void UpdatePoints();
    }


    internal static class CartesianOutputSeries
    {
        public static ISeries CreateSeries(OutputDescriptor descriptor, IOutputConfig config, IndicatorChartSettings settings)
        {
            var target = descriptor.Target;
            var type = descriptor.PlotType;

            var mainColor = new SKColor(config.LineColorArgb);

            var stroke = new SolidColorPaint
            {
                Color = mainColor,
                StrokeThickness = config.LineThickness,
                PathEffect = GetLineStyle(descriptor.DefaultLineStyle),
            };

            var color = new SolidColorPaint
            {
                Color = mainColor,
            };

            ISeries series;

            if (type is PlotType.Line or PlotType.DiscontinuousLine)
            {
                series = new LineSeries<IndicatorPoint>
                {
                    Fill = Customizer.EmptyPaint,
                    Stroke = stroke,
                    GeometryFill = color,
                    GeometrySize = 0,
                    LineSmoothness = 0,
                    TooltipLabelFormatter = p => p.Model.ToPointTooltipInfo(settings),
                    EnableNullSplitting = type == PlotType.DiscontinuousLine,
                    AnimationsSpeed = Customizer.DefaultAnimationSpeed,
                };
            }
            else if (type is PlotType.Histogram)
            {
                series = new ColumnSeries<IndicatorPoint>
                {
                    IgnoresBarPosition = true,
                    Fill = stroke,
                    TooltipLabelFormatter = p => p.Model.ToPointTooltipInfo(settings),
                    AnimationsSpeed = Customizer.DefaultAnimationSpeed,
                };
            }
            else
            {
                series = new ScatterSeries<IndicatorPoint>
                {
                    Fill = stroke,
                    TooltipLabelFormatter = p => p.Model.ToPointTooltipInfo(settings),
                    AnimationsSpeed = Customizer.DefaultAnimationSpeed,
                    GeometrySize = 3,
                };
            }

            series.Name = settings.Name;
            series.ZIndex = 5;

            return series;
        }

        private static DashEffect GetLineStyle(LineStyle style) =>
            style switch
            {
                LineStyle.Dots or LineStyle.DotsRare or LineStyle.DotsVeryRare => new DashEffect(new float[] { 1, 3 }),
                LineStyle.Lines or LineStyle.LinesDots => new DashEffect(new float[] { 5, 5 }),
                _ => null,
            };
    }


    internal sealed class StaticOutputSeriesViewModel : IOutputSeriesViewModel
    {
        public ISeries Series { get; }


        public StaticOutputSeriesViewModel(OutputSeriesModel output, IndicatorChartSettings settings)
        {
            Series = CartesianOutputSeries.CreateSeries(output.Descriptor, output.Config, settings);
            Series.Values = output.Values.Select(IndicatorPointsFactory.GetDefaultPoint).ToList();
        }


        public void Dispose() { }

        public void UpdatePoints() { }
    }


    internal sealed class DynamicOutputSeriesViewModel : IOutputSeriesViewModel
    {
        private readonly OutputSeriesProxy _output;
        private readonly VarDictionary<UtcTicks, OutputPoint> _points = new();
        private readonly IObservableList<IndicatorPoint> _observablePoints;


        public ISeries Series { get; }


        public DynamicOutputSeriesViewModel(OutputSeriesProxy output, IndicatorChartSettings settings)
        {
            _output = output;
            _observablePoints = _points.OrderBy((k, v) => k).Chain().Select(IndicatorPointsFactory.GetDefaultPoint).Chain().AsObservable();
            Series = CartesianOutputSeries.CreateSeries(output.Descriptor, output.Config, settings);
            Series.Values = _observablePoints;
        }


        public void Dispose()
        {
            _observablePoints.Dispose();
        }

        public void UpdatePoints()
        {
            var updates = _output.TakePendingUpdates();
            foreach (var update in updates)
            {
                switch (update.UpdateAction)
                {
                    case DataSeriesUpdate.Types.Action.Append:
                    case DataSeriesUpdate.Types.Action.Update:
                        foreach (var wirePoint in update.Points)
                        {
                            if (double.IsNaN(wirePoint.Value))
                            {
                                _points.Remove(new UtcTicks(wirePoint.Time));
                            }
                            else
                            {
                                var point = wirePoint.Unpack();
                                _points[point.Time] = point;
                            }
                        }
                        break;
                    case DataSeriesUpdate.Types.Action.Reset:
                        _points.Clear();
                        foreach (var wirePoint in update.Points)
                        {
                            if (double.IsNaN(wirePoint.Value))
                                continue; // NaN values don't exist on chart

                            var point = wirePoint.Unpack();
                            _points[point.Time] = point;
                        }
                        break;
                }
            }
        }
    }
}