using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

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


        public ParameterSetupViewModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
            DefaultValue = default(T);

            if (metadata.DefaultValue != null)
            {
                if (metadata.DefaultValue is T)
                    DefaultValue = (T)metadata.DefaultValue;
                else
                {
                    if (Converter.FromObject(metadata.DefaultValue, out var convertedFromObj))
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


    public class BoolParamSetupModel : ParameterSetupViewModel<bool>
    {
        internal override UiConverter<bool> Converter => UiConverter.Bool;


        public BoolParamSetupModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override Property Save()
        {
            return SaveTyped<BoolParameter>();
        }
    }


    public class IntParamSetupModel : ParameterSetupViewModel<int>
    {
        internal override UiConverter<int> Converter => UiConverter.Int;


        public IntParamSetupModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override Property Save()
        {
            return SaveTyped<IntParameter>();
        }
    }


    public class NullableIntParamSetupModel : ParameterSetupViewModel<int?>
    {
        internal override UiConverter<int?> Converter => UiConverter.NullableInt;


        public NullableIntParamSetupModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override Property Save()
        {
            return SaveTyped<NullableIntParameter>();
        }
    }


    public class DoubleParamSetupModel : ParameterSetupViewModel<double>
    {
        internal override UiConverter<double> Converter => UiConverter.Double;


        public DoubleParamSetupModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override Property Save()
        {
            return SaveTyped<DoubleParameter>();
        }
    }


    public class NullableDoubleParamSetupModel : ParameterSetupViewModel<double?>
    {
        internal override UiConverter<double?> Converter => UiConverter.NullableDouble;


        public NullableDoubleParamSetupModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override Property Save()
        {
            return SaveTyped<NullableDoubleParameter>();
        }
    }


    public class StringParamSetupModel : ParameterSetupViewModel<string>
    {
        internal override UiConverter<string> Converter => UiConverter.String;


        public StringParamSetupModel(ParameterMetadataInfo metadata)
            : base(metadata)
        {
        }


        public override Property Save()
        {
            return SaveTyped<StringParameter>();
        }
    }
}
