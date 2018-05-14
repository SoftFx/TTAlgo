using System.Runtime.Serialization;

namespace TickTrader.Algo.Common.Info
{
    [DataContract(Namespace = "")]
    public class MappingKey
    {
        private int _hash;


        [DataMember]
        public ReductionKey BarReduction { get; }

        [DataMember]
        public ReductionKey DoubleReduction { get; }


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

            _hash = $"{BarReduction.PackageName}{BarReduction.PackageLocation}{BarReduction.DescriptorId}{DoubleReduction?.PackageName}{DoubleReduction?.PackageLocation}{DoubleReduction?.DescriptorId}".GetHashCode();
        }


        public override string ToString()
        {
            return $"Mapping Bar {BarReduction}; Double {DoubleReduction}";
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public override bool Equals(object obj)
        {
            var key = obj as MappingKey;
            return key != null
                && key.BarReduction != BarReduction
                && key.DoubleReduction != DoubleReduction;
        }
    }
}
