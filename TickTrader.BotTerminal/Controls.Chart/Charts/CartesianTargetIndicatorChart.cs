using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Windows;
using static TickTrader.Algo.Domain.Metadata.Types;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public class CartesianTargetIndicatorChart : CartesianChart
    {
        protected static readonly DependencyProperty _outputTargetIdSourceProperty = DependencyProperty.Register(nameof(OutputTargetId),
           typeof(OutputTarget), typeof(CartesianTargetIndicatorChart));

        protected static readonly DependencyProperty _indicatorObserverSourceProperty = DependencyProperty.Register(nameof(IndicatorObserver),
           typeof(IIndicatorObserver), typeof(CartesianTargetIndicatorChart), new PropertyMetadata(ChangeIndicatorObserverSource));

        protected static readonly DependencyProperty _showLegendSourceProperty = DependencyProperty.Register(nameof(ShowLegend),
            typeof(bool), typeof(CartesianTargetIndicatorChart), new PropertyMetadata(ChangeShowLegendSource));

        protected readonly Axis _yAxis;


        public OutputTarget OutputTargetId
        {
            get => (OutputTarget)GetValue(_outputTargetIdSourceProperty);
            set => SetValue(_outputTargetIdSourceProperty, value);
        }

        public IIndicatorObserver IndicatorObserver
        {
            get => (IIndicatorObserver)GetValue(_indicatorObserverSourceProperty);
            set => SetValue(_indicatorObserverSourceProperty, value);
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


        private void LoadIndicatorOutputs()
        {
            var points = IndicatorObserver[OutputTargetId];

            //Visibility = points.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            Series = points;
        }

        private void UpdateAxis()
        {
            _yAxis.SetYSettings(IndicatorObserver.GetSettings(OutputTargetId));
        }


        private static void ChangeIndicatorObserverSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTargetIndicatorChart chart)
            {
                chart.IndicatorObserver.InitIndicatorsEvent += chart.LoadIndicatorOutputs;

                chart.UpdateAxis();
                chart.LoadIndicatorOutputs();
            }
        }

        private static void ChangeShowLegendSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTargetIndicatorChart chart)
                chart.LegendPosition = chart.ShowLegend ? LegendPosition.Left : LegendPosition.Hidden;
        }
    }
}
