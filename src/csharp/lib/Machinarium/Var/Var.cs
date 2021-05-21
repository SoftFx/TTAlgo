using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    [TypeConverter(typeof(VarConverter))]
    public abstract class Var : INotifyPropertyChanged
    {
        private object _context;

        internal Var()
        {
            //_context = VarContext.Register(this);
        }

        internal event Action Changed;
        public event PropertyChangedEventHandler PropertyChanged;
        public abstract void Dispose();

        public static BoolVar Const(bool val)
        {
            return new BoolVar(val);
        }

        public static IntVar Const(int val)
        {
            return new IntVar(val);
        }

        public static DoubleVar Const(double val)
        {
            return new DoubleVar(val);
        }

        public static Var<T> Const<T>(T val)
        {
            return new Var<T>(val);
        }

        internal abstract void AttachSource(object src);
        internal abstract object GetBoxedValue();

        internal void SetContext(object context)
        {
            if (_context == null)
                _context = context;
        }

        internal void DisposeIfNoContext()
        {
            if (_context == null)
                Dispose();
        }

        protected void OnChanged()
        {
            Changed?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
        }

        //protected void OnDisposed()
        //{
        //    //Changed?.Invoke(true); // propogate dispose up
        //}
    }

    public class Var<T> : Var
    {
        private T _val;
        private IDisposable _srcOperator;

        public Action<T> ChangeEvent;

        public Var()
        {
        }

        public Var(T initialValue = default(T))
        {
            _val = initialValue;
        }

        public static BoolVar operator ==(Var<T> c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => EqualityComparer<T>.Default.Equals(c1.Value, c2.Value), c1, c2);
        }

        public static BoolVar operator ==(Var<T> c1, T c2)
        {
            return BoolVar.Operator<BoolVar>(() => EqualityComparer<T>.Default.Equals(c1.Value, c2), c1);
        }

        public static BoolVar operator ==(T c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => EqualityComparer<T>.Default.Equals(c1, c2.Value), c2);
        }

        public static BoolVar operator !=(Var<T> c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => !EqualityComparer<T>.Default.Equals(c1.Value, c2.Value), c1, c2);
        }

        public static BoolVar operator !=(Var<T> c1, T c2)
        {
            return BoolVar.Operator<BoolVar>(() => !EqualityComparer<T>.Default.Equals(c1.Value, c2), c1);
        }

        public static BoolVar operator !=(T c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => !EqualityComparer<T>.Default.Equals(c1, c2.Value), c2);
        }

        public static BoolVar operator >(Var<T> c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) > 0, c1, c2);
        }

        public static BoolVar operator >(Var<T> c1, T c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1.Value, c2) > 0, c1);
        }

        public static BoolVar operator >(T c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1, c2.Value) > 0, c2);
        }

        public static BoolVar operator <(Var<T> c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) < 0, c1, c2);
        }

        public static BoolVar operator <(Var<T> c1, T c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1.Value, c2) < 0, c1);
        }

        public static BoolVar operator <(T c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1, c2.Value) < 0, c2);
        }

        public static BoolVar operator >=(Var<T> c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) >= 0, c1);
        }

        public static BoolVar operator <=(Var<T> c1, Var<T> c2)
        {
            return BoolVar.Operator<BoolVar>(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) <= 0, c1);
        }

        public T Value
        {
            get => _val;
            set
            {
                if (_srcOperator != null)
                    throw new Exception();

                SetValueInternal(value);
            }
        }

        #region Internal

        internal virtual bool Equals(T val1, T val2)
        {
            return EqualityComparer<T>.Default.Equals(val1, val2);
        }

        internal void SetValueInternal(T value)
        {
            if (!Equals(_val, value))
            {
                _val = value;

                ChangeEvent?.Invoke(value);
                OnChanged();
            }
        }

        internal override void AttachSource(object src)
        {
            var srcVar = (Var<T>)src;
            new Operator<T>(this, () => srcVar.Value, srcVar);
        }

        internal void SetOperator(IDisposable srcOperator)
        {
            if (_srcOperator != null)
                _srcOperator.Dispose();

            _srcOperator = srcOperator;
        }

        internal override object GetBoxedValue()
        {
            return _val;
        }

        public override void Dispose()
        {
            _srcOperator?.Dispose();
            //OnDisposed();
        }

        internal static TVar Operator<TVar>(Func<T> operatorDef, params Var[] baseSources)
           where TVar : Var<T>, new()
        {
            TVar varObj = new TVar();
            new Operator<T>(varObj, operatorDef, baseSources);
            return varObj;
        }

        internal static TVar SelectOperator<TVar, TEntity>(Func<TEntity, Var<T>> selector, Var<TEntity> entityRef)
            where TVar : Var<T>, new()
            where TEntity : class
        {
            TVar varObj = new TVar();
            new SelectOperator<T, TEntity>(varObj, entityRef, selector);
            return varObj;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
