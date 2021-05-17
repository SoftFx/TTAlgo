namespace TickTrader.Algo.Domain
{
    public partial class MappingInfo
    {
        public MappingInfo(ReductionInfo primaryReduction)
        {
            Key = new MappingKey(primaryReduction.Key);
            DisplayName = primaryReduction.Descriptor_.DisplayName;
        }

        public MappingInfo(ReductionInfo primaryReduction, ReductionInfo secondaryReduction)
        {
            Key = new MappingKey(primaryReduction.Key, secondaryReduction.Key);
            DisplayName = $"{primaryReduction.Descriptor_.DisplayName}.{secondaryReduction.Descriptor_.DisplayName}";
        }

        public MappingInfo(string displayName, ReductionKey primaryReduction, ReductionKey secondaryReduction)
        {
            DisplayName = displayName;
            Key = new MappingKey(primaryReduction, secondaryReduction);
        }
    }
}
