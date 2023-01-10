using System;
using System.Collections.Generic;

namespace Machinarium.Var
{
    public class EntityBase : IDisposable
    {
        private List<IDisposable> _disposableChildren;
        private List<IValidable> _validableChildren;

        public BoolVar HasError { get; private set; }

        public EntityBase()
        {
            HasError = new BoolVar();
        }

        public BoolVar GetValidationModelResult()
        {
            BoolVar result = new BoolVar(true);

            foreach (var v in _validableChildren)
                result &= v.ErrorVar.IsEmpty();

            return result;
        }

        protected void AddChild(IDisposable child)
        {
            if (_disposableChildren == null)
                _disposableChildren = new List<IDisposable>();

            _disposableChildren.Add(child);

            if (child is IValidable valid)
            {
                if (_validableChildren == null)
                    _validableChildren = new List<IValidable>();

                _validableChildren.Add(valid);

                HasError |= valid.HasError;
            }
        }

        protected Property<T> AddProperty<T>(T initialValue = default, IDisplayValueConverter<T> displayConverter = null, string notifyName = null)
        {
            var property = new Property<T>
            {
                Value = initialValue,
                Name = notifyName,
                DisplayConverter = displayConverter,
            };

            AddChild(property);
            return property;
        }

        protected IntProperty AddIntProperty(int initialValue = 0, string notifyName = null)
        {
            var property = new IntProperty
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected StrProperty AddStrProperty(string initialValue = null, string notifyName = null)
        {
            var property = new StrProperty
            {
                Value = initialValue,
                Name = notifyName,
            };

            AddChild(property);
            return property;
        }

        protected DoubleProperty AddDoubleProperty(double initialValue = 0, string notifyName = null)
        {
            var property = new DoubleProperty
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected BoolProperty AddBoolProperty(bool initialValue = false, string notifyName = null)
        {
            var property = new BoolProperty
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected Validable<T> AddValidable<T>(T initialValue = default, string notifyName = null)
        {
            var property = new Validable<T>(null)
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected IntValidable AddIntValidable(int initialValue = 0, string notifyName = null)
        {
            var property = new IntValidable
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected DoubleValidable AddDoubleValidable(double initialValue = 0, IValueConverter<double, string> conv = null, string notifyName = null)
        {
            var property = new DoubleValidable(conv)
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected BoolValidable AddBoolValidable(bool initialValue = false, string notifyName = null)
        {
            var property = new BoolValidable
            {
                Value = initialValue,
                Name = notifyName
            };

            AddChild(property);
            return property;
        }

        protected PropConverter<TProp, T> AddConverter<TProp, T>(IValidable<TProp> property, IValueConverter<TProp, T> valueConverter)
        {
            var converter = new PropConverter<TProp, T>(property, valueConverter);
            AddChild(converter);
            return converter;
        }

        //private void RegisterPropertyNotification<T>(Property<T> property, string name)
        //{
        //    if (!string.IsNullOrWhiteSpace(name))
        //        property.PropertyChanged += (s, a) => OnPropertyChanged(name);
        //}

        protected IDisposable TriggerOn(BoolVar condition, Action action)
        {
            return TriggerOn(condition, action, null);
        }

        protected IDisposable TriggerOn(BoolVar condition, Action onTrue, Action onFalse)
        {
            var trigger = new BoolEvent(condition, onTrue, onFalse);
            AddChild(trigger);
            return trigger;
        }

        protected IDisposable TriggerOnChange<T>(Var<T> var, Action<VarChangeEventArgs<T>> changeHandler)
        {
            var trigger = new ChangeEvent<T>(var, changeHandler);
            AddChild(trigger);
            return trigger;
        }

        protected IDisposable TriggerOnChange<T>(IProperty<T> property, Action<VarChangeEventArgs<T>> changeHandler)
        {
            var trigger = new ChangeEvent<T>(property.Var, changeHandler);
            AddChild(trigger);
            return trigger;
        }

        public virtual void Dispose()
        {
            _validableChildren?.ForEach(u => u.Dispose());
            _disposableChildren?.ForEach(u => u.Dispose());
        }
    }
}

