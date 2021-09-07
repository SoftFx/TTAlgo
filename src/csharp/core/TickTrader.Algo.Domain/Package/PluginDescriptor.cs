namespace TickTrader.Algo.Domain
{
    public partial class PluginDescriptor
    {
        public bool IsValid => Error == Metadata.Types.MetadataErrorCode.NoMetadataError;

        public bool IsIndicator => Type == Metadata.Types.PluginType.Indicator;

        public bool IsTradeBot => Type == Metadata.Types.PluginType.TradeBot;
    }
}
