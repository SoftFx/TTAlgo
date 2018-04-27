using System.Runtime.Serialization;

namespace TickTrader.Algo.Common.Info
{
    [DataContract(Namespace = "")]
    public class MappingKey
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }


        public MappingKey() { }

        public MappingKey(string id, string location)
        {
            Id = id;
            Location = location;
        }

        public MappingKey(ReductionKey reductionKey)
        {
            Id = reductionKey.DescriptorId;
            Location = reductionKey.PackagePath;
        }

        /// <summary>
        /// Create double mapping from bar reduction and double reduction
        /// </summary>
        public MappingKey(ReductionKey reductionBarKey, ReductionKey reductionDoubleKey)
        {
            Id = $"{reductionBarKey.DescriptorId}:{reductionDoubleKey.DescriptorId}";
            Location = $"{reductionBarKey.PackagePath}:{reductionDoubleKey.PackagePath}";
        }

        /// <summary>
        /// Create double mapping from bar mapping and double reduction
        /// </summary>
        public MappingKey(MappingKey mappingKey, ReductionKey reductionDoubleKey)
        {
            Id = $"{mappingKey.Id}:{reductionDoubleKey.DescriptorId}";
            Location = $"{mappingKey.Location}:{reductionDoubleKey.PackagePath}";
        }


        public override string ToString()
        {
            return $"mapping {Id} at {Location}";
        }

        public override bool Equals(object obj)
        {
            var other = obj as MappingKey;
            if (other != null)
            {
                return other.Id == Id && other.Location == Location;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return $"{Id}{Location}".GetHashCode();
        }
    }
}
