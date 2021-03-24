using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public interface IParameterConfig : IPropertyConfig
    {
        object ValObj { get; }
    }

    public interface IParameterConfig<T> : IParameterConfig
    {
        T Value { get; set; }
    }

    public static class ParameterConfig
    {
        public static bool TryUnpack(Any payload, out IPropertyConfig parameter)
        {
            parameter = null;

            if (payload.Is(BoolParameterConfig.Descriptor))
                parameter = payload.Unpack<BoolParameterConfig>();
            else if (payload.Is(Int32ParameterConfig.Descriptor))
                parameter = payload.Unpack<Int32ParameterConfig>();
            else if (payload.Is(NullableInt32ParameterConfig.Descriptor))
                parameter = payload.Unpack<NullableInt32ParameterConfig>();
            else if (payload.Is(DoubleParameterConfig.Descriptor))
                parameter = payload.Unpack<DoubleParameterConfig>();
            else if (payload.Is(NullableDoubleParameterConfig.Descriptor))
                parameter = payload.Unpack<NullableDoubleParameterConfig>();
            else if (payload.Is(StringParameterConfig.Descriptor))
                parameter = payload.Unpack<StringParameterConfig>();
            else if (payload.Is(EnumParameterConfig.Descriptor))
                parameter = payload.Unpack<EnumParameterConfig>();
            else if (payload.Is(FileParameterConfig.Descriptor))
                parameter = payload.Unpack<FileParameterConfig>();

            return parameter != null;
        }
    }


    public partial class BoolParameterConfig : IParameterConfig<bool>
    {
        public object ValObj => Value;
    }

    public partial class Int32ParameterConfig : IParameterConfig<int>
    {
        public object ValObj => Value;
    }

    public partial class NullableInt32ParameterConfig : IParameterConfig<int?>
    {
        public object ValObj => Value;
    }

    public partial class DoubleParameterConfig : IParameterConfig<double>
    {
        public object ValObj => Value;
    }

    public partial class NullableDoubleParameterConfig : IParameterConfig<double?>
    {
        public object ValObj => Value;
    }

    public partial class StringParameterConfig : IParameterConfig<string>
    {
        public object ValObj => Value;
    }

    public partial class EnumParameterConfig : IParameterConfig
    {
        public object ValObj => Value;
    }

    public partial class FileParameterConfig : IParameterConfig
    {
        public object ValObj => FileName;
    }
}
