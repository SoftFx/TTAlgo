using System;

namespace TickTrader.Algo.Domain
{
    public partial class ReductionKey : IComparable<ReductionKey>
    {
        public ReductionKey(string packageId, string descriptorId)
        {
            PackageId = packageId;
            DescriptorId = descriptorId;
        }


        public int CompareTo(ReductionKey other)
        {
            var res = PackageId.CompareTo(other.PackageId);
            if (res == 0)
                return DescriptorId.CompareTo(DescriptorId);
            return res;
        }
    }
}
