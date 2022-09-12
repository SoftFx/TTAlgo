using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Windows;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public class CartesianTargetIndicatorChart : CartesianChart
    {
        protected static readonly DependencyProperty _chartSettingsSourceProperty = DependencyProperty.Register(nameof(ChartSettings),
            typeof(ChartSettings), typeof(CartesianTargetIndicatorChart), new PropertyMetadata(ChangeChartSettingsSource));

        protected static readonly DependencyProperty _showLegendSourceProperty = DependencyProperty.Register(nameof(ShowLegend),
            typeof(bool), typeof(CartesianTargetIndicatorChart), new PropertyMetadata(ChangeShowLegendSource));

        protected readonly Axis _yAxis;


        public ChartSettings ChartSettings
        {
            get => (ChartSettings)GetValue(_chartSettingsSourceProperty);
            set => SetValue(_chartSettingsSourceProperty, value);
        }

        public bool ShowLegend
        {
            get => (bool)GetValue(_showLegendSourceProperty);
            set => SetValue(_showLegendSourceProperty, value);
        }


        public CartesianTargetIndicatorChart()
        {
            TooltipPosition = TooltipPosition.Right;

            _yAxis = Customizer.GetDefaultAxis();

            YAxes = new Axis[] { _yAxis };
        }


        private void UpdateAxis()
        {
            _yAxis.SetYSettings(ChartSettings);
        }

        private static void ChangeChartSettingsSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTargetIndicatorChart chart)
            {
                chart.UpdateAxis();
            }
        }

        private static void ChangeShowLegendSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTargetIndicatorChart chart)
                chart.LegendPosition = chart.ShowLegend ? LegendPosition.Left : LegendPosition.Hidden;
        }
    }
}
