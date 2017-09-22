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
        private Guid _id;
        private VarContext _varContext = new VarContext();

        public SymbolCfgEditorViewModel(CustomSymbol symbol, TraderClientModel client)
        {
            if (symbol == null)
            {
                DisplayName = "Add Custom Symbol";
                symbol = new CustomSymbol()
                {
                    Name = "Symbol1",
                    Description = "",
                    BaseCurr = "EUR",
                    ProfitCurr = "USD",
                    Digits = 5
                };
            }
            else
                DisplayName = "Edit Custom Symbol Config";

            Name = _varContext.AddValidable<string>(symbol.Name);
            Description = _varContext.AddValidable<string>(symbol.Description);
            BaseCurr = _varContext.AddValidable<string>(symbol.BaseCurr);
            ProfitCurr = _varContext.AddValidable<string>(symbol.ProfitCurr);
            Digits = _varContext.AddIntValidable(symbol.Digits);
            DigitsStr = _varContext.AddConverter(Digits, new StringToInt());
            Error = _varContext.AddProperty<string>();

            Name.MustBeNotEmpy();
            BaseCurr.MustBeNotEmpy();
            ProfitCurr.MustBeNotEmpy();

            Digits.AddValidationRule(v => v >= 1 && v <= 11, "Digits can be from 1 to 11!");

            AvailableCurrencies = client.Currencies.TransformToList((k, v) => k).AsObservable();

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
                Name = Name.Value,
                Description = Description.Value,
                BaseCurr = BaseCurr.Value,
                ProfitCurr = ProfitCurr.Value
            };
        }

        public Validable<string> Name { get; }
        public Validable<string> Description { get; }
        public Validable<string> BaseCurr { get; }
        public Validable<string> ProfitCurr { get; }
        public IntValidable Digits { get; }
        public IValidable<string> DigitsStr { get; }
        public IObservableListSource<string> AvailableCurrencies { get; }
        public Property<string> Error { get; }

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
