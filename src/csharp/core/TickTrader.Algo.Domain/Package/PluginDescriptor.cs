using Google.Protobuf;

namespace TickTrader.Algo.Domain
{
    public partial class PluginDescriptor
    {
        public static string JsonUri => Descriptor.FullName + "/Json";

        public static JsonParser JsonParser { get; } = new JsonParser(new JsonParser.Settings(16));

        public static JsonFormatter JsonFormatter { get; } = new JsonFormatter(new JsonFormatter.Settings(false));


        public bool IsValid => Error == Metadata.Types.MetadataErrorCode.NoMetadataError;

        public bool IsIndicator => Type == Metadata.Types.PluginType.Indicator;

        public bool IsTradeBot => Type == Metadata.Types.PluginType.TradeBot;
    }
}
