using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

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

            MaxStr = AddConverter(_max, GetConverter());
            MinStr = AddConverter(_min, GetConverter());
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

        public IValidable<string> MaxStr { get; protected set; }
        public IValidable<string> MinStr { get; protected set; }
        public IValidable<string> StepStr { get; protected set; }

        public override void Apply(Optimizer optimizer)
        {
            //optimizer.SetParameter( _range);
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
            SizeProp.Value = _range.Size;
        }
    }

    internal class DoubleRangeSet : NumericSetModel<double>
    {
        protected override IValidable<double> CreateValidable() => AddDoubleValidable();
        protected override IValueConverter<double, string> GetConverter() => new StringToDouble();
        protected override RangeParamSet<double> CreateRangeModel() => new RangeParamSet.Double();

        protected override void Reset(object defaultValue)
        {
        }

        public override ParamSeekSetModel Clone()
        {
            var clone =  new DoubleRangeSet();
            CopyTo(clone);
            return clone;
        }
    }

    internal class Int32RangeSet : NumericSetModel<int>
    {
        protected override IValidable<int> CreateValidable() => AddIntValidable();
        protected override IValueConverter<int, string> GetConverter() => new StringToInt();
        protected override RangeParamSet<int> CreateRangeModel() => new RangeParamSet.Int32();

        protected override void Reset(object defaultValue)
        {
        }

        public override ParamSeekSetModel Clone()
        {
            var clone = new Int32RangeSet();
            CopyTo(clone);
            return clone;
        }
    }
}
