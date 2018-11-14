using System;
using System.Runtime.Serialization;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    [DataContract(Name = "ReductionKey", Namespace = "TTAlgo.Config.v2")]
    public class ReductionKey : IComparable<ReductionKey>
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

        public int CompareTo(ReductionKey other)
        {
            var res1 = PackageName.CompareTo(other.PackageName);
            if (res1 == 0)
            {
                var res2 = PackageLocation.CompareTo(other.PackageLocation);
                if (res2 == 0)
                    return DescriptorId.CompareTo(DescriptorId);
            }
            return res1;
        }

        public ReductionKey Clone()
        {
            return new ReductionKey
            {
                PackageName = PackageName,
                PackageLocation = PackageLocation,
                DescriptorId = DescriptorId,
            };
        }
    }
}
