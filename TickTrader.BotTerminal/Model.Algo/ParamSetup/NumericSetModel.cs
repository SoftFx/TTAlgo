using Machinarium.Var;
using TickTrader.Algo.BacktesterApi;

namespace TickTrader.BotTerminal
{
    internal abstract class NumericSetModel<T> : ParamSeekSetModel
    {
        private RangeParamSet<T> _range;
        private IValidable<T> _max;
        private IValidable<T> _min;
        private IValidable<T> _step;

        public NumericSetModel()
        {
            _range = CreateRangeModel();

            _max = CreateValidable();
            _min = CreateValidable();
            _step = CreateValidable();

            var converter = GetConverter();
            MaxStr = AddConverter(_max, converter);
            MinStr = AddConverter(_min, converter);
            StepStr = AddConverter(_step, GetConverter());

            TriggerOnChange(_max, a => OnMinMaxStepChanged());
            TriggerOnChange(_min, a => OnMinMaxStepChanged());
            TriggerOnChange(_step, a => OnMinMaxStepChanged());

            IsValid = MaxStr.IsValid() & MinStr.IsValid() & StepStr.IsValid();
        }

        protected abstract IValidable<T> CreateValidable();
        protected abstract IValueConverter<T, string> GetConverter();
        protected abstract RangeParamSet<T> CreateRangeModel();

        public override BoolVar IsValid { get; }
        public override string Description => GetDescription();
        public override int Size => _range.Size;
        public override string EditorType => "NumericRange";

        public IValidable<string> MaxStr { get; protected set; }
        public IValidable<string> MinStr { get; protected set; }
        public IValidable<string> StepStr { get; protected set; }

        protected void SetValues(T min, T max, T step)
        {
            _min.Value = min;
            _max.Value = max;
            _step.Value = step;
        }

        public override ParamSeekSet GetSeekSet()
        {
            return _range;
        }

        private string GetDescription()
        {
            return string.Format("[{0} - {1}, {2}]", MinStr.Value, MaxStr.Value, StepStr.Value);
        }

        protected void CopyTo(NumericSetModel<T> clone)
        {
            clone._min.Value = _min.Value;
            clone._max.Value = _max.Value;
            clone._step.Value = _step.Value;
        }

        private void OnMinMaxStepChanged()
        {
            _range.Max = _max.Value;
            _range.Min = _min.Value;
            _range.Step = _step.Value;
        }
    }

    internal class DoubleRangeSet : NumericSetModel<double>
    {
        protected override IValidable<double> CreateValidable() => AddDoubleValidable();
        protected override IValueConverter<double, string> GetConverter() => new StringToDouble();
        protected override RangeParamSet<double> CreateRangeModel() => new RangeParamSet.Double();

        public override ParamSeekSetModel Clone()
        {
            var clone =  new DoubleRangeSet();
            CopyTo(clone);
            return clone;
        }

        protected override void Reset(object defaultValue)
        {
            if (defaultValue is double)
            {
                var defNumVal = (double)defaultValue;
                SetValues(defNumVal, defNumVal, 0.1);
            }
            else
                SetValues(0, 0, 0.1);
        }
    }

    internal class Int32RangeSet : NumericSetModel<int>
    {
        protected override IValidable<int> CreateValidable() => AddIntValidable();
        protected override IValueConverter<int, string> GetConverter() => new StringToInt();
        protected override RangeParamSet<int> CreateRangeModel() => new RangeParamSet.Int32();

        public override ParamSeekSetModel Clone()
        {
            var clone = new Int32RangeSet();
            CopyTo(clone);
            return clone;
        }

        protected override void Reset(object defaultValue)
        {
            if (defaultValue is int)
            {
                var defNumVal = (int)defaultValue;
                SetValues(defNumVal, defNumVal, 1);
            }
            else
                SetValues(0, 0, 1);
        }
    }
}
