using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public class MappingCollectionInfo
    {
        public List<MappingInfo> BarToBarMappings { get; set; }

        public List<MappingInfo> BarToDoubleMappings { get; set; }

        public List<MappingInfo> QuoteToBarMappings { get; set; }

        public List<MappingInfo> QuoteToDoubleMappings { get; set; }

        public ReductionKey DefaultFullBarToBarReduction { get; set; }

        public ReductionKey DefaultFullBarToDoubleReduction { get; set; }

        public ReductionKey DefaultBarToDoubleReduction { get; set; }

        public ReductionKey DefaultQuoteToBarReduction { get; set; }

        public ReductionKey DefaultQuoteToDoubleReduction { get; set; }


        public MappingCollectionInfo()
        {
            BarToBarMappings = new List<MappingInfo>();
            BarToDoubleMappings = new List<MappingInfo>();
            QuoteToBarMappings = new List<MappingInfo>();
            QuoteToDoubleMappings = new List<MappingInfo>();
        }

        public MappingCollectionInfo(List<MappingInfo> fullBarToBarMappings, List<MappingInfo> barToDoubleMappings, List<MappingInfo> quoteToBarMappings, List<MappingInfo> quoteToDoubleMappings)
        {
            BarToBarMappings = fullBarToBarMappings;
            BarToDoubleMappings = barToDoubleMappings;
            QuoteToBarMappings = quoteToBarMappings;
            QuoteToDoubleMappings = quoteToDoubleMappings;
        }
    }
}
