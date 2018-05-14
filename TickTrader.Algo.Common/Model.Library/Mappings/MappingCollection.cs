using System.Collections.Generic;
using TickTrader.Algo.Common.Info;

namespace TickTrader.Algo.Common.Model.Library
{
    public class MappingCollection
    {
        private ReductionCollection _extCollection;
        private Dictionary<MappingKey, Mapping> _barToBarMappings;
        private Dictionary<MappingKey, Mapping> _barToDoubleMappings;
        private Dictionary<MappingKey, Mapping> _quoteToBarMappings;
        private Dictionary<MappingKey, Mapping> _quoteToDoubleMappings;


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
            return BarToBarMappings.ContainsKey(mappingKey) ? BarToBarMappings[mappingKey] : new BidBarMapping();
        }

        public Mapping GetBarToDoubleMappingOrDefault(MappingKey mappingKey)
        {
            return BarToDoubleMappings.ContainsKey(mappingKey) ? BarToDoubleMappings[mappingKey] : new BidBarToDoubleMapping();
        }

        public Mapping GetQuoteToBarMappingOrDefault(MappingKey mappingKey)
        {
            return QuoteToBarMappings.ContainsKey(mappingKey) ? QuoteToBarMappings[mappingKey] : new QuoteToBarMapping();
        }

        public Mapping GetQuoteToDoubleMappingOrDefault(MappingKey mappingKey)
        {
            return QuoteToDoubleMappings.ContainsKey(mappingKey) ? QuoteToDoubleMappings[mappingKey] : new QuoteToDoubleMapping();
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
                    _barToBarMappings.Add(new FullBarToBarMapping(reduction.Key, reduction.Value));
                }
            }

            if (_extCollection?.FullBarToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.FullBarToDoubleReductions)
                {
                    _barToDoubleMappings.Add(new FullBarToDoubleMapping(reduction.Key, reduction.Value));
                }
            }

            if (_extCollection?.QuoteToBarReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToBarReductions)
                {
                    _quoteToBarMappings.Add(new QuoteToBarMapping(reduction.Key, reduction.Value));
                }
            }

            if (_extCollection?.QuoteToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToDoubleReductions)
                {
                    _quoteToDoubleMappings.Add(new QuoteToDoubleMapping(reduction.Key, reduction.Value));
                }
            }

            _barToBarMappings.Add(new BidBarMapping());
            _barToBarMappings.Add(new AskBarMapping());

            if (_extCollection?.BarToDoubleReductions != null)
            {
                foreach (var reductionDouble in _extCollection.BarToDoubleReductions)
                {
                    _barToDoubleMappings.Add(new BidBarToDoubleMapping(reductionDouble.Key, reductionDouble.Value));
                    _barToDoubleMappings.Add(new AskBarToDoubleMapping(reductionDouble.Key, reductionDouble.Value));

                    if (_extCollection?.FullBarToBarReductions != null)
                    {
                        foreach (var reductionBar in _extCollection.FullBarToBarReductions)
                        {
                            _barToDoubleMappings.Add(new FullBarToDoubleMapping(reductionBar.Key,
                                reductionBar.Value, reductionDouble.Key, reductionDouble.Value));
                        }
                    }

                    if (_extCollection?.QuoteToBarReductions != null)
                    {
                        foreach (var reductionBar in _extCollection.QuoteToBarReductions)
                        {
                            _quoteToDoubleMappings.Add(new QuoteToDoubleMapping(reductionBar.Key,
                                reductionBar.Value, reductionDouble.Key, reductionDouble.Value));
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
