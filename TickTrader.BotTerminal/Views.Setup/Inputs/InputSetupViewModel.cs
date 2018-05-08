using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public abstract class InputSetupViewModel : PropertySetupViewModel
    {
        private string _defaultSymbolCode;
        private SymbolInfo _selectedSymbol;


        protected SetupMetadata SetupMetadata { get; }


        public InputDescriptor Descriptor { get; }

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


        private InputSetupViewModel(InputDescriptor descriptor, string defaultSymbolCode)
        {
            Descriptor = descriptor;
            _defaultSymbolCode = defaultSymbolCode;

            SetMetadata(descriptor);
        }

        public InputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : this(descriptor, setupMetadata.Context.DefaultSymbolCode)
        {
            SetupMetadata = setupMetadata;

            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(_defaultSymbolCode);
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
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, (string)null)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
                else
                    Error = new ErrorMsgModel(error);
            }

            public Invalid(InputDescriptor descriptor, string symbol, ErrorMsgModel error)
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
