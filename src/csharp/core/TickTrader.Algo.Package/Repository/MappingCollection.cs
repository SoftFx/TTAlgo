using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package.Repository
{
    public class MappingCollection
    {
        private ReductionCollection _extCollection;
        private List<MappingInfo> _barToBarMappings;
        private List<MappingInfo> _barToDoubleMappings;
        private List<MappingInfo> _quoteToBarMappings;
        private List<MappingInfo> _quoteToDoubleMappings;


        public static string DefaultExtPackageId = PackageId.Pack(SharedConstants.EmbeddedRepositoryId, typeof(Ext.BarCloseReduction).Assembly.Location);
        public static ReductionKey BidBarReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.BidBarReduction).FullName);
        public static ReductionKey AskBarReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.AskBarReduction).FullName);
        public static ReductionKey DefaultFullBarToBarReduction = BidBarReduction;
        public static ReductionKey DefaultBarToDoubleReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.BarCloseReduction).FullName);
        public static ReductionKey DefaultQuoteToBarReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.QuoteBidBarReduction).FullName);
        public static ReductionKey DefaultQuoteToDoubleReduction = new ReductionKey(DefaultExtPackageId, typeof(Ext.QuoteBestBidReduction).FullName);

        public static readonly MappingInfo DefaultBarToBarMapping = new MappingInfo("Bid", DefaultFullBarToBarReduction, null);
        public static readonly MappingInfo DefaultBarToDoubleMapping = new MappingInfo("Bid.Close", DefaultFullBarToBarReduction, DefaultBarToDoubleReduction);
        public static readonly MappingInfo DefaultQuoteToBarMapping = new MappingInfo("Bid", DefaultQuoteToBarReduction, null);
        public static readonly MappingInfo DefaultQuoteToDoubleMapping = new MappingInfo("BestBid", DefaultQuoteToDoubleReduction, null);


        public IReadOnlyList<MappingInfo> BarToBarMappings => _barToBarMappings;

        public IReadOnlyList<MappingInfo> BarToDoubleMappings => _barToDoubleMappings;

        public IReadOnlyList<MappingInfo> QuoteToBarMappings => _quoteToBarMappings;

        public IReadOnlyList<MappingInfo> QuoteToDoubleMappings => _quoteToDoubleMappings;


        public MappingCollection(ReductionCollection extCollection)
        {
            _extCollection = extCollection;

            _barToBarMappings = new List<MappingInfo>();
            _barToDoubleMappings = new List<MappingInfo>();
            _quoteToBarMappings = new List<MappingInfo>();
            _quoteToDoubleMappings = new List<MappingInfo>();

            InitMappings();
        }


        public MappingInfo GetBarToBarMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return DefaultBarToBarMapping;

            return BarToBarMappings.FirstOrDefault(m => m.Key == mappingKey) ?? DefaultBarToBarMapping;
        }

        public MappingInfo GetBarToDoubleMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return DefaultBarToDoubleMapping;

            return BarToDoubleMappings.FirstOrDefault(m => m.Key == mappingKey) ?? DefaultBarToDoubleMapping;
        }

        public MappingInfo GetQuoteToBarMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return DefaultQuoteToBarMapping;

            return QuoteToBarMappings.FirstOrDefault(m => m.Key == mappingKey) ?? DefaultQuoteToBarMapping;
        }

        public MappingInfo GetQuoteToDoubleMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return DefaultQuoteToDoubleMapping;

            return QuoteToDoubleMappings.FirstOrDefault(m => m.Key == mappingKey) ?? DefaultQuoteToDoubleMapping;
        }


        private void InitMappings()
        {
            _barToBarMappings.Clear();
            _barToDoubleMappings.Clear();
            _quoteToBarMappings.Clear();
            _quoteToDoubleMappings.Clear();

            if (_extCollection?.FullBarToBarReductions != null)
            {
                foreach (var reduction in _extCollection.FullBarToBarReductions)
                {
                    _barToBarMappings.Add(new MappingInfo(reduction));
                }
            }

            if (_extCollection?.FullBarToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.FullBarToDoubleReductions)
                {
                    _barToDoubleMappings.Add(new MappingInfo(reduction));
                }
            }

            if (_extCollection?.QuoteToBarReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToBarReductions)
                {
                    _quoteToBarMappings.Add(new MappingInfo(reduction));
                }
            }

            if (_extCollection?.QuoteToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToDoubleReductions)
                {
                    _quoteToDoubleMappings.Add(new MappingInfo(reduction));
                }
            }

            if (_extCollection?.BarToDoubleReductions != null && _extCollection?.FullBarToBarReductions != null)
            {
                foreach (var reductionDouble in _extCollection.BarToDoubleReductions)
                {
                    foreach (var reductionBar in _extCollection.FullBarToBarReductions)
                    {
                        _barToDoubleMappings.Add(new MappingInfo(reductionBar, reductionDouble));
                    }
                }
            }
        }
    }
}
