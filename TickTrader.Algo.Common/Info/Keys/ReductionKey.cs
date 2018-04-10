namespace TickTrader.Algo.Common.Info
{
    public class ReductionKey
    {
        public string PackagePath { get; set; }

        public string DescriptorId { get; set; }


        public ReductionKey() { }

        public ReductionKey(string packagePath, string descriptorId)
        {
            PackagePath = packagePath;
            DescriptorId = descriptorId;
        }


        public override string ToString()
        {
            return $"reduction {DescriptorId} from {PackagePath}";
        }
    }
}
