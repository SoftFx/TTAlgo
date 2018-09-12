using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class BarInputSetupBase : InputSetup
    {
        public BarInputSetupBase(InputDescriptor descriptor, ISymbolInfo defaultSymbol, IReadOnlyList<ISymbolInfo> symbols = null)
            : base(descriptor, defaultSymbol, symbols)
        {
            SetMetadata(descriptor);
        }
    }

    public abstract class SingleBarInputSetupBase : BarInputSetupBase
    {
        private BarPriceType _defPriceType;
        private BarPriceType _priceType;


        public BarPriceType PriceType
        {
            get { return _priceType; }
            set
            {
                _priceType = value;
                NotifyPropertyChanged(nameof(PriceType));
            }
        }


        public SingleBarInputSetupBase(InputDescriptor descriptor, ISymbolInfo defaultSymbol, BarPriceType defPriceType, IReadOnlyList<ISymbolInfo> symbols = null)
            : base(descriptor, defaultSymbol, symbols)
        {
            _defPriceType = defPriceType;
        }


        public override void Reset()
        {
            base.Reset();
            PriceType = _defPriceType;
        }


        protected override void LoadConfig(Input input)
        {
            var barInput = input as SingleBarInputBase;
            _priceType = barInput?.PriceType ?? _defPriceType;

            base.LoadConfig(input);
        }

        protected override void SaveConfig(Input input)
        {
            var barInput = input as SingleBarInputBase;
            if (barInput != null)
            {
                barInput.PriceType = _priceType;
            }

            base.SaveConfig(input);
        }
    }
}
