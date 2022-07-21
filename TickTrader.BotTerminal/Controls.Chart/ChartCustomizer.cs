using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal static class Customizer
    {
        internal const float DefaultChartStroke = 1f;
        internal const float DefaultSupportLineStrokeTickness = 0.3f;

        private const int PeriodStep = 30;

        private static readonly SKColor _defaultAxisColor = SKColors.LightGray;
        private static readonly SKColor _lineColor = SKColors.SteelBlue;


        internal static TimeSpan DefaultAnimationSpeed { get; } = new TimeSpan();

        internal static SKColor UpColor { get; } = SKColors.Green;

        internal static SKColor DownColor { get; } = SKColors.OrangeRed;


        internal static Axis GetDefaultAxis(int textSize = 10)
        {
            return new Axis
            {
                AnimationsSpeed = DefaultAnimationSpeed,
                TextSize = textSize,

                LabelsPaint = new SolidColorPaint
                {
                    Color = _defaultAxisColor
                },

                SeparatorsPaint = new SolidColorPaint
                {
                    Color = _defaultAxisColor,
                    StrokeThickness = DefaultSupportLineStrokeTickness
                },
            };
        }

        internal static Axis SetXSettings(this Axis axis, ChartTradeSettings settings)
        {
            axis.Padding = new LiveChartsCore.Drawing.Padding(-50, 0, 0, 5);
            axis.Labeler = value => value >= DateTime.MinValue.Ticks ? new DateTime((long)value).ToString(settings.DateFormat) : null;
            axis.UnitWidth = settings.Period.ToTimespan().Ticks;

            //axis.ForceStepToMin = true;
            axis.MinStep = GetPeriodStep(settings.Period);

            return axis;
        }

        internal static Axis SetYSettings(this Axis axis, ChartTradeSettings settings)
        {
            axis.Labeler = value => value.ToString(settings.PriceFormat);
            axis.Position = AxisPosition.End;

            return axis;
        }


        internal static ColumnSeries<ObservablePoint> GetXLabel(ObservablePoint point)
        {
            var label = GetDefaultLabel(point);

            //label.DataLabelsPosition = DataLabelsPosition.Left;
            //label.DataLabelsPadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);

            return label;
        }

        internal static ColumnSeries<ObservablePoint> SetPeriod(this ColumnSeries<ObservablePoint> label, ChartTradeSettings settings)
        {
            label.DataLabelsFormatter = value => value.Model.X >= DateTime.MinValue.Ticks ? new DateTime((long)value.Model.X).ToString(settings.DateFormat) : null;

            return label;
        }

        internal static ColumnSeries<ObservablePoint> GetYLabel(ObservablePoint point, ChartTradeSettings settings)
        {
            var label = GetDefaultLabel(point);

            label.DataLabelsFormatter = value => value.Model.Y?.ToString(settings.PriceFormat);
            label.DataLabelsPadding = new LiveChartsCore.Drawing.Padding(55, 0, 0, -2);
            label.DataLabelsPosition = DataLabelsPosition.End;

            return label;
        }

        private static ColumnSeries<ObservablePoint> GetDefaultLabel(ObservablePoint point)
        {
            return new ColumnSeries<ObservablePoint>
            {
                Values = new ObservablePoint[] { point },
                Fill = new SolidColorPaint(SKColor.Empty),

                DataLabelsSize = 11,

                IsHoverable = false,
                IgnoresBarPosition = true,
            };
        }


        internal static ISeries GetBarSeries(ObservableBarVector source, ChartTradeSettings settings)
        {
            var series = settings.ChartType switch
            {
                ChartTypes.Candle => GetCandelSeries(settings),
                ChartTypes.Line => GetLineSeries(settings),
                ChartTypes.Mountain => GetLineSeries(settings, fill: true),
                _ => throw new ArgumentException($"Unsupported type for bar chart {settings.ChartType}"),
            };

            series.Values = source;
            series.AnimationsSpeed = DefaultAnimationSpeed;

            return series;
        }

        internal static ISeries GetCandelSeries(ChartTradeSettings settings)
        {
            return new CandlesticksSeries<FinancialPoint>
            {
                MaxBarWidth = 5,
                UpFill = new SolidColorPaint(UpColor),
                UpStroke = new SolidColorPaint(UpColor),
                DownFill = new SolidColorPaint(DownColor),
                DownStroke = new SolidColorPaint(DownColor),
                TooltipLabelFormatter = p => p.Model.ToCandelTooltipInfo(settings),
            };
        }

        internal static ISeries GetLineSeries(ChartTradeSettings settings, bool fill = false)
        {
            return new LineSeries<FinancialPoint>
            {
                Fill = fill ? new SolidColorPaint(_lineColor) : null,
                GeometrySize = 0,
                LineSmoothness = 0,
                TooltipLabelFormatter = p => p.Model.ToLineTooltipInfo(settings),
                Stroke = new SolidColorPaint(_lineColor, DefaultChartStroke),
            };
        }

        internal static ISeries GetScatterSeries<T>(TradeEventSettings settings) where T : class, ISizedVisualChartPoint<SkiaSharpDrawingContext>, new()
        {
            return new ScatterSeries<TradeEventPoint, T>
            {
                Values = settings.Events,
                Name = settings.Name,

                Fill = new SolidColorPaint(settings.Color),
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.Black,
                    StrokeThickness = 0.1f,
                },

                TooltipLabelFormatter = m => m.Model?.ToolTip,

                GeometrySize = 20,
                ZIndex = 10,
                AnimationsSpeed = DefaultAnimationSpeed,
            };
        }


        internal static string GetPeriodDateFormat(Feed.Types.Timeframe period)
        {
            return period switch
            {
                Feed.Types.Timeframe.MN => "MMM yyyy",
                Feed.Types.Timeframe.W or Feed.Types.Timeframe.D => "d MMM yyyy",
                Feed.Types.Timeframe.H4 or Feed.Types.Timeframe.H1 or Feed.Types.Timeframe.M30 or Feed.Types.Timeframe.M15 or Feed.Types.Timeframe.M5 or Feed.Types.Timeframe.M1 => "d MMM yyyy HH:mm",
                _ => "d MMM yyyy HH:mm:ss",
            };
        }


        private static long GetPeriodStep(Feed.Types.Timeframe period)
        {
            return period switch
            {
                Feed.Types.Timeframe.Ticks or
                Feed.Types.Timeframe.TicksLevel2 or
                Feed.Types.Timeframe.TicksVwap => PeriodStep,
                _ => period.ToTimespan().Ticks * PeriodStep,
            };
        }
    }
}