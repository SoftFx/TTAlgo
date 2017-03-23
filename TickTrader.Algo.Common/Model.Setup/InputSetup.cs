using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    //public abstract class InputSetup : PropertySetupBase
    //{
    //    public override void CopyFrom(PropertySetupBase srcProperty) { }
    //    public override void Reset() { }
    //}

    public abstract class InputSetup : PropertySetupBase
    {
        private ISymbolInfo selectedSymbol;
        private string defaultSymbolCode;

        public InputSetup(InputDescriptor descriptor, string defaultSymbolCode, IReadOnlyList<ISymbolInfo> symbols)
        {
            this.defaultSymbolCode = defaultSymbolCode;

            SetMetadata(descriptor);
            if (symbols == null)
                AvailableSymbols = new ISymbolInfo[] { new DummySymbolInfo(defaultSymbolCode) };
            else
                AvailableSymbols = symbols;
        }

        public ISymbolInfo SelectedSymbol
        {
            get { return selectedSymbol; }
            set
            {
                selectedSymbol = value;
                NotifyPropertyChanged(nameof(SelectedSymbol));
            }
        }

        public IReadOnlyList<ISymbolInfo> AvailableSymbols { get; private set; }

        public override void Reset()
        {
            SelectedSymbol = AvailableSymbols.First(s => s.Name == defaultSymbolCode);
        }

        public class Invalid : InputSetup
        {
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, null, null)
            {
                if (error == null)
                    this.Error = new GuiModelMsg(descriptor.Error.Value);
                else
                    this.Error = new GuiModelMsg(error);
            }

            public Invalid(InputDescriptor descriptor, string symbol, GuiModelMsg error)
                : base(descriptor, symbol, null)
            {
                this.Error = error;
            }

            public override void Apply(IPluginSetupTarget target)
            {
                throw new Exception("Cannot configure invalid input!");
            }

            public override void Load(Property srcProperty)
            {
            }

            public override Property Save()
            {
                throw new Exception("Cannot save invalid input!");
            }
        }

        public enum QuoteToDoubleMappings { Ask, Bid, Median }
    }

    internal class DummySymbolInfo : ISymbolInfo
    {
        public DummySymbolInfo(string symbol)
        {
            this.Name = symbol;
        }

        public string Name { get; private set; }
    }
}
