using System;

namespace TickTrader.Algo.Domain
{
    public partial class ReductionKey : IComparable<ReductionKey>
    {
        public ReductionKey(PackageKey packageKey, string descriptorId)
        {
            Package = packageKey;
            DescriptorId = descriptorId;
        }

        public ReductionKey(string packageName, RepositoryLocation packageLocation, string descriptorId)
            : this(new PackageKey(packageName, packageLocation), descriptorId)
        {
        }


        public int CompareTo(ReductionKey other)
        {
            var res = Package.CompareTo(other.Package);
            if (res == 0)
                return DescriptorId.CompareTo(DescriptorId);
            return res;
        }
    }
}
