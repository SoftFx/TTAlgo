namespace TickTrader.Algo.Common.Info
{
    public class ReductionInfo
    {
        public ReductionKey Key { get; set; }

        public ReductionMetadataInfo Metadata { get; set; }


        public ReductionInfo() { }

        public ReductionInfo(ReductionKey key, ReductionMetadataInfo metadata)
        {
            Key = key;
            Metadata = metadata;
        }
    }
}
