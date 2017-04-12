using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarInputBase : InputSetup
    {
        public BarInputBase(InputDescriptor descriptor, string symbolCode, IReadOnlyList<ISymbolInfo> symbols = null)
            : base(descriptor, symbolCode, symbols)
        {
            SetMetadata(descriptor);
        }

        public override Property Save()
        {
            throw new NotImplementedException();
        }

        public override void Load(Property srcProperty)
        {
            throw new NotImplementedException();
            //var otherInput = srcProperty as BarToBarInput;
            //SelectedSymbol = otherInput.SelectedSymbol;
        }
    }

    public class SingleBarInputBase : BarInputBase
    {
        private BarPriceType defPriceType;
        private BarPriceType priceType;

        public SingleBarInputBase(InputDescriptor descriptor, string symbolCode, BarPriceType defPriceType, IReadOnlyList<ISymbolInfo> symbols = null)
            : base(descriptor, symbolCode, symbols)
        {
            this.defPriceType = defPriceType;
        }

        public BarPriceType PriceType
        {
            get { return priceType; }
            set
            {
                this.priceType = value;
                NotifyPropertyChanged(nameof(PriceType));
            }
        }

        public override void Reset()
        {
            base.Reset();
            PriceType = defPriceType;
        }

        public override Property Save()
        {
            throw new NotImplementedException();
        }

        public override void Load(Property srcProperty)
        {
            throw new NotImplementedException();
            //var otherInput = srcProperty as BarToBarInput;
            //SelectedSymbol = otherInput.SelectedSymbol;
        }
    }
}
