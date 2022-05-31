using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using System.Windows.Input;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed class Crosshair
    {
        private const int ZIndex = 10;

        private static readonly SKColor _crosshairColor = SKColors.DarkOliveGreen;

        private readonly ObservablePoint _xCoordinate = new();
        private readonly ObservablePoint _yCoordinate = new();
        private readonly ChartTradeSettings _settings;
        private readonly CartesianChart _chart;
        private bool _isEnable;


        internal ISeries XLable { get; }

        internal ISeries YLable { get; }


        internal LineSection XLine { get; }

        internal LineSection YLine { get; }


        private double? Y
        {
            set
            {
                if (_yCoordinate.Y == value)
                    return;

                _yCoordinate.Y = value;

                YLine.Y = value;
            }
        }

        private double? X
        {
            set
            {
                if (_xCoordinate.X == value)
                    return;

                _xCoordinate.X = value;

                XLine.X = value;
            }
        }


        internal Crosshair(CartesianChart chart, ChartTradeSettings settings, int yAxisIndex = 0)
        {
            _settings = settings;
            _chart = chart;

            var stroke = new SolidColorPaint
            {
                Color = _crosshairColor,
                StrokeThickness = 0.5f,
                ZIndex = ZIndex,
            };


            XLine = new LineSection(stroke);
            YLine = new LineSection(stroke);

            YLable = BuildLabel(_yCoordinate, yAxisIndex);
            XLable = BuildLabel(_xCoordinate, yAxisIndex).SetPeriod(settings);

            SetVisibility(_isEnable);
        }


        internal void OnCrossHairMove(object sender, MouseEventArgs e) => UpdateLines();

        internal void SwitchCrosshair(bool enable)
        {
            _isEnable = enable;

            SetVisibility(enable);

            if (_isEnable)
                UpdateLines();
        }

        internal void UpdateYLabelPosition(double? newXPointForYLabel)
        {
            _yCoordinate.X = newXPointForYLabel;

            UpdateLines();
        }

        internal void UpdateXLabelPosition(double? newYPointForXLabel)
        {
            _xCoordinate.Y = newYPointForXLabel;

            UpdateLines();
        }


        internal void UpdateLines()
        {
            if (!_isEnable)
                return;

            try
            {
                var mouseWindowCoords = Mouse.GetPosition(_chart);

                if (mouseWindowCoords.X < 0 || mouseWindowCoords.Y < 0)
                    return;

                var mouseChartCoords = _chart.ScaleUIPoint(new LvcPoint((float)mouseWindowCoords.X, (float)mouseWindowCoords.Y));

                X = mouseChartCoords[0];
                Y = mouseChartCoords[1];
            }
            catch
            {
                SetVisibility(false);
            }
        }


        private ColumnSeries<ObservablePoint> BuildLabel(ObservablePoint point, int yAxisIndex)
        {
            var label = Customizer.GetYLabel(point, _settings);

            label.DataLabelsPaint = new SolidColorPaint(_crosshairColor);
            label.ScalesYAt = yAxisIndex;
            label.DataLabelsSize = 13;
            label.ZIndex = ZIndex;

            return label;
        }

        private void SetVisibility(bool value)
        {
            XLine.IsVisible = value;
            YLine.IsVisible = value;

            XLable.IsVisible = value;
            YLable.IsVisible = value;
        }
    }
}
