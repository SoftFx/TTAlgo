using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed class SupportPriceLine : LineSection
    {
        private const int ZIndexConst = 1;

        private readonly ObservablePoint _coordinate = new();

        internal ISeries Label { get; }

        internal double? Price
        {
            get => _coordinate.Y;

            set
            {
                _coordinate.Y = value;

                Y = value;
            }
        }

        internal override double? X
        {
            get => _coordinate.X;

            set => _coordinate.X = value;
        }


        internal SupportPriceLine(SKColor color, TradeChartSettings settings, int labelAxisIndex = 0)
        {
            ZIndex = ZIndexConst;

            Stroke = new SolidColorPaint
            {
                Color = color,
                StrokeThickness = Customizer.DefaultSupportLineStrokeTickness,
            };

            var label = Customizer.GetYLabel(_coordinate, settings);

            label.DataLabelsPaint = new SolidColorPaint(color);
            label.ScalesYAt = labelAxisIndex;
            label.ZIndex = ZIndexConst;
            label.IsVisibleAtLegend = false;

            Label = label;
        }
    }
}
