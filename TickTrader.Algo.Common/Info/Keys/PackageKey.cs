using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Info
{
    public class PackageKey
    {
        private int _hash;


        public string Name { get; }

        public RepositoryLocation Location { get; }


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
    }
}
