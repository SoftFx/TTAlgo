using System;

namespace TickTrader.Algo.Domain
{
    public partial class PackageKey : IComparable<PackageKey>
    {
        public PackageKey(string name, RepositoryLocation location)
        {
            Name = name;
            Location = location;
        }

        public int CompareTo(PackageKey other)
        {
            var res = Location.CompareTo(other.Location);
            if (res == 0)
                return Name.CompareTo(other.Name);
            return res;
        }
    }
}
