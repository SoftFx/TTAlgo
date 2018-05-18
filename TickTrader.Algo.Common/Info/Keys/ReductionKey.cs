using System.Runtime.Serialization;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    [DataContract(Namespace = "")]
    public class ReductionKey
    {
        private int _hash;


        [DataMember]
        public string PackageName { get; private set; }

        [DataMember]
        public RepositoryLocation PackageLocation { get; private set; }

        [DataMember]
        public string DescriptorId { get; private set; }


        public ReductionKey(PackageKey packageKey, string descriptorId)
            : this(packageKey.Name, packageKey.Location, descriptorId)
        {
        }

        public ReductionKey(string packageName, RepositoryLocation packageLocation, string descriptorId)
        {
            PackageName = packageName;
            PackageLocation = packageLocation;
            DescriptorId = descriptorId;

            _hash = $"{PackageName}{PackageLocation}{DescriptorId}".GetHashCode();
        }


        public override string ToString()
        {
            return $"Reduction {DescriptorId} in {PackageName} from {PackageLocation}";
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public override bool Equals(object obj)
        {
            var key = obj as ReductionKey;
            return key != null
                && key.DescriptorId == DescriptorId
                && key.PackageName == PackageName
                && key.PackageLocation == PackageLocation;
        }

        public static bool operator ==(ReductionKey first, ReductionKey second)
        {
            return ReferenceEquals(first, second) || first != null && first.Equals(second);
        }

        public static bool operator !=(ReductionKey first, ReductionKey second)
        {
            return !ReferenceEquals(first, second) || first != null && !first.Equals(second);
        }
    }
}
