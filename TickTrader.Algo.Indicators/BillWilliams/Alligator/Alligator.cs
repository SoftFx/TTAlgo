using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.BillWilliams.Alligator
{
    [Indicator(IsOverlay = true, Category = "Bill Williams", DisplayName = "Bill Williams/Alligator")]
    public class Alligator : Indicator
    {
        private IMA _jaws;
        private Queue<double> _jawsCache;
        private IMA _teeth;
        private Queue<double> _teethCache;
        private IMA _lips;
        private Queue<double> _lipsCache;

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

        private MovingAverage.Method _targetMethod;
        [Parameter(DefaultValue = 2, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int)_targetMethod; }
            set { _targetMethod = (MovingAverage.Method)value; }
        }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 4, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int)_targetPrice; }
            set { _targetPrice = (AppliedPrice.Target)value; }
        }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DefaultColor = Colors.Blue)]
        public DataSeries Jaws { get; set; }

        [Output(DefaultColor = Colors.Red)]
        public DataSeries Teeth { get; set; }

        [Output(DefaultColor = Colors.Green)]
        public DataSeries Lips { get; set; }

        private void InitMAInstance(out IMA instance, out Queue<double> cache, int period, int shift)
        {
            cache = new Queue<double>();
            instance = MovingAverage.CreateMAInstance(period, shift, _targetMethod, _targetPrice);
            instance.Init();
        }

        protected override void Init()
        {
            InitMAInstance(out _jaws, out _jawsCache, JawsPeriod, JawsShift);
            InitMAInstance(out _teeth, out _teethCache, TeethPeriod, TeethShift);
            InitMAInstance(out _lips, out _lipsCache, LipsPeriod, LipsShift);
        }

        protected override void Calculate()
        {
            //--------------------
            if (Bars.Count == 1)
            {
                Init();
            }
            //--------------------
            Utility.ApplyShiftedValue(Jaws, _jaws.Shift, _jaws.Calculate(Bars[0]), _jawsCache, Bars.Count);
            Utility.ApplyShiftedValue(Teeth, _teeth.Shift, _teeth.Calculate(Bars[0]), _teethCache, Bars.Count);
            Utility.ApplyShiftedValue(Lips, _lips.Shift, _lips.Calculate(Bars[0]), _lipsCache, Bars.Count);
        }
    }

    //[Indicator(IsOverlay = true, Category = "Bill Williams", DisplayName = "Bill Williams/Alligator")]
    //public class Alligator : Indicator
    //{
    //    private MovingAverage _jaws;
    //    private MovingAverage _teeth;
    //    private MovingAverage _lips;

    //    private int _jawsPeriod;
    //    [Parameter(DefaultValue = 13, DisplayName = "Jaws Period")]
    //    public int JawsPeriod
    //    {
    //        get { return _jawsPeriod; }
    //        set
    //        {
    //            _jawsPeriod = value;
    //            if (_jaws != null)
    //            {
    //                _jaws.Period = _jawsPeriod;
    //            }
    //        }
    //    }

    //    private int _jawsShift;
    //    [Parameter(DefaultValue = 8, DisplayName = "Jaws Shift")]
    //    public int JawsShift
    //    {
    //        get { return _jawsShift; }
    //        set
    //        {
    //            _jawsShift = value;
    //            if (_jaws != null)
    //            {
    //                _jaws.Shift = _jawsShift;
    //            }
    //        }
    //    }

    //    private int _teethPeriod;
    //    [Parameter(DefaultValue = 8, DisplayName = "Teeth Period")]
    //    public int TeethPeriod
    //    {
    //        get { return _teethPeriod; }
    //        set
    //        {
    //            _teethPeriod = value;
    //            if (_teeth != null)
    //            {
    //                _teeth.Period = _teethPeriod;
    //            }
    //        }
    //    }

    //    private int _teethShift;
    //    [Parameter(DefaultValue = 5, DisplayName = "Teeth Shift")]
    //    public int TeethShift
    //    {
    //        get { return _teethShift; }
    //        set
    //        {
    //            _teethShift = value;
    //            if (_teeth != null)
    //            {
    //                _teeth.Shift = _teethShift;
    //            }
    //        }
    //    }

    //    private int _lipsPeriod;
    //    [Parameter(DefaultValue = 5, DisplayName = "Lips Period")]
    //    public int LipsPeriod
    //    {
    //        get { return _lipsPeriod; }
    //        set
    //        {
    //            _lipsPeriod = value;
    //            if (_lips != null)
    //            {
    //                _lips.Period = _lipsPeriod;
    //            }
    //        }
    //    }

    //    private int _lipsShift;
    //    [Parameter(DefaultValue = 3, DisplayName = "Lips Shift")]
    //    public int LipsShift
    //    {
    //        get { return _lipsShift; }
    //        set
    //        {
    //            _lipsShift = value;
    //            if (_lips != null)
    //            {
    //                _lips.Shift = _lipsShift;
    //            }
    //        }
    //    }

    //    private MovingAverage.Method _targetMethod;
    //    [Parameter(DefaultValue = 0, DisplayName = "Method")]
    //    public int TargetMethod
    //    {
    //        get { return (int)_targetMethod; }
    //        set { _targetMethod = (MovingAverage.Method) value; }
    //    }

    //    private AppliedPrice.Target _targetPrice;

    //    [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
    //    public int TargetPrice
    //    {
    //        get { return (int)_targetPrice; }
    //        set { _targetPrice = (AppliedPrice.Target) value; }
    //    }

    //    [Input]
    //    public DataSeries<Bar> Bars { get; set; }

    //    [Output(DefaultColor = Colors.Blue)]
    //    public DataSeries Jaws { get; set; }

    //    [Output(DefaultColor = Colors.Red)]
    //    public DataSeries Teeth { get; set; }

    //    [Output(DefaultColor = Colors.Green)]
    //    public DataSeries Lips { get; set; }

    //    public Alligator() : base()
    //    {
    //        _jaws = new MovingAverage();
    //        _teeth = new MovingAverage();
    //        _lips = new MovingAverage();
    //    }

    //    protected void SetValue(DataSeries series, MovingAverage movingAverage)
    //    {
    //        if (movingAverage.Shift > 0)
    //        {
    //            series[0] = movingAverage.MA[0];
    //        }
    //        else if (movingAverage.Shift <= 0 && -movingAverage.Shift < Bars.Count)
    //        {
    //            series[-movingAverage.Shift] = movingAverage.MA[-movingAverage.Shift];
    //        }
    //    }

    //    protected override void Calculate()
    //    {
    //        //--------------------
    //        if (Bars.Count == 1)
    //        {
    //            Init();
    //        }
    //        //--------------------
    //        SetValue(Jaws, _jaws);
    //        SetValue(Teeth, _teeth);
    //        SetValue(Lips, _lips);
    //    }
    //}
}
