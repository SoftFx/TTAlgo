using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Info
{
    public class PluginKey
    {
        private int _hash;


        public string PackageName { get; }

        public RepositoryLocation PackageLocation { get; }

        public string DescriptorId { get; }


        public PluginKey(PackageKey packageKey, string descriptorId)
            : this(packageKey.Name, packageKey.Location, descriptorId)
        {
        }

        public PluginKey(string packageName, RepositoryLocation packageLocation, string descriptorId)
        {
            PackageName = packageName;
            PackageLocation = packageLocation;
            DescriptorId = descriptorId;

            _hash = $"{PackageName}{PackageLocation}{DescriptorId}".GetHashCode();
        }


        public override string ToString()
        {
            return $"Plugin {DescriptorId} in {PackageName} from {PackageLocation}";
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public override bool Equals(object obj)
        {
            var key = obj as PluginKey;
            return key != null
                && key.DescriptorId == DescriptorId
                && key.PackageName == PackageName
                && key.PackageLocation == PackageLocation;
        }

        public bool IsFromPackage(PackageKey key)
        {
            return key.Name == PackageName && key.Location == PackageLocation;
        }
    }
}
