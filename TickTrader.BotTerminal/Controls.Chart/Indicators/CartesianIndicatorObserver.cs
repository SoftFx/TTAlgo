using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Machinarium.Var;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static TickTrader.Algo.Domain.Metadata.Types;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public interface IIndicatorObserver
    {
        ObservableCollection<OutputTarget> Targets { get; }

        List<ISeries> this[OutputTarget target] { get; }

        BoolProperty HasSubIndicators { get; }


        ChartSettings GetSettings(OutputTarget target);

        event Action InitIndicatorsEvent;
    }


    internal sealed class IndicatorObserver : IIndicatorObserver
    {
        private readonly Dictionary<OutputTarget, List<ISeries>> _subIndicators = new();
        private readonly Dictionary<OutputTarget, ChartSettings> _settings = new();

        private readonly List<ISeries> _emptyList = Enumerable.Empty<ISeries>().ToList();

        private readonly VarContext _context = new();

        public List<ISeries> this[OutputTarget target] => _subIndicators.TryGetValue(target, out var source) ? source : _emptyList;

        public ObservableCollection<OutputTarget> Targets { get; } = new();

        public BoolProperty HasSubIndicators { get; }

        public event Action InitIndicatorsEvent;


        internal IndicatorObserver()
        {
            HasSubIndicators = _context.AddBoolProperty();

            foreach (var key in Enum.GetValues<OutputTarget>())
                _settings[key] = new ChartSettings();
        }


        public void LoadIndicators(OutputModel output, int digits)
        {
            _subIndicators.Clear();

            foreach (var pair in output.Series)
            {
                var name = pair.Key;
                var model = pair.Value;
                var target = model.Descriptor.Target;
                var precision = model.Descriptor.Precision;
                var type = model.Descriptor.PlotType;

                var mainColor = new SKColor(model.Config.LineColorArgb);
                var settings = new IndicatorChartSettings
                {
                    Name = name,
                    Precision = precision == -1 ? digits : precision,
                    Period = output.Config.Timeframe,
                };

                if (_settings.TryGetValue(target, out var global))
                    global.Precision = Math.Max(settings.Precision, global.Precision);

                var values = model.Values.Select(IndicatorPointsFactory.GetDefaultPoint);

                var stroke = new SolidColorPaint
                {
                    Color = mainColor,
                    StrokeThickness = model.Config.LineThickness,
                    PathEffect = GetLineStyle(model.Descriptor.DefaultLineStyle),
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

                series.Name = name;
                series.Values = values;
                series.ZIndex = 5;

                AddTargetToWindow(target, series);
            }

            Targets.Clear();

            foreach (var targets in _subIndicators.Keys)
                if (targets != OutputTarget.Overlay)
                    Targets.Add(targets);

            HasSubIndicators.Value = Targets.Count > 0;

            InitIndicatorsEvent?.Invoke();
        }

        public ChartSettings GetSettings(OutputTarget target)
        {
            return _settings.TryGetValue(target, out var setting) ? setting : new ChartSettings();
        }


        private void AddTargetToWindow(OutputTarget target, ISeries series)
        {
            if (!_subIndicators.ContainsKey(target))
                _subIndicators.Add(target, new List<ISeries>(1 << 2));

            _subIndicators[target].Add(series);
        }

        private static DashEffect GetLineStyle(LineStyle style) =>
            style switch
            {
                LineStyle.Dots or LineStyle.DotsRare or LineStyle.DotsVeryRare => new DashEffect(new float[] { 1, 3 }),
                LineStyle.Lines or LineStyle.LinesDots => new DashEffect(new float[] { 5, 5 }),
                _ => null,
            };
    }
}