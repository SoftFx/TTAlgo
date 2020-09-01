using System;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class ParameterSetupModel<T> : ParameterSetupModel
    {
        private T _value;
        private string _valueStr;

        public T DefaultValue { get; }

        public override string ValueAsText => ValueStr;

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Error = null;

                if (Converter != null)
                {
                    _valueStr = Converter.ToString(value);
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


        public ParameterSetupModel(ParameterMetadata descriptor)
            : base(descriptor)
        {
            DefaultValue = default(T);

            if (descriptor.DefaultValue != null)
            {
                if (descriptor.DefaultValue is T)
                    DefaultValue = (T)descriptor.DefaultValue;
                else
                {
                    if (Converter.FromObject(descriptor.DefaultValue, out var convertedFromObj))
                        DefaultValue = convertedFromObj;
                }
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

        public override object GetValueToApply()
        {
            return Value;
        }

        public override void Load(Property srcProperty)
        {
            var typedSrcProperty = srcProperty as Parameter<T>;
            if (typedSrcProperty != null)
                Value = typedSrcProperty.Value;
        }


        protected Property SaveTyped<TCfg>()
            where TCfg : Parameter<T>, new()
        {
            return new TCfg() { Id = Id, Value = Value };
        }


        private UiConverter<T> GetConverterOrThrow()
        {
            if (Converter == null)
                throw new InvalidOperationException("This type of propety does not support string conversions.");
            return Converter;
        }
    }


    public class BoolParamSetupModel : ParameterSetupModel<bool>
    {
        internal override UiConverter<bool> Converter => UiConverter.Bool;

        public BoolParamSetupModel(ParameterMetadata descriptor)
            : base(descriptor)
        {
        }

        public override Property Save()
        {
            return SaveTyped<BoolParameter>();
        }
    }

    public class IntParamSetupModel : ParameterSetupModel<int>
    {
        internal override UiConverter<int> Converter => UiConverter.Int;

        public IntParamSetupModel(ParameterMetadata descriptor)
            : base(descriptor)
        {
        }

        public override Property Save()
        {
            return SaveTyped<IntParameter>();
        }
    }


    public class NullableIntParamSetupModel : ParameterSetupModel<int?>
    {
        internal override UiConverter<int?> Converter => UiConverter.NullableInt;

        public NullableIntParamSetupModel(ParameterMetadata descriptor) : base(descriptor)
        {
        }

        public override Property Save()
        {
            return SaveTyped<NullableIntParameter>();
        }
    }


    public class DoubleParamSetupModel : ParameterSetupModel<double>
    {
        internal override UiConverter<double> Converter => UiConverter.Double;

        public DoubleParamSetupModel(ParameterMetadata descriptor) : base(descriptor)
        {
        }

        public override Property Save()
        {
            return SaveTyped<DoubleParameter>();
        }
    }


    public class NullableDoubleParamSetupModel : ParameterSetupModel<double?>
    {
        internal override UiConverter<double?> Converter => UiConverter.NullableDouble;

        public NullableDoubleParamSetupModel(ParameterMetadata descriptor) : base(descriptor)
        {
        }

        public override Property Save()
        {
            return SaveTyped<NullableDoubleParameter>();
        }
    }


    public class StringParamSetupModel : ParameterSetupModel<string>
    {
        internal override UiConverter<string> Converter => UiConverter.String;

        public StringParamSetupModel(ParameterMetadata descriptor) : base(descriptor)
        {
        }

        public override Property Save()
        {
            return SaveTyped<StringParameter>();
        }
    }
}
