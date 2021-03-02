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
