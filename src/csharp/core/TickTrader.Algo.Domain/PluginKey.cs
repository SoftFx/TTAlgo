using System;

namespace TickTrader.Algo.Domain
{
    public partial class PluginKey : IComparable<PluginKey>
    {
        public PluginKey(string packageId, string descriptorId)
        {
            PackageId = packageId;
            DescriptorId = descriptorId;
        }


        public int CompareTo(PluginKey other)
        {
            var res = PackageId.CompareTo(other.PackageId);
            if (res == 0)
                return DescriptorId.CompareTo(DescriptorId);
            return res;
        }
    }
}
