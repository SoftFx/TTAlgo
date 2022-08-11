using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Collections.Generic;
using System.Windows;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public class CartesianTradeChart : CartesianChart
    {
        private const double RightShiftBarsCount = 30.0;
        private const int BarsOnWindow = 150;

        protected static readonly DependencyProperty _tradeEventHandlerSourceProperty = DependencyProperty.Register(nameof(TradeEventHandler),
            typeof(ITradeEventsWriter), typeof(CartesianTradeChart), new PropertyMetadata(ChangeTradeEventHandlerSource));

        protected static readonly DependencyProperty _indicatorObserverSourceProperty = DependencyProperty.Register(nameof(IndicatorObserver),
            typeof(IIndicatorObserver), typeof(CartesianTradeChart), new PropertyMetadata(ChangeIndicatorObserverSource));

        protected static readonly DependencyProperty _barsSourceProperty = DependencyProperty.Register(nameof(BarsSource),
            typeof(ObservableBarVector), typeof(CartesianTradeChart), new PropertyMetadata(ChangeBarsSource));

        protected static readonly DependencyProperty _periodSourceProperty = DependencyProperty.Register(nameof(Period),
            typeof(Feed.Types.Timeframe), typeof(CartesianTradeChart), new PropertyMetadata(ChangePeriodSource));

        protected static readonly DependencyProperty _precisionSourceProperty = DependencyProperty.Register(nameof(PricePrecision),
            typeof(int), typeof(CartesianTradeChart), new PropertyMetadata(ChangePricePrecisionSource));

        protected static readonly DependencyProperty _showLegendSourceProperty = DependencyProperty.Register(nameof(ShowLegend),
            typeof(bool), typeof(CartesianTradeChart), new PropertyMetadata(ChangeShowLegendSource));


        private protected readonly TradeChartSettings _settings;
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

        public ITradeEventsWriter TradeEventHandler
        {
            get => (ITradeEventsWriter)GetValue(_tradeEventHandlerSourceProperty);
            set => SetValue(_tradeEventHandlerSourceProperty, value);
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


        public CartesianTradeChart()
        {
            TooltipPosition = TooltipPosition.Right;

            _settings = new TradeChartSettings
            {
                Precision = PricePrecision,
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
            {
                var series = new List<ISeries>()
                {
                    Customizer.GetBarSeries(BarsSource, _settings),
                };

                if (TradeEventHandler is not null)
                    series.AddRange(TradeEventHandler.Markers);

                if (IndicatorObserver is not null)
                    series.AddRange(IndicatorObserver[Metadata.Types.OutputTarget.Overlay]);

                Series = series;
            }
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
                chart._settings.Precision = chart.PricePrecision;
        }

        private static void ChangeTradeEventHandlerSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTradeChart chart)
                chart.TradeEventHandler.InitNewDataEvent += chart.UpdateDrawableSeries;
        }

        private static void ChangeIndicatorObserverSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTradeChart chart)
                chart.IndicatorObserver.InitIndicatorsEvent += chart.UpdateDrawableSeries;
        }

        private static void ChangeShowLegendSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianTradeChart chart)
                chart.LegendPosition = chart.ShowLegend ? LegendPosition.Left : LegendPosition.Hidden;
        }
    }
}