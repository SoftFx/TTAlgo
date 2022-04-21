using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Machinarium.Var
{
    public interface IProperty<T>
    {
        Var<T> Var { get; }

        T Value { get; set; }

        bool HasValue { get; }
    }


    public class Property<T> : PropertyBase<Var<T>, T>
    {
        public new Property<T> AddPreTrigger(Action<T> trigger) => (Property<T>)base.AddPreTrigger(trigger);

        public new Property<T> AddPostTrigger(Action<T> trigger) => (Property<T>)base.AddPostTrigger(trigger);
    }

    public class IntProperty : PropertyBase<IntVar, int> { }

    public class BoolProperty : PropertyBase<BoolVar, bool> { }

    public class DoubleProperty : PropertyBase<DoubleVar, double> { }


    public abstract class PropertyBase<TVar, T> : IProperty<T>, INotifyPropertyChanged, IDisposable
        where TVar : Var<T>, new()
    {
        private List<Action<T>> _preChangeTriggers, _postChangeTriggers;
        private readonly TVar _var;

        internal PropertyBase()
        {
            _var = new TVar();
            _var.SetContext(this);
            _var.Changed += NotificationCall;
        }

        internal string Name { get; set; }

        internal IDisplayValueConverter<T> DisplayConverter { get; set; }

        public virtual void Dispose()
        {
            _var.Changed -= NotificationCall;
            _var.Dispose();
        }

        public TVar Var
        {
            get => _var;
            set => _var.AttachSource(value);
        }

        public T Value
        {
            get => _var.Value;
            set
            {
                _preChangeTriggers?.ForEach(u => u?.Invoke(_var.Value));

                _var.Value = value;

                _postChangeTriggers?.ForEach(u => u?.Invoke(value));
            }
        }

        protected PropertyBase<TVar, T> AddPreTrigger(Action<T> trigger)
        {
            if (_preChangeTriggers == null)
                _preChangeTriggers = new List<Action<T>>(1);

            _preChangeTriggers.Add(trigger);

            return this;
        }

        protected PropertyBase<TVar, T> AddPostTrigger(Action<T> trigger)
        {
            if (_postChangeTriggers == null)
                _postChangeTriggers = new List<Action<T>>(1);

            _postChangeTriggers.Add(trigger);

            trigger?.Invoke(Value);

            return this;
        }

        public string DisplayValue => ConvertValueTo(Value);

        public bool HasValue => !EqualityComparer<T>.Default.Equals(Value, default);

        Var<T> IProperty<T>.Var => Var;

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString() => $"{Name ?? nameof(TVar)}: {Value}";

        protected void NotifyPropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string ConvertValueTo(T value)
        {
            try
            {
                return DisplayConverter != null ? DisplayConverter.Convert(value) : value?.ToString();
            }
            catch
            {
                return value?.ToString();
            }
        }

        protected virtual void NotificationCall()
        {
            NotifyPropertyChange(nameof(Value));
            NotifyPropertyChange(nameof(HasValue));
            NotifyPropertyChange(nameof(DisplayValue));
        }
    }
}
