using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public abstract class IntVar : Var, IVar<int>
    {
        public abstract int Value { get; set; }

        public static implicit operator IntVar(int v)
        {
            return new Variable(v);
        }

        public static IntVar operator +(IntVar c1, IntVar c2)
        {
            return new Operator(() => c1.Value + c2.Value, c1, c2);
        }

        public static IntVar operator +(IntVar c1, int c2)
        {
            return new Operator(() => c1.Value + c2, c1);
        }

        public static IntVar operator +(int c1, IntVar c2)
        {
            return new Operator(() => c1 + c2.Value, c2);
        }

        public static IntVar operator -(IntVar c1, IntVar c2)
        {
            return new Operator(() => c1.Value - c2.Value, c1, c2);
        }

        public static IntVar operator -(int c1, IntVar c2)
        {
            return new Operator(() => c1 - c2.Value, c2);
        }

        public static IntVar operator -(IntVar c1, int c2)
        {
            return new Operator(() => c1.Value - c2, c1);
        }

        public static IntVar operator /(IntVar c1, IntVar c2)
        {
            return new Operator(() => c1.Value / c2.Value, c1, c2);
        }

        public static IntVar operator /(int c1, IntVar c2)
        {
            return new Operator(() => c1 / c2.Value, c2);
        }

        public static IntVar operator /(IntVar c1, int c2)
        {
            return new Operator(() => c1.Value / c2, c1);
        }

        public static IntVar operator *(IntVar c1, IntVar c2)
        {
            return new Operator(() => c1.Value * c2.Value, c1, c2);
        }

        public static IntVar operator *(int c1, IntVar c2)
        {
            return new Operator(() => c1 * c2.Value, c2);
        }

        public static IntVar operator *(IntVar c1, int c2)
        {
            return new Operator(() => c1.Value * c2, c1);
        }

        public static BoolVar operator >(IntVar c1, IntVar c2)
        {
            return new BoolVar.Operator(() => c1.Value > c2.Value, c1);
        }

        public static BoolVar operator <(IntVar c1, IntVar c2)
        {
            return new BoolVar.Operator(() => c1.Value < c2.Value, c1);
        }

        public static BoolVar operator >=(IntVar c1, IntVar c2)
        {
            return new BoolVar.Operator(() => c1.Value >= c2.Value, c1);
        }

        public static BoolVar operator <=(IntVar c1, IntVar c2)
        {
            return new BoolVar.Operator(() => c1.Value <= c2.Value, c1);
        }

        public class Variable : IntVar
        {
            private int _val;

            public Variable(int initialValue = 0)
            {
                _val = initialValue;
            }

            public override int Value
            {
                get => _val;
                set
                {
                    if (_val != value)
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

        internal class Operator : IntVar
        {
            private IVar[] _baseSources;
            private Func<int> _operatorDef;

            public Operator(Func<int> operatorDef, params IVar[] baseSources)
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

            public override int Value
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
