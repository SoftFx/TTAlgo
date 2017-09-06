using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public static class CmpVar
    {
        public static CmpVar<T> New<T>(T initialValue = default(T))
            where T : IComparable<T>, IEquatable<T>
        {
            return new CmpVar<T>.Variable(initialValue);
        }
    }

    public abstract class CmpVar<T> : Var
      where T : IComparable<T>, IEquatable<T>
    {
        public abstract T Value { get; set; }

        public static BoolVar operator ==(CmpVar<T> c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => EqualityComparer<T>.Default.Equals(c1.Value, c2.Value), c1, c2);
        }

        public static BoolVar operator ==(CmpVar<T> c1, T c2)
        {
            return new BoolVar.Operator(() => EqualityComparer<T>.Default.Equals(c1.Value, c2), c1);
        }

        public static BoolVar operator ==(T c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => EqualityComparer<T>.Default.Equals(c1, c2.Value), c2);
        }

        public static BoolVar operator !=(CmpVar<T> c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => !EqualityComparer<T>.Default.Equals(c1.Value, c2.Value), c1, c2);
        }

        public static BoolVar operator !=(CmpVar<T> c1, T c2)
        {
            return new BoolVar.Operator(() => !EqualityComparer<T>.Default.Equals(c1.Value, c2), c1);
        }

        public static BoolVar operator !=(T c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => !EqualityComparer<T>.Default.Equals(c1, c2.Value), c2);
        }

        public static BoolVar operator >(CmpVar<T> c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) > 0, c1);
        }

        public static BoolVar operator <(CmpVar<T> c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) < 0, c1);
        }

        public static BoolVar operator >=(CmpVar<T> c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) >= 0, c1);
        }

        public static BoolVar operator <=(CmpVar<T> c1, CmpVar<T> c2)
        {
            return new BoolVar.Operator(() => Comparer<T>.Default.Compare(c1.Value, c2.Value) <= 0, c1);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        internal class Variable : CmpVar<T>
        {
            private T _val;

            public Variable(T initialValue)
            {
                _val = initialValue;
            }

            public override T Value
            {
                get => _val;
                set
                {
                    if (!EqualityComparer<T>. Default.Equals(_val, value))
                    {
                        _val = value;
                        OnChanged();
                    }
                }
            }

            public override void Dispose()
            {
                OnDisposed();
            }
        }

        internal class Operator : CmpVar<T>
        {
            private IVar[] _baseSources;
            private Func<T> _operatorDef;

            public Operator(Func<T> operatorDef, params IVar[] baseSources)
            {
                _operatorDef = operatorDef;
                _baseSources = baseSources;

                foreach (var src in baseSources)
                    src.Changed += Src_Changed;
            }

            private void Src_Changed(bool disposed)
            {
                if (disposed)
                    Dispose();
                else
                    OnChanged();
            }

            public override T Value
            {
                get => _operatorDef();
                set => throw new Exception("Cannot set value for operator!");
            }

            public override void Dispose()
            {
                foreach (var src in _baseSources)
                    src.Changed -= Src_Changed;

                OnDisposed();
            }
        }
    }
}
