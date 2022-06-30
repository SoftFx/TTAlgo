using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Machinarium.Var
{
    public interface IValidable<T> : IProperty<T>, IValidable { }

    public interface IValidable : IDisposable
    {
        Var<string> ErrorVar { get; }

        string Error { get; }

        BoolVar HasError { get; }

        void Validate();
    }


    public class Validable<T> : ValidableBase<Var<T>, T>
    {
        public new Validable<T> AddPreTrigger(Action<T> trigger) => (Validable<T>)base.AddPreTrigger(trigger);

        public new Validable<T> AddPostTrigger(Action<T> trigger) => (Validable<T>)base.AddPostTrigger(trigger);


        public Validable(IValueConverter<T, string> validationConverter) : base(validationConverter) { }
    }

    public class IntValidable : ValidableBase<IntVar, int>
    {
        public IntValidable(IValueConverter<int, string> validationConverter = null) :
               base(validationConverter ?? new StringToInt())
        {
        }
    }

    public class BoolValidable : ValidableBase<BoolVar, bool>
    {
        public BoolValidable(IValueConverter<bool, string> validationConverter = null) : base(validationConverter) { }
    }

    public class DoubleValidable : ValidableBase<DoubleVar, double>
    {
        public DoubleValidable(IValueConverter<double, string> validationConverter = null) :
               base(validationConverter ?? new StringToDouble())
        {
        }
    }


    public abstract class ValidableBase<TVar, T> : PropertyBase<TVar, T>, IValidable<T>, IDataErrorInfo
                                                   where TVar : Var<T>, new()
    {
        private readonly struct Rule
        {
            public Predicate<T> Condition { get; }

            public Func<string> MessageFactory { get; }


            public Rule(Predicate<T> cond, Func<string> mesFac)
            {
                Condition = cond;
                MessageFactory = mesFac;
            }
        }


        private readonly List<Rule> _rules = new List<Rule>();

        private readonly IValueConverter<T, string> _validationConverter;

        private string _valValue;


        public BoolVar HasError { get; }

        public Var<string> ErrorVar { get; private set; }


        public string Error => ErrorVar.Value;

        string IDataErrorInfo.this[string propName] =>
               (propName == nameof(Value) || propName == nameof(ValValue)) ? Error : string.Empty;


        public string ValValue
        {
            get => _valValue;

            set
            {
                if (string.Equals(_valValue, value))
                    return;

                _valValue = value;

                var val = _validationConverter.ConvertTo(value, out var error);

                if (string.IsNullOrEmpty(error))
                {
                    Value = val;
                    Validate(false);
                }
                else
                    SetError(error);
            }
        }


        internal ValidableBase(IValueConverter<T, string> validationConverter)
        {
            _validationConverter = validationConverter;

            ErrorVar = new Var<string>();
            ErrorVar.SetContext(this);

            HasError = new BoolVar();

            Var.Changed += () => Validate();
            ErrorVar.Changed += () => NotifyPropertyChange(nameof(Error));
        }


        public void Validate() => Validate(true);

        public void Validate(bool updateVal = true) // TODO: add after post Trigger Event
        {
            if (updateVal)
                _valValue = Value?.ToString();

            foreach (var rule in _rules)
                if (!rule.Condition(Value))
                {
                    SetError(rule.MessageFactory()); //crutch IDataErrorInfo.this[string columnName] call before this function. Error template don't update
                    return;
                }

            SetError(null); //crutch IDataErrorInfo.this[string columnName] call before this function. Error template don't update
        }


        public void AddValidationRule(Predicate<T> condition, string msg)
        {
            AddValidationRule(condition, () => msg);
        }

        public void AddValidationRule(Predicate<T> condition, Func<string> errorFunc)
        {
            _rules.Add(new Rule(condition, errorFunc));
            Validate();
        }

        private void SetError(string error)
        {
            ErrorVar.Value = error;
            HasError.Value = !string.IsNullOrEmpty(error);
            NotificationCall();
        }

        protected override void NotificationCall()
        {
            base.NotificationCall();
            NotifyPropertyChange(nameof(ValValue));
        }
    }
}
