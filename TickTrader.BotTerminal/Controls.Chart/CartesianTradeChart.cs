using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Windows;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public class CartesianTradeChart : CartesianChart
    {
        private const double RightShiftBarsCount = 30.0;
        private const int BarsOnWindow = 150;

        protected static readonly DependencyProperty _barsSourceProperty = DependencyProperty.Register(nameof(BarsSource),
            typeof(ObservableBarVector), typeof(CartesianTradeChart), new PropertyMetadata(ChangeBarsSource));

        protected static readonly DependencyProperty _periodSourceProperty = DependencyProperty.Register(nameof(Period),
            typeof(Feed.Types.Timeframe), typeof(CartesianTradeChart), new PropertyMetadata(ChangePeriodSource));

        protected static readonly DependencyProperty _precisionSourceProperty = DependencyProperty.Register(nameof(PricePrecision),
            typeof(int), typeof(CartesianTradeChart), new PropertyMetadata(ChangePricePrecisionSource));


        private protected readonly ChartTradeSettings _settings;
        protected readonly Axis _xAxis, _yAxis;

        protected double _rightSeriesShift;
        private int _currentPosition;


        private int MaxIndexBorder => BarsSource?.Count - 1 ?? 0;


        public ObservableBarVector BarsSource
        {
            get => (ObservableBarVector)GetValue(_barsSourceProperty);
            set => SetValue(_barsSourceProperty, value);
        }

        public Feed.Types.Timeframe Period
        {
            get => (Feed.Types.Timeframe)GetValue(_periodSourceProperty);
            set => SetValue(_periodSourceProperty, value);
        }

        public int PricePrecision
        {
            get => (int)GetValue(_precisionSourceProperty);
            set => SetValue(_precisionSourceProperty, value);
        }


        public CartesianTradeChart()
        {
            _settings = new ChartTradeSettings
            {
                SymbolDigits = PricePrecision,
                ChartType = ChartTypes.Candle,
                Period = Period,
            };

            _xAxis = Customizer.GetDefaultAxis().SetXSettings(_settings);
            _yAxis = Customizer.GetDefaultAxis().SetYSettings(_settings);

            Loaded += CartesianScrollBarChart_Loaded; // Required for correct tab switching
        }


        protected virtual void CartesianScrollBarChart_Loaded(object sender, RoutedEventArgs e)
        {
            ZoomMode = ZoomAndPanMode.X;

            XAxes = new Axis[] { _xAxis };
            YAxes = new Axis[] { _yAxis };

            UpdateDrawableSeries();
            InitStartPosition();
        }

        protected virtual void ScrollPage(int delta)
        {
            _currentPosition += delta;

            var currentBarSize = MaxIndexBorder;
            var currentWindowsSize = Math.Min(currentBarSize, BarsOnWindow);

            _currentPosition = Math.Min(_currentPosition, currentBarSize);
            _currentPosition = Math.Max(_currentPosition, currentWindowsSize);

            if (_currentPosition < 0 || BarsSource is null)
                return;

            _xAxis.MinLimit = BarsSource[_currentPosition - currentWindowsSize].Date.Ticks;
            _xAxis.MaxLimit = BarsSource[_currentPosition].Date.Ticks;

            if (_currentPosition == currentBarSize && _xAxis.MaxLimit is not null)
                _xAxis.MaxLimit += _rightSeriesShift;
        }

        protected virtual void InitStartPosition()
        {
            _currentPosition = MaxIndexBorder;
            ScrollPage(0);
        }

        protected virtual void UpdateDrawableSeries()
        {
            if (BarsSource != null)
                Series = new ISeries[]
                {
                    Customizer.GetBarSeries(BarsSource, _settings),
                };
        }

        protected virtual void UpdatePeriod()
        {
            _settings.Period = Period;

            _xAxis?.SetXSettings(_settings);

            _rightSeriesShift = Period.ToTimespan().Ticks * RightShiftBarsCount;
        }

        protected virtual void InitializeBarsSource()
        {
            BarsSource.VectorInitEvent += InitStartPosition;

            UpdateDrawableSeries();
        }


        private static void ChangeBarsSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as CartesianTradeChart)?.InitializeBarsSource();
        }

        private static void ChangePeriodSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as CartesianTradeChart)?.UpdatePeriod();
        }

        private static void ChangePricePrecisionSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTradeChart chart)
                chart._settings.SymbolDigits = chart.PricePrecision;
        }
    }
}
