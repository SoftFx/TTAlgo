using System;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public abstract class ParameterSetupViewModel<T> : ParameterSetupViewModel
    {
        private T _value;
        private string _valueStr;


        public T DefaultValue { get; }

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyOfPropertyChange(nameof(Value));
                Error = null;

                if (Converter != null)
                {
                    _valueStr = Converter.ToString(value);
                    NotifyOfPropertyChange(nameof(ValueStr));
                }
            }
        }

        public string ValueStr
        {
            get { return _valueStr; }
            set
            {
                if (_valueStr == value)
                    return;

                Value = GetConverterOrThrow().Parse(value, out var error);
                _valueStr = value;
                Error = error;
            }
        }


        internal virtual UiConverter<T> Converter => null;


        public ParameterSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
            DefaultValue = default(T);

            if (descriptor.DefaultValue != null && Converter != null)
            {
                DefaultValue = Converter.Parse(Descriptor.DefaultValue, out var error);
            }
        }


        public override string ToString()
        {
            return $"{DisplayName}: {Value}";
        }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            var typedSrcProperty = srcProperty as IParameterConfig<T>;
            if (typedSrcProperty != null)
                Value = typedSrcProperty.Value;
        }


        protected IPropertyConfig SaveTyped<TCfg>()
            where TCfg : IParameterConfig<T>, new()
        {
            return new TCfg() { PropertyId = Id, Value = Value };
        }


        private UiConverter<T> GetConverterOrThrow()
        {
            if (Converter == null)
                throw new InvalidOperationException("This type of propety does not support string conversions.");
            return Converter;
        }
    }


    public class BoolParamSetupViewModel : ParameterSetupViewModel<bool>
    {
        public static string[] _boolValues = { "False", "True" };


        public string[] BoolValues => _boolValues;


        internal override UiConverter<bool> Converter => UiConverter.Bool;


        public BoolParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override IPropertyConfig Save()
        {
            return SaveTyped<BoolParameterConfig>();
        }
    }


    public class IntParamSetupViewModel : ParameterSetupViewModel<int>
    {
        internal override UiConverter<int> Converter => UiConverter.Int;


        public IntParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override IPropertyConfig Save()
        {
            return SaveTyped<Int32ParameterConfig>();
        }
    }


    public class NullableIntParamSetupViewModel : ParameterSetupViewModel<int?>
    {
        internal override UiConverter<int?> Converter => UiConverter.NullableInt;


        public NullableIntParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override IPropertyConfig Save()
        {
            return SaveTyped<NullableInt32ParameterConfig>();
        }
    }


    public class DoubleParamSetupViewModel : ParameterSetupViewModel<double>
    {
        internal override UiConverter<double> Converter => UiConverter.Double;


        public DoubleParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override IPropertyConfig Save()
        {
            return SaveTyped<DoubleParameterConfig>();
        }
    }


    public class NullableDoubleParamSetupViewModel : ParameterSetupViewModel<double?>
    {
        internal override UiConverter<double?> Converter => UiConverter.NullableDouble;


        public NullableDoubleParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override IPropertyConfig Save()
        {
            return SaveTyped<NullableDoubleParameterConfig>();
        }
    }


    public class StringParamSetupViewModel : ParameterSetupViewModel<string>
    {
        internal override UiConverter<string> Converter => UiConverter.String;


        public StringParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
        }


        public override IPropertyConfig Save()
        {
            return SaveTyped<StringParameterConfig>();
        }
    }
}
