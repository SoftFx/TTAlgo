using System;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    public class PackageKey : IComparable<PackageKey>
    {
        public string Name { get; set; }

        public RepositoryLocation Location { get; set; }


        public PackageKey()
        {
        }

        public PackageKey(string name, RepositoryLocation location)
        {
            Name = name;
            Location = location;
        }


        public override string ToString()
        {
            return $"Package {Name} from {Location}";
        }

        public override int GetHashCode()
        {
            return $"{Name}{Location}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as PackageKey;
            return key != null
                && key.Name == Name
                && key.Location == Location;
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
