using System.Runtime.Serialization;

namespace TickTrader.Algo.Common.Info
{
    [DataContract]
    public class MappingKey
    {
        [DataMember]
        public ReductionKey BarReduction { get; set; }

        [DataMember]
        public ReductionKey DoubleReduction { get; set; }


        public MappingKey()
        {
        }

        public MappingKey(ReductionKey barReduction)
            : this(barReduction, null)
        {
        }

        /// <summary>
        /// Create double mapping from bar mapping and double reduction
        /// </summary>
        public MappingKey(MappingKey mappingKey, ReductionKey doubleReduction)
            : this(mappingKey.BarReduction, doubleReduction)
        {
        }

        /// <summary>
        /// Create double mapping from bar reduction and double reduction
        /// </summary>
        public MappingKey(ReductionKey barReduction, ReductionKey doubleReduction)
        {
            BarReduction = barReduction;
            DoubleReduction = doubleReduction;
        }


        public override string ToString()
        {
            return $"Mapping Bar {BarReduction}; Double {DoubleReduction}";
        }

        public override int GetHashCode()
        {
            return $"{BarReduction.PackageName}{BarReduction.PackageLocation}{BarReduction.DescriptorId}{DoubleReduction?.PackageName}{DoubleReduction?.PackageLocation}{DoubleReduction?.DescriptorId}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as MappingKey;
            return key != null
                && key.BarReduction != null && key.BarReduction.Equals(BarReduction)
                && (key.DoubleReduction == null || key.DoubleReduction.Equals(DoubleReduction));
        }
    }
}
