using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public interface IProperty<T>
    {
        Var<T> Var { get; }
        T Value { get; }
    }

    public class PropertyBase<TVar, T> : IProperty<T>, INotifyPropertyChanged, IDisposable
        where TVar : Var<T>, new()
    {
        private TVar _var;

        internal PropertyBase()
        {
            _var = new TVar();
            _var.SetContext(this);
            _var.Changed += () => NotifyPropertyChange(nameof(Value));
        }

        protected void NotifyPropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public virtual void Dispose()
        {
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

        Var<T> IProperty<T>.Var => Var;


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Property<T> : PropertyBase<Var<T>, T> { }
    public class IntProperty : PropertyBase<IntVar, int> { }
    public class BoolProperty : PropertyBase<BoolVar, bool> { }
}
