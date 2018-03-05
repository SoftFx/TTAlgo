using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class SymbolMappingsCollection
    {
        private ExtCollection _extCollection;
        private List<SymbolMapping> _barToBarMappings;
        private List<SymbolMapping> _barToDoubleMappings;
        private List<SymbolMapping> _quoteToBarMappings;
        private List<SymbolMapping> _quoteToDoubleMappings;


        public IReadOnlyList<SymbolMapping> BarToBarMappings => _barToBarMappings;

        public IReadOnlyList<SymbolMapping> BarToDoubleMappings => _barToDoubleMappings;

        public IReadOnlyList<SymbolMapping> QuoteToBarMappings => _quoteToBarMappings;

        public IReadOnlyList<SymbolMapping> QuoteToDoubleMappings => _quoteToDoubleMappings;


        public SymbolMappingsCollection(ExtCollection extCollection)
        {
            _extCollection = extCollection;

            _barToBarMappings = new List<SymbolMapping>();
            _barToDoubleMappings = new List<SymbolMapping>();
            _quoteToBarMappings = new List<SymbolMapping>();
            _quoteToDoubleMappings = new List<SymbolMapping>();

            InitMappings();
        }


        public SymbolMapping GetBarToBarMappingOrDefault(string mappingKey)
        {
            return BarToBarMappings.FirstOrDefault(m => m.Name == mappingKey) ?? new BidBarMapping();
        }

        public SymbolMapping GetBarToDoubleMappingOrDefault(string mappingKey)
        {
            return BarToDoubleMappings.FirstOrDefault(m => m.Name == mappingKey) ?? new BidBarToDoubleMapping();
        }

        public SymbolMapping GetQuoteToBarMappingOrDefault(string mappingKey)
        {
            return QuoteToBarMappings.FirstOrDefault(m => m.Name == mappingKey) ?? new QuoteToBarMapping();
        }

        public SymbolMapping GetQuoteToDoubleMappingOrDefault(string mappingKey)
        {
            return QuoteToDoubleMappings.FirstOrDefault(m => m.Name == mappingKey) ?? new QuoteToDoubleMapping();
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
                    _barToBarMappings.Add(new FullBarToBarMapping(reduction.DisplayName,
                        reduction.CreateInstance<FullBarToBarReduction>()));
                }
            }

            if (_extCollection?.FullBarToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.FullBarToDoubleReductions)
                {
                    _barToDoubleMappings.Add(new FullBarToDoubleMapping(reduction.DisplayName,
                        reduction.CreateInstance<FullBarToDoubleReduction>()));
                }
            }

            if (_extCollection?.QuoteToBarReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToBarReductions)
                {
                    _quoteToBarMappings.Add(new QuoteToBarMapping(reduction.DisplayName,
                        reduction.CreateInstance<QuoteToBarReduction>()));
                }
            }

            if (_extCollection?.QuoteToDoubleReductions != null)
            {
                foreach (var reduction in _extCollection.QuoteToDoubleReductions)
                {
                    _quoteToDoubleMappings.Add(new QuoteToDoubleMapping(reduction.DisplayName,
                        reduction.CreateInstance<QuoteToDoubleReduction>()));
                }
            }

            _barToBarMappings.Add(new BidBarMapping());
            _barToBarMappings.Add(new AskBarMapping());

            if (_extCollection?.BarToDoubleReductions != null)
            {
                foreach (var reductionDouble in _extCollection.BarToDoubleReductions)
                {
                    var instanceDouble = reductionDouble.CreateInstance<BarToDoubleReduction>();
                    _barToDoubleMappings.Add(new BidBarToDoubleMapping(reductionDouble.DisplayName, instanceDouble));
                    _barToDoubleMappings.Add(new AskBarToDoubleMapping(reductionDouble.DisplayName, instanceDouble));

                    if (_extCollection?.FullBarToBarReductions != null)
                    {
                        foreach (var reductionBar in _extCollection.FullBarToBarReductions)
                        {
                            var instanceBar = reductionBar.CreateInstance<FullBarToBarReduction>();
                            _barToDoubleMappings.Add(new FullBarToDoubleMapping(reductionBar.DisplayName,
                                instanceBar, reductionDouble.DisplayName, instanceDouble));
                        }
                    }

                    if (_extCollection?.QuoteToBarReductions != null)
                    {
                        foreach (var reductionBar in _extCollection.QuoteToBarReductions)
                        {
                            var instanceBar = reductionBar.CreateInstance<QuoteToBarReduction>();
                            _quoteToDoubleMappings.Add(new QuoteToDoubleMapping(reductionBar.DisplayName,
                                instanceBar, reductionDouble.DisplayName, instanceDouble));
                        }
                    }
                }
            }
        }
    }
}
