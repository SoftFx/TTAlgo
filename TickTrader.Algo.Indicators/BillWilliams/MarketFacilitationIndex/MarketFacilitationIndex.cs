using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.BillWilliams.MarketFacilitationIndex
{
    [Indicator(Category = "Bill Williams", DisplayName = "Market Facilitation Index", Version = "1.0")]
    public class MarketFacilitationIndex : Indicator
    {
        private bool _volumeUp, _mfiUp;

        [Parameter(DisplayName = "Point Size", DefaultValue = 10e-5)]
        public double PointSize { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "MFI Up Volume Up", Target = OutputTargets.Window1, DefaultColor = Colors.Lime, PlotType = PlotType.Histogram)]
        public DataSeries MfiUpVolumeUp { get; set; }

        [Output(DisplayName = "MFI Down Volume Down", Target = OutputTargets.Window1, DefaultColor = Colors.SaddleBrown, PlotType = PlotType.Histogram)]
        public DataSeries MfiDownVolumeDown { get; set; }

        [Output(DisplayName = "MFI Up Volume Down", Target = OutputTargets.Window1, DefaultColor = Colors.Blue, PlotType = PlotType.Histogram)]
        public DataSeries MfiUpVolumeDown { get; set; }

        [Output(DisplayName = "MFI Down Volume Up", Target = OutputTargets.Window1, DefaultColor = Colors.Pink, PlotType = PlotType.Histogram)]
        public DataSeries MfiDownVolumeUp { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public MarketFacilitationIndex() { }

        public MarketFacilitationIndex(BarSeries bars, double pointSize)
        {
            Bars = bars;
            PointSize = pointSize;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _mfiUp = true;
            _volumeUp = true;
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var i = LastPositionChanged;
            MfiUpVolumeUp[i] = double.NaN;
            MfiDownVolumeDown[i] = double.NaN;
            MfiUpVolumeDown[i] = double.NaN;
            MfiDownVolumeUp[i] = double.NaN;
            var val = (Bars[i].High - Bars[i].Low)/(Bars[i].Volume*PointSize);
            if (Bars.Count > 1)
            {
                var prev = (Bars[i + 1].High - Bars[i + 1].Low)/(Bars[i + 1].Volume*PointSize);
                if (val > prev) { _mfiUp = true; }
                if (val < prev) { _mfiUp = false; }
                if (Bars[i].Volume > Bars[i+1].Volume) { _volumeUp = true; }
                if (Bars[i].Volume < Bars[i+1].Volume) { _volumeUp = false; }
            }
            if (_mfiUp && _volumeUp)
            {
                MfiUpVolumeUp[i] = val;
            }
            if (!_mfiUp && !_volumeUp)
            {
                MfiDownVolumeDown[i] = val;
            }
            if (_mfiUp && !_volumeUp)
            {
                MfiUpVolumeDown[i] = val;
            }
            if (!_mfiUp && _volumeUp)
            {
                MfiDownVolumeUp[i] = val;
            }
        }
    }
}
