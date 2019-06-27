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
        private SymbolKey _defaultSymbol;
        private SymbolKey _selectedSymbol;


        protected SetupMetadata SetupMetadata { get; }


        public InputDescriptor Descriptor { get; }

        public IReadOnlyList<SymbolKey> AvailableSymbols { get; private set; }

        public SymbolKey SelectedSymbol
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


        private InputSetupViewModel(InputDescriptor descriptor, SymbolKey defaultSymbol)
        {
            Descriptor = descriptor;
            _defaultSymbol = defaultSymbol;

            SetMetadata(descriptor);
        }

        public InputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : this(descriptor, setupMetadata.DefaultSymbol)
        {
            SetupMetadata = setupMetadata;

            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(_defaultSymbol);
        }


        public override void Reset()
        {
            SelectedSymbol = AvailableSymbols.GetSymbolOrAny(_defaultSymbol);
        }


        protected virtual void LoadConfig(Input input)
        {
            SelectedSymbol = AvailableSymbols.GetSymbolOrDefault(input.SelectedSymbol)
                ?? AvailableSymbols.GetSymbolOrAny(_defaultSymbol);
        }

        protected virtual void SaveConfig(Input input)
        {
            input.Id = Id;
            input.SelectedSymbol = _selectedSymbol.ToConfig();
        }


        public class Invalid : InputSetupViewModel
        {
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, (SymbolKey)null)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
                else
                    Error = new ErrorMsgModel(error);
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
