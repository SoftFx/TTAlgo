using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    public class PackageKey
    {
        private int _hash;


        public string Name { get; private set; }

        public RepositoryLocation Location { get; private set; }


        public PackageKey(string name, RepositoryLocation location)
        {
            Name = name;
            Location = location;

            _hash = $"{Name}{Location}".GetHashCode();
        }


        public override string ToString()
        {
            return $"Package {Name} from {Location}";
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public override bool Equals(object obj)
        {
            var key = obj as PackageKey;
            return key != null
                && key.Name == Name
                && key.Location == Location;
        }


        public static bool operator ==(PackageKey first, PackageKey second)
        {
            return ReferenceEquals(first, second) || first != null && first.Equals(second);
        }

        public static bool operator !=(PackageKey first, PackageKey second)
        {
            return !ReferenceEquals(first, second) || first != null && !first.Equals(second);
        }
    }
}
