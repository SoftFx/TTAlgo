using System;

namespace TickTrader.Algo.Core.Metadata
{
    public enum ReductionType
    {
        Unknown,
        BarToDouble,
        FullBarToDouble,
        FullBarToBar,
        QuoteToDouble,
        QuoteToBar,
    }


    [Serializable]
    public class ReductionDescriptor
    {
        public string ApiVersionStr { get; set; }

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public ReductionType Type { get; set; }
    }
}
