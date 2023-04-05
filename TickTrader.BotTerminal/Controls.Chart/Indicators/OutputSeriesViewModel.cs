using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System;
using TickTrader.Algo.Domain;
using static TickTrader.Algo.Domain.Metadata.Types;
using Machinarium.Qnil;
using TickTrader.Algo.IndicatorHost;
using System.Linq;
using System.IO;
using System.Globalization;
using Caliburn.Micro;
using System.Collections.Generic;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public interface IOutputSeriesViewModel : IDisposable
    {
        public ISeries Series { get; }


        void UpdatePoints();

        void DumpPoints(string dirPath, int symbolDigits);
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
                    Fill = null,
                    Stroke = stroke,
                    GeometryFill = color, // ideally we want it null, but then legend shows random color
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


    internal sealed class StaticOutputSeriesViewModel : PropertyChangedBase, IOutputSeriesViewModel
    {
        public ISeries Series { get; }


        public StaticOutputSeriesViewModel(OutputSeriesModel output, IndicatorChartSettings settings)
        {
            Series = CartesianOutputSeries.CreateSeries(output.Descriptor, output.Config, settings);
            Series.Values = output.Values.Select(IndicatorPointsFactory.GetDefaultPoint).ToList();
        }


        public void Dispose()
        {
            Series.Values = null;
        }

        public void UpdatePoints() { }

        public void DumpPoints(string dirPath, int symbolDigits) { }
    }


    internal sealed class DynamicOutputSeriesViewModel : PropertyChangedBase, IOutputSeriesViewModel
    {
        private readonly OutputSeriesProxy _output;
        private readonly SortedVarSetList<OutputPoint> _points = new(PointTimeComparer.Instance);
        private readonly IObservableList<IndicatorPoint> _observablePoints;


        public ISeries Series { get; }


        public DynamicOutputSeriesViewModel(OutputSeriesProxy output, IndicatorChartSettings settings)
        {
            _output = output;
            _observablePoints = _points.Where(p => !double.IsNaN(p.Value)).Chain().Select(IndicatorPointsFactory.GetDefaultPoint).Chain().AsObservable();
            Series = CartesianOutputSeries.CreateSeries(output.Descriptor, output.Config, settings);
            Series.Values = _observablePoints;
        }


        public void Dispose()
        {
            Series.Values = null;
            _observablePoints.Dispose();
        }

        public void UpdatePoints()
        {
            var updates = _output.TakePendingUpdates();
            foreach (var update in updates)
            {
                // Internally output buffer is index based, sparse buffers will have very few real points
                // We should preserve all original points to remove correct points when buffers are truncated

                switch (update.UpdateAction)
                {
                    case DataSeriesUpdate.Types.Action.Append:
                    case DataSeriesUpdate.Types.Action.Update:
                        for (var i = 0; i < update.BufferTruncatedBy; i++)
                            _points.RemoveAt(0);
                        break;
                    case DataSeriesUpdate.Types.Action.Reset:
                        _points.Clear();
                        break;
                }

                foreach (var wirePoint in update.Points)
                {
                    var point = wirePoint.Unpack();
                    _points.Add(point);
                }
            }
        }

        public void DumpPoints(string dirPath, int symbolDigits)
        {
            var precision = _output.Descriptor.Precision;
            if (precision == -1)
                precision = symbolDigits;
            var doubleFormat = "F" + precision.ToString();

            var path = Path.Combine(dirPath, $"{_output.PluginId} - {_output.Descriptor.Id}.csv");
            using (var file = File.Open(path, FileMode.Create))
            using (var writer = new StreamWriter(file, System.Text.Encoding.UTF8))
            {
                writer.WriteLine("Time,Value,Metadata"); // header
                foreach (var p in _points.Values)
                {
                    writer.Write(p.Time.ToUtcDateTime().ToString("yyyy-MM-dd HH-mm-ss"));
                    writer.Write(",");
                    writer.Write(p.Value.ToString(doubleFormat, CultureInfo.InvariantCulture));
                    writer.Write(",");
                    if (p.Metadata != null)
                    {
                        writer.Write(p.Metadata);
                    }

                    writer.WriteLine();
                }
            }
        }


        private sealed class PointTimeComparer : Algo.Core.Lib.Singleton<PointTimeComparer>, IComparer<OutputPoint>
        {
            public int Compare(OutputPoint x, OutputPoint y) => x.Time.CompareTo(y.Time);
        }
    }
}
