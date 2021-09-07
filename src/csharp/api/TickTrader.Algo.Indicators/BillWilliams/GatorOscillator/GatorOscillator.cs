using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.BillWilliams.GatorOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Gator Oscillator", Version = "1.0")]
    public class GatorOscillator : Indicator, IGatorOscillator
    {
        private IMovingAverage _jaws, _lips, _teethLips, _jawsTeeth;
        private IShift _teethLipsUpShifter, _teethLipsDownShifter, _jawsTeethUpShiter, _jawsTeethDownShiter;
        private bool _lipsUp, _jawsUp;

        [Parameter(DefaultValue = 13, DisplayName = "Jaws Period")]
        public int JawsPeriod { get; set; }

        [Parameter(DefaultValue = 8, DisplayName = "Jaws Shift")]
        public int JawsShift { get; set; }

        [Parameter(DefaultValue = 8, DisplayName = "Teeth Period")]
        public int TeethPeriod { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Teeth Shift")]
        public int TeethShift { get; set; }

        [Parameter(DefaultValue = 5, DisplayName = "Lips Period")]
        public int LipsPeriod { get; set; }

        [Parameter(DefaultValue = 3, DisplayName = "Lips Shift")]
        public int LipsShift { get; set; }

        [Parameter(DefaultValue = MovingAverageMethod.Smoothed, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Gator Teeth-Lips Up", Target = OutputTargets.Window1, DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries TeethLipsUp { get; set; }

        [Output(DisplayName = "Gator Teeth-Lips Down", Target = OutputTargets.Window1, DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries TeethLipsDown { get; set; }

        [Output(DisplayName = "Gator Jaws-Teeth Up", Target = OutputTargets.Window1, DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries JawsTeethUp { get; set; }

        [Output(DisplayName = "Gator Jaws-Teeth Down", Target = OutputTargets.Window1, DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries JawsTeethDown { get; set; }

        public int LastPositionChanged { get { return -1; } }

        public GatorOscillator() { }

        public GatorOscillator(DataSeries price, int jawsPeriod, int jawsShift, int teethPeriod, int teethShift,
            int lipsPeriod, int lipsShift, MovingAverageMethod targetMethod = MovingAverageMethod.Smoothed)
        {
            Price = price;
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
            TargetMethod = targetMethod;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _jaws = Indicators.MovingAverage(Price, JawsPeriod, JawsShift - TeethShift, TargetMethod);
            _jawsTeeth = Indicators.MovingAverage(Price, TeethPeriod, 0, TargetMethod);
            _teethLips = Indicators.MovingAverage(Price, TeethPeriod, TeethShift - LipsShift, TargetMethod);
            _lips = Indicators.MovingAverage(Price, LipsPeriod, 0, TargetMethod);
            var jawsTeethPos = Math.Max(JawsPeriod + JawsShift, TeethPeriod + TeethShift);
            var teethLipsPos = Math.Max(TeethPeriod + TeethShift, LipsPeriod + LipsShift);
            var jawsTeethShift = Math.Max(JawsPeriod + JawsShift - TeethShift, TeethPeriod - JawsShift + TeethShift);
            var teethLipsShift = Math.Max(TeethPeriod + TeethShift - LipsShift, LipsPeriod - TeethShift + LipsShift);
            _jawsTeethUpShiter = new SimpleShifter(jawsTeethPos - jawsTeethShift);
            _jawsTeethUpShiter.Init();
            _jawsTeethDownShiter = new SimpleShifter(jawsTeethPos - jawsTeethShift);
            _jawsTeethDownShiter.Init();
            _teethLipsUpShifter = new SimpleShifter(teethLipsPos - teethLipsShift);
            _teethLipsUpShifter.Init();
            _teethLipsDownShifter = new SimpleShifter(teethLipsPos - teethLipsShift);
            _teethLipsDownShifter.Init();
            _lipsUp = true;
            _jawsUp = true;
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        private void ProcessShifterValue(bool isNewBar, IShift shifter, double val, DataSeries series)
        {
            if (!isNewBar)
            {
                shifter.UpdateLast(val);
            }
            else
            {
                shifter.Add(val);
            }
            series[shifter.Position] = shifter.Result;
        }

        protected override void Calculate(bool isNewBar)
        {
            var lipsPos = Math.Max(_teethLips.LastPositionChanged, _lips.LastPositionChanged);
            var jawsPos = Math.Max(_jawsTeeth.LastPositionChanged, _jaws.LastPositionChanged);
            var lipsVal = -Math.Abs(_teethLips.Average[lipsPos] - _lips.Average[lipsPos]);
            var jawsVal = Math.Abs(_jaws.Average[jawsPos] - _jawsTeeth.Average[jawsPos]);
            if (Price.Count > 1)
            {
                var prevLipsVal = -Math.Abs(_teethLips.Average[lipsPos + 1] - _lips.Average[lipsPos + 1]);
                if (!double.IsNaN(prevLipsVal))
                {
                    if (lipsVal < prevLipsVal)
                    {
                        _lipsUp = true;
                    }
                    if (lipsVal > prevLipsVal)
                    {
                        _lipsUp = false;
                    }
                }
            }
            if (Price.Count > 1)
            {
                var prevJawsVal = Math.Abs(_jaws.Average[jawsPos + 1] - _jawsTeeth.Average[jawsPos + 1]);
                if (!double.IsNaN(prevJawsVal))
                {
                    if (jawsVal > prevJawsVal)
                    {
                        _jawsUp = true;
                    }
                    if (jawsVal < prevJawsVal)
                    {
                        _jawsUp = false;
                    }
                }
            }
            if (_lipsUp)
            {
                ProcessShifterValue(isNewBar, _teethLipsUpShifter, lipsVal, TeethLipsUp);
                ProcessShifterValue(isNewBar, _teethLipsDownShifter, double.NaN, TeethLipsDown);
            }
            if (!_lipsUp)
            {
                ProcessShifterValue(isNewBar, _teethLipsUpShifter, double.NaN, TeethLipsUp);
                ProcessShifterValue(isNewBar, _teethLipsDownShifter, lipsVal, TeethLipsDown);
            }
            if (_jawsUp)
            {
                ProcessShifterValue(isNewBar, _jawsTeethUpShiter, jawsVal, JawsTeethUp);
                ProcessShifterValue(isNewBar, _jawsTeethDownShiter, double.NaN, JawsTeethDown);
            }
            if (!_jawsUp)
            {
                ProcessShifterValue(isNewBar, _jawsTeethUpShiter, double.NaN, JawsTeethUp);
                ProcessShifterValue(isNewBar, _jawsTeethDownShiter, jawsVal, JawsTeethDown);
            }
        }
    }
}
