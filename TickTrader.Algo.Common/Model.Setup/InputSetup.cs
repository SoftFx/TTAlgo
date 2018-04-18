using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class InputSetup : PropertySetupBase
    {
        private ISymbolInfo _selectedSymbol;
        private string _defaultSymbolCode;

        public ISymbolInfo SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                _selectedSymbol = value;
                NotifyPropertyChanged(nameof(SelectedSymbol));
            }
        }

        public IReadOnlyList<ISymbolInfo> AvailableSymbols { get; private set; }


        public InputSetup(InputDescriptor descriptor, string defaultSymbolCode, IReadOnlyList<ISymbolInfo> symbols)
        {
            _defaultSymbolCode = defaultSymbolCode;

            SetMetadata(descriptor);
            if (symbols == null)
                AvailableSymbols = new ISymbolInfo[] { new DummySymbolInfo(defaultSymbolCode) };
            else
                AvailableSymbols = symbols;
        }


        public override void Reset()
        {
            SelectedSymbol = AvailableSymbols.First(s => s.Name == _defaultSymbolCode);
        }

        protected virtual void LoadConfig(Input input)
        {
            _selectedSymbol = AvailableSymbols.FirstOrDefault(s => s.Name == input.SelectedSymbol);
            if (_selectedSymbol == null)
            {
                _selectedSymbol = AvailableSymbols.First(s => s.Name == _defaultSymbolCode);
            }
        }

        protected virtual void SaveConfig(Input input)
        {
            input.Id = Id;
            input.SelectedSymbol = _selectedSymbol.Name;
        }


        public class Invalid : InputSetup
        {
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, null, null)
            {
                if (error == null)
                    Error = new GuiModelMsg(descriptor.Error.Value);
                else
                    Error = new GuiModelMsg(error);
            }

            public Invalid(InputDescriptor descriptor, string symbol, GuiModelMsg error)
                : base(descriptor, symbol, null)
            {
                Error = error;
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
    }


    internal class DummySymbolInfo : ISymbolInfo
    {
        public DummySymbolInfo(string symbol)
        {
            Name = symbol;
        }

        public string Name { get; private set; }
    }
}
