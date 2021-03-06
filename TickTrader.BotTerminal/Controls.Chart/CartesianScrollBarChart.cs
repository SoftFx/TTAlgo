using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System.Windows;
using System.Windows.Input;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public enum ChartTypes { Candle, Line, Mountain, DigitalLine, DigitalMountain, Scatter }


    public class CartesianScrollBarChart : CartesianTradeChart
    {
        private const int MouseScrollSpeed = 10;

        private static readonly DependencyProperty _typeSourceProperty = DependencyProperty.Register(nameof(ChartType),
            typeof(ChartTypes), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeChartTypeSource));

        private static readonly DependencyProperty _autoScrollSourceProperty = DependencyProperty.Register(nameof(AutoScroll),
            typeof(bool), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeAutoScrollSource));

        private static readonly DependencyProperty _enableCrosshairSourceProperty = DependencyProperty.Register(nameof(EnableCrosshair),
            typeof(bool), typeof(CartesianScrollBarChart), new PropertyMetadata(ChangeEnableCrosshairSource));

        private readonly SupportPriceLine _bidSupportLine, _askSupportLine;
        private readonly YLabelAxis _yLabelAxis;
        private readonly Crosshair _crosshair;


        public ChartTypes ChartType
        {
            get => (ChartTypes)GetValue(_typeSourceProperty);
            set => SetValue(_typeSourceProperty, value);
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


        public CartesianScrollBarChart() : base()
        {
            _settings.ChartType = ChartType;

            _yLabelAxis = new YLabelAxis(_yAxis);
            _crosshair = new Crosshair(this, _settings, yAxisIndex: 1);

            _bidSupportLine = new SupportPriceLine(Customizer.DownColor, _settings, labelAxisIndex: 1);
            _askSupportLine = new SupportPriceLine(Customizer.UpColor, _settings, labelAxisIndex: 1);

            MouseMove += _crosshair.OnCrossHairMove;
        }


        protected override void CartesianScrollBarChart_Loaded(object sender, RoutedEventArgs e)
        {
            base.CartesianScrollBarChart_Loaded(sender, e);

            ZoomMode = ZoomAndPanMode.None;

            YAxes = new Axis[] { _yAxis, _yLabelAxis };
            Sections = new RectangularSection[] { _bidSupportLine, _askSupportLine, _crosshair.XLine, _crosshair.YLine };
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) => ScrollPage(e.Delta / MouseScrollSpeed);


        protected override void ScrollPage(int delta)
        {
            base.ScrollPage(delta);

            _askSupportLine.X = _xAxis.MaxLimit;
            _bidSupportLine.X = _xAxis.MaxLimit;

            _crosshair.UpdateYLabelPosition(_xAxis.MaxLimit);
            _yLabelAxis.SyncWithMain();
        }

        protected override void UpdateDrawableSeries()
        {
            _settings.ChartType = ChartType;

            if (BarsSource != null)
                Series = new ISeries[]
                {
                    Customizer.GetBarSeries(BarsSource, _settings),

                    _askSupportLine.Label,
                    _bidSupportLine.Label,

                    _crosshair.XLable,
                    _crosshair.YLable,
                };
        }

        protected override void InitializeBarsSource()
        {
            base.InitializeBarsSource();

            BarsSource.NewBarEvent += () =>
            {
                if (AutoScroll)
                    InitStartPosition();
            };

            BarsSource.ApplyNewTickEvent += (bid, ask) =>
            {
                _bidSupportLine.Price = bid;
                _askSupportLine.Price = ask;

                _crosshair.UpdateXLabelPosition(_yAxis.VisibleDataBounds.Min);
                _yLabelAxis.SyncWithMain();
            };
        }


        private static void ChangeChartTypeSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is CartesianScrollBarChart chart)
            {
                chart.UpdateDrawableSeries();
                chart.InitStartPosition();
            }
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