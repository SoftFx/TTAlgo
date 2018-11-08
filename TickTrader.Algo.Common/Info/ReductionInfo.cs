using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    public class ReductionInfo
    {
        public ReductionKey Key { get; set; }

        public ReductionDescriptor Descriptor { get; set; }


        public ReductionInfo() { }

        public ReductionInfo(ReductionKey key, ReductionDescriptor descriptor)
        {
            Key = key;
            Descriptor = descriptor;
        }
    }
}
