using System;
using System.ComponentModel;

namespace Machinarium.Var
{
    public interface IProperty<T>
    {
        Var<T> Var { get; }
        T Value { get; set; }
    }

    public class PropertyBase<TVar, T> : IProperty<T>, INotifyPropertyChanged, IDisposable
        where TVar : Var<T>, new()
    {
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
            set => _var.Value = value;
        }

        public string DisplayValue => ConvertValueTo(Value);

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
                return DisplayConverter != null ? DisplayConverter.Convert(value) : value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        private void NotificationCall()
        {
            NotifyPropertyChange(nameof(Value));
            NotifyPropertyChange(nameof(DisplayValue));
        }
    }

    public class Property<T> : PropertyBase<Var<T>, T> { }
    public class IntProperty : PropertyBase<IntVar, int> { }
    public class BoolProperty : PropertyBase<BoolVar, bool> { }
    public class DoubleProperty : PropertyBase<DoubleVar, double> { }
    //public class DecimalProperty : PropertyBase<DecimalVar, double> { }
}
