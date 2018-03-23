using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Info
{
    public class ReductionMetadataInfo
    {
        public string ApiVersion { get; set; }

        public string DescriptorId { get; set; }

        public string DisplayName { get; set; }

        public ReductionType Type { get; private set; }
    }
}
