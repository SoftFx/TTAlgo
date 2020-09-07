using System;

namespace TickTrader.Algo.Domain
{
    public partial class MappingKey : IComparable<MappingKey>
    {
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


        public int CompareTo(MappingKey other)
        {
            var res = PrimaryReduction.CompareTo(other.PrimaryReduction);
            if (res == 0 && SecondaryReduction != null && other.SecondaryReduction != null)
                return SecondaryReduction.CompareTo(other.SecondaryReduction);
            return res;
        }
    }
}
