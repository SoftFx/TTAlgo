using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.ZigZag
{

    [Indicator(IsOverlay = true, DisplayName = "Zigzag")]
    public class ZigZag : Indicator
    {
        private bool? _peakNext, _prevPeakNext;
        private double _lastLow, _lastHigh, _prevLastLow, _prevLastHigh;
        private double _lastZzLow, _lastZzHigh, _prevLastZzLow, _prevLastZzHigh;
        private int _lastLowPos, _lastHighPos, _prevLastLowPos, _prevLastHighPos;
        private RevertableList<double> _low, _high;
        private RevertableDataSeries<double> _zigzag;
            
        [Parameter(DefaultValue = 12, DisplayName = "Depth")]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Deviation")]
        public int Deviation { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "BackStep")]
        public int Backstep { get; set; }

        [Parameter(DisplayName = "Point Size", DefaultValue = 10e-5)]
        public double PointSize { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "Zigzag", DefaultColor = Colors.Red)]
        public DataSeries Zigzag { get; set; }

        public int LastPositionChanged { get { return Backstep; } }

        public ZigZag() { }

        public ZigZag(BarSeries bars, int depth, int deviation, int backstep, double pointSize)
        {
            Bars = bars;
            Depth = depth;
            Deviation = deviation;
            Backstep = backstep;
            PointSize = pointSize;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _prevPeakNext = null;
            _prevLastLow = double.NaN;
            _prevLastHigh = double.NaN;
            _prevLastZzLow = double.NaN;
            _prevLastZzHigh = double.NaN;
            _prevLastLowPos = 0;
            _prevLastHighPos = 0;
            _low = new RevertableList<double>();
            _high = new RevertableList<double>();
            _zigzag = new RevertableDataSeries<double>(Zigzag);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = 0;
            Zigzag[pos] = double.NaN;
            var n = Bars.Count - 1;
            if (!IsUpdate)
            {
                _prevPeakNext = _peakNext;
                _prevLastLow = _lastLow;
                _prevLastHigh = _lastHigh;
                _prevLastZzLow = _lastZzLow;
                _prevLastZzHigh = _lastZzHigh;
                _prevLastLowPos = _lastLowPos;
                _prevLastHighPos = _lastHighPos;
                _low.SaveCacheChanges();
                _high.SaveCacheChanges();
                _zigzag.ClearCache();
            }
            _peakNext = _prevPeakNext;
            _lastLow = _prevLastLow;
            _lastHigh = _prevLastHigh;
            _lastZzLow = _prevLastZzLow;
            _lastZzHigh = _prevLastZzHigh;
            _lastLowPos = _prevLastLowPos;
            _lastHighPos = _prevLastHighPos;
            _low.ClearCache();
            _high.ClearCache();
            _low.Add(double.NaN);
            _high.Add(double.NaN);
            _zigzag.RevertChanges();
            if (Bars.Count > Math.Max(Depth, Backstep))
            {
                var extremum = PeriodHelper.FindMin(Bars.Low, Depth);
                if (Math.Abs(extremum - _lastLow) < 1e-20)
                {
                    extremum = double.NaN;
                }
                else
                {
                    _lastLow = extremum;
                    if (Bars.Low[pos] - extremum > Deviation*PointSize)
                    {
                        extremum = double.NaN;
                    }
                    else
                    {
                        for (var i = 1; i <= Backstep; i++)
                        {
                            if (!double.IsNaN(_low[n - i]) && _low[n - i] > extremum)
                            {
                                _low[n - i] = double.NaN;
                            }
                        }
                    }
                }
                if (!double.IsNaN(extremum) && Math.Abs(Bars.Low[pos] - extremum) < 1e-20)
                {
                    _low[n] = extremum;
                }
                else
                {
                    _low[n] = double.NaN;
                }

                extremum = PeriodHelper.FindMax(Bars.High, Depth);
                if (Math.Abs(extremum - _lastHigh) < 1e-20)
                {
                    extremum = double.NaN;
                }
                else
                {
                    _lastHigh = extremum;
                    if (extremum - Bars.High[pos] > Deviation*PointSize)
                    {
                        extremum = double.NaN;
                    }
                    else
                    {
                        for (var i = 1; i <= Backstep; i++)
                        {
                            if (!double.IsNaN(_high[n - i]) && _high[n - i] < extremum)
                            {
                                _high[n - i] = double.NaN;
                            }
                        }
                    }
                }
                if (!double.IsNaN(extremum) && Math.Abs(Bars.High[pos] - extremum) < 1e-20)
                {
                    _high[n] = extremum;
                }
                else
                {
                    _high[n] = double.NaN;
                }

                if (_peakNext == null)
                {
                    if (!double.IsNaN(_high[n - Backstep]))
                    {
                        _lastHighPos = n;
                        _peakNext = false;
                        _lastZzHigh = Bars.High[Backstep];
                        _zigzag[Backstep] = _lastZzHigh;
                    }
                    if (!double.IsNaN(_low[n - Backstep]))
                    {
                        _lastLowPos = n;
                        _peakNext = true;
                        _lastZzLow = Bars.Low[Backstep];
                        _zigzag[Backstep] = _lastZzLow;
                    }
                }
                else if (_peakNext.Value)
                {
                    if (!double.IsNaN(_low[n - Backstep]) && _low[n - Backstep] < _lastZzLow &&
                        double.IsNaN(_high[n - Backstep]))
                    {
                        _zigzag[n - _lastLowPos + Backstep] = double.NaN;
                        _lastLowPos = n;
                        _lastZzLow = _low[n - Backstep];
                        _zigzag[Backstep] = _lastZzLow;
                    }
                    if (!double.IsNaN(_high[n - Backstep]) && double.IsNaN(_low[n - Backstep]))
                    {
                        _lastZzHigh = _high[n - Backstep];
                        _lastHighPos = n;
                        _zigzag[Backstep] = _lastZzHigh;
                        _peakNext = false;
                    }
                }
                else
                {
                    if (!double.IsNaN(_high[n - Backstep]) && _high[n - Backstep] > _lastZzHigh &&
                        double.IsNaN(_low[n - Backstep]))
                    {
                        _zigzag[n - _lastHighPos + Backstep] = double.NaN;
                        _lastHighPos = n;
                        _lastZzHigh = _high[n - Backstep];
                        _zigzag[Backstep] = _lastZzHigh;
                    }
                    if (!double.IsNaN(_low[n - Backstep]) && double.IsNaN(_high[n - Backstep]))
                    {
                        _lastZzLow = _low[n - Backstep];
                        _lastLowPos = n;
                        _zigzag[Backstep] = _lastZzLow;
                        _peakNext = true;
                    }
                }
            }
        }
    }
}
