using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.EntityModel
{
    public abstract class Field : ObservableObject
    {
        public abstract string Name { get; }
        public abstract object ValueObj { get; }
        public abstract bool HasError { get; }
        public abstract bool HasConverterError { get; }

        internal event Action<Field> ValueChanged;
        internal event Action<Field> ErrorChanged = delegate { };
        internal event Action<Field> HasErrorChanged = delegate { };
        internal event Action<Field> HasConverterErrorChanged = delegate { };

        protected void OnValueChanged()
        {
            ValueChanged?.Invoke(this);
        }

        protected void OnErrorChanged()
        {
            ErrorChanged?.Invoke(this);
        }

        protected void OnHasErrorChanged()
        {
            HasErrorChanged?.Invoke(this);
        }

        protected void OnHasConverterErrorChanged()
        {
            HasConverterErrorChanged?.Invoke(this);
        }
    }

    public class Field<T> : Field
    {
        private T value;
        private object error;
        private string name;
        private List<ValidationRule<T>> validationRules;
        private Dictionary<Type, object> proxies;
        private int converterErrorCount;

        public Field(string name)
        {
            this.name = name;
        }

        public override string Name { get { return name; } }

        public T Value
        {
            get { return value; }
            set
            {
                this.value = value;
                Validate();
                OnValueChanged();
                OnPropertyChanged(nameof(Value));
            }
        }

        public override object ValueObj { get { return value; } }
        public override bool HasConverterError { get; }

        public Field<T> AddValidationRule(Predicate<T> validationFunc, object error)
        {
            AddValidationRule(new GenericValidationRule<T>(v =>
            {
                if (validationFunc(v))
                    return null;
                else
                    return error;
            }));
            Validate();
            return this;
        }

        public Field<T> AddValidationRule(Predicate<T> validationFunc, Func<object> errorFactory)
        {
            AddValidationRule(new GenericValidationRule<T>(v =>
            {
                if (validationFunc(v))
                    return null;
                else
                    return errorFactory();
            }));
            Validate();
            return this;
        }

        public Field<T> AddValidationRule(Func<T, object> validationFunc)
        {
            AddValidationRule(new GenericValidationRule<T>(validationFunc));
            Validate();
            return this;
        }

        public Field<T> AddValidationRule(ValidationRule<T> rule)
        {
            if (validationRules == null)
                validationRules = new List<ValidationRule<T>>();
            validationRules.Add(rule);
            return this;
        }

        public FieldProxy<TSrc> AddConverter<TSrc>(PropertyConverter<TSrc, T> converter)
        {
            if (proxies == null)
                proxies = new Dictionary<Type, object>();

            var proxy = new FieldProxy<TSrc, T>(this, converter);
            proxy.HasConvertErrorChanged += Proxy_HasConvertErrorChanged;
            proxies.Add(typeof(TSrc), proxy);
            return proxy;
        }

        public void Validate()
        {
            Error = CheckAllRules();
        }

        public override bool HasError { get { return error != null; } }

        public object Error
        {
            get { return error; }
            private set
            {
                bool hadError = HasError;

                if (error != value)
                {
                    error = value;
                    OnErrorChanged();
                    OnPropertyChanged(nameof(Error));

                    if (hadError != HasError)
                    {
                        OnHasErrorChanged();
                        OnPropertyChanged(nameof(HasError));
                    }
                }
            }
        }

        private object CheckAllRules()
        {
            if (validationRules != null)
            {
                foreach (var rule in validationRules)
                {
                    var validationError = rule.Validate(value);
                    if (validationError != null)
                        return validationError;
                }
            }

            return null;
        }

        private void Proxy_HasConvertErrorChanged(bool errorFlag)
        {
            if (errorFlag)
            {
                converterErrorCount++;
                if (converterErrorCount == 1)
                {
                    OnHasConverterErrorChanged();
                    OnPropertyChanged(nameof(HasConverterError));
                }
            }
            else
            {
                converterErrorCount--;
                if (converterErrorCount == 0)
                {
                    OnHasConverterErrorChanged();
                    OnPropertyChanged(nameof(HasConverterError));
                }
            }
        }
    }
}
