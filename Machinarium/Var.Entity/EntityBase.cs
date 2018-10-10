using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class EntityBase : IDisposable
    {
        private List<IDisposable> _disposableChildren;

        public virtual void Dispose()
        {
            if (_disposableChildren != null)
            {
                foreach (var c in _disposableChildren)
                    c.Dispose();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void AddDisposableChild(IDisposable child)
        {
            if (_disposableChildren == null)
                _disposableChildren = new List<IDisposable>();
            _disposableChildren.Add(child);
        }

        protected Property<T> AddProperty<T>(T initialValue = default(T), string notifyName = null)
        {
            var property = new Property<T>();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        protected IntProperty AddIntProperty(int initialValue = 0, string notifyName = null)
        {
            var property = new IntProperty();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        protected DoubleProperty AddDoubleProperty(double initialValue = 0, string notifyName = null)
        {
            var property = new DoubleProperty();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }


        protected BoolProperty AddBoolProperty(bool initialValue = false, string notifyName = null)
        {
            var property = new BoolProperty();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        protected Validable<T> AddValidable<T>(T initialValue = default(T), string notifyName = null)
        {
            var property = new Validable<T>();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        protected IntValidable AddIntValidable(int initialValue = 0, string notifyName = null)
        {
            var property = new IntValidable();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        protected DoubleValidable AddDoubleValidable(double initialValue = 0, string notifyName = null)
        {
            var property = new DoubleValidable();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        protected BoolValidable AddBoolValidable(bool initialValue = false, string notifyName = null)
        {
            var property = new BoolValidable();
            property.Value = initialValue;
            property.Name = notifyName;
            AddDisposableChild(property);
            return property;
        }

        private void RegisterPropertyNotification<T>(Property<T> property, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                property.PropertyChanged += (s, a) => OnPropertyChanged(name);
        }

        protected void TriggerOn(BoolVar condition, Action action)
        {
            TriggerOn(condition, action, null);
        }

        protected void TriggerOn(BoolVar condition, Action onTrue, Action onFalse)
        {
            var trigger = new BoolEvent(condition, onTrue, onFalse);
            AddDisposableChild(trigger);
        }

        protected void TriggerOnChange<T>(Var<T> var, Action<VarChangeEventArgs<T>> changeHandler)
        {
            var trigger = new ChangeEvent<T>(var, changeHandler);
            AddDisposableChild(trigger);
        }

        protected void TriggerOnChange<T>(IProperty<T> property, Action<VarChangeEventArgs<T>> changeHandler)
        {
            var trigger = new ChangeEvent<T>(property.Var, changeHandler);
            AddDisposableChild(trigger);
        }
    }
}

