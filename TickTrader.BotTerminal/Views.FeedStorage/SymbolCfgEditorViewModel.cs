using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class SymbolCfgEditorViewModel : Screen, IWindowModel
    {
        private VarContext _varContext = new VarContext();

        public SymbolCfgEditorViewModel(CustomSymbol symbol, IObservableList<string> availableCurrencies, Predicate<string> symbolExistsFunc, bool createNew = false)
        {
            if (symbol == null)
            {
                IsEditMode = false;
                symbol = new CustomSymbol()
                {
                    Name = "Symbol1",
                    Description = "",
                    BaseCurr = "EUR",
                    ProfitCurr = "USD",
                    Digits = 5,
                    ContractSize = 1,
                    MinVolume = 0.01,
                    MaxVolume = 1000,
                    VolumeStep = 0.01,
                    Commission = 0.0018,
                };
            }
            else
            {
                IsEditMode = !createNew;
            }
            DisplayName = IsEditMode ? "Edit Symbol Settings" : "Add Custom Symbol";

            Name = _varContext.AddValidable<string>(symbol.Name);
            Description = _varContext.AddValidable<string>(symbol.Description);
            BaseCurr = _varContext.AddValidable<string>(symbol.BaseCurr);
            ProfitCurr = _varContext.AddValidable<string>(symbol.ProfitCurr);
            Digits = _varContext.AddIntValidable(symbol.Digits);
            DigitsStr = _varContext.AddConverter(Digits, new StringToInt());
            ContractSize = _varContext.AddDoubleValidable(symbol.ContractSize);
            ContractSizeStr = _varContext.AddConverter(ContractSize, new StringToDouble());
            MinVolume = _varContext.AddDoubleValidable(symbol.MinVolume);
            MinVolumeStr = _varContext.AddConverter(MinVolume, new StringToDouble());
            MaxVolume = _varContext.AddDoubleValidable(symbol.MaxVolume);
            MaxVolumeStr = _varContext.AddConverter(MaxVolume, new StringToDouble());
            VolumeStep = _varContext.AddDoubleValidable(symbol.VolumeStep);
            VolumeStepStr = _varContext.AddConverter(VolumeStep, new StringToDouble());
            Commission = _varContext.AddDoubleValidable(symbol.Commission);
            CommissionStr = _varContext.AddConverter(Commission, new StringToDouble());
            Error = _varContext.AddProperty<string>();

            if (!IsEditMode)
            {
                Name.MustBeNotEmpy();
                Name.AddValidationRule(v => !symbolExistsFunc(v), "Symbol with such name already exists!");
            }

            BaseCurr.MustBeNotEmpy();
            ProfitCurr.MustBeNotEmpy();

            Digits.AddValidationRule(v => v >= 1 && v <= 11, "Digits can be from 1 to 11!");

            AvailableCurrencies = availableCurrencies;

            Action<VarChangeEventArgs<string>> validate = a =>
            {
                if (BaseCurr.Value == ProfitCurr.Value)
                    Error.Value = "Profit currency cannot be same as base currency!";
                else
                    Error.Value = null;
            };

            _varContext.TriggerOnChange(BaseCurr.Var, validate);
            _varContext.TriggerOnChange(ProfitCurr.Var, validate);

            IsValid = Name.IsValid() & BaseCurr.IsValid() & ProfitCurr.IsValid() & DigitsStr.IsValid() & Error.Var.IsEmpty();
        }

        public CustomSymbol GetResultingSymbol()
        {
            return new CustomSymbol()
            {
                Name = Name.Value.Trim(),
                Description = Description.Value,
                BaseCurr = BaseCurr.Value.Trim(),
                ProfitCurr = ProfitCurr.Value.Trim(),
                Digits = Digits.Value,
                ContractSize = ContractSize.Value,
                MinVolume = MinVolume.Value,
                MaxVolume = MaxVolume.Value,
                VolumeStep = VolumeStep.Value,
                Commission = Commission.Value,
            };
        }

        public Validable<string> Name { get; }
        public Validable<string> Description { get; }
        public Validable<string> BaseCurr { get; }
        public Validable<string> ProfitCurr { get; }
        public IntValidable Digits { get; }
        public IValidable<string> DigitsStr { get; }
        public DoubleValidable ContractSize { get; }
        public IValidable<string> ContractSizeStr { get; }
        public DoubleValidable MinVolume { get; }
        public IValidable<string> MinVolumeStr { get; }
        public DoubleValidable MaxVolume { get; }
        public IValidable<string> MaxVolumeStr { get; }
        public DoubleValidable VolumeStep { get; }
        public IValidable<string> VolumeStepStr { get; }
        public DoubleValidable Commission { get; }
        public IValidable<string> CommissionStr { get; }
        public IObservableList<string> AvailableCurrencies { get; }
        public Property<string> Error { get; }
        public bool IsEditMode { get; }
        public bool IsAddMode => !IsEditMode;

        public BoolVar IsValid { get; }

        public void Ok()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
