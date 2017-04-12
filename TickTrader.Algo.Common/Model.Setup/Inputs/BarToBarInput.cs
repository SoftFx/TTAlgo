using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarToBarInput : BarInputBase
    {
        private Mapping selectedMapping;
        private BarPriceType defPriceType;
        private List<Mapping> mappings = new List<Mapping>();

        public BarToBarInput(InputDescriptor descriptor, string symbolCode, BarPriceType defPriceType, IAlgoGuiMetadata metadata = null)
            : base(descriptor, symbolCode, metadata?.Symbols)
        {
            SetMetadata(descriptor);
            this.defPriceType = defPriceType;

            mappings.Add(new DirectMapping("Bid", BarPriceType.Bid));
            mappings.Add(new DirectMapping("Ask", BarPriceType.Ask));

            if (metadata != null)
            {
                foreach (var d in metadata.Extentions.FullBarToBarReductions)
                {
                    try
                    {
                        mappings.Add(new ReductionMapping(d.DisplayName, d));
                    }
                    catch (Exception)
                    {
                        // TO DO : logging
                    }
                }
            }
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
            selectedMapping?.MapInput(target.GetFeedStrategy<BarStrategy>(), Descriptor.Id, SelectedSymbol.Name);
        }

        public override void Load(Property srcProperty)
        {
            throw new NotImplementedException();
            //var otherInput = srcProperty as BarToBarInput;
            //SelectedSymbol = otherInput.SelectedSymbol;
            //SetDefaultMapping();
        }

        public override void Reset()
        {
            base.Reset();
            SetDefaultMapping();
        }

        private void SetDefaultMapping()
        {
            SelectedMapping = mappings.FirstOrDefault(m => m.Name == defPriceType.ToString());
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

        private class DirectMapping : Mapping
        {
            private BarPriceType barSide;

            public DirectMapping(string name, BarPriceType barSide) : base(name)
            {
                this.barSide = barSide;
            }

            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput<Api.Bar>(inputName, symbol, barSide, b => b);
            }
        }

        private class ReductionMapping : Mapping
        {
            private FullBarToBarReduction reduction;

            public ReductionMapping(string name, ReductionDescriptor descriptor)
                : base(name)
            {
                this.reduction = descriptor.CreateInstance<FullBarToBarReduction>();
            }

            internal override void MapInput(BarStrategy strategy, string inputName, string symbol)
            {
                strategy.MapInput<Api.Bar>(inputName, symbol, (b, a) =>
                {
                    BarEntity entity = new BarEntity();
                    reduction.Reduce(b, a, entity);
                    return entity;
                });
            }
        }
    }
}
