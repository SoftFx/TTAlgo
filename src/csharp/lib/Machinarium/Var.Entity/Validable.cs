using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Machinarium.Var
{
    public class ValidableBase<TVar, T> : PropertyBase<TVar, T>, IValidable<T>, IDataErrorInfo
        where TVar : Var<T>, new()
    {
        private readonly List<Rule> _rules = new List<Rule>();

        internal ValidableBase()
        {
            ErrorVar = new Var<string>();
            ErrorVar.SetContext(this);

            HasError = new BoolVar();

            Var.Changed += () => Validate();
            ErrorVar.Changed += () => NotifyPropertyChange(nameof(Error));
        }

        public void Validate() // TODO: add after post Trigger Event
        {
            foreach (var rule in _rules)
                if (!rule.Condition(Value))
                {
                    ErrorVar.Value = rule.MessageFactory();
                    HasError.Value = true;
                    NotificationCall(); //crutch IDataErrorInfo.this[string columnName] call before this function. Error template don't update
                    return;
                }

            ErrorVar.Value = null;
            HasError.Value = false;
            NotificationCall(); //crutch IDataErrorInfo.this[string columnName] call before this function. Error template don't update
        }

        public Var<string> ErrorVar { get; private set; }

        public string Error => ErrorVar.Value;

        string IDataErrorInfo.Error => Error;

        public BoolVar HasError { get; }

        string IDataErrorInfo.this[string columnName] => columnName == nameof(Value) ? Error : string.Empty;

        public void AddValidationRule(Predicate<T> condition, Func<string> errorFunc)
        {
            _rules.Add(new Rule { Condition = condition, MessageFactory = errorFunc });
            Validate();
        }

        public void AddValidationRule(Predicate<T> condition, string msg)
        {
            _rules.Add(new Rule { Condition = condition, MessageFactory = () => msg });
            Validate();
        }

        private struct Rule
        {
            public Predicate<T> Condition { get; set; }

            public Func<string> MessageFactory { get; set; }
        }
    }

    public class Validable<T> : ValidableBase<Var<T>, T>
    {
        public new Validable<T> AddPreTrigger(Action<T> trigger) => (Validable<T>)base.AddPreTrigger(trigger);

        public new Validable<T> AddPostTrigger(Action<T> trigger) => (Validable<T>)base.AddPostTrigger(trigger);
    }

    public class IntValidable : ValidableBase<IntVar, int> { }

    public class BoolValidable : ValidableBase<BoolVar, bool> { }

    public class DoubleValidable : ValidableBase<DoubleVar, double> { }

    public interface IValidable<T> : IProperty<T>, IValidable { }

    public interface IValidable : IDisposable
    {
        Var<string> ErrorVar { get; }

        string Error { get; }

        BoolVar HasError { get; }

        void Validate();
    }
}
