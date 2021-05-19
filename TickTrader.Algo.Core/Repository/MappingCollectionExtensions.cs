using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    public static class MappingDefaults
    {
        public static readonly string DefaultExtPackageId = PackageId.FromPath(SharedConstants.EmbeddedRepositoryId, typeof(Ext.BarCloseReduction).Assembly.Location);
        public static readonly ReductionKey BidBarReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.BidBarReduction).FullName);
        public static readonly ReductionKey AskBarReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.AskBarReduction).FullName);
        public static readonly ReductionKey DefaultFullBarToBarReduction = BidBarReduction;
        public static readonly ReductionKey DefaultBarToDoubleReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.BarCloseReduction).FullName);
        public static readonly ReductionKey DefaultQuoteToBarReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.QuoteBidBarReduction).FullName);
        public static readonly ReductionKey DefaultQuoteToDoubleReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.QuoteBestBidReduction).FullName);

        public static readonly MappingInfo DefaultBarToBarMapping = new MappingInfo("Bid", DefaultFullBarToBarReduction, null);
        public static readonly MappingInfo DefaultBarToDoubleMapping = new MappingInfo("Bid.Close", DefaultFullBarToBarReduction, DefaultBarToDoubleReduction);
        public static readonly MappingInfo DefaultQuoteToBarMapping = new MappingInfo("Bid", DefaultQuoteToBarReduction, null);
        public static readonly MappingInfo DefaultQuoteToDoubleMapping = new MappingInfo("BestBid", DefaultQuoteToDoubleReduction, null);
    }


    public static class MappingCollectionExtensions
    {
        public static MappingInfo GetBarToBarMappingOrDefault(this MappingCollectionInfo mappings, MappingKey mappingKey)
        {
            if (mappingKey == null)
                return MappingDefaults.DefaultBarToBarMapping;

            return mappings.BarToBarMappings.FirstOrDefault(m => m.Key.RecursiveEquals(mappingKey)) ?? MappingDefaults.DefaultBarToBarMapping;
        }

        public static MappingInfo GetBarToDoubleMappingOrDefault(this MappingCollectionInfo mappings, MappingKey mappingKey)
        {
            if (mappingKey == null)
                return MappingDefaults.DefaultBarToDoubleMapping;

            return mappings.BarToDoubleMappings.FirstOrDefault(m => m.Key.RecursiveEquals(mappingKey)) ?? MappingDefaults.DefaultBarToDoubleMapping;
        }

        public static MappingInfo GetQuoteToBarMappingOrDefault(this MappingCollectionInfo mappings, MappingKey mappingKey)
        {
            if (mappingKey == null)
                return MappingDefaults.DefaultQuoteToBarMapping;

            return mappings.QuoteToBarMappings.FirstOrDefault(m => m.Key.RecursiveEquals(mappingKey)) ?? MappingDefaults.DefaultQuoteToBarMapping;
        }

        public static MappingInfo GetQuoteToDoubleMappingOrDefault(this MappingCollectionInfo mappings, MappingKey mappingKey)
        {
            if (mappingKey == null)
                return MappingDefaults.DefaultQuoteToDoubleMapping;

            return mappings.QuoteToDoubleMappings.FirstOrDefault(m => m.Key.RecursiveEquals(mappingKey)) ?? MappingDefaults.DefaultQuoteToDoubleMapping;
        }


        public static MappingCollectionInfo CreateMappings(this ReductionCollection extCollection)
        {
            var res = new MappingCollectionInfo
            {
                DefaultBarToBarMapping = MappingDefaults.DefaultBarToBarMapping,
                DefaultBarToDoubleMapping = MappingDefaults.DefaultBarToDoubleMapping,
                DefaultQuoteToBarMapping = MappingDefaults.DefaultQuoteToBarMapping,
                DefaultQuoteToDoubleMapping = MappingDefaults.DefaultQuoteToDoubleMapping,
            };

            if (extCollection?.FullBarToBarReductions != null)
            {
                foreach (var reduction in extCollection.FullBarToBarReductions)
                {
                    res.BarToBarMappings.Add(new MappingInfo(reduction));
                }
            }

            if (extCollection?.FullBarToDoubleReductions != null)
            {
                foreach (var reduction in extCollection.FullBarToDoubleReductions)
                {
                    res.BarToDoubleMappings.Add(new MappingInfo(reduction));
                }
            }

            if (extCollection?.QuoteToBarReductions != null)
            {
                foreach (var reduction in extCollection.QuoteToBarReductions)
                {
                    res.QuoteToBarMappings.Add(new MappingInfo(reduction));
                }
            }

            if (extCollection?.QuoteToDoubleReductions != null)
            {
                foreach (var reduction in extCollection.QuoteToDoubleReductions)
                {
                    res.QuoteToDoubleMappings.Add(new MappingInfo(reduction));
                }
            }

            if (extCollection?.BarToDoubleReductions != null && extCollection?.FullBarToBarReductions != null)
            {
                foreach (var reductionDouble in extCollection.BarToDoubleReductions)
                {
                    foreach (var reductionBar in extCollection.FullBarToBarReductions)
                    {
                        res.BarToDoubleMappings.Add(new MappingInfo(reductionBar, reductionDouble));
                    }
                }
            }

            return res;
        }
    }
}
