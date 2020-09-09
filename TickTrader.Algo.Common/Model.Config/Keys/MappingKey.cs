using System;
using System.Runtime.Serialization;

namespace TickTrader.Algo.Common.Model.Config
{
    [Serializable]
    [DataContract(Name = "MappingKey", Namespace = "TTAlgo.Config.v2")]
    public class MappingKey : IComparable<MappingKey>
    {
        [DataMember]
        public ReductionKey PrimaryReduction { get; set; }

        [DataMember]
        public ReductionKey SecondaryReduction { get; set; }


        public MappingKey()
        {
        }

        public MappingKey(ReductionKey primaryReduction)
            : this(primaryReduction, null)
        {
        }

        /// <summary>
        /// Create double mapping from bar mapping and double reduction
        /// </summary>
        public MappingKey(MappingKey mappingKey, ReductionKey secondaryReduction)
            : this(mappingKey.PrimaryReduction, secondaryReduction)
        {
        }

        /// <summary>
        /// Create double mapping from bar reduction and double reduction
        /// </summary>
        public MappingKey(ReductionKey primaryReduction, ReductionKey secondaryReduction)
        {
            PrimaryReduction = primaryReduction;
            SecondaryReduction = secondaryReduction;
        }


        public override string ToString()
        {
            return $"Mapping: Primary {PrimaryReduction}; Secondary {SecondaryReduction}";
        }

        public override int GetHashCode()
        {
            return $"{PrimaryReduction.PackageName}{PrimaryReduction.PackageLocation}{PrimaryReduction.DescriptorId}{SecondaryReduction?.PackageName}{SecondaryReduction?.PackageLocation}{SecondaryReduction?.DescriptorId}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as MappingKey;
            return key != null
                && key.PrimaryReduction != null && key.PrimaryReduction.Equals(PrimaryReduction)
                && (key.SecondaryReduction == null || key.SecondaryReduction.Equals(SecondaryReduction));
        }

        public int CompareTo(MappingKey other)
        {
            var res = PrimaryReduction.CompareTo(other.PrimaryReduction);
            if (res == 0 && SecondaryReduction != null && other.SecondaryReduction != null)
                return SecondaryReduction.CompareTo(other.SecondaryReduction);
            return res;
        }

        public MappingKey Clone()
        {
            return new MappingKey
            {
                PrimaryReduction = PrimaryReduction.Clone(),
                SecondaryReduction = SecondaryReduction?.Clone(),
            };
        }

        public PackageKey[] GetPackageKeys()
        {
            return SecondaryReduction == null
                ? (new[] { PrimaryReduction.GetPackageKey() })
                : (new[] { PrimaryReduction.GetPackageKey(), SecondaryReduction.GetPackageKey() });
        }
    }
}
