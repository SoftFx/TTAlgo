using System;

namespace TickTrader.Algo.Domain
{
    public partial class PluginKey : IComparable<PluginKey>
    {
        public PluginKey(PackageKey packageKey, string descriptorId)
        {
            Package = packageKey;
            DescriptorId = descriptorId;
        }

        public PluginKey(string packageName, RepositoryLocation packageLocation, string descriptorId)
            : this(new PackageKey(packageName, packageLocation), descriptorId)
        {
        }


        public int CompareTo(PluginKey other)
        {
            var res = Package.CompareTo(other.Package);
            if (res == 0)
                return DescriptorId.CompareTo(DescriptorId);
            return res;
        }
    }
}
