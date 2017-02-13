using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public class BarToDoubleInput : BarInputBase
    {
        private Mapping selectedMapping;
        private BarPriceType defPriceType;
        private List<Mapping> mappings = new List<Mapping>();

        public BarToDoubleInput(InputDescriptor descriptor, string symbolCode, BarPriceType defPriceType, IAlgoGuiMetadata metadata = null)
            : base(descriptor, symbolCode, metadata?.Symbols)
        {
            SetMetadata(descriptor);
            this.defPriceType = defPriceType;

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
                        mappings.Add(new ReductionMapping("Bid." + d.DisplayName, BarPriceType.Bid, d));
                        mappings.Add(new ReductionMapping("Ask." + d.DisplayName, BarPriceType.Ask, d));
                    }
                    catch (Exception)
                    {
                        // TO DO : logging
                    }
                }
            }

            mappings.Sort((x, y) => x.Name.CompareTo(y.Name));

            if (metadata != null)
            {
                foreach (var d in metadata.Extentions.FullBarToDoubleReductions)
                {
                    try
                    {
                        mappings.Add(new FullBarReductionMapping(d.DisplayName, d));
                    }
                    catch (Exception)
                    {
                        // TO DO : logging
                    }
                }
            }   
        }

        private void AddMapping(string name, Func<Bar, double> formula)
        {
            mappings.Add(new FormulaMapping("Bid." + name, BarPriceType.Bid, formula));
            mappings.Add(new FormulaMapping("Ask." + name, BarPriceType.Ask, formula));
        }

        public Mapping SelectedMapping
        {
            get { return selectedMapping; }
            set
            {
                this.selectedMapping = value;
                NotifyPropertyChanged(nameof(Mapping));
            }
        }

        public IEnumerable<Mapping> AvailableMappings => mappings;

        public override void Apply(IPluginSetupTarget target)
        {
            selectedMapping.MapInput(target.GetFeedStrategy<BarStrategy>(), Descriptor.Id, SelectedSymbol.Name);
        }

        public override void Reset()
        {
            base.Reset();
            SetDefaultMapping();
        }

        private void SetDefaultMapping()
        {
            SelectedMapping = mappings.FirstOrDefault(m => m.Name == defPriceType + ".Close");
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as BarToDoubleInput;
            SelectedSymbol = otherInput.SelectedSymbol;
            SelectedMapping = mappings.FirstOrDefault(m => m.Name == otherInput.selectedMapping.Name);
            if (selectedMapping == null)
                SetDefaultMapping();
        }

        public abstract class Mapping
        {
            public Mapping(string name)
            {
                this.Name = name;
            }

            internal abstract void MapInput(BarStrategy strategy, string inputName, string symbol);

            public string Name { get; private set; }
        }

        private class FormulaMapping : Mapping
        {
            private Func<Bar, double> formula;
            private BarPriceType priceType;

            public FormulaMapping(string name, BarPriceType priceType, Func<Bar, double> formula) : base(name)
            {
                this.formula = formula;
                this.priceType = priceType;
            }

            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput(inputName, symbol, priceType, formula);
            }
        }

        private class ReductionMapping : Mapping
        {
            private BarToDoubleReduction reduction;
            private BarPriceType priceType;

            public ReductionMapping(string name, BarPriceType priceType, ReductionDescriptor descriptor)
                : base(name)
            {
                this.reduction = descriptor.CreateInstance<BarToDoubleReduction>();
                this.priceType = priceType;
            }

            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput(inputName, symbol, priceType, reduction.Reduce);
            }
        }

        private class FullBarReductionMapping : Mapping
        {
            private FullBarToDoubleReduction reduction;

            public FullBarReductionMapping(string name, ReductionDescriptor descriptor)
                : base(name)
            {
                this.reduction = descriptor.CreateInstance<FullBarToDoubleReduction>();
            }

            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput(inputName, symbol, reduction.Reduce);
            }
        }
    }
}
