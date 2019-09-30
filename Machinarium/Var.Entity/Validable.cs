using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class ValidableBase<TVar, T> : PropertyBase<TVar, T>, IValidable<T>, IDataErrorInfo
        where TVar : Var<T>, new()
    {
        private Var<string> _errorVar;
        private List<Rule> _rules = new List<Rule>();

        internal ValidableBase()
        {
            _errorVar = new Var<string>();
            _errorVar.SetContext(this);

            Var.Changed += () => ApplyValidationRules();
            _errorVar.Changed += () => NotifyPropertyChange(nameof(Error));
        }

        private void ApplyValidationRules()
        {
            foreach (var rule in _rules)
            {
                if (!rule.Condition(Value))
                {
                    _errorVar.Value = rule.MessageFactory();
                    return;
                }
            }

            _errorVar.Value = null;
        }

        public Var<string> ErrorVar => _errorVar;
        public string Error => _errorVar.Value;

        string IDataErrorInfo.Error => null;
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == nameof(Value))
                    return (string)Error;
                return null;
            }
        }

        public void AddValidationRule(Predicate<T> condition, Func<string> errorFunc)
        {
            _rules.Add(new Rule { Condition = condition, MessageFactory = errorFunc });
            ApplyValidationRules();
        }

        public void AddValidationRule(Predicate<T> condition, string msg)
        {
            _rules.Add(new Rule { Condition = condition, MessageFactory = () => msg });
            ApplyValidationRules();
        }

        public void Validate()
        {
            ApplyValidationRules();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private struct Rule
        {
            public Predicate<T> Condition { get; set; }
            public Func<string> MessageFactory { get; set; }
        }
    }

    public class Validable<T> : ValidableBase<Var<T>, T> { }
    public class IntValidable : ValidableBase<IntVar, int> { }
    public class BoolValidable : ValidableBase<BoolVar, bool> { }
    public class DoubleValidable : ValidableBase<DoubleVar, double> { }

    public interface IValidable<T> : IProperty<T>, IValidable { }

    public interface IValidable
    {
        Var<string> ErrorVar { get; }

        string Error { get; }
    }
}
