using System.Runtime.Serialization;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    [DataContract]
    public class ReductionKey
    {
        [DataMember]
        public string PackageName { get; set; }

        [DataMember]
        public RepositoryLocation PackageLocation { get; set; }

        [DataMember]
        public string DescriptorId { get; set; }


        public ReductionKey()
        {
        }

        public ReductionKey(PackageKey packageKey, string descriptorId)
            : this(packageKey.Name, packageKey.Location, descriptorId)
        {
        }

        public ReductionKey(string packageName, RepositoryLocation packageLocation, string descriptorId)
        {
            PackageName = packageName;
            PackageLocation = packageLocation;
            DescriptorId = descriptorId;
        }


        public override string ToString()
        {
            return $"Reduction {DescriptorId} in {PackageName} from {PackageLocation}";
        }

        public override int GetHashCode()
        {
            return $"{PackageName}{PackageLocation}{DescriptorId}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as ReductionKey;
            return key != null
                && key.DescriptorId == DescriptorId
                && key.PackageName == PackageName
                && key.PackageLocation == PackageLocation;
        }
    }
}
