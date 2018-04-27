using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class InputSetupModel : PropertySetupModel
    {
        private string _defaultSymbolCode;
        private ISymbolInfo _selectedSymbol;


        protected IAlgoSetupMetadata Metadata { get; }


        public InputMetadata Descriptor { get; }

        public IReadOnlyList<ISymbolInfo> AvailableSymbols { get; private set; }

        public ISymbolInfo SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                if (_selectedSymbol == value)
                    return;

                _selectedSymbol = value;
                NotifyPropertyChanged(nameof(SelectedSymbol));
            }
        }


        private InputSetupModel(InputMetadata descriptor, string defaultSymbolCode)
        {
            Descriptor = descriptor;
            _defaultSymbolCode = defaultSymbolCode;

            SetMetadata(descriptor);
        }

        public InputSetupModel(InputMetadata descriptor, IAlgoSetupMetadata metadata, string defaultSymbolCode)
            : this(descriptor, defaultSymbolCode)
        {
            Metadata = metadata;

            AvailableSymbols = metadata.GetAvaliableSymbols(_defaultSymbolCode);
        }


        public override void Reset()
        {
            SelectedSymbol = AvailableSymbols.GetSymbolOrAny(_defaultSymbolCode);
        }


        protected virtual void LoadConfig(Input input)
        {
            _selectedSymbol = AvailableSymbols.FirstOrDefault(s => s.Name == input.SelectedSymbol)
                ?? AvailableSymbols.GetSymbolOrAny(_defaultSymbolCode);
        }

        protected virtual void SaveConfig(Input input)
        {
            input.Id = Id;
            input.SelectedSymbol = _selectedSymbol.Name;
        }


        public class Invalid : InputSetupModel
        {
            public Invalid(InputMetadata descriptor, object error = null)
                : base(descriptor, null)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
                else
                    Error = new ErrorMsgModel(error);
            }

            public Invalid(InputMetadata descriptor, string symbol, ErrorMsgModel error)
                : base(descriptor, symbol)
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

            public override void Reset()
            {
            }
        }
    }
}
