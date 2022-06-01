using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Windows;
using System.Windows.Input;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public enum ChartTypes { Candle, Line, Mountain, DigitalLine, DigitalMountain, Scatter }


    public class CartesianScrollBarChart : CartesianChart
    {
        private const double RightShiftBarsCount = 30.0;
        private const int BarsOnWindow = 150;
        private const int ScrollSpeed = 10;

        private static readonly DependencyProperty _barsSourceProperty = DependencyProperty.Register(nameof(BarsSource),
            typeof(ObservableBarVector), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeBarsSource));

        private static readonly DependencyProperty _periodSourceProperty = DependencyProperty.Register(nameof(Period),
            typeof(Feed.Types.Timeframe), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangePeriodSource));

        private static readonly DependencyProperty _typeSourceProperty = DependencyProperty.Register(nameof(ChartType),
            typeof(ChartTypes), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeChartTypeSource));

        private static readonly DependencyProperty _precisionSourceProperty = DependencyProperty.Register(nameof(PricePrecision),
            typeof(int), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangePricePrecisionSource));

        private static readonly DependencyProperty _autoScrollSourceProperty = DependencyProperty.Register(nameof(AutoScroll),
            typeof(bool), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeAutoScrollSource));

        private static readonly DependencyProperty _enableCrosshairSourceProperty = DependencyProperty.Register(nameof(EnableCrosshair),
            typeof(bool), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeEnableCrosshairSource));

        private readonly SupportPriceLine _bidSupportLine, _askSupportLine;
        private readonly YLabelAxis _yLabelAxis;
        private readonly Crosshair _crosshair;
        private readonly Axis _xAxis, _yAxis;
        private readonly ChartTradeSettings _settings;

        private double _rightSeriesShift;
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

        public ChartTypes ChartType
        {
            get => (ChartTypes)GetValue(_typeSourceProperty);
            set => SetValue(_typeSourceProperty, value);
        }

        public int PricePrecision
        {
            get => (int)GetValue(_precisionSourceProperty);
            set => SetValue(_precisionSourceProperty, value);
        }

        public bool AutoScroll
        {
            get => (bool)GetValue(_autoScrollSourceProperty);
            set => SetValue(_autoScrollSourceProperty, value);
        }

        public bool EnableCrosshair
        {
            get => (bool)GetValue(_enableCrosshairSourceProperty);
            set => SetValue(_enableCrosshairSourceProperty, value);
        }


        public CartesianScrollBarChart()
        {
            _settings = new ChartTradeSettings
            {
                SymbolDigits = PricePrecision,
                ChartType = ChartType,
                Period = Period,
            };

            ZoomMode = ZoomAndPanMode.None;

            _xAxis = Customizer.GetDefaultAxis().SetXSettings(_settings);
            _yAxis = Customizer.GetDefaultAxis().SetYSettings(_settings);

            _yLabelAxis = new YLabelAxis(_yAxis);

            XAxes = new Axis[] { _xAxis };
            YAxes = new Axis[] { _yAxis, _yLabelAxis };

            _crosshair = new Crosshair(this, _settings, yAxisIndex: 1);

            _bidSupportLine = new SupportPriceLine(Customizer.DownColor, _settings, labelAxisIndex: 1);
            _askSupportLine = new SupportPriceLine(Customizer.UpColor, _settings, labelAxisIndex: 1);

            MouseMove += _crosshair.OnCrossHairMove;

            Sections = new RectangularSection[] { _bidSupportLine, _askSupportLine, _crosshair.XLine, _crosshair.YLine };
        }


        protected override void OnMouseWheel(MouseWheelEventArgs e) => ScrollPage(e.Delta / ScrollSpeed);


        private void ScrollPage(int delta)
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

            _askSupportLine.X = _xAxis.MaxLimit;
            _bidSupportLine.X = _xAxis.MaxLimit;

            _crosshair.UpdateYLabelPosition(_xAxis.MaxLimit);
            _yLabelAxis.SyncWithMain();
        }

        private void InitStartPosition()
        {
            _currentPosition = MaxIndexBorder;
            ScrollPage(0);
        }

        private void SubscribeToTickUpdates()
        {
            BarsSource.ApplyNewTickEvent += (bid, ask) =>
            {
                _bidSupportLine.Price = bid;
                _askSupportLine.Price = ask;

                _crosshair.UpdateXLabelPosition(_yAxis.VisibleDataBounds.Min);
                _yLabelAxis.SyncWithMain();
            };
        }

        private void UpdatePeriod()
        {
            _settings.Period = Period;

            _xAxis?.SetXSettings(_settings);

            _rightSeriesShift = Period.ToTimespan().Ticks * RightShiftBarsCount;
        }

        private void SyncSeriesPosition()
        {
            if (AutoScroll)
                InitStartPosition();
        }

        private void UpdateDrawableSeries()
        {
            _settings.ChartType = ChartType;

            if (BarsSource is not null)
                Series = new ISeries[]
                {
                    Customizer.GetBarSeries(BarsSource, _settings),

                    _askSupportLine.Label,
                    _bidSupportLine.Label,

                    _crosshair.XLable,
                    _crosshair.YLable,
                };
        }


        private static void ChangeBarsSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var barSource = (ObservableBarVector)e.NewValue;
            var chart = (CartesianScrollBarChart)obj;

            barSource.VectorInitEvent += chart.InitStartPosition;
            barSource.NewBarEvent += chart.SyncSeriesPosition;

            chart.SubscribeToTickUpdates();
            chart.UpdateDrawableSeries();
        }

        private static void ChangeChartTypeSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianScrollBarChart chart)
            {
                chart.UpdateDrawableSeries();
                chart.InitStartPosition();
            }
        }

        private static void ChangePeriodSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as CartesianScrollBarChart)?.UpdatePeriod();
        }

        private static void ChangePricePrecisionSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianScrollBarChart chart)
                chart._settings.SymbolDigits = chart.PricePrecision;
        }

        private static void ChangeAutoScrollSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (obj as CartesianScrollBarChart)?.InitStartPosition();
        }

        private static void ChangeEnableCrosshairSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as CartesianScrollBarChart)?._crosshair.SwitchCrosshair((bool)e.NewValue);
        }
    }
}