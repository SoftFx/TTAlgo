﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public abstract class InputSetupViewModel : PropertySetupViewModel, IDataErrorInfo
    {
        private StorageSymbolKey _defaultSymbol;
        private StorageSymbolKey _selectedSymbol;


        protected SetupMetadata SetupMetadata { get; }


        public InputDescriptor Descriptor { get; }

        public IReadOnlyList<StorageSymbolKey> AvailableSymbols { get; private set; }

        public StorageSymbolKey SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                if (_selectedSymbol == value)
                    return;

                _selectedSymbol = value;
                NotifyOfPropertyChange(nameof(SelectedSymbol));
                CheckSelectedSymbol();
            }
        }

        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] => columnName == nameof(SelectedSymbol) ? Error?.Code : null;


        private InputSetupViewModel(InputDescriptor descriptor, StorageSymbolKey defaultSymbol)
        {
            Descriptor = descriptor;
            _defaultSymbol = defaultSymbol;

            SetMetadata(descriptor);
        }

        public InputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata, SymbolKey _mainSymbol = null)
            : this(descriptor, setupMetadata.DefaultSymbol.ToKey())
        {
            SetupMetadata = setupMetadata;

            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(_defaultSymbol);
        }


        public override void Reset()
        {
            SelectedSymbol = AvailableSymbols.GetMainTokenOrNull() ?? AvailableSymbols.GetSymbolOrAny(_defaultSymbol);
        }


        protected virtual void LoadConfig(IInputConfig input)
        {
            SelectedSymbol = AvailableSymbols.GetSymbolOrDefault(input.SelectedSymbol.ToKey())
                ?? AvailableSymbols.GetSymbolOrAny(_defaultSymbol);
        }

        protected virtual void SaveConfig(IInputConfig input)
        {
            input.PropertyId = Id;
            input.SelectedSymbol = _selectedSymbol.ToConfig();
        }


        private void CheckSelectedSymbol()
        {
            Error = _selectedSymbol == null ? new ErrorMsgModel(ErrorMsgCodes.RequiredButNotSet) : null;
        }


        public class Invalid : InputSetupViewModel
        {
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, (StorageSymbolKey)null)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.ErrorCode);
                else
                    Error = new ErrorMsgModel(error);
            }


            public override void Load(IPropertyConfig srcProperty)
            {
            }

            public override IPropertyConfig Save()
            {
                throw new Exception("Cannot save invalid input!");
            }

            public override void Reset()
            {
            }
        }
    }
}
