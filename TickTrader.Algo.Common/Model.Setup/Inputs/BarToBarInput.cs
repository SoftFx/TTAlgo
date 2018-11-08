using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarToBarInputSetup : BarInputSetupBase
    {
        private Mapping _selectedMapping;
        private BarPriceType _defPriceType;
        private List<Mapping> _mappings = new List<Mapping>();


        public Mapping SelectedMapping
        {
            get { return _selectedMapping; }
            set
            {
                _selectedMapping = value;
                NotifyPropertyChanged(nameof(Mapping));
            }
        }

        public IEnumerable<Mapping> AvailableMappings => _mappings;


        public BarToBarInputSetup(InputDescriptor descriptor, ISymbolInfo defaultSymbol, BarPriceType defPriceType, IAlgoGuiMetadata metadata = null)
            : base(descriptor, defaultSymbol, metadata?.Symbols)
        {
            SetMetadata(descriptor);
            _defPriceType = defPriceType;

            _mappings.Add(new DirectMapping("Bid", BarPriceType.Bid));
            _mappings.Add(new DirectMapping("Ask", BarPriceType.Ask));

            if (metadata != null)
            {
                foreach (var d in metadata.Extentions.FullBarToBarReductions)
                {
                    try
                    {
                        _mappings.Add(new ReductionMapping(d.DisplayName, d));
                    }
                    catch (Exception)
                    {
                        // TO DO : logging
                    }
                }
            }
        }

        public override void Apply(IPluginSetupTarget target)
        {
            _selectedMapping?.MapInput(target.GetFeedStrategy<BarStrategy>(), Descriptor.Id, SelectedSymbol.Name);
        }

        public override void Reset()
        {
            base.Reset();
            SetDefaultMapping();
        }

        public override void Load(Property srcProperty)
        {
            var input = srcProperty as BarToBarInput;
            if (input != null)
            {
                _selectedMapping = AvailableMappings.FirstOrDefault(m => m.Name == input.SelectedMapping);
                if (_selectedMapping == null)
                {
                    SetDefaultMapping();
                }
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new BarToBarInput { SelectedMapping = SelectedMapping.Name };
            SaveConfig(input);
            return input;
        }


        private void SetDefaultMapping()
        {
            SelectedMapping = _mappings.FirstOrDefault(m => m.Name == _defPriceType.ToString());
        }


        public abstract class Mapping
        {
            public string Name { get; private set; }


            public Mapping(string name)
            {
                Name = name;
            }


            internal abstract void MapInput(BarStrategy strategy, string inputName, string symbol);
        }


        private class DirectMapping : Mapping
        {
            private BarPriceType _barSide;


            public DirectMapping(string name, BarPriceType barSide) : base(name)
            {
                _barSide = barSide;
            }


            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput<Bar>(inputName, symbol, _barSide, b => b);
            }
        }


        private class ReductionMapping : Mapping
        {
            private FullBarToBarReduction _reduction;


            public ReductionMapping(string name, ReductionDescriptor descriptor)
                : base(name)
            {
                _reduction = descriptor.CreateInstance<FullBarToBarReduction>();
            }


            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput<Bar>(inputName, symbol, (b, a) =>
                {
                    BarEntity entity = new BarEntity();
                    _reduction.Reduce(b, a, entity);
                    return entity;
                });
            }
        }
    }
}
