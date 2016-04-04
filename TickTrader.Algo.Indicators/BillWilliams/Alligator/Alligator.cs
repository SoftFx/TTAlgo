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
            switch (_targetMethod)
            {
                case MovingAverage.Method.Simple:
                    instance = new SMA(period, shift, _targetPrice);
                    break;
                case MovingAverage.Method.Exponential:
                    instance = new EMA(period, shift, _targetPrice);
                    break;
                case MovingAverage.Method.Smoothed:
                    instance = new SMMA(period, shift, _targetPrice);
                    break;
                case MovingAverage.Method.LinearWeighted:
                    instance = new LWMA(period, shift, _targetPrice);
                    break;
                case MovingAverage.Method.CustomExponential:
                    instance = new CustomEMA(period, shift, _targetPrice, 2.0/(period + 1));
                    break;
                case MovingAverage.Method.Triangular:
                    instance = new TriMA(period, shift, _targetPrice);
                    break;
                default:
                    instance = null;
                    return;
            }
            instance.Init();
        }

        protected override void Init()
        {
            InitMAInstance(out _jaws, out _jawsCache, JawsPeriod, JawsShift);
            InitMAInstance(out _teeth, out _teethCache, TeethPeriod, TeethShift);
            InitMAInstance(out _lips, out _lipsCache, LipsPeriod, LipsShift);
        }

        private void SetValue(DataSeries series, IMA instance, Queue<double> cache)
        {
            var res = instance.Calculate(Bars[0]);
            if (instance.Shift > 0)
            {
                if (Bars.Count > instance.Shift)
                {
                    series[0] = cache.Dequeue();
                }
                else
                {
                    series[0] = double.NaN;
                }
                cache.Enqueue(res);
            }
            else if (instance.Shift <= 0 && -instance.Shift < Bars.Count)
            {
                series[-instance.Shift] = res;
            }
        }

        protected override void Calculate()
        {
            //--------------------
            if (Bars.Count == 1)
            {
                Init();
            }
            //--------------------
            SetValue(Jaws, _jaws, _jawsCache);
            SetValue(Teeth, _teeth, _teethCache);
            SetValue(Lips, _lips, _lipsCache);
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
