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
    public class BarToDoubleInputSetup : BarInputSetupBase
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


        public BarToDoubleInputSetup(InputDescriptor descriptor, ISymbolInfo defaultSymbol, BarPriceType defPriceType, IAlgoGuiMetadata metadata = null)
            : base(descriptor, defaultSymbol, metadata?.Symbols)
        {
            SetMetadata(descriptor);
            _defPriceType = defPriceType;

            AddMapping("Open", b => b.Open);
            AddMapping("Close", b => b.Close);
            AddMapping("High", b => b.High);
            AddMapping("Low", b => b.Low);
            AddMapping("Median", b => (b.High + b.Low) / 2);
            AddMapping("Typical", b => (b.High + b.Low + b.Close) / 3);
            AddMapping("Weighted", b => (b.High + b.Low + b.Close * 2) / 4);
            AddMapping("Move", b => b.Close - b.Open);
            AddMapping("Range", b => b.High - b.Low);

            if (metadata != null)
            {
                foreach (var d in metadata.Extentions.BarToDoubleReductions)
                {
                    try
                    {
                        _mappings.Add(new ReductionMapping("Bid." + d.DisplayName, BarPriceType.Bid, d));
                        _mappings.Add(new ReductionMapping("Ask." + d.DisplayName, BarPriceType.Ask, d));
                    }
                    catch (Exception)
                    {
                        // TO DO : logging
                    }
                }
            }

            _mappings.Sort((x, y) => x.Name.CompareTo(y.Name));

            if (metadata != null)
            {
                foreach (var d in metadata.Extentions.FullBarToDoubleReductions)
                {
                    try
                    {
                        _mappings.Add(new FullBarReductionMapping(d.DisplayName, d));
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
            _selectedMapping.MapInput(target.GetFeedStrategy<BarStrategy>(), Descriptor.Id, SelectedSymbol.Name);
        }

        public override void Reset()
        {
            base.Reset();
            SetDefaultMapping();
        }

        public override void Load(Property srcProperty)
        {
            var input = srcProperty as BarToDoubleInput;
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
            var input = new BarToDoubleInput { SelectedMapping = SelectedMapping.Name };
            SaveConfig(input);
            return input;
        }


        private void AddMapping(string name, Func<Bar, double> formula)
        {
            _mappings.Add(new FormulaMapping("Bid." + name, BarPriceType.Bid, formula));
            _mappings.Add(new FormulaMapping("Ask." + name, BarPriceType.Ask, formula));
        }

        private void SetDefaultMapping()
        {
            SelectedMapping = _mappings.FirstOrDefault(m => m.Name == _defPriceType + ".Close");
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


        private class FormulaMapping : Mapping
        {
            private Func<Bar, double> _formula;
            private BarPriceType _priceType;


            public FormulaMapping(string name, BarPriceType priceType, Func<Bar, double> formula) : base(name)
            {
                _formula = formula;
                _priceType = priceType;
            }


            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput(inputName, symbol, _priceType, _formula);
            }
        }


        private class ReductionMapping : Mapping
        {
            private BarToDoubleReduction _reduction;
            private BarPriceType _priceType;


            public ReductionMapping(string name, BarPriceType priceType, ReductionDescriptor descriptor)
                : base(name)
            {
                _reduction = descriptor.CreateInstance<BarToDoubleReduction>();
                _priceType = priceType;
            }


            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput(inputName, symbol, _priceType, _reduction.Reduce);
            }
        }


        private class FullBarReductionMapping : Mapping
        {
            private FullBarToDoubleReduction _reduction;


            public FullBarReductionMapping(string name, ReductionDescriptor descriptor)
                : base(name)
            {
                _reduction = descriptor.CreateInstance<FullBarToDoubleReduction>();
            }


            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput(inputName, symbol, _reduction.Reduce);
            }
        }
    }
}
