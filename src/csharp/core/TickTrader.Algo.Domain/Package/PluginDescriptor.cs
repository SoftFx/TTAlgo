namespace TickTrader.Algo.Domain
{
    public partial class PluginDescriptor
    {
        public bool IsValid => Error == Metadata.Types.MetadataErrorCode.NoMetadataError;
    }
}
