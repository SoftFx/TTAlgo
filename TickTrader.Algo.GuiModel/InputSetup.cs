using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    //public abstract class InputSetup : PropertySetupBase
    //{
    //    public override void CopyFrom(PropertySetupBase srcProperty) { }
    //    public override void Reset() { }
    //}

    public abstract class InputSetup : PropertySetupBase
    {
        private string selectedSymbol;
        private string defaultSymbolCode;

        public InputSetup(InputDescriptor descriptor, string defaultSymbolCode)
        {
            this.defaultSymbolCode = defaultSymbolCode;

            SetMetadata(descriptor);
            AvailableSymbols = new string[] { defaultSymbolCode };

            selectedSymbol = defaultSymbolCode;
        }

        public string SymbolCode
        {
            get { return selectedSymbol; }
            set
            {
                selectedSymbol = value;
                NotifyPropertyChanged(nameof(SymbolCode));
            }
        }

        public IEnumerable<string> AvailableSymbols { get; private set; }

        public abstract void Configure(IndicatorBuilder builder);

        public override void Reset()
        {
            SymbolCode = defaultSymbolCode;
        }

        public class Invalid : InputSetup
        {
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, null)
            {
                if (error == null)
                    this.Error = new GuiModelMsg(descriptor.Error.Value);
                else
                    this.Error = new GuiModelMsg(error);
            }

            public Invalid(InputDescriptor descriptor, string symbol, GuiModelMsg error)
                : base(descriptor, symbol)
            {
                this.Error = error;
            }

            public override void Apply(IPluginSetupTarget target)
            {
                throw new Exception("Cannot configure invalid input!");
            }

            public override void Configure(IndicatorBuilder builder)
            {
                throw new Exception("Cannot configure invalid input!");
            }

            public override void CopyFrom(PropertySetupBase srcProperty)
            {
            }
        }

        public enum BarToDoubleMappings { Open, Close, High, Low, Median, Typical, Weighted, Move, Range }
        public enum QuoteToDoubleMappings { Ask, Bid, Median }
    }
}
