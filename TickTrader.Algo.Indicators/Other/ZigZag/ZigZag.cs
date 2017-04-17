using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Other.ZigZag
{

    [Indicator(IsOverlay = true, Category = "Other", DisplayName = "ZigZag", Version = "1.0")]
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

        [Parameter(DisplayName = "Point Size", DefaultValue = 1e-5)]
        public double PointSize { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "ZigZag", DefaultColor = Colors.Red)]
        public DataSeries Zigzag { get; set; }

        [Output(DisplayName = "ZigZag Line", DefaultColor = Colors.Red)]
        public DataSeries ZigzagLine { get; set; }

        public int LastPositionChanged
        {
            get { return Backstep; }
        }

        public ZigZag()
        {
        }

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
            RevertZigZagChanges();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = 0;
            Zigzag[pos] = double.NaN;
            ZigzagLine[pos] = double.NaN;
            var n = Bars.Count - 1;
            if (!IsUpdate)
            {
                _prevLastLow = _lastLow;
                _prevLastHigh = _lastHigh;
                _low.ApplyChanges();
                _high.ApplyChanges();
            }
            _lastLow = _prevLastLow;
            _lastHigh = _prevLastHigh;
            _low.RevertChanges();
            _high.RevertChanges();
            _low.Add(double.NaN);
            _high.Add(double.NaN);
            if (Bars.Count >= Math.Max(Depth, Backstep))
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

                DrawZigZagSection(false);

                if (!IsUpdate)
                {
                    RevertZigZagChanges();
                    CalculateZigZag(Backstep + 1);
                    ApplyZigZagChanges();
                    DrawZigZagSection();
                }
                RevertZigZagChanges(false);
                for (var i = Backstep; i >= 0; i--)
                {
                    CalculateZigZag(i);
                }

                DrawZigZagSection();
            }
        }

        private void DrawZigZagSection(bool isVisible = true)
        {
            if (double.IsNaN(_lastZzLow) || double.IsNaN(_lastZzHigh))
                return;
            var n = Bars.Count - 1;
            var start = Math.Min(_lastHighPos, _lastLowPos);
            var end = Math.Max(_lastHighPos, _lastLowPos);
            var step = isVisible ? (Zigzag[n - end] - Zigzag[n - start])/(end - start) : double.NaN;
            ZigzagLine[n - start] = Zigzag[n - start] + 0*step;
            for (var i = start; i < end; i++)
            {
                ZigzagLine[n - i - 1] = ZigzagLine[n - i] + step;
            }
        }

        private void ApplyZigZagChanges()
        {
            _prevPeakNext = _peakNext;
            _prevLastZzLow = _lastZzLow;
            _prevLastZzHigh = _lastZzHigh;
            _prevLastLowPos = _lastLowPos;
            _prevLastHighPos = _lastHighPos;
            _zigzag.ApplyChanges();
        }

        private void RevertZigZagChanges(bool isNextStep = true)
        {
            _peakNext = _prevPeakNext;
            _lastZzLow = _prevLastZzLow;
            _lastZzHigh = _prevLastZzHigh;
            _lastLowPos = _prevLastLowPos;
            _lastHighPos = _prevLastHighPos;
            _zigzag.RevertChanges(isNextStep);
        }

        private void CalculateZigZag(int shift)
        {
            var n = Bars.Count - 1;
            if (_peakNext == null)
            {
                if (!double.IsNaN(_high[n - shift]))
                {
                    _lastHighPos = n - shift;
                    _peakNext = false;
                    _lastZzHigh = Bars.High[shift];
                    _zigzag[shift] = _lastZzHigh;
                }
                if (!double.IsNaN(_low[n - shift]))
                {
                    _lastLowPos = n - shift;
                    _peakNext = true;
                    _lastZzLow = Bars.Low[shift];
                    _zigzag[shift] = _lastZzLow;
                }
            }
            else if (_peakNext.Value)
            {
                if (!double.IsNaN(_low[n - shift]) && _low[n - shift] < _lastZzLow &&
                    double.IsNaN(_high[n - shift]))
                {
                    _zigzag[n - _lastLowPos] = double.NaN;
                    _lastLowPos = n - shift;
                    _lastZzLow = _low[n - shift];
                    _zigzag[shift] = _lastZzLow;
                }
                if (!double.IsNaN(_high[n - shift]) && double.IsNaN(_low[n - shift]))
                {
                    _lastZzHigh = _high[n - shift];
                    _lastHighPos = n - shift;
                    _zigzag[shift] = _lastZzHigh;
                    _peakNext = false;
                }
            }
            else
            {
                if (!double.IsNaN(_high[n - shift]) && _high[n - shift] > _lastZzHigh &&
                    double.IsNaN(_low[n - shift]))
                {
                    _zigzag[n - _lastHighPos] = double.NaN;
                    _lastHighPos = n - shift;
                    _lastZzHigh = _high[n - shift];
                    _zigzag[shift] = _lastZzHigh;
                }
                if (!double.IsNaN(_low[n - shift]) && double.IsNaN(_high[n - shift]))
                {
                    _lastZzLow = _low[n - shift];
                    _lastLowPos = n - shift;
                    _zigzag[shift] = _lastZzLow;
                    _peakNext = true;
                }
            }
        }
    }
}