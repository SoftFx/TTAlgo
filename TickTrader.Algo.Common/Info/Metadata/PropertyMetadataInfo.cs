using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Info
{
    public class PropertyMetadataInfo
    {
        public string DescriptorId { get; set; }

        public string DisplayName { get; set; }

        public AlgoPropertyTypes PropertyType { get; set; }

        public AlgoMetadataErrors? Error { get; set; }

        public bool IsValid => Error == null;
    }
}
