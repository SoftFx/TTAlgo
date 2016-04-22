using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Trend.ParabolicSAR
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Parabolic SAR")]
    public class ParabolicSar : Indicator
    {
        private double _acceleration, _lastLow, _lastHigh, _price;
        private double _prevAcceleration, _prevLastLow, _prevLastHigh, _prevPrice;
        private bool? _positionLong;

        [Parameter(DefaultValue = 0.02, DisplayName = "Step")]
        public double Step { get; set; }

        [Parameter(DefaultValue = 0.2, DisplayName = "Maximum")]
        public double Maximum { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "SAR", DefaultColor = Colors.Lime, PlotType = PlotType.Points)]
        public DataSeries Sar { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ParabolicSar() { }

        public ParabolicSar(BarSeries bars, double step, double maximum)
        {
            Bars = bars;
            Step = step;
            Maximum = maximum;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _acceleration = double.NaN;
            _lastLow = double.PositiveInfinity;
            _lastHigh = double.NegativeInfinity;
            _price = double.NaN;
            _prevAcceleration = double.NaN;
            _prevLastLow = double.PositiveInfinity;
            _prevLastHigh = double.NegativeInfinity;
            _prevPrice = double.NaN;
            _positionLong = null;
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            if (!IsUpdate)
            {
                _prevAcceleration = _acceleration;
                _prevLastLow = _lastLow;
                _prevLastHigh = _lastHigh;
                _prevPrice = _price;
            }
            if (double.IsNaN(_prevAcceleration))
            {
                _acceleration = Step;
            }
            //else if (_prevAcceleration + Step < Maximum)
            //{
            //    _acceleration += _prevAcceleration + Step;
            //}
            _lastLow = Math.Min(_prevLastLow, Bars.Low[pos]);
            _lastHigh = Math.Max(_prevLastHigh, Bars.High[pos]);
            _price = _prevPrice;
            if (_positionLong == null)
            {
                Sar[pos] = double.NaN;
                if (Bars.Count > 1)
                {
                    if (Bars.High[pos] > Bars.High[pos + 1] && Bars.Low[pos] > Bars.Low[pos + 1])
                    {
                        _positionLong = true;
                        Sar[pos] = Bars.Low[pos + 1];
                        _price = Bars.High[pos];
                    }
                    if (Bars.High[pos] < Bars.High[pos + 1] && Bars.Low[pos] < Bars.Low[pos + 1])
                    {
                        _positionLong = false;
                        Sar[pos] = Bars.High[pos + 1];
                        _price = Bars.Low[pos];
                    }
                }
            }
            else
            {
                if (_positionLong.Value && Bars.Low[pos] < Sar[pos + 1])
                {
                    _acceleration = double.NaN;
                    _positionLong = false;
                    _lastLow = Bars.Low[pos];
                    Sar[pos] = _lastHigh;
                    return;
                }
                if (!_positionLong.Value && Bars.Low[pos] < Sar[pos + 1])
                {
                    _acceleration = double.NaN;
                    _positionLong = true;
                    _lastHigh = Bars.High[pos];
                    Sar[pos] = _lastLow;
                    return;
                }
                var sar = (_price - Sar[pos + 1])*_acceleration + Sar[pos + 1];
                if (_positionLong.Value)
                {
                    if (_price < Bars.High[pos])
                    {
                        if (_acceleration + Step <= Maximum)
                        {
                            _acceleration += Step;
                        }
                    }
                    if (Bars.High[pos] < Bars.High[pos + 1] && Bars.Count == 3)
                    {
                        sar = Sar[pos + 1];
                    }
                    sar = Math.Min(sar, Bars.Low[pos + 1]);
                    sar = Math.Min(sar, Bars.Low[pos + 2]);
                    if (sar > Bars.Low[pos])
                    {
                        _acceleration = double.NaN;
                        _positionLong = false;
                        _lastLow = Bars.Low[pos];
                        Sar[pos] = _lastHigh;
                        return;
                    }
                    if (_price < Bars.High[pos])
                    {
                        _lastHigh = Bars.High[pos];
                        _price = Bars.High[pos];
                    }
                }
                else
                {
                    if (_price > Bars.Low[pos])
                    {
                        if (_acceleration + Step <= Maximum)
                        {
                            _acceleration += Step;
                        }
                    }
                    if (Bars.Low[pos] > Bars.Low[pos + 1] && Bars.Count == 3)
                    {
                        sar = Sar[pos + 1];
                    }
                    sar = Math.Max(sar, Bars.High[pos + 1]);
                    sar = Math.Max(sar, Bars.High[pos + 2]);
                    if (sar < Bars.High[pos])
                    {
                        _acceleration = double.NaN;
                        _positionLong = true;
                        _lastHigh = Bars.High[pos];
                        Sar[pos] = _lastLow;
                        return;
                    }
                    if (_price > Bars.Low[pos])
                    {
                        _lastLow = Bars.Low[pos];
                        _price = Bars.Low[pos];
                    }
                }
                Sar[pos] = sar;
            }
        }
    }
}
