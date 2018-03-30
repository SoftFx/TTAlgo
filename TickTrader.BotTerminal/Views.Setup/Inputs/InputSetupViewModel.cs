using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    public abstract class InputSetupViewModel : PropertySetupViewModel
    {
        private string _defaultSymbolCode;
        private SymbolInfo _selectedSymbol;


        protected AccountMetadataInfo AccountMetadata { get; }


        public InputMetadataInfo Metadata { get; }

        public IReadOnlyList<SymbolInfo> AvailableSymbols { get; private set; }

        public SymbolInfo SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                if (_selectedSymbol == value)
                    return;

                _selectedSymbol = value;
                NotifyOfPropertyChange(nameof(SelectedSymbol));
            }
        }


        private InputSetupViewModel(InputMetadataInfo metadata, string defaultSymbolCode)
        {
            Metadata = metadata;
            _defaultSymbolCode = defaultSymbolCode;

            SetMetadata(metadata);
        }

        public InputSetupViewModel(InputMetadataInfo metadata, AccountMetadataInfo accountMetadata, string defaultSymbolCode)
            : this(metadata, defaultSymbolCode)
        {
            AccountMetadata = accountMetadata;

            AvailableSymbols = accountMetadata.GetAvaliableSymbols(_defaultSymbolCode);
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


        public class Invalid : InputSetupViewModel
        {
            public Invalid(InputMetadataInfo descriptor, object error = null)
                : base(descriptor, null)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error.Value);
                else
                    Error = new ErrorMsgModel(error);
            }

            public Invalid(InputMetadataInfo descriptor, string symbol, ErrorMsgModel error)
                : base(descriptor, symbol)
            {
                Error = error;
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
