namespace TickTrader.Algo.Domain
{
    public interface IPropertyDescriptor
    {
        string Id { get; set; }

        string DisplayName { get; set; }

        Metadata.Types.PropertyErrorCode ErrorCode { get; set; }

        bool IsValid { get; }
    }

    public partial class PropertyDescriptor : IPropertyDescriptor
    {
        public bool IsValid => ErrorCode == Metadata.Types.PropertyErrorCode.NoPropertyError;
    }

    public partial class ParameterDescriptor : IPropertyDescriptor
    {
        // due to initially bad design decision we require this for backwards compatibility with netfx public api clients
        public const string NullableDoubleTypeName = "System.Nullable`1[[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";
        public const string NullableIntTypeName = "System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";


        public bool IsValid => ErrorCode == Metadata.Types.PropertyErrorCode.NoPropertyError;
    }

    public interface IDataSeriesDescriptor : IPropertyDescriptor 
    {
        string DataSeriesBaseTypeFullName { get; set; }
    }

    public partial class InputDescriptor : IDataSeriesDescriptor
    {
        public bool IsValid => ErrorCode == Metadata.Types.PropertyErrorCode.NoPropertyError;
    }

    public partial class OutputDescriptor : IDataSeriesDescriptor
    {
        public bool IsValid => ErrorCode == Metadata.Types.PropertyErrorCode.NoPropertyError;
    }
}
