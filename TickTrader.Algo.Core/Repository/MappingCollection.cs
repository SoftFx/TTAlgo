using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    public class MappingCollection
    {
        private ReductionCollection _extCollection;
        private Dictionary<MappingKey, Mapping> _barToBarMappings;
        private Dictionary<MappingKey, Mapping> _barToDoubleMappings;
        private Dictionary<MappingKey, Mapping> _quoteToBarMappings;
        private Dictionary<MappingKey, Mapping> _quoteToDoubleMappings;


        public static ReductionKey BidBarReduction = new ReductionKey("ticktrader.algo.ext.dll", RepositoryLocation.Embedded, "TickTrader.Algo.Ext.BidBarReduction");
        public static ReductionKey AskBarReduction = new ReductionKey("ticktrader.algo.ext.dll", RepositoryLocation.Embedded, "TickTrader.Algo.Ext.AskBarReduction");
        public static ReductionKey DefaultFullBarToBarReduction = BidBarReduction;
        public static ReductionKey DefaultBarToDoubleReduction = new ReductionKey("ticktrader.algo.ext.dll", RepositoryLocation.Embedded, "TickTrader.Algo.Ext.BarCloseReduction");
        public static ReductionKey DefaultFullBarToDoubleReduction = new ReductionKey("ticktrader.algo.ext.dll", RepositoryLocation.Embedded, "TickTrader.Algo.Ext.FullBarToBidCloseReduction");
        public static ReductionKey DefaultQuoteToBarReduction = new ReductionKey("ticktrader.algo.ext.dll", RepositoryLocation.Embedded, "TickTrader.Algo.Ext.QuoteBidBarReduction");
        public static ReductionKey DefaultQuoteToDoubleReduction = new ReductionKey("ticktrader.algo.ext.dll", RepositoryLocation.Embedded, "TickTrader.Algo.Ext.QuoteBestBidReduction");


        public IReadOnlyDictionary<MappingKey, Mapping> BarToBarMappings => _barToBarMappings;

        public IReadOnlyDictionary<MappingKey, Mapping> BarToDoubleMappings => _barToDoubleMappings;

        public IReadOnlyDictionary<MappingKey, Mapping> QuoteToBarMappings => _quoteToBarMappings;

        public IReadOnlyDictionary<MappingKey, Mapping> QuoteToDoubleMappings => _quoteToDoubleMappings;


        public MappingCollection(ReductionCollection extCollection)
        {
            _extCollection = extCollection;

            _barToBarMappings = new Dictionary<MappingKey, Mapping>();
            _barToDoubleMappings = new Dictionary<MappingKey, Mapping>();
            _quoteToBarMappings = new Dictionary<MappingKey, Mapping>();
            _quoteToDoubleMappings = new Dictionary<MappingKey, Mapping>();

            InitMappings();
        }


        public Mapping GetBarToBarMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return new BidBarMapping();

            return BarToBarMappings.ContainsKey(mappingKey) ? BarToBarMappings[mappingKey] : new BidBarMapping();
        }

        public Mapping GetBarToDoubleMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return new BidBarToDoubleMapping();

            return BarToDoubleMappings.ContainsKey(mappingKey) ? BarToDoubleMappings[mappingKey] : new BidBarToDoubleMapping();
        }

        public Mapping GetQuoteToBarMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return new QuoteToBarMapping();

            return QuoteToBarMappings.ContainsKey(mappingKey) ? QuoteToBarMappings[mappingKey] : new QuoteToBarMapping();
        }

        public Mapping GetQuoteToDoubleMappingOrDefault(MappingKey mappingKey)
        {
            if (mappingKey == null)
                return new QuoteToDoubleMapping();

            return QuoteToDoubleMappings.ContainsKey(mappingKey) ? QuoteToDoubleMappings[mappingKey] : new QuoteToDoubleMapping();
        }

        public MappingKey GetBarToBarMappingKeyByNameOrDefault(string displayName)
        {
            return (BarToBarMappings.Values.FirstOrDefault(m => m.DisplayName == displayName) ?? new BidBarMapping()).Key;
        }

        public MappingKey GetBarToDoubleMappingKeyByNameOrDefault(string displayName)
        {
            return (BarToDoubleMappings.Values.FirstOrDefault(m => m.DisplayName == displayName) ?? new BidBarToDoubleMapping()).Key;
        }

        public MappingKey GetQuoteToBarMappingKeyByNameOrDefault(string displayName)
        {
            return (QuoteToBarMappings.Values.FirstOrDefault(m => m.DisplayName == displayName) ?? new QuoteToBarMapping()).Key;
        }

        public MappingKey GetQuoteToDoubleMappingKeyByNameOrDefault(string displayName)
        {
            return (QuoteToDoubleMappings.Values.FirstOrDefault(m => m.DisplayName == displayName) ?? new QuoteToDoubleMapping()).Key;
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
                    _barToBarMappings.Add(new FullBarToBarMapping(reduction.Key, reduction.Value.DisplayName));
                }
            }

            if (_extCollection?.FullBarToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.FullBarToDoubleReductions)
                {
                    _barToDoubleMappings.Add(new FullBarToDoubleMapping(reduction.Key, reduction.Value.DisplayName));
                }
            }

            if (_extCollection?.QuoteToBarReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToBarReductions)
                {
                    _quoteToBarMappings.Add(new QuoteToBarMapping(reduction.Key, reduction.Value.DisplayName));
                }
            }

            if (_extCollection?.QuoteToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToDoubleReductions)
                {
                    _quoteToDoubleMappings.Add(new QuoteToDoubleMapping(reduction.Key, reduction.Value.DisplayName));
                }
            }

            _barToBarMappings.Add(new BidBarMapping());
            _barToBarMappings.Add(new AskBarMapping());

            if (_extCollection?.BarToDoubleReductions != null)
            {
                foreach (var reductionDouble in _extCollection.BarToDoubleReductions)
                {
                    _barToDoubleMappings.Add(new BidBarToDoubleMapping(reductionDouble.Key, reductionDouble.Value.DisplayName));
                    _barToDoubleMappings.Add(new AskBarToDoubleMapping(reductionDouble.Key, reductionDouble.Value.DisplayName));

                    if (_extCollection?.FullBarToBarReductions != null)
                    {
                        foreach (var reductionBar in _extCollection.FullBarToBarReductions)
                        {
                            _barToDoubleMappings.Add(new FullBarToDoubleMapping(reductionBar.Key,
                                reductionBar.Value.DisplayName, reductionDouble.Key, reductionDouble.Value.DisplayName));
                        }
                    }

                    if (_extCollection?.QuoteToBarReductions != null)
                    {
                        foreach (var reductionBar in _extCollection.QuoteToBarReductions)
                        {
                            _quoteToDoubleMappings.Add(new QuoteToDoubleMapping(reductionBar.Key,
                                reductionBar.Value.DisplayName, reductionDouble.Key, reductionDouble.Value.DisplayName));
                        }
                    }
                }
            }
        }
    }


    internal static class MappingDictionaryExtensions
    {
        public static void Add(this Dictionary<MappingKey, Mapping> dict, Mapping mapping)
        {
            dict.Add(mapping.Key, mapping);
        }
    }

}
